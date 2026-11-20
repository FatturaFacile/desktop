// <copyright file="CopilotMessageViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Copilot;
using FatturaFacile.Wpf.Copilot;

namespace FatturaFacile.Wpf.ViewModels.Copilot;

/// <summary>
/// One message of the conversation shown in the Copilot drawer: Markdown, citations, visualization and action card.
/// </summary>
internal sealed partial class CopilotMessageViewModel : ObservableObject
{
    private readonly ICopilotHost _host;

    [ObservableProperty]
    private string _contentMarkdown = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<CopilotCitation> _citations = [];

    [ObservableProperty]
    private CopilotVisualizationViewModel? _visualization;

    [ObservableProperty]
    private CopilotActionViewModel? _action;

    [ObservableProperty]
    private bool _isStreaming;

    [ObservableProperty]
    private string _activity = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotMessageViewModel"/> class.
    /// </summary>
    /// <param name="id">The server identifier ("local" for a message not yet known to the server).</param>
    /// <param name="role">The role (user or assistant).</param>
    /// <param name="host">The owner.</param>
    public CopilotMessageViewModel(string id, string role, ICopilotHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        Id = id;
        Role = role;
        _host = host;
    }

    /// <summary>
    /// Gets the server identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the role.
    /// </summary>
    public string Role { get; }

    /// <summary>
    /// Gets a value indicating whether the message was written by the user.
    /// </summary>
    public bool IsUser => string.Equals(Role, "user", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the citation chips: regulatory snippets of CH/IT law (kept for offline viewing) and entity links.
    /// </summary>
    public IReadOnlyList<CopilotCitationViewModel> CitationChips => [.. Citations.Select(c => new CopilotCitationViewModel(c, OpenCitationCommand))];

    /// <summary>
    /// Gets a value indicating whether the message has citations.
    /// </summary>
    public bool HasCitations => Citations.Count > 0;

    /// <summary>
    /// Creates the view model of a message loaded from the server or the cache.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="host">The owner.</param>
    /// <returns>The view model.</returns>
    public static CopilotMessageViewModel From(CopilotMessage message, ICopilotHost host)
    {
        ArgumentNullException.ThrowIfNull(message);

        CopilotMessageViewModel viewModel = new(message.Id, message.Role, host)
        {
            ContentMarkdown = message.ContentMarkdown,
            Citations = message.Citations,
        };

        viewModel.SetVisualization(message.Visualization);
        viewModel.SetAction(message.ProposedAction, message.ActionStatus, message.Id);
        return viewModel;
    }

    /// <summary>
    /// Shows a visualization.
    /// </summary>
    /// <param name="visualization">The payload, or null.</param>
    public void SetVisualization(CopilotVisualization? visualization) =>
        Visualization = visualization is null ? null : new CopilotVisualizationViewModel(visualization);

    /// <summary>
    /// Shows an action card.
    /// </summary>
    /// <param name="action">The proposed action, or null.</param>
    /// <param name="status">The action status.</param>
    /// <param name="messageId">The server identifier of the message carrying the action.</param>
    public void SetAction(CopilotProposedAction? action, string? status, string messageId) =>
        Action = action is null ? null : new CopilotActionViewModel(messageId, action, status, _host);

    /// <summary>
    /// Opens a link of the rendered Markdown: "entity:{index}" navigates to the native view, http(s) opens the browser.
    /// </summary>
    /// <param name="target">The link target.</param>
    /// <returns>A task representing the navigation.</returns>
    [RelayCommand]
    public Task OpenLinkAsync(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return Task.CompletedTask;
        }

        if (target.StartsWith(MarkdownFlowDocumentBuilder.EntityPrefix, StringComparison.Ordinal)
            && int.TryParse(target[MarkdownFlowDocumentBuilder.EntityPrefix.Length..], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var index)
            && index >= 0
            && index < Citations.Count)
        {
            return _host.OpenCitationAsync(Citations[index]);
        }

        if (Uri.TryCreate(target, UriKind.Absolute, out var uri) && (string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal) || string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal)))
        {
            _host.OpenExternal(uri.AbsoluteUri);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Opens a citation chip.
    /// </summary>
    /// <param name="citation">The citation.</param>
    /// <returns>A task representing the navigation.</returns>
    [RelayCommand]
    public Task OpenCitationAsync(CopilotCitation? citation) =>
        citation is null ? Task.CompletedTask : _host.OpenCitationAsync(citation);

    partial void OnCitationsChanged(IReadOnlyList<CopilotCitation> value)
    {
        OnPropertyChanged(nameof(CitationChips));
        OnPropertyChanged(nameof(HasCitations));
    }
}
