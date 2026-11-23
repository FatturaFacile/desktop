// <copyright file="SupplierRebatesViewModel.Runs.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.SupplierRebates;
using FatturaFacile.Application.SupplierRebates;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Accrual runs (with the journal entry preview and the invoices behind each run) and contributions of the rebate cockpit.
/// </summary>
internal sealed partial class SupplierRebatesViewModel
{
    [ObservableProperty]
    private DateTime? _periodEnd;

    [ObservableProperty]
    private string _reverseReason = string.Empty;

    [ObservableProperty]
    private RebateRunRow? _selectedRun;

    [ObservableProperty]
    private bool _hasJournal;

    [ObservableProperty]
    private string _journalDebitText = string.Empty;

    [ObservableProperty]
    private string _journalCreditText = string.Empty;

    [ObservableProperty]
    private string _journalStatusText = string.Empty;

    [ObservableProperty]
    private RebateChoice? _selectedContributionKind;

    [ObservableProperty]
    private string _contributionDescription = string.Empty;

    [ObservableProperty]
    private string _contributionAmountText = string.Empty;

    [ObservableProperty]
    private DateTime? _contributionDue;

    [ObservableProperty]
    private RebateContributionRow? _selectedContribution;

    /// <summary>
    /// Gets the kinds of contribution of the picker.
    /// </summary>
    public ObservableCollection<RebateChoice> ContributionKinds { get; } = [];

    /// <summary>
    /// Runs the periodic accrual of the selected agreement up to the period end (today when it is empty). The server books the
    /// journal entry and is idempotent: a period already covered returns the existing run.
    /// </summary>
    /// <returns>A task representing the accrual.</returns>
    [RelayCommand(CanExecute = nameof(CanRunAccrual))]
    public async Task RunAccrualAsync()
    {
        if (SelectedAgreement is not { } row)
        {
            return;
        }

        _ = await RunActionAsync(() => _sender.Send(new RunRebateAccrualCommand(row.Id, ToDate(PeriodEnd))));
    }

    /// <summary>
    /// Reverses the selected posted run with the typed reason.
    /// </summary>
    /// <returns>A task representing the reversal.</returns>
    [RelayCommand(CanExecute = nameof(CanReverseRun))]
    public async Task ReverseRunAsync()
    {
        if (SelectedAgreement is not { } row || SelectedRun is not { } run)
        {
            return;
        }

        var result = await RunActionAsync(() => _sender.Send(new ReverseRebateAccrualRunCommand(row.Id, run.Id, ReverseReason.Trim())));
        if (result is { IsSuccess: true })
        {
            ReverseReason = string.Empty;
        }
    }

    /// <summary>
    /// Plans the typed contribution on the selected agreement.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand(CanExecute = nameof(CanAddContribution))]
    public async Task AddContributionAsync()
    {
        if (SelectedAgreement is not { } row || SelectedContributionKind is not { } kind)
        {
            return;
        }

        if (RebateFormat.ParseAmount(ContributionAmountText, Culture) is not { } amount || ContributionDue is not { } due)
        {
            StatusMessage = _localization[ContributionDue is null ? SupplierRebatesErrors.Required : SupplierRebatesErrors.Amount];

            return;
        }

        RebateContributionInput input = new(kind.Id, ContributionDescription.Trim(), amount, DateOnly.FromDateTime(due));
        var result = await RunActionAsync(() => _sender.Send(new SaveRebateContributionCommand(row.Id, ContributionId: null, input)));
        if (result is { IsSuccess: true })
        {
            ContributionDescription = string.Empty;
            ContributionAmountText = string.Empty;
            ContributionDue = null;
        }
    }

    /// <summary>
    /// Removes the selected planned contribution.
    /// </summary>
    /// <returns>A task representing the removal.</returns>
    [RelayCommand(CanExecute = nameof(CanRemoveContribution))]
    public async Task RemoveContributionAsync()
    {
        if (SelectedAgreement is not { } row || SelectedContribution is not { } contribution)
        {
            return;
        }

        _ = await RunActionAsync(() => _sender.Send(new DeleteRebateContributionCommand(row.Id, contribution.Id)));
    }

    private static DateOnly? ToDate(DateTime? value) => value is { } date ? DateOnly.FromDateTime(date) : null;

    partial void OnSelectedRunChanged(RebateRunRow? value)
    {
        RunLines.Clear();
        if (value is null || SelectedAgreement?.Agreement is not { } agreement)
        {
            ShowJournal(preview: null);
            NotifyRunCommandStates();

            return;
        }

        foreach (var line in value.Run.Lines)
        {
            RunLines.Add(new RebateRunLineRow(
                line.InvoiceNumber,
                string.Equals(line.DocumentType, RebateCodes.CreditNoteDocumentType, StringComparison.OrdinalIgnoreCase) ? _localization["SupplierRebates_LineCreditNote"] : _localization["SupplierRebates_LineInvoice"],
                RebateFormat.Date(line.InvoiceDate, Culture),
                RebateFormat.Money(line.NetAmount, agreement.Currency, Culture)));
        }

        ShowJournal(value.Run.JournalPreview(agreement));
        NotifyRunCommandStates();
    }

    partial void OnReverseReasonChanged(string value) => NotifyRunCommandStates();

    partial void OnSelectedContributionChanged(RebateContributionRow? value) => NotifyRunCommandStates();

    private bool CanRunAccrual() => !IsBusy && SelectedAgreement?.Agreement.CanAccrue == true;

    private bool CanReverseRun() => !IsBusy && SelectedAgreement?.Agreement.CanAccrue == true && SelectedRun?.Run.IsPosted == true && !string.IsNullOrWhiteSpace(ReverseReason);

    private bool CanAddContribution() => !IsBusy && SelectedAgreement is { Agreement: { IsDraft: true } or { CanAccrue: true } };

    private bool CanRemoveContribution() => !IsBusy && SelectedContribution is { Status: "planned" };

    private void ShowJournal(RebateJournalPreview? preview)
    {
        HasJournal = preview is not null;
        if (preview is null || SelectedAgreement?.Agreement is not { } agreement)
        {
            JournalDebitText = string.Empty;
            JournalCreditText = string.Empty;
            JournalStatusText = string.Empty;

            return;
        }

        var signed = preview.DebitsAccruedIncome ? preview.Amount : -preview.Amount;
        var amount = RebateFormat.Money(signed, agreement.Currency, Culture);
        JournalDebitText = preview.IsPosted ? _localization["SupplierRebates_DebitAccruedIncome"] + " " + amount : string.Empty;
        JournalCreditText = preview.IsPosted ? _localization["SupplierRebates_CreditSupplierIncome"] + " " + amount : string.Empty;
        JournalStatusText = preview.IsPosted ? string.Empty : _localization["SupplierRebates_NotPosted"];
    }

    private void NotifyRunCommandStates()
    {
        RunAccrualCommand.NotifyCanExecuteChanged();
        ReverseRunCommand.NotifyCanExecuteChanged();
        AddContributionCommand.NotifyCanExecuteChanged();
        RemoveContributionCommand.NotifyCanExecuteChanged();
    }
}
