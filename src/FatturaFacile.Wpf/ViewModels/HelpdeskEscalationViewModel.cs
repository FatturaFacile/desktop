// <copyright file="HelpdeskEscalationViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Helpdesk;
using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The escalation actions of a ticket: a Field Service work order or an RMA. Both are idempotent on the server, so a repeat
/// returns the record that already exists; offline they are queued.
/// </summary>
internal sealed partial class HelpdeskEscalationViewModel : ObservableObject
{
    private const string Good = "Good";
    private const string Warn = "Warn";
    private const string Bad = "Bad";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _time;
    private readonly HelpdeskTicketRisk _ticket;
    private readonly Func<Task>? _completed;

    [ObservableProperty]
    private string _workType = string.Empty;

    [ObservableProperty]
    private DateTime? _scheduledStart;

    [ObservableProperty]
    private DateTime? _scheduledEnd;

    [ObservableProperty]
    private string _location = string.Empty;

    [ObservableProperty]
    private string _serialNumber = string.Empty;

    [ObservableProperty]
    private bool _goodwill;

    [ObservableProperty]
    private string _symptom = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _resultText = string.Empty;

    [ObservableProperty]
    private string _resultState = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpdeskEscalationViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    /// <param name="ticket">The ticket to escalate.</param>
    /// <param name="assets">The assets of the customer, offered as serial numbers.</param>
    /// <param name="completed">Called after an escalation was sent or queued, to refresh the workbench.</param>
    public HelpdeskEscalationViewModel(ISender sender, ILocalizationService localization, TimeProvider time, HelpdeskTicketRisk ticket, IEnumerable<string> assets, Func<Task>? completed)
    {
        _sender = sender;
        _localization = localization;
        _time = time;
        _ticket = ticket;
        _completed = completed;
        foreach (var serial in assets)
        {
            Serials.Add(serial);
        }
    }

    /// <summary>
    /// Gets the number of the ticket.
    /// </summary>
    public string TicketNumber => _ticket.TicketNumber;

    /// <summary>
    /// Gets the serial numbers of the assets of the customer.
    /// </summary>
    public ObservableCollection<string> Serials { get; } = [];

    /// <summary>
    /// Escalates the ticket to a Field Service work order.
    /// </summary>
    /// <returns>A task representing the escalation.</returns>
    [RelayCommand]
    public Task EscalateToFieldServiceAsync()
    {
        HelpdeskFieldServiceRequest request = new(
            Guid.NewGuid(),
            _ticket.TicketId,
            _ticket.TicketNumber,
            WorkType,
            Utc(ScheduledStart),
            Utc(ScheduledEnd),
            Location,
            _time.GetUtcNow().UtcDateTime);

        return RunAsync(new EscalateTicketToFieldServiceCommand(request), "Helpdesk_FsDone");
    }

    /// <summary>
    /// Escalates the ticket to an RMA.
    /// </summary>
    /// <returns>A task representing the escalation.</returns>
    [RelayCommand]
    public Task EscalateToRmaAsync()
    {
        HelpdeskRmaRequest request = new(
            Guid.NewGuid(),
            _ticket.TicketId,
            _ticket.TicketNumber,
            SerialNumber,
            EquipmentPassportId: null,
            Goodwill,
            Symptom,
            _time.GetUtcNow().UtcDateTime);

        return RunAsync(new EscalateTicketToRmaCommand(request), "Helpdesk_RmaDone");
    }

    private static DateTime? Utc(DateTime? local) =>
        local is { } value ? DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime() : null;

    private async Task RunAsync(IRequest<HelpdeskActionResult> command, string doneKey)
    {
        IsBusy = true;

        try
        {
            var result = await _sender.Send(command);
            Apply(result, doneKey);

            if (result.Status is HelpdeskActionStatus.Sent or HelpdeskActionStatus.Queued && _completed is not null)
            {
                await _completed();
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Show(ex.Message, Bad);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Apply(HelpdeskActionResult result, string doneKey)
    {
        switch (result.Status)
        {
            case HelpdeskActionStatus.Sent:
                Show(
                    _localization[doneKey] + (result.TrackingToken is { } token ? " " + string.Format(_localization.CurrentCulture, _localization["Helpdesk_RmaToken"], token) : string.Empty),
                    Good);
                break;

            case HelpdeskActionStatus.Queued:
                Show(_localization["Helpdesk_Queued"], Warn);
                break;

            case HelpdeskActionStatus.Conflict:
                Show(string.Format(_localization.CurrentCulture, _localization["Helpdesk_Refused"], result.Detail ?? string.Empty), Bad);
                break;

            case HelpdeskActionStatus.Rejected:
                Show(_localization[result.ErrorKey ?? HelpdeskService.TicketKey], Bad);
                break;

            default:
                throw new InvalidOperationException($"Unexpected action status {result.Status}.");
        }
    }

    private void Show(string text, string state)
    {
        ResultText = text;
        ResultState = state;
    }
}
