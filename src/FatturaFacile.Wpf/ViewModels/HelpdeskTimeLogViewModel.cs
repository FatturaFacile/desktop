// <copyright file="HelpdeskTimeLogViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Helpdesk;
using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The time log dialog of a ticket: validates the entry, saves it (queued offline) and shows how it was settled, deducted from
/// the contract hour bank or billable.
/// </summary>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localization">The localization service.</param>
/// <param name="time">The clock.</param>
/// <param name="ticket">The ticket the time is logged on.</param>
/// <param name="remainingHours">The hours left on the contract bank, when known.</param>
/// <param name="completed">Called after the entry was saved or queued, to refresh the workbench.</param>
internal sealed partial class HelpdeskTimeLogViewModel(ISender sender, ILocalizationService localization, TimeProvider time, HelpdeskTicketRisk ticket, decimal? remainingHours, Func<Task>? completed) : ObservableObject
{
    private const string NumberFormat = "0.##";
    private const string Good = "Good";
    private const string Warn = "Warn";
    private const string Bad = "Bad";

    [ObservableProperty]
    private string _minutesText = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _isBillable = true;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _previewText = string.Empty;

    [ObservableProperty]
    private string _resultText = string.Empty;

    [ObservableProperty]
    private string _resultState = string.Empty;

    /// <summary>
    /// Gets the number of the ticket.
    /// </summary>
    public string TicketNumber => ticket.TicketNumber;

    /// <summary>
    /// Saves the time entry; it is sent now when the server is reachable and queued otherwise.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task SubmitAsync()
    {
        if (!int.TryParse(MinutesText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var minutes))
        {
            Show(localization[HelpdeskService.MinutesKey], Bad);

            return;
        }

        IsBusy = true;

        try
        {
            HelpdeskTimeLogRequest request = new(
                Guid.NewGuid(),
                ticket.TicketId,
                ticket.TicketNumber,
                minutes,
                TechnicianId: null,
                Description,
                IsBillable,
                time.GetUtcNow().UtcDateTime,
                remainingHours);
            var result = await sender.Send(new LogTicketTimeCommand(request));
            Apply(result);

            if (result.Status is HelpdeskActionStatus.Sent or HelpdeskActionStatus.Queued && completed is not null)
            {
                await completed();
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

    partial void OnMinutesTextChanged(string value) => UpdatePreview();

    partial void OnIsBillableChanged(bool value) => UpdatePreview();

    private void UpdatePreview()
    {
        if (!int.TryParse(MinutesText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var minutes) || minutes is < 1 or > HelpdeskService.MaxMinutes)
        {
            PreviewText = string.Empty;

            return;
        }

        var estimate = HelpdeskTimeBilling.Estimate(minutes, IsBillable, remainingHours);
        PreviewText = Format("Helpdesk_Time_Preview", Describe(estimate));
    }

    private void Apply(HelpdeskActionResult result)
    {
        switch (result.Status)
        {
            case HelpdeskActionStatus.Sent:
                Show(result.TimeOutcome is { } outcome ? Describe(outcome) : localization["Helpdesk_Time_Logged"], Good);
                break;

            case HelpdeskActionStatus.Queued:
                Show(
                    localization["Helpdesk_Queued"] + (result.TimeOutcome is { } estimate ? " " + Format("Helpdesk_Time_Estimate", Describe(estimate)) : string.Empty),
                    Warn);
                break;

            case HelpdeskActionStatus.Conflict:
                Show(Format("Helpdesk_Refused", result.Detail ?? string.Empty), Bad);
                break;

            case HelpdeskActionStatus.Rejected:
                Show(localization[result.ErrorKey ?? HelpdeskService.MinutesKey], Bad);
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

    private string Describe(HelpdeskTimeOutcome outcome)
    {
        var contract = Hours(outcome.ContractHours);
        var billable = Hours(outcome.BillableHours);

        return outcome.BillingStatus switch
        {
            HelpdeskTimeOutcome.ContractDeducted => Format("Helpdesk_Time_ContractDeducted", contract),
            HelpdeskTimeOutcome.PartiallyDeducted => Format("Helpdesk_Time_Partial", contract, billable),
            HelpdeskTimeOutcome.BillablePending => Format("Helpdesk_Time_Billable", billable) + Charge(outcome),
            HelpdeskTimeOutcome.NonBillable => localization["Helpdesk_Time_NonBillable"],
            _ => localization["Helpdesk_Time_Logged"],
        };
    }

    private string Charge(HelpdeskTimeOutcome outcome) =>
        outcome.TotalCharge > 0m ? " " + Format("Helpdesk_Time_Charge", outcome.TotalCharge.ToString("N2", localization.CurrentCulture)) : string.Empty;

    private string Hours(decimal value) => value.ToString(NumberFormat, localization.CurrentCulture);

    private string Format(string key, params string[] arguments) => string.Format(localization.CurrentCulture, localization[key], arguments);
}
