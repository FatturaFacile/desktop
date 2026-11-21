// <copyright file="CopilotActionViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Copilot;

namespace FatturaFacile.Wpf.ViewModels.Copilot;

/// <summary>
/// The human-in-the-loop card of an action the assistant proposes: nothing runs until the user confirms; only the
/// fields listed in "proposed_action.editable_fields" can be edited.
/// </summary>
internal sealed partial class CopilotActionViewModel : ObservableObject
{
    private readonly ICopilotHost _host;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPending))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    private string _status;

    [ObservableProperty]
    private string? _resultText;

    [ObservableProperty]
    private string? _errorText;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotActionViewModel"/> class.
    /// </summary>
    /// <param name="messageId">The server identifier of the message carrying the action.</param>
    /// <param name="action">The proposed action.</param>
    /// <param name="status">The action status (pending, confirmed, rejected, executed, failed).</param>
    /// <param name="host">The owner handling confirm and reject.</param>
    public CopilotActionViewModel(string messageId, CopilotProposedAction action, string? status, ICopilotHost host)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(host);

        MessageId = messageId;
        Action = action;
        _host = host;
        _status = string.IsNullOrWhiteSpace(status) ? "pending" : status;
        _resultText = action.Result;
        _errorText = action.Error;
        Details = [.. action.Details.Select(static d => new CopilotDetailItem(d.Key, d.Value))];
        EditableFields =
        [
            .. action.EditableFields
                .Select(path => action.Arguments.FirstOrDefault(a => string.Equals(a.Path, path, StringComparison.Ordinal)))
                .OfType<CopilotArgument>()
                .Select(static a => new CopilotEditableFieldViewModel(a.Path, a.Value)),
        ];
    }

    /// <summary>
    /// Gets the server identifier of the message carrying the action.
    /// </summary>
    public string MessageId { get; }

    /// <summary>
    /// Gets the proposed action.
    /// </summary>
    public CopilotProposedAction Action { get; }

    /// <summary>
    /// Gets the facts shown on the card.
    /// </summary>
    public IReadOnlyList<CopilotDetailItem> Details { get; }

    /// <summary>
    /// Gets the fields the user may change.
    /// </summary>
    public ObservableCollection<CopilotEditableFieldViewModel> EditableFields { get; }

    /// <summary>
    /// Gets a value indicating whether the action still waits for the decision of the user.
    /// </summary>
    public bool IsPending => string.Equals(Status, "pending", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the card has editable fields.
    /// </summary>
    public bool HasEditableFields => EditableFields.Count > 0;

    /// <summary>
    /// Gets a value indicating whether a decision was taken (the status line is shown instead of the buttons).
    /// </summary>
    public bool HasStatus => !IsPending;

    /// <summary>
    /// Gets the localized status line.
    /// </summary>
    public string StatusText => _host.Localize("Copilot_Action_Status_" + Status.ToLowerInvariant());

    /// <summary>
    /// Gets the values typed by the user, by dotted path.
    /// </summary>
    /// <returns>The edited values.</returns>
    public IReadOnlyDictionary<string, string> GetEditedValues() =>
        EditableFields.ToDictionary(static f => f.Path, static f => f.Value, StringComparer.Ordinal);

    /// <summary>
    /// Applies the message returned by the server after a decision.
    /// </summary>
    /// <param name="message">The updated message.</param>
    public void Apply(CopilotMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Status = string.IsNullOrWhiteSpace(message.ActionStatus) ? Status : message.ActionStatus;
        ResultText = message.ProposedAction?.Result;
        ErrorText = message.ProposedAction?.Error;
    }

    /// <summary>
    /// Confirms and executes the action.
    /// </summary>
    /// <returns>A task representing the call.</returns>
    [RelayCommand(CanExecute = nameof(IsPending))]
    public async Task ConfirmAsync()
    {
        IsBusy = true;

        try
        {
            await _host.ConfirmAsync(this);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Cancels the action.
    /// </summary>
    /// <returns>A task representing the call.</returns>
    [RelayCommand(CanExecute = nameof(IsPending))]
    public async Task RejectAsync()
    {
        IsBusy = true;

        try
        {
            await _host.RejectAsync(this);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnStatusChanged(string value)
    {
        ConfirmCommand.NotifyCanExecuteChanged();
        RejectCommand.NotifyCanExecuteChanged();
    }
}
