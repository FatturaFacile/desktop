// <copyright file="SupplierRebatesViewModel.Editor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.Accounting.Queries;
using FatturaFacile.Application.Features.SupplierRebates;
using FatturaFacile.Application.SupplierRebates;

namespace FatturaFacile.Wpf.ViewModels;

/// <content>
/// The agreement form of the rebate cockpit: programme setup (tiered volume rebate, fixed target bonus, marketing contributions),
/// the scale of tiers, the blanket agreement the spend is limited to and the ledger accounts of the accrual.
/// </content>
internal sealed partial class SupplierRebatesViewModel
{
    private const string DefaultCurrency = "EUR";

    private string? _editingAgreementId;
    private int? _editingVersion;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editorTitle = string.Empty;

    [ObservableProperty]
    private string _formCode = string.Empty;

    [ObservableProperty]
    private string _formName = string.Empty;

    [ObservableProperty]
    private RebateChoice? _formSupplier;

    [ObservableProperty]
    private RebateChoice? _formRebateType;

    [ObservableProperty]
    private bool _formIsTiered = true;

    [ObservableProperty]
    private bool _formIsFixedBonus;

    [ObservableProperty]
    private RebateChoice? _formTierMode;

    [ObservableProperty]
    private RebateChoice? _formFrequency;

    [ObservableProperty]
    private RebateChoice? _formBlanket;

    [ObservableProperty]
    private RebateChoice? _formAccrualAccount;

    [ObservableProperty]
    private RebateChoice? _formIncomeAccount;

    [ObservableProperty]
    private DateTime _formStartsOn = DateTime.Today;

    [ObservableProperty]
    private DateTime _formEndsOn = DateTime.Today.AddYears(1).AddDays(-1);

    [ObservableProperty]
    private string _formCurrency = DefaultCurrency;

    [ObservableProperty]
    private string _formBonusTarget = string.Empty;

    [ObservableProperty]
    private string _formBonusAmount = string.Empty;

    [ObservableProperty]
    private string _formNotes = string.Empty;

    /// <summary>
    /// Gets the suppliers of the picker.
    /// </summary>
    public ObservableCollection<RebateChoice> Suppliers { get; } = [];

    /// <summary>
    /// Gets the blanket agreements of the picker, led by the "all purchases" entry.
    /// </summary>
    public ObservableCollection<RebateChoice> BlanketChoices { get; } = [];

    /// <summary>
    /// Gets the ledger accounts of the picker, led by an empty entry.
    /// </summary>
    public ObservableCollection<RebateChoice> Accounts { get; } = [];

    /// <summary>
    /// Gets the rebate types of the picker.
    /// </summary>
    public ObservableCollection<RebateChoice> RebateTypes { get; } = [];

    /// <summary>
    /// Gets the scale modes of the picker.
    /// </summary>
    public ObservableCollection<RebateChoice> TierModes { get; } = [];

    /// <summary>
    /// Gets the accrual frequencies of the picker.
    /// </summary>
    public ObservableCollection<RebateChoice> Frequencies { get; } = [];

    /// <summary>
    /// Gets the tiers of the scale being edited.
    /// </summary>
    public ObservableCollection<RebateTierEdit> FormTiers { get; } = [];

    /// <summary>
    /// Opens the form for a new agreement.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenEditor))]
    public void NewAgreement()
    {
        _editingAgreementId = null;
        _editingVersion = null;
        EditorTitle = _localization["SupplierRebates_NewAgreement"];
        FormCode = string.Empty;
        FormName = string.Empty;
        FormSupplier = Suppliers.FirstOrDefault();
        FormRebateType = RebateTypes.FirstOrDefault();
        FormTierMode = TierModes.FirstOrDefault();
        FormFrequency = Frequencies.FirstOrDefault();
        FormBlanket = BlanketChoices.FirstOrDefault();
        FormAccrualAccount = Accounts.FirstOrDefault();
        FormIncomeAccount = Accounts.FirstOrDefault();
        FormStartsOn = DateTime.Today;
        FormEndsOn = DateTime.Today.AddYears(1).AddDays(-1);
        FormCurrency = Agreements.FirstOrDefault()?.Agreement.Currency ?? DefaultCurrency;
        FormBonusTarget = string.Empty;
        FormBonusAmount = string.Empty;
        FormNotes = string.Empty;
        FormTiers.Clear();
        FormTiers.Add(new RebateTierEdit());
        IsEditing = true;
        NotifyEditorCommandStates();
    }

    /// <summary>
    /// Opens the form for the selected agreement.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanEditAgreement))]
    public void EditAgreement()
    {
        if (SelectedAgreement?.Agreement is not { } agreement)
        {
            return;
        }

        _editingAgreementId = agreement.Id;
        _editingVersion = agreement.Version;
        EditorTitle = _localization["SupplierRebates_EditAgreement"];
        FormCode = agreement.Code;
        FormName = agreement.Name;
        FormSupplier = Suppliers.FirstOrDefault(choice => string.Equals(choice.Id, agreement.ContactId, StringComparison.OrdinalIgnoreCase)) ?? new RebateChoice(agreement.ContactId, agreement.ContactName ?? agreement.ContactId);
        FormRebateType = RebateTypes.FirstOrDefault(choice => string.Equals(choice.Id, agreement.RebateType, StringComparison.Ordinal));
        FormTierMode = TierModes.FirstOrDefault(choice => string.Equals(choice.Id, RebateWire.ToWire(agreement.TierMode), StringComparison.Ordinal));
        FormFrequency = Frequencies.FirstOrDefault(choice => string.Equals(choice.Id, agreement.AccrualFrequency, StringComparison.Ordinal));
        FormBlanket = BlanketChoices.FirstOrDefault(choice => string.Equals(choice.Id, agreement.SupplierContractId ?? string.Empty, StringComparison.OrdinalIgnoreCase)) ?? BlanketChoices.FirstOrDefault();
        FormAccrualAccount = Accounts.FirstOrDefault(choice => string.Equals(choice.Id, agreement.AccrualAccountId ?? string.Empty, StringComparison.OrdinalIgnoreCase)) ?? Accounts.FirstOrDefault();
        FormIncomeAccount = Accounts.FirstOrDefault(choice => string.Equals(choice.Id, agreement.IncomeAccountId ?? string.Empty, StringComparison.OrdinalIgnoreCase)) ?? Accounts.FirstOrDefault();
        FormStartsOn = agreement.StartsOn.ToDateTime(TimeOnly.MinValue);
        FormEndsOn = agreement.EndsOn.ToDateTime(TimeOnly.MinValue);
        FormCurrency = agreement.Currency;
        FormBonusTarget = agreement.FixedBonusTarget?.ToString("0.##", Culture) ?? string.Empty;
        FormBonusAmount = agreement.FixedBonusAmount?.ToString("0.##", Culture) ?? string.Empty;
        FormNotes = agreement.Notes ?? string.Empty;
        FormTiers.Clear();
        foreach (var tier in agreement.Tiers)
        {
            FormTiers.Add(new RebateTierEdit
            {
                Threshold = tier.ThresholdAmount.ToString("0.##", Culture),
                Rate = tier.RatePercent.ToString("0.##", Culture),
                Flat = tier.FlatAmount?.ToString("0.##", Culture) ?? string.Empty,
            });
        }

        IsEditing = true;
        NotifyEditorCommandStates();
    }

    /// <summary>
    /// Closes the form without saving.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsEditing))]
    public void CancelEdit()
    {
        IsEditing = false;
        NotifyEditorCommandStates();
    }

    /// <summary>
    /// Adds an empty tier to the scale being edited.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsEditing))]
    public void AddTier() => FormTiers.Add(new RebateTierEdit());

    /// <summary>
    /// Removes a tier from the scale being edited.
    /// </summary>
    /// <param name="tier">The tier to remove.</param>
    [RelayCommand(CanExecute = nameof(IsEditing))]
    public void RemoveTier(RebateTierEdit? tier)
    {
        if (tier is not null)
        {
            _ = FormTiers.Remove(tier);
        }
    }

    /// <summary>
    /// Saves the agreement on the server: creates it, or updates the one being edited.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand(CanExecute = nameof(CanSaveAgreement))]
    public async Task SaveAgreementAsync()
    {
        if (BuildInput() is not { } input)
        {
            StatusMessage = _localization[SupplierRebatesErrors.Amount];

            return;
        }

        var id = _editingAgreementId;
        var result = await RunActionAsync(() => _sender.Send(new SaveRebateAgreementCommand(id, input)));
        if (result is not { IsSuccess: true })
        {
            return;
        }

        IsEditing = false;
        NotifyEditorCommandStates();
        SelectSaved(id, result.Data);
    }

    partial void OnIsEditingChanged(bool value) => NotifyEditorCommandStates();

    partial void OnFormRebateTypeChanged(RebateChoice? value)
    {
        FormIsTiered = value?.Id == RebateCodes.TypeTieredVolume;
        FormIsFixedBonus = value?.Id == RebateCodes.TypeFixedBonus;
    }

    private bool CanOpenEditor() => !IsBusy && !IsEditing;

    private bool CanEditAgreement() => !IsBusy && !IsEditing && SelectedAgreement is not null;

    private bool CanSaveAgreement() => !IsBusy && IsEditing;

    private void InitializePickers()
    {
        RebateTypes.Add(new RebateChoice(RebateCodes.TypeTieredVolume, _localization["SupplierRebates_TypeTieredVolume"]));
        RebateTypes.Add(new RebateChoice(RebateCodes.TypeFixedBonus, _localization["SupplierRebates_TypeFixedBonus"]));
        RebateTypes.Add(new RebateChoice(RebateCodes.TypeMarketingContribution, _localization["SupplierRebates_TypeMarketingContribution"]));
        TierModes.Add(new RebateChoice(RebateCodes.ModeRetroactive, _localization["SupplierRebates_ModeRetroactive"]));
        TierModes.Add(new RebateChoice(RebateCodes.ModeIncremental, _localization["SupplierRebates_ModeIncremental"]));
        Frequencies.Add(new RebateChoice(RebateCodes.FrequencyMonthly, _localization["SupplierRebates_FreqMonthly"]));
        Frequencies.Add(new RebateChoice(RebateCodes.FrequencyQuarterly, _localization["SupplierRebates_FreqQuarterly"]));
        Frequencies.Add(new RebateChoice(RebateCodes.FrequencyCustom, _localization["SupplierRebates_FreqCustom"]));
        ContributionKinds.Add(new RebateChoice(RebateCodes.KindMarketing, _localization["SupplierRebates_KindMarketing"]));
        ContributionKinds.Add(new RebateChoice(RebateCodes.KindCoMarketing, _localization["SupplierRebates_KindCoMarketing"]));
        ContributionKinds.Add(new RebateChoice(RebateCodes.KindListing, _localization["SupplierRebates_KindListing"]));
        ExecutionModes.Add(new RebateChoice(RebateWire.ToWire(RebateExecutionMode.CreditNoteRequest), _localization["SupplierRebates_ExecCreditNoteRequest"]));
        ExecutionModes.Add(new RebateChoice(RebateWire.ToWire(RebateExecutionMode.ChargeInvoice), _localization["SupplierRebates_ExecChargeInvoice"]));
        ExecutionModes.Add(new RebateChoice(RebateWire.ToWire(RebateExecutionMode.ApSetOff), _localization["SupplierRebates_ExecApSetOff"]));

        SelectedContributionKind = ContributionKinds.FirstOrDefault();
        SelectedMode = ExecutionModes.FirstOrDefault();
        FormRebateType = RebateTypes.FirstOrDefault();
        FormTierMode = TierModes.FirstOrDefault();
        FormFrequency = Frequencies.FirstOrDefault();
    }

    private async Task LoadAccountsAsync()
    {
        try
        {
            var accounts = await _sender.Send(new GetChartOfAccountsQuery());

            Accounts.Clear();
            Accounts.Add(new RebateChoice(string.Empty, RebateFormat.NoValue));
            foreach (var account in accounts.Where(a => a.IsActive))
            {
                Accounts.Add(new RebateChoice(account.Id.ToString(), account.DisplayText));
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Without access to the chart of accounts the pickers hold only the empty entry: the agreement then books nothing.
            Accounts.Clear();
            Accounts.Add(new RebateChoice(string.Empty, RebateFormat.NoValue));
        }
    }

    private RebateAgreementInput? BuildInput()
    {
        List<RebateTier> tiers = [];
        foreach (var edit in FormTiers.Where(t => !string.IsNullOrWhiteSpace(t.Threshold) || !string.IsNullOrWhiteSpace(t.Rate) || !string.IsNullOrWhiteSpace(t.Flat)))
        {
            var threshold = RebateFormat.ParseAmount(edit.Threshold, Culture);
            var rate = RebateFormat.ParseAmount(edit.Rate, Culture);
            var flat = RebateFormat.ParseAmount(edit.Flat, Culture);
            if (threshold is null || rate is null || (flat is null && !string.IsNullOrWhiteSpace(edit.Flat)))
            {
                return null;
            }

            tiers.Add(new RebateTier(threshold.Value, rate.Value, flat));
        }

        var bonusTarget = RebateFormat.ParseAmount(FormBonusTarget, Culture);
        var bonusAmount = RebateFormat.ParseAmount(FormBonusAmount, Culture);
        if ((bonusTarget is null && !string.IsNullOrWhiteSpace(FormBonusTarget)) || (bonusAmount is null && !string.IsNullOrWhiteSpace(FormBonusAmount)))
        {
            return null;
        }

        var fixedBonus = string.Equals(FormRebateType?.Id, RebateCodes.TypeFixedBonus, StringComparison.Ordinal);

        return new RebateAgreementInput(
            FormCode.Trim(),
            FormName.Trim(),
            FormSupplier?.Id ?? string.Empty,
            RebateWire.ParseTierMode(FormTierMode?.Id),
            DateOnly.FromDateTime(FormStartsOn),
            DateOnly.FromDateTime(FormEndsOn),
            FormCurrency.Trim().ToUpperInvariant(),
            FormRebateType?.Id ?? RebateCodes.TypeTieredVolume,
            NonEmpty(FormBlanket?.Id),
            fixedBonus ? bonusTarget : null,
            fixedBonus ? bonusAmount : null,
            FormFrequency?.Id ?? RebateCodes.FrequencyMonthly,
            NonEmpty(FormAccrualAccount?.Id),
            NonEmpty(FormIncomeAccount?.Id),
            string.IsNullOrWhiteSpace(FormNotes) ? null : FormNotes.Trim(),
            tiers,
            _editingVersion);
    }

    private void SelectSaved(string? editedId, string? data)
    {
        var id = editedId;
        if (id is null && !string.IsNullOrWhiteSpace(data))
        {
            id = SupplierRebateParser.ParseAgreement(data).Id;
        }

        if (Agreements.FirstOrDefault(row => string.Equals(row.Id, id, StringComparison.Ordinal)) is { } saved)
        {
            SelectedAgreement = saved;
        }
    }

    private void NotifyEditorCommandStates()
    {
        NewAgreementCommand.NotifyCanExecuteChanged();
        EditAgreementCommand.NotifyCanExecuteChanged();
        SaveAgreementCommand.NotifyCanExecuteChanged();
        CancelEditCommand.NotifyCanExecuteChanged();
        AddTierCommand.NotifyCanExecuteChanged();
        RemoveTierCommand.NotifyCanExecuteChanged();
    }
}
