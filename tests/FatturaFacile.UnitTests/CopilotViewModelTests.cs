// <copyright file="CopilotViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.

using System.Runtime.CompilerServices;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Copilot;
using FatturaFacile.Application.Features.Copilot;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.Copilot;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Copilot drawer view model: streaming, context injection, offline browsing, actions and navigation.
/// </summary>
public sealed class CopilotViewModelTests
{
    private static readonly CopilotProposedAction Proposed = new(
        "create_invoice",
        "invoicing",
        "pending",
        "Crea fattura",
        [
            new CopilotArgument("customer", "ACME", CopilotArgumentKind.Text),
            new CopilotArgument("items.0.quantity", "2", CopilotArgumentKind.Number),
        ],
        new Dictionary<string, string> { ["Cliente"] = "ACME" },
        ["items.0.quantity"],
        null,
        null);

    /// <summary>
    /// A prompt creates the conversation, injects the active context, streams the answer and caches the server copy.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Send_InjectsContextStreamsAndCaches()
    {
        using CopilotTestHost host = new();
        FakeApi api = new() { ServerMessages = [Msg("m1", "user", "Quanto?"), Msg("m2", "assistant", "Risposta **ok**")] };
        using var vm = Create(host, api, online: true);
        vm.ContextProvider = static () => new CopilotContext("/invoicing", "invoice", "42");
        vm.Prompt = "Quanto?";

        await vm.SendAsync();

        Assert.Equal("/invoicing", api.LastContext?.Route);
        Assert.Equal("invoice", api.LastContext?.EntityType);
        Assert.Equal("42", api.LastContext?.EntityId);
        Assert.Equal(["Quanto?", "Risposta **ok**"], vm.Messages.Select(static m => m.ContentMarkdown));
        Assert.False(vm.IsBusy);
        Assert.Empty(vm.StatusMessage);

        var cached = await host.Sender.Send(new GetCachedCopilotConversationQuery("c1"));
        Assert.Equal(2, cached?.Messages.Count);
    }

    /// <summary>
    /// Offline, the online-required badge is shown and sending is blocked, but cached conversations remain viewable.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Offline_ShowsBadgeBlocksSendButCachedHistoryIsViewable()
    {
        using CopilotTestHost host = new();
        var citation = new CopilotCitation("regulatory", "[LIVA Art. 25]", "LIVA", "Art. 25", "CH", null, null, null);
        _ = await host.Sender.Send(new CacheCopilotConversationCommand(new CopilotConversation(
            "c1",
            "Cronologia",
            "it",
            CopilotContext.Empty,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            [new CopilotMessage("m2", "c1", "assistant", "Risposta [LIVA Art. 25]", [citation], null, null, null, DateTimeOffset.UtcNow)])));

        FakeApi api = new();
        using var vm = Create(host, api, online: false);

        await vm.OpenAsync();
        await vm.SelectConversationAsync(vm.Conversations.Single());
        vm.Prompt = "Ciao";

        Assert.True(vm.ShowOnlineBadge);
        Assert.False(vm.SendCommand.CanExecute(null));
        Assert.Equal("Risposta [LIVA Art. 25]", Assert.Single(vm.Messages).ContentMarkdown);
        Assert.Equal("LIVA", Assert.Single(vm.Messages[0].Citations).Title);
        Assert.Equal(0, api.Calls);
    }

    /// <summary>
    /// A server that cannot be reached switches to the offline badge and shows a localized message.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Send_ServerUnreachable_ShowsLocalizedOfflineMessage()
    {
        using CopilotTestHost host = new();
        FakeApi api = new() { FailWith = new CopilotApiException(CopilotApiErrorKind.Offline, statusCode: null, message: "down") };
        using var vm = Create(host, api, online: true);
        vm.Prompt = "Ciao";

        await vm.SendAsync();

        Assert.True(vm.IsOffline);
        Assert.True(vm.ShowOnlineBadge);
        Assert.Equal(LocalizationService.Instance["Copilot_Error_Offline"], vm.StatusMessage);
        Assert.NotEqual("Copilot_Error_Offline", vm.StatusMessage);
    }

    /// <summary>
    /// Rate limit and provider failures show their localized messages.
    /// </summary>
    /// <param name="kind">The failure class.</param>
    /// <returns>A task representing the test.</returns>
    [Theory]
    [InlineData(CopilotApiErrorKind.RateLimited)]
    [InlineData(CopilotApiErrorKind.ProviderUnavailable)]
    [InlineData(CopilotApiErrorKind.Forbidden)]
    public async Task Send_ApiError_ShowsLocalizedMessage(CopilotApiErrorKind kind)
    {
        using CopilotTestHost host = new();
        FakeApi api = new() { FailWith = new CopilotApiException(kind, statusCode: 429, message: "x") };
        using var vm = Create(host, api, online: true);
        vm.Prompt = "Ciao";

        await vm.SendAsync();

        var expected = LocalizationService.Instance["Copilot_Error_" + kind];
        Assert.Equal(expected, vm.StatusMessage);
        Assert.NotEqual("Copilot_Error_" + kind, vm.StatusMessage);
        Assert.False(vm.IsOffline);
    }

    /// <summary>
    /// Confirming sends only the changed editable fields; the card then shows the executed status and the result.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Confirm_SendsOnlyEditableOverridesAndUpdatesCard()
    {
        using CopilotTestHost host = new();
        FakeApi api = new() { ServerMessages = [Msg("m1", "user", "Crea"), new CopilotMessage("m2", "c1", "assistant", "Proposta", [], null, Proposed, "pending", DateTimeOffset.UtcNow)] };
        using var vm = Create(host, api, online: true);
        vm.Prompt = "Crea";
        await vm.SendAsync();

        var card = vm.Messages[1].Action;
        Assert.NotNull(card);
        var field = Assert.Single(card.EditableFields);
        Assert.Equal("items.0.quantity", field.Path);
        field.Value = "5";

        await card.ConfirmAsync();

        Assert.Equal("m2", api.ConfirmedMessageId);
        var overrides = Assert.IsType<IReadOnlyDictionary<string, object?>>(api.ConfirmedOverrides, exactMatch: false);
        Assert.Equal(5m, Assert.Single(overrides).Value);
        Assert.Equal("executed", card.Status);
        Assert.Equal("Fattura creata", card.ResultText);
        Assert.False(card.IsPending);
    }

    /// <summary>
    /// Cancelling rejects the action on the server.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Cancel_RejectsAction()
    {
        using CopilotTestHost host = new();
        FakeApi api = new() { ServerMessages = [Msg("m1", "user", "Crea"), new CopilotMessage("m2", "c1", "assistant", "Proposta", [], null, Proposed, "pending", DateTimeOffset.UtcNow)] };
        using var vm = Create(host, api, online: true);
        vm.Prompt = "Crea";
        await vm.SendAsync();

        var card = vm.Messages[1].Action!;
        await card.RejectAsync();

        Assert.Equal("m2", api.RejectedMessageId);
        Assert.Equal("rejected", card.Status);
        Assert.False(card.IsPending);
    }

    /// <summary>
    /// An entity citation navigates to the native module; a regulatory citation shows its source instead.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Citation_EntityNavigatesAndRegulatoryShowsSource()
    {
        using CopilotTestHost host = new();
        using var vm = Create(host, new FakeApi(), online: true);
        (string Module, string? Type, string? Id)? navigated = null;
        vm.NavigateToEntity = (module, type, id) =>
        {
            navigated = (module, type, id);
            return Task.CompletedTask;
        };

        var message = CopilotMessageViewModel.From(
            new CopilotMessage(
                "m1",
                "c1",
                "assistant",
                "Vedi #Invoice-2026-0042",
                [
                    new CopilotCitation("entity", "#Invoice-2026-0042", null, null, null, "invoice", "42", "/invoices/42"),
                    new CopilotCitation("regulatory", "[LIVA Art. 25]", "LIVA", "Art. 25", "CH", null, null, null),
                ],
                null,
                null,
                null,
                null),
            vm);

        await message.OpenLinkAsync("entity:0");
        Assert.Equal(("Invoicing", "invoice", "42"), navigated);

        await message.OpenCitationAsync(message.Citations[1]);
        Assert.Contains("LIVA", vm.StatusMessage, StringComparison.Ordinal);
        Assert.Contains("Art. 25", vm.StatusMessage, StringComparison.Ordinal);
    }

    private static CopilotMessage Msg(string id, string role, string text) =>
        new(id, "c1", role, text, [], null, null, null, DateTimeOffset.UtcNow);

    private static CopilotViewModel Create(CopilotTestHost host, FakeApi api, bool online) =>
        new(host.Sender, api, host.Tenant, LocalizationService.Instance, new FakeNetwork(online), new FakeShell());

    private sealed class FakeNetwork(bool online) : INetworkStatus
    {
        public event EventHandler? Changed
        {
            add => _ = value;
            remove => _ = value;
        }

        public bool IsOnline { get; } = online;
    }

    private sealed class FakeShell : IShellLauncher
    {
        public List<string> Opened { get; } = [];

        public void Open(string path) => Opened.Add(path);
    }

    private sealed class FakeApi : ICopilotApiClient
    {
        public IReadOnlyList<CopilotMessage> ServerMessages { get; set; } = [];

        public CopilotApiException? FailWith { get; set; }

        public int Calls { get; private set; }

        public CopilotContext? LastContext { get; private set; }

        public string? ConfirmedMessageId { get; private set; }

        public IReadOnlyDictionary<string, object?>? ConfirmedOverrides { get; private set; }

        public string? RejectedMessageId { get; private set; }

        public bool IsConfigured => true;

        public Task<IReadOnlyList<CopilotConversation>> GetConversationsAsync(CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult<IReadOnlyList<CopilotConversation>>([]);
        }

        public Task<CopilotConversation> CreateConversationAsync(string? title, string locale, CopilotContext context, CancellationToken cancellationToken = default)
        {
            Calls++;
            return FailWith is null
                ? Task.FromResult(Conversation([]))
                : Task.FromException<CopilotConversation>(FailWith);
        }

        public Task<CopilotConversation> GetConversationAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(Conversation(ServerMessages));
        }

        public Task DeleteConversationAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<CopilotStreamEvent> StreamMessageAsync(string conversationId, string prompt, string locale, CopilotContext context, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Calls++;
            LastContext = context;
            yield return new CopilotStreamEvent(CopilotStreamEventKind.MessageStarted);
            yield return new CopilotStreamEvent(CopilotStreamEventKind.Token, "Risposta ");
            yield return new CopilotStreamEvent(CopilotStreamEventKind.MessageCompleted, MessageId: "m2", ConversationId: conversationId);
            await Task.CompletedTask;
        }

        public Task<CopilotMessage> ConfirmActionAsync(string messageId, IReadOnlyDictionary<string, object?>? overrides, CancellationToken cancellationToken = default)
        {
            ConfirmedMessageId = messageId;
            ConfirmedOverrides = overrides;
            return Task.FromResult(new CopilotMessage(messageId, "c1", "assistant", "Proposta", [], null, Proposed with { Result = "Fattura creata" }, "executed", DateTimeOffset.UtcNow));
        }

        public Task<CopilotMessage> RejectActionAsync(string messageId, string? reason, CancellationToken cancellationToken = default)
        {
            RejectedMessageId = messageId;
            return Task.FromResult(new CopilotMessage(messageId, "c1", "assistant", "Proposta", [], null, Proposed, "rejected", DateTimeOffset.UtcNow));
        }

        private static CopilotConversation Conversation(IReadOnlyList<CopilotMessage> messages) =>
            new("c1", "Titolo", "it", CopilotContext.Empty, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, messages);
    }
}
