// <copyright file="SupplierRebatesViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Contacts.Queries;
using FatturaFacile.Application.Features.SupplierRebates;
using FatturaFacile.Application.SupplierRebates;
using FluentValidation;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the supplier rebate cockpit (Mod 049): the agreements, the live tier progress with the distance to the next tier, the
/// accrual runs with the journal preview, the settlement proposals with the analytical breakdown and the workflow, and the open
/// payable selection of the set-off. It reads the local cache first, so it shows the last synced figures offline; the actions
/// (create, activate, accrue, propose, approve, execute) are validated and posted by the server, so offline they change nothing and
/// say so.
/// </summary>
internal sealed partial class SupplierRebatesViewModel : ObservableObject
{
    private const string DateTimeFormat = "g";
    private const decimal PercentScale = 100m;

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;

    private bool _suppressSelection;
    private RebatePendingAction _pending;

    [ObservableProperty]
    private RebateAgreementRow? _selectedAgreement;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _hasDetail;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _qualifyingText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _earnedText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _accruedText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _settledText = RebateFormat.NoValue;

    [ObservableProperty]
    private bool _hasTier;

    [ObservableProperty]
    private bool _hasNextStep;

    [ObservableProperty]
    private string _currentTierText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _nextTierText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _distanceLabel = string.Empty;

    [ObservableProperty]
    private string _distanceText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _projectedAtNextText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _projectedGainText = RebateFormat.NoValue;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _progressText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _contributionsText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _simulatedVolumeText = string.Empty;

    [ObservableProperty]
    private string _simulatedRebateText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _pendingConfirmText = string.Empty;

    [ObservableProperty]
    private bool _hasPendingConfirmation;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierRebatesViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public SupplierRebatesViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _localization = localization;
        LastSyncedText = _localization["SupplierRebates_NeverSynced"];
        InitializePickers();
    }

    /// <summary>
    /// Gets the agreements of the list.
    /// </summary>
    public ObservableCollection<RebateAgreementRow> Agreements { get; } = [];

    /// <summary>
    /// Gets the scale of the selected agreement.
    /// </summary>
    public ObservableCollection<RebateScaleRow> Scale { get; } = [];

    /// <summary>
    /// Gets the contributions of the selected agreement.
    /// </summary>
    public ObservableCollection<RebateContributionRow> Contributions { get; } = [];

    /// <summary>
    /// Gets the accrual runs of the selected agreement, latest first.
    /// </summary>
    public ObservableCollection<RebateRunRow> Runs { get; } = [];

    /// <summary>
    /// Gets the invoices that contributed to the selected run.
    /// </summary>
    public ObservableCollection<RebateRunLineRow> RunLines { get; } = [];

    /// <summary>
    /// Gets the settlement proposals of the selected agreement, latest first.
    /// </summary>
    public ObservableCollection<RebateProposalRow> Proposals { get; } = [];

    /// <summary>
    /// Gets the analytical breakdown of the selected proposal.
    /// </summary>
    public ObservableCollection<RebateProposalLineRow> ProposalLines { get; } = [];

    /// <summary>
    /// Gets the open payables of the supplier the set-off can be made against.
    /// </summary>
    public ObservableCollection<RebatePayableRow> Payables { get; } = [];

    /// <summary>
    /// Gets the display culture.
    /// </summary>
    private CultureInfo Culture => _localization.CurrentCulture;

    /// <summary>
    /// Loads the pickers and the agreements from the cache, pulling from the server when the cache is empty.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            await LoadPickersAsync();

            if (!await ShowAgreementsAsync())
            {
                await PullAsync(RebateSyncScope.Agreements, id: null);
                _ = await ShowAgreementsAsync();
            }

            await ShowDetailAsync(syncIfMissing: true);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Pulls fresh data from the server and shows it; offline, the cached figures stay on screen.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand(CanExecute = nameof(CanSync))]
    public async Task SyncAsync()
    {
        IsBusy = true;

        try
        {
            await PullAsync(RebateSyncScope.Agreements, id: null);
            _ = await ShowAgreementsAsync();

            if (SelectedAgreement is { } selected)
            {
                await PullAsync(RebateSyncScope.Agreement, selected.Id);
            }

            await ShowDetailAsync(syncIfMissing: false);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Activates the selected draft agreement.
    /// </summary>
    /// <returns>A task representing the action.</returns>
    [RelayCommand(CanExecute = nameof(CanActivateAgreement))]
    public Task ActivateAgreementAsync() => ChangeAgreementAsync(RebateAgreementAction.Activate);

    /// <summary>
    /// Closes the selected active agreement.
    /// </summary>
    /// <returns>A task representing the action.</returns>
    [RelayCommand(CanExecute = nameof(CanCloseAgreement))]
    public Task CloseAgreementAsync() => ChangeAgreementAsync(RebateAgreementAction.Close);

    /// <summary>
    /// Asks the confirmation to delete the selected draft agreement.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteAgreement))]
    public void RequestDeleteAgreement() => Ask(RebatePendingAction.DeleteAgreement, "SupplierRebates_ConfirmDelete");

    /// <summary>
    /// Runs the confirmed action.
    /// </summary>
    /// <returns>A task representing the action.</returns>
    [RelayCommand(CanExecute = nameof(HasPendingConfirmation))]
    public async Task ConfirmPendingAsync()
    {
        var pending = _pending;
        Dismiss();

        switch (pending)
        {
            case RebatePendingAction.ExecuteProposal:
                await AdvanceAsync(SettlementAction.Execute);
                break;
            case RebatePendingAction.CancelProposal:
                await AdvanceAsync(SettlementAction.Cancel);
                break;
            case RebatePendingAction.DeleteAgreement:
                await ChangeAgreementAsync(RebateAgreementAction.Delete);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Drops the action that waits for a confirmation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasPendingConfirmation))]
    public void DismissPending() => Dismiss();

    private static string Label(string? company, string? first, string? last)
    {
        if (!string.IsNullOrWhiteSpace(company))
        {
            return company;
        }

        var person = ((first ?? string.Empty) + " " + (last ?? string.Empty)).Trim();

        return person.Length > 0 ? person : RebateFormat.NoValue;
    }

    private static List<RebateScaleBand> BandsOf(IReadOnlyList<RebateTier> tiers)
    {
        var sorted = tiers.OrderBy(tier => tier.ThresholdAmount).ToList();

        return [.. sorted.Select((tier, index) => new RebateScaleBand(tier.ThresholdAmount, index + 1 < sorted.Count ? sorted[index + 1].ThresholdAmount : (decimal?)null, tier.RatePercent, tier.FlatAmount))];
    }

    partial void OnSelectedAgreementChanged(RebateAgreementRow? value)
    {
        NotifyCommandStates();
        if (_suppressSelection || value is null)
        {
            return;
        }

        _ = ReloadSelectedAsync();
    }

    partial void OnIsBusyChanged(bool value) => NotifyCommandStates();

    partial void OnHasPendingConfirmationChanged(bool value) => NotifyCommandStates();

    partial void OnSimulatedVolumeTextChanged(string value) => ShowSimulation();

    private bool CanSync() => !IsBusy;

    private bool CanActivateAgreement() => !IsBusy && SelectedAgreement?.Agreement.CanActivate == true;

    private bool CanCloseAgreement() => !IsBusy && SelectedAgreement?.Agreement.CanClose == true;

    private bool CanDeleteAgreement() => !IsBusy && SelectedAgreement?.Agreement.IsDraft == true;

    private async Task ReloadSelectedAsync()
    {
        IsBusy = true;

        try
        {
            await ShowDetailAsync(syncIfMissing: true);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadPickersAsync()
    {
        await LoadSuppliersAsync();
        await LoadAccountsAsync();
        await LoadBlanketsAsync();
    }

    private async Task LoadSuppliersAsync()
    {
        try
        {
            var contacts = await _sender.Send(new GetContactsQuery());

            Suppliers.Clear();
            foreach (var contact in contacts.Where(c => c.Type is "supplier" or "both"))
            {
                Suppliers.Add(new RebateChoice(contact.Id.ToString(), Label(contact.CompanyName, contact.FirstName, contact.LastName)));
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Without access to the contacts the supplier picker stays empty; the agreements still show the supplier name.
            Suppliers.Clear();
        }
    }

    private async Task LoadBlanketsAsync()
    {
        var cached = await _sender.Send(new GetBlanketContractsQuery());

        BlanketChoices.Clear();
        BlanketChoices.Add(new RebateChoice(string.Empty, _localization["SupplierRebates_NoBlanket"]));
        foreach (var contract in cached.Value)
        {
            BlanketChoices.Add(new RebateChoice(contract.Id, contract.Label));
        }
    }

    private async Task<bool> ShowAgreementsAsync()
    {
        var cached = await _sender.Send(new GetRebateAgreementsQuery());
        var keep = SelectedAgreement?.Id;

        _suppressSelection = true;
        try
        {
            Agreements.Clear();
            foreach (var agreement in cached.Value)
            {
                Agreements.Add(RowOf(agreement));
            }

            SelectedAgreement = Agreements.FirstOrDefault(row => string.Equals(row.Id, keep, StringComparison.Ordinal)) ?? Agreements.FirstOrDefault();
        }
        finally
        {
            _suppressSelection = false;
        }

        LastSyncedText = SyncedText(cached.SyncedAtUtc);

        return cached.SyncedAtUtc is not null;
    }

    private string SyncedText(DateTime? syncedAtUtc) =>
        syncedAtUtc is { } at
            ? Format("SupplierRebates_LastSynced", at.ToLocalTime().ToString(DateTimeFormat, Culture))
            : _localization["SupplierRebates_NeverSynced"];

    private RebateAgreementRow RowOf(RebateAgreement agreement) =>
        new(
            agreement.Id,
            agreement.Code,
            agreement.Name,
            agreement.ContactName ?? RebateFormat.NoValue,
            StatusText(agreement.Status),
            RebateFormat.Date(agreement.StartsOn, Culture) + " - " + RebateFormat.Date(agreement.EndsOn, Culture),
            RebateFormat.Money(agreement.OpenRebate, agreement.Currency, Culture),
            agreement);

    private async Task PullAsync(RebateSyncScope scope, string? id)
    {
        var result = await _sender.Send(new SyncSupplierRebatesCommand(scope, id));
        IsOffline = result.Status != SupplierRebatesStatus.Synced;
        StatusMessage = SyncMessage(result);
    }

    private string SyncMessage(SupplierRebatesResult result) => result.Status switch
    {
        SupplierRebatesStatus.Synced => _localization["SupplierRebates_Synced"],
        SupplierRebatesStatus.Offline => _localization["SupplierRebates_OfflineUsingCache"],
        SupplierRebatesStatus.Unauthorized => _localization["SupplierRebates_SignInRequired"],
        SupplierRebatesStatus.Forbidden => _localization["SupplierRebates_Forbidden"],
        _ => Format("SupplierRebates_SyncFailed", result.Detail ?? string.Empty),
    };

    private async Task ShowDetailAsync(bool syncIfMissing)
    {
        if (SelectedAgreement is not { } row)
        {
            ApplyDetail(detail: null);

            return;
        }

        var detail = await _sender.Send(new GetRebateAgreementDetailQuery(row.Id));
        if (detail is null && syncIfMissing)
        {
            await PullAsync(RebateSyncScope.Agreement, row.Id);
            detail = await _sender.Send(new GetRebateAgreementDetailQuery(row.Id));
        }

        ApplyDetail(detail);
    }

    private void ApplyDetail(RebateAgreementDetail? detail)
    {
        ClearDetail();
        HasDetail = detail is not null;

        if (detail is null)
        {
            ShowProgress(agreement: null, progress: null);
            ContributionsText = RebateFormat.NoValue;
            ShowJournal(preview: null);
            if (!IsOffline && StatusMessage.Length == 0)
            {
                StatusMessage = _localization[Agreements.Count == 0 ? "SupplierRebates_Empty" : "SupplierRebates_LoadError"];
            }

            NotifyCommandStates();

            return;
        }

        var agreement = detail.Agreement;
        LastSyncedText = SyncedText(detail.SyncedAtUtc);
        ShowProgress(agreement, detail.Progress);
        ShowScale(agreement, detail.Progress);

        foreach (var contribution in agreement.Contributions)
        {
            Contributions.Add(ContributionRowOf(contribution, agreement.Currency));
        }

        foreach (var run in detail.Runs)
        {
            Runs.Add(RunRowOf(run, agreement.Currency));
        }

        foreach (var proposal in detail.Proposals)
        {
            Proposals.Add(ProposalRowOf(proposal));
        }

        SelectedProposal = Proposals.FirstOrDefault();
        ShowSimulation();
        NotifyCommandStates();
    }

    private void ClearDetail()
    {
        Scale.Clear();
        Contributions.Clear();
        Runs.Clear();
        RunLines.Clear();
        Proposals.Clear();
        ProposalLines.Clear();
        Payables.Clear();
        _payableSource = [];
        SelectedRun = null;
        SelectedProposal = null;
        SelectedPayable = null;
    }

    private void ShowProgress(RebateAgreement? agreement, RebateProgress? progress)
    {
        HasTier = false;
        HasNextStep = false;
        CurrentTierText = RebateFormat.NoValue;
        NextTierText = RebateFormat.NoValue;
        DistanceLabel = string.Empty;
        DistanceText = RebateFormat.NoValue;
        ProjectedAtNextText = RebateFormat.NoValue;
        ProjectedGainText = RebateFormat.NoValue;
        ProgressPercent = 0;
        ProgressText = RebateFormat.NoValue;

        if (agreement is null || progress is null)
        {
            QualifyingText = RebateFormat.NoValue;
            EarnedText = RebateFormat.NoValue;
            AccruedText = RebateFormat.NoValue;
            SettledText = RebateFormat.NoValue;
            ContributionsText = RebateFormat.NoValue;

            return;
        }

        var currency = agreement.Currency;
        QualifyingText = RebateFormat.Money(progress.QualifyingVolume, currency, Culture);
        EarnedText = RebateFormat.Money(progress.EarnedRebate, currency, Culture);
        AccruedText = RebateFormat.Money(progress.AccruedRebate, currency, Culture);
        SettledText = RebateFormat.Money(progress.SettledRebate, currency, Culture);
        ContributionsText = ContributionSummaryText(progress.Contributions, currency);

        if (progress.Bonus is { } bonus)
        {
            ShowBonus(bonus, currency);
        }
        else if (agreement.RebateType == RebateCodes.TypeTieredVolume)
        {
            // The server sends the tier progress; a cached payload without it is recomputed with the same arithmetic.
            ShowTier(progress.Tier ?? RebateTierCalculator.Progress(agreement.Tiers, progress.QualifyingVolume, agreement.TierMode), currency);
        }
    }

    private string ContributionSummaryText(RebateContributionSummary summary, string currency) =>
        _localization["SupplierRebates_ContribPlanned"] + " " + RebateFormat.Money(summary.Planned, currency, Culture)
        + " | " + _localization["SupplierRebates_ContribAccrued"] + " " + RebateFormat.Money(summary.Accrued, currency, Culture)
        + " | " + _localization["SupplierRebates_ContribSettled"] + " " + RebateFormat.Money(summary.Settled, currency, Culture);

    private void ShowBonus(RebateBonusProgress bonus, string currency)
    {
        HasTier = false;
        HasNextStep = !bonus.Reached;
        CurrentTierText = bonus.Reached ? _localization["SupplierRebates_TargetReached"] : RebateFormat.Money(bonus.Target, currency, Culture);
        DistanceLabel = _localization["SupplierRebates_DistanceToTarget"];
        DistanceText = RebateFormat.Money(bonus.DistanceToTarget, currency, Culture);
        ProjectedAtNextText = RebateFormat.Money(bonus.Bonus, currency, Culture);
        ProgressPercent = (double)Math.Clamp(bonus.ProgressPercent, 0m, PercentScale);
        ProgressText = RebateFormat.Percent(bonus.ProgressPercent, Culture);
    }

    private void ShowTier(RebateTierProgress tier, string currency)
    {
        HasTier = true;
        HasNextStep = tier.NextTier is not null;
        CurrentTierText = tier.CurrentTier is { } current ? TierText(current, currency) : _localization["SupplierRebates_NoTier"];
        NextTierText = tier.NextTier is { } next ? TierText(next, currency) : _localization["SupplierRebates_TopTier"];
        DistanceLabel = _localization["SupplierRebates_DistanceToNext"];
        DistanceText = RebateFormat.Money(tier.DistanceToNext, currency, Culture);
        ProjectedAtNextText = RebateFormat.Money(tier.ProjectedRebateAtNext, currency, Culture);
        ProjectedGainText = RebateFormat.Money(tier.ProjectedGain, currency, Culture);
        ProgressPercent = (double)Math.Clamp(tier.ProgressPercent, 0m, PercentScale);
        ProgressText = RebateFormat.Percent(tier.ProgressPercent, Culture);
    }

    private string TierText(RebateTier tier, string currency) =>
        RebateFormat.Percent(tier.RatePercent, Culture) + " / " + RebateFormat.Money(tier.ThresholdAmount, currency, Culture)
        + (tier.FlatAmount is { } flat ? " + " + RebateFormat.Money(flat, currency, Culture) : string.Empty);

    private void ShowScale(RebateAgreement agreement, RebateProgress? progress)
    {
        decimal? currentFloor = null;
        if (progress is not null && agreement.RebateType == RebateCodes.TypeTieredVolume)
        {
            currentFloor = (progress.Tier ?? RebateTierCalculator.Progress(agreement.Tiers, progress.QualifyingVolume, agreement.TierMode)).CurrentTier?.ThresholdAmount;
        }

        var bands = progress is { Scale.Count: > 0 } ? progress.Scale : BandsOf(agreement.Tiers);

        foreach (var band in bands)
        {
            Scale.Add(new RebateScaleRow(
                RebateFormat.Money(band.ThresholdAmount, agreement.Currency, Culture),
                band.MaxThreshold is { } max ? RebateFormat.Money(max, agreement.Currency, Culture) : _localization["SupplierRebates_OpenEnded"],
                RebateFormat.Percent(band.RatePercent, Culture),
                RebateFormat.Money(band.FlatAmount, agreement.Currency, Culture),
                currentFloor is { } floor && band.ThresholdAmount == floor));
        }
    }

    private RebateContributionRow ContributionRowOf(RebateContribution contribution, string currency) =>
        new(contribution.Id, KindText(contribution.Kind), contribution.Description, RebateFormat.Money(contribution.Amount, currency, Culture), RebateFormat.Date(contribution.DueOn, Culture), contribution.Status, contribution);

    private RebateRunRow RunRowOf(RebateAccrualRun run, string currency) =>
        new(
            run.Id,
            RebateFormat.Date(run.PeriodStart, Culture) + " - " + RebateFormat.Date(run.PeriodEnd, Culture),
            _localization[run.IsPosted ? "SupplierRebates_RunPosted" : "SupplierRebates_RunReversed"],
            RebateFormat.Money(run.BaseAmount, currency, Culture),
            RebateFormat.Money(run.CumulativeRebate, currency, Culture),
            RebateFormat.Money(run.DeltaAmount, currency, Culture),
            run);

    private RebateProposalRow ProposalRowOf(RebateSettlementProposal proposal) =>
        new(
            proposal.Id,
            proposal.ExecutionMode is { } mode ? ModeText(mode) : RebateFormat.NoValue,
            SettlementStatusText(proposal.Status),
            RebateFormat.Money(proposal.Amount, proposal.Currency, Culture),
            RebateFormat.Money(proposal.TaxAmount, proposal.Currency, Culture),
            RebateFormat.Money(proposal.GrossAmount, proposal.Currency, Culture),
            proposal);

    private void ShowSimulation()
    {
        var volume = RebateFormat.ParseAmount(SimulatedVolumeText, Culture);
        if (volume is not { } simulated || SelectedAgreement?.Agreement is not { } agreement)
        {
            SimulatedRebateText = RebateFormat.NoValue;

            return;
        }

        var rebate = agreement.RebateType == RebateCodes.TypeFixedBonus
            ? RebateTierCalculator.FixedBonusOn(agreement.FixedBonusTarget ?? 0m, agreement.FixedBonusAmount ?? 0m, simulated)
            : RebateTierCalculator.RebateOn(agreement.Tiers, simulated, agreement.TierMode).Rebate;
        SimulatedRebateText = RebateFormat.Money(rebate, agreement.Currency, Culture);
    }

    private string StatusText(string status) => status switch
    {
        RebateCodes.StatusDraft => _localization["SupplierRebates_StatusDraft"],
        RebateCodes.StatusActive => _localization["SupplierRebates_StatusActive"],
        RebateCodes.StatusClosed => _localization["SupplierRebates_StatusClosed"],
        RebateCodes.StatusCancelled => _localization["SupplierRebates_StatusCancelled"],
        _ => status,
    };

    private string KindText(string kind) => kind switch
    {
        RebateCodes.KindMarketing => _localization["SupplierRebates_KindMarketing"],
        RebateCodes.KindCoMarketing => _localization["SupplierRebates_KindCoMarketing"],
        RebateCodes.KindListing => _localization["SupplierRebates_KindListing"],
        _ => kind,
    };

    private string SettlementStatusText(SettlementStatus status) => status switch
    {
        SettlementStatus.Draft => _localization["SupplierRebates_SettlementDraft"],
        SettlementStatus.PendingApproval => _localization["SupplierRebates_SettlementPendingApproval"],
        SettlementStatus.Approved => _localization["SupplierRebates_SettlementApproved"],
        SettlementStatus.Settled => _localization["SupplierRebates_SettlementSettled"],
        _ => _localization["SupplierRebates_SettlementCancelled"],
    };

    private string ModeText(RebateExecutionMode mode) => mode switch
    {
        RebateExecutionMode.CreditNoteRequest => _localization["SupplierRebates_ExecCreditNoteRequest"],
        RebateExecutionMode.ChargeInvoice => _localization["SupplierRebates_ExecChargeInvoice"],
        _ => _localization["SupplierRebates_ExecApSetOff"],
    };

    private string Format(string key, params string[] arguments) => string.Format(Culture, _localization[key], arguments);

    private void Ask(RebatePendingAction action, string confirmKey)
    {
        _pending = action;
        PendingConfirmText = _localization[confirmKey];
        HasPendingConfirmation = true;
    }

    private void Dismiss()
    {
        _pending = RebatePendingAction.None;
        PendingConfirmText = string.Empty;
        HasPendingConfirmation = false;
    }

    private async Task ChangeAgreementAsync(RebateAgreementAction action)
    {
        if (SelectedAgreement is not { } row)
        {
            return;
        }

        _ = await RunActionAsync(() => _sender.Send(new ChangeRebateAgreementCommand(row.Id, action)));
    }

    /// <summary>
    /// Runs a server action: shows how it ended and, when the server accepted it, shows the refreshed cache.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>The result, or null when the action was refused by the validation.</returns>
    private async Task<SupplierRebatesResult?> RunActionAsync(Func<Task<SupplierRebatesResult>> action)
    {
        IsBusy = true;

        try
        {
            var result = await action();
            IsOffline = result.Status == SupplierRebatesStatus.Offline;
            StatusMessage = ActionMessage(result);

            if (result.IsSuccess)
            {
                _ = await ShowAgreementsAsync();
                await ShowDetailAsync(syncIfMissing: false);
            }

            return result;
        }
        catch (ValidationException ex)
        {
            StatusMessage = ValidationText(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }

        return null;
    }

    private string ActionMessage(SupplierRebatesResult result) => result.Status switch
    {
        SupplierRebatesStatus.Synced => _localization["SupplierRebates_ActionDone"],
        SupplierRebatesStatus.Offline => _localization["SupplierRebates_OfflineActionBlocked"],
        SupplierRebatesStatus.Unauthorized => _localization["SupplierRebates_SignInRequired"],
        SupplierRebatesStatus.Forbidden => _localization["SupplierRebates_Forbidden"],
        SupplierRebatesStatus.Rejected => string.IsNullOrWhiteSpace(result.Detail) ? _localization["SupplierRebates_SaveError"] : result.Detail,
        _ => Format("SupplierRebates_SyncFailed", result.Detail ?? string.Empty),
    };

    private string ValidationText(ValidationException ex) =>
        ex.Errors.Select(error => _localization[error.ErrorMessage]).FirstOrDefault() ?? ex.Message;

    private void NotifyCommandStates()
    {
        SyncCommand.NotifyCanExecuteChanged();
        ActivateAgreementCommand.NotifyCanExecuteChanged();
        CloseAgreementCommand.NotifyCanExecuteChanged();
        RequestDeleteAgreementCommand.NotifyCanExecuteChanged();
        ConfirmPendingCommand.NotifyCanExecuteChanged();
        DismissPendingCommand.NotifyCanExecuteChanged();
        NotifyRunCommandStates();
        NotifySettlementCommandStates();
        NotifyEditorCommandStates();
    }
}
