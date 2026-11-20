// <copyright file="CopilotViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Copilot;
using FatturaFacile.Application.Features.Copilot;
using FatturaFacile.Domain.Common;
using FatturaFacile.Wpf.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Copilot;

/// <summary>
/// The ERP Copilot drawer: conversation history (cached in SQLite, viewable offline), streaming assistant turns,
/// visualizations, citations and human-in-the-loop action cards. Live model calls need a connection.
/// </summary>
internal sealed partial class CopilotViewModel : ObservableObject, ICopilotHost, IDisposable
{
    private const int TitleLength = 60;
    private readonly ISender _sender;
    private readonly ICopilotApiClient _api;
    private readonly ILocalizationService _localization;
    private readonly INetworkStatus _network;
    private readonly IShellLauncher _shell;
    private CancellationTokenSource? _cts;
    private bool _serverUnreachable;
    private int _focusCounter;

    [ObservableProperty]
    private bool _isOpen;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string _prompt = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOnlineBadge))]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private bool _isOffline;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private CopilotConversation? _currentConversation;

    [ObservableProperty]
    private int _focusRequest;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender (offline cache).</param>
    /// <param name="api">The Copilot API client.</param>
    /// <param name="currentTenantService">The active tenant context service.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="network">The network availability.</param>
    /// <param name="shell">The shell launcher used for external links.</param>
    public CopilotViewModel(
        ISender sender,
        ICopilotApiClient api,
        ICurrentTenantService currentTenantService,
        ILocalizationService localization,
        INetworkStatus network,
        IShellLauncher shell)
    {
        ArgumentNullException.ThrowIfNull(currentTenantService);
        ArgumentNullException.ThrowIfNull(network);

        _sender = sender;
        _api = api;
        _localization = localization;
        _network = network;
        _shell = shell;
        _isOffline = !network.IsOnline;

        network.Changed += (_, _) =>
        {
            _serverUnreachable = false;
            IsOffline = !network.IsOnline;
        };

        currentTenantService.CurrentTenantChanged += (_, _) => _ = ResetForTenantAsync();
    }

    /// <summary>
    /// Gets or sets the callback returning what the user is looking at (route, selected entity); injected into each prompt.
    /// </summary>
    public Func<CopilotContext>? ContextProvider { get; set; }

    /// <summary>
    /// Gets or sets the callback navigating to a native view (module name, entity type, entity identifier).
    /// </summary>
    public Func<string, string?, string?, Task>? NavigateToEntity { get; set; }

    /// <summary>
    /// Gets the conversations of the active tenant, newest first.
    /// </summary>
    public ObservableCollection<CopilotConversation> Conversations { get; } = [];

    /// <summary>
    /// Gets the messages of the current conversation.
    /// </summary>
    public ObservableCollection<CopilotMessageViewModel> Messages { get; } = [];

    /// <summary>
    /// Gets a value indicating whether the "online required" badge is shown: live model calls are not possible.
    /// </summary>
    public bool ShowOnlineBadge => IsOffline || !_api.IsConfigured;

    private bool CanCallApi => _api.IsConfigured && !IsOffline;

    private bool CanSend => !IsBusy && CanCallApi && !string.IsNullOrWhiteSpace(Prompt);

    /// <summary>
    /// Releases the cancellation source of the running stream.
    /// </summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// Opens or closes the drawer (bound to Ctrl+K).
    /// </summary>
    /// <returns>A task representing the load of the history.</returns>
    [RelayCommand]
    public Task ToggleAsync() => IsOpen ? CloseAsync() : OpenAsync();

    /// <summary>
    /// Opens the drawer, loads the cached history and refreshes it when online.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task OpenAsync()
    {
        IsOpen = true;
        RequestFocus();
        IsOffline = !_network.IsOnline || _serverUnreachable;
        return LoadConversationsAsync();
    }

    /// <summary>
    /// Closes the drawer.
    /// </summary>
    /// <returns>A completed task.</returns>
    [RelayCommand]
    public Task CloseAsync()
    {
        IsOpen = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Starts a new, empty conversation (created on the server with the first prompt).
    /// </summary>
    [RelayCommand]
    public void NewConversation()
    {
        CancelStream();
        CurrentConversation = null;
        Messages.Clear();
        StatusMessage = string.Empty;
        RequestFocus();
    }

    /// <summary>
    /// Shows a conversation: the cached copy first (works offline), then the server copy when online.
    /// </summary>
    /// <param name="conversation">The conversation.</param>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task SelectConversationAsync(CopilotConversation? conversation)
    {
        if (conversation is null)
        {
            return;
        }

        CancelStream();
        CurrentConversation = conversation;
        StatusMessage = string.Empty;

        var cached = await _sender.Send(new GetCachedCopilotConversationQuery(conversation.Id), CancellationToken.None);
        ShowMessages(cached?.Messages ?? []);

        if (CanCallApi)
        {
            _ = await TryRunAsync(async token =>
            {
                var fresh = await _api.GetConversationAsync(conversation.Id, token);
                _ = await _sender.Send(new CacheCopilotConversationCommand(fresh), token);
                ShowMessages(fresh.Messages);
            });
        }
    }

    /// <summary>
    /// Deletes the current conversation on the server (online) and in the cache.
    /// </summary>
    /// <returns>A task representing the call.</returns>
    [RelayCommand]
    public async Task DeleteConversationAsync()
    {
        var conversation = CurrentConversation;

        if (conversation is null)
        {
            return;
        }

        if (!CanCallApi)
        {
            StatusMessage = _localization["Copilot_OnlineRequired"];
            return;
        }

        var deleted = await TryRunAsync(token => _api.DeleteConversationAsync(conversation.Id, token));

        if (deleted)
        {
            _ = await _sender.Send(new DeleteCachedCopilotConversationCommand(conversation.Id), CancellationToken.None);
            _ = Conversations.Remove(conversation);
            NewConversation();
        }
    }

    /// <summary>
    /// Sends the prompt: creates the conversation when needed, streams the answer and refreshes the cache.
    /// </summary>
    /// <returns>A task representing the turn.</returns>
    [RelayCommand(CanExecute = nameof(CanSend))]
    public async Task SendAsync()
    {
        var text = Prompt.Trim();

        if (text.Length == 0)
        {
            return;
        }

        Prompt = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        var token = RenewCancellation();

        try
        {
            await RunTurnAsync(text, token);
        }
        catch (CopilotApiException ex)
        {
            ShowError(ex);
        }
        catch (OperationCanceledException)
        {
            // The user started another conversation or closed the application.
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException)
        {
            ShowError(new CopilotApiException(CopilotApiErrorKind.Offline, statusCode: null, message: ex.Message, innerException: ex));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cancels a running stream.
    /// </summary>
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning", Justification = "The previous source is cancelled first; it holds no registrations and S2952 forbids disposing outside Dispose().")]
    [RelayCommand]
    public void CancelStream()
    {
        // A cancelled source without registrations holds no resources; it is released by the garbage collector.
        _cts?.Cancel();
        _cts = null;
    }

    /// <inheritdoc />
    string ICopilotHost.Localize(string key) => _localization[key];

    /// <inheritdoc />
    Task ICopilotHost.ConfirmAsync(CopilotActionViewModel action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return DecideAsync(action, confirm: true);
    }

    /// <inheritdoc />
    Task ICopilotHost.RejectAsync(CopilotActionViewModel action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return DecideAsync(action, confirm: false);
    }

    /// <inheritdoc />
    Task ICopilotHost.OpenCitationAsync(CopilotCitation citation)
    {
        ArgumentNullException.ThrowIfNull(citation);
        return OpenCitationCoreAsync(citation);
    }

    /// <inheritdoc />
    void ICopilotHost.OpenExternal(string url) => _shell.Open(url);

    private static string TitleOf(string prompt) => prompt.Length <= TitleLength ? prompt : prompt[..TitleLength].TrimEnd() + "…";

    private void RequestFocus() => FocusRequest = Interlocked.Increment(ref _focusCounter);

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning", Justification = "The previous source is cancelled first; it holds no registrations and S2952 forbids disposing outside Dispose().")]
    private CancellationToken RenewCancellation()
    {
        CancelStream();
        _cts = new CancellationTokenSource();
        return _cts.Token;
    }

    private async Task DecideAsync(CopilotActionViewModel action, bool confirm)
    {
        if (!CanCallApi)
        {
            StatusMessage = _localization["Copilot_OnlineRequired"];
            return;
        }

        var overrides = confirm ? CopilotOverrideBuilder.Build(action.Action, action.GetEditedValues()) : null;

        _ = await TryRunAsync(async token =>
        {
            var updated = confirm
                ? await _api.ConfirmActionAsync(action.MessageId, overrides, token)
                : await _api.RejectActionAsync(action.MessageId, reason: null, token);
            _ = await _sender.Send(new CacheCopilotMessageCommand(updated), token);
            action.Apply(updated);
        });
    }

    private async Task OpenCitationCoreAsync(CopilotCitation citation)
    {
        if (!citation.IsEntity)
        {
            StatusMessage = string.Join(" · ", new[] { citation.Label, citation.Title, citation.SectionRef, citation.Jurisdiction }.Where(static s => !string.IsNullOrWhiteSpace(s)));
            return;
        }

        var module = CopilotRouteResolver.ResolveModule(citation.EntityType, citation.Route);

        if (module is null || NavigateToEntity is null)
        {
            StatusMessage = _localization["Copilot_NoNativeView"];
            return;
        }

        await NavigateToEntity(module, citation.EntityType, citation.EntityId);
    }

    private async Task RunTurnAsync(string text, CancellationToken token)
    {
        var context = ContextProvider?.Invoke() ?? CopilotContext.Empty;
        var locale = CopilotLocale.From(_localization.CurrentCulture);

        if (CurrentConversation is null)
        {
            var created = await _api.CreateConversationAsync(TitleOf(text), locale, context, token);
            _ = await _sender.Send(new CacheCopilotConversationCommand(created), token);
            CurrentConversation = created;
            Conversations.Insert(0, created);
        }

        var conversationId = CurrentConversation.Id;
        Messages.Add(new CopilotMessageViewModel("local-user", "user", this) { ContentMarkdown = text });
        CopilotMessageViewModel answer = new("local-assistant", "assistant", this) { IsStreaming = true };
        Messages.Add(answer);

        try
        {
            await foreach (var streamEvent in _api.StreamMessageAsync(conversationId, text, locale, context, token))
            {
                Apply(streamEvent, answer);
            }
        }
        finally
        {
            answer.IsStreaming = false;
            answer.Activity = string.Empty;
        }

        // The server copy carries the identifiers needed to confirm actions, and is what the offline cache stores.
        var fresh = await _api.GetConversationAsync(conversationId, token);
        _ = await _sender.Send(new CacheCopilotConversationCommand(fresh), token);
        ShowMessages(fresh.Messages);
    }

    private void Apply(CopilotStreamEvent streamEvent, CopilotMessageViewModel answer)
    {
        var kind = streamEvent.Kind;

        if (kind == CopilotStreamEventKind.Token)
        {
            answer.ContentMarkdown += streamEvent.Text;
        }
        else if (kind is CopilotStreamEventKind.ToolCall or CopilotStreamEventKind.ToolResult)
        {
            answer.Activity = _localization["Copilot_Working"];
        }
        else if (kind == CopilotStreamEventKind.Visualization)
        {
            answer.SetVisualization(streamEvent.Visualization);
        }
        else if (kind == CopilotStreamEventKind.ActionProposed)
        {
            answer.SetAction(streamEvent.ProposedAction, "pending", streamEvent.MessageId ?? answer.Id);
        }
        else if (kind == CopilotStreamEventKind.Error)
        {
            StatusMessage = string.IsNullOrWhiteSpace(streamEvent.Text) ? _localization["Copilot_Error_Unknown"] : streamEvent.Text;
        }
    }

    private void ShowMessages(IReadOnlyList<CopilotMessage> messages)
    {
        Messages.Clear();

        foreach (var message in messages.Where(static m => string.Equals(m.Role, "user", StringComparison.OrdinalIgnoreCase) || string.Equals(m.Role, "assistant", StringComparison.OrdinalIgnoreCase)))
        {
            Messages.Add(CopilotMessageViewModel.From(message, this));
        }
    }

    private async Task LoadConversationsAsync()
    {
        ReplaceConversations(await _sender.Send(new GetCachedCopilotConversationsQuery(), CancellationToken.None));

        if (!CanCallApi)
        {
            return;
        }

        _ = await TryRunAsync(async token =>
        {
            foreach (var remote in await _api.GetConversationsAsync(token))
            {
                _ = await _sender.Send(new CacheCopilotConversationCommand(remote), token);
            }

            ReplaceConversations(await _sender.Send(new GetCachedCopilotConversationsQuery(), token));
        });
    }

    private void ReplaceConversations(IReadOnlyList<CopilotConversation> conversations)
    {
        Conversations.Clear();

        foreach (var conversation in conversations)
        {
            Conversations.Add(conversation);
        }
    }

    private async Task ResetForTenantAsync()
    {
        CancelStream();
        CurrentConversation = null;
        Messages.Clear();
        StatusMessage = string.Empty;
        ReplaceConversations(await _sender.Send(new GetCachedCopilotConversationsQuery(), CancellationToken.None));
    }

    private async Task<bool> TryRunAsync(Func<CancellationToken, Task> call)
    {
        try
        {
            await call(CancellationToken.None);
            _serverUnreachable = false;
            return true;
        }
        catch (CopilotApiException ex)
        {
            ShowError(ex);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException)
        {
            ShowError(new CopilotApiException(CopilotApiErrorKind.Offline, statusCode: null, message: ex.Message, innerException: ex));
        }

        return false;
    }

    private void ShowError(CopilotApiException exception)
    {
        if (exception.Kind == CopilotApiErrorKind.Offline)
        {
            _serverUnreachable = true;
            IsOffline = true;
        }

        StatusMessage = _localization["Copilot_Error_" + exception.Kind];
    }
}
