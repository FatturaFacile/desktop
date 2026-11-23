// <copyright file="SupplierRebatesViewModel.Settlement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.SupplierRebates;
using FatturaFacile.Application.SupplierRebates;

namespace FatturaFacile.Wpf.ViewModels;

/// <content>
/// Settlement proposals of the rebate cockpit: the proposal, its analytical breakdown, the approval workflow and the selection of
/// the open payable of the set-off.
/// </content>
internal sealed partial class SupplierRebatesViewModel
{
    private List<RebatePayableInvoice> _payableSource = [];

    [ObservableProperty]
    private RebateChoice? _selectedMode;

    [ObservableProperty]
    private bool _isSetOff;

    [ObservableProperty]
    private string _taxRateText = string.Empty;

    [ObservableProperty]
    private string _proposalNotes = string.Empty;

    [ObservableProperty]
    private RebateProposalRow? _selectedProposal;

    [ObservableProperty]
    private bool _hasProposal;

    [ObservableProperty]
    private string _proposalSummaryText = string.Empty;

    [ObservableProperty]
    private RebatePayableRow? _selectedPayable;

    [ObservableProperty]
    private string _payablesMessage = string.Empty;

    [ObservableProperty]
    private RebateChoice? _selectedPayablesAccount;

    [ObservableProperty]
    private RebateChoice? _selectedVatAccount;

    /// <summary>
    /// Gets the execution modes of the picker.
    /// </summary>
    public ObservableCollection<RebateChoice> ExecutionModes { get; } = [];

    /// <summary>
    /// Proposes the settlement of the open accruals of the selected agreement with the chosen execution mode.
    /// </summary>
    /// <returns>A task representing the proposal.</returns>
    [RelayCommand(CanExecute = nameof(CanPropose))]
    public async Task ProposeSettlementAsync()
    {
        if (SelectedAgreement is not { } row || SelectedMode?.Id is not { Length: > 0 } code || RebateWire.ParseMode(code) is not { } mode)
        {
            return;
        }

        RebateProposalInput input = new(mode, RebateFormat.ParseAmount(TaxRateText, Culture), mode == RebateExecutionMode.ApSetOff ? SelectedPayable?.Id : null, string.IsNullOrWhiteSpace(ProposalNotes) ? null : ProposalNotes.Trim());
        var result = await RunActionAsync(() => _sender.Send(new ProposeRebateSettlementCommand(row.Id, input)));
        if (result is { IsSuccess: true })
        {
            ProposalNotes = string.Empty;
        }
    }

    /// <summary>
    /// Sends the selected draft proposal in approval.
    /// </summary>
    /// <returns>A task representing the step.</returns>
    [RelayCommand(CanExecute = nameof(CanSubmit))]
    public Task SubmitProposalAsync() => AdvanceAsync(SettlementAction.Submit);

    /// <summary>
    /// Approves the selected proposal that waits for the approval.
    /// </summary>
    /// <returns>A task representing the step.</returns>
    [RelayCommand(CanExecute = nameof(CanApprove))]
    public Task ApproveProposalAsync() => AdvanceAsync(SettlementAction.Approve);

    /// <summary>
    /// Asks the confirmation to execute the selected approved proposal.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteProposal))]
    public void RequestExecuteProposal() => Ask(RebatePendingAction.ExecuteProposal, "SupplierRebates_ConfirmExecute");

    /// <summary>
    /// Asks the confirmation to cancel the selected proposal.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancelProposal))]
    public void RequestCancelProposal() => Ask(RebatePendingAction.CancelProposal, "SupplierRebates_ConfirmCancel");

    /// <summary>
    /// Reads, live from the server, the open invoices of the supplier the set-off can be made against.
    /// </summary>
    /// <returns>A task representing the read.</returns>
    [RelayCommand(CanExecute = nameof(CanLoadPayables))]
    public async Task LoadPayablesAsync()
    {
        if (SelectedAgreement is not { } row)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _sender.Send(new GetRebatePayablesQuery(row.Agreement.ContactId));
            IsOffline = result.Status == SupplierRebatesStatus.Offline;
            _payableSource = [.. result.Invoices];
            ShowPayables();
            PayablesMessage = result.Status switch
            {
                SupplierRebatesStatus.Synced => _payableSource.Count == 0 ? _localization["SupplierRebates_NoOpenInvoices"] : string.Empty,
                SupplierRebatesStatus.Offline => _localization["SupplierRebates_OfflineActionBlocked"],
                SupplierRebatesStatus.Unauthorized => _localization["SupplierRebates_SignInRequired"],
                SupplierRebatesStatus.Forbidden => _localization["SupplierRebates_Forbidden"],
                SupplierRebatesStatus.Failed => _localization["SupplierRebates_LoadError"],
                SupplierRebatesStatus.Rejected => _localization["SupplierRebates_LoadError"],
                _ => _localization["SupplierRebates_LoadError"],
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            PayablesMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string? NonEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    partial void OnSelectedModeChanged(RebateChoice? value)
    {
        IsSetOff = RebateWire.ParseMode(value?.Id) == RebateExecutionMode.ApSetOff;
        NotifySettlementCommandStates();
    }

    partial void OnTaxRateTextChanged(string value) => ShowPayables();

    partial void OnSelectedPayableChanged(RebatePayableRow? value) => NotifySettlementCommandStates();

    partial void OnSelectedProposalChanged(RebateProposalRow? value)
    {
        ProposalLines.Clear();
        HasProposal = value is not null;
        ProposalSummaryText = string.Empty;
        NotifySettlementCommandStates();

        if (value is null || SelectedAgreement?.Agreement is not { } agreement)
        {
            return;
        }

        foreach (var line in value.Proposal.Lines)
        {
            ProposalLines.Add(new RebateProposalLineRow(LineTypeText(line.LineType), line.Reference, RebateFormat.Date(line.ReferenceDate, Culture), RebateFormat.Money(line.NetAmount, agreement.Currency, Culture), RebateFormat.Money(line.RebateAmount, agreement.Currency, Culture)));
        }

        ProposalSummaryText = value.Mode + " | " + value.Status + " | " + _localization["SupplierRebates_Net"] + " " + value.Net + " | "
            + _localization["SupplierRebates_Tax"] + " " + value.Tax + " | " + _localization["SupplierRebates_Gross"] + " " + value.Gross;
    }

    private bool CanPropose() => !IsBusy && SelectedAgreement?.Agreement.CanAccrue == true && HasProposalInputs();

    private bool HasProposalInputs() => SelectedMode is not null && (!IsSetOff || SelectedPayable?.Covers == true);

    private bool CanSubmit() => !IsBusy && Allows(SettlementAction.Submit);

    private bool CanApprove() => !IsBusy && Allows(SettlementAction.Approve);

    private bool CanExecuteProposal() => !IsBusy && Allows(SettlementAction.Execute);

    private bool CanCancelProposal() => !IsBusy && Allows(SettlementAction.Cancel);

    private bool CanLoadPayables() => !IsBusy && SelectedAgreement is not null && IsSetOff;

    private bool Allows(SettlementAction action) => SelectedProposal is { } row && SettlementWorkflow.CanApply(row.Proposal.Status, action);

    private string LineTypeText(string lineType) => lineType switch
    {
        "invoice" => _localization["SupplierRebates_LineInvoice"],
        "credit_note" => _localization["SupplierRebates_LineCreditNote"],
        "contribution" => _localization["SupplierRebates_LineContribution"],
        "bonus" => _localization["SupplierRebates_LineBonus"],
        _ => lineType,
    };

    private void ShowPayables()
    {
        var keep = SelectedPayable?.Id;
        Payables.Clear();
        SelectedPayable = null;
        if (SelectedAgreement?.Agreement is not { } agreement)
        {
            return;
        }

        var net = agreement.OpenRebate ?? 0m;
        var gross = RebateSettlementCalculator.Gross(net, RebateFormat.ParseAmount(TaxRateText, Culture) ?? 0m);

        foreach (var invoice in _payableSource)
        {
            Payables.Add(new RebatePayableRow(
                invoice.Id,
                invoice.InvoiceNumber,
                RebateFormat.Date(invoice.InvoiceDate, Culture),
                RebateFormat.Money(invoice.OpenBalance, invoice.Currency, Culture),
                RebateSettlementCalculator.CanSetOff(invoice, gross),
                invoice));
        }

        SelectedPayable = Payables.FirstOrDefault(payable => string.Equals(payable.Id, keep, StringComparison.Ordinal));
    }

    private async Task AdvanceAsync(SettlementAction action)
    {
        if (SelectedAgreement is not { } row || SelectedProposal is not { } proposalRow)
        {
            return;
        }

        var proposal = proposalRow.Proposal;
        var setOff = action == SettlementAction.Execute && proposal.ExecutionMode == RebateExecutionMode.ApSetOff;
        RebateSettlementStep details = new(
            proposal.Version,
            ExecutedOn: null,
            setOff ? NonEmpty(SelectedPayablesAccount?.Id) : null,
            setOff && proposal.TaxAmount > 0m ? NonEmpty(SelectedVatAccount?.Id) : null);

        _ = await RunActionAsync(() => _sender.Send(new AdvanceRebateSettlementCommand(row.Id, proposal.Id, proposal.Status, action, details)));
    }

    private void NotifySettlementCommandStates()
    {
        ProposeSettlementCommand.NotifyCanExecuteChanged();
        SubmitProposalCommand.NotifyCanExecuteChanged();
        ApproveProposalCommand.NotifyCanExecuteChanged();
        RequestExecuteProposalCommand.NotifyCanExecuteChanged();
        RequestCancelProposalCommand.NotifyCanExecuteChanged();
        LoadPayablesCommand.NotifyCanExecuteChanged();
    }
}
