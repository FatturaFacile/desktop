// <copyright file="ReceivablesCreditViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The credit position of the selected customer: the block status indicator, the limit, the exposure, the composite risk, the
/// limit history with its editor, the overrides and the credit check. It works from the cache offline; the credit check is then
/// decided locally with the same policy rules.
/// </summary>
internal sealed partial class ReceivablesCreditViewModel : ObservableObject
{
    private const string Blocked = "Blocked";
    private const string Hold = "Hold";
    private const string Warning = "Warning";
    private const string Ok = "Ok";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _time;
    private readonly Func<Task>? _changed;

    private Guid? _customerId;
    private string _customerLabel = string.Empty;
    private string? _currency;

    [ObservableProperty]
    private bool _hasCustomer;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _noticeText = string.Empty;

    [ObservableProperty]
    private string _blockState = Ok;

    [ObservableProperty]
    private string _blockText = string.Empty;

    [ObservableProperty]
    private string _limitText = string.Empty;

    [ObservableProperty]
    private string _utilizationText = string.Empty;

    [ObservableProperty]
    private string _availableText = string.Empty;

    [ObservableProperty]
    private string _insuredText = string.Empty;

    [ObservableProperty]
    private string _openInvoicesText = string.Empty;

    [ObservableProperty]
    private string _deliveryNotesText = string.Empty;

    [ObservableProperty]
    private string _ordersText = string.Empty;

    [ObservableProperty]
    private string _paymentsText = string.Empty;

    [ObservableProperty]
    private string _exposureText = string.Empty;

    [ObservableProperty]
    private string _overdueText = string.Empty;

    [ObservableProperty]
    private bool _hasRisk;

    [ObservableProperty]
    private string _riskText = string.Empty;

    [ObservableProperty]
    private string _riskState = string.Empty;

    [ObservableProperty]
    private ReceivablesChoice? _selectedOverrideScope;

    [ObservableProperty]
    private string _overrideReason = string.Empty;

    [ObservableProperty]
    private DateTime? _overrideExpires;

    [ObservableProperty]
    private ReceivablesChoice? _selectedCheckScope;

    [ObservableProperty]
    private string _checkAmountText = "0";

    [ObservableProperty]
    private string _checkText = string.Empty;

    [ObservableProperty]
    private string _checkState = string.Empty;

    [ObservableProperty]
    private ReceivablesLimitEditorViewModel? _editor;

    [ObservableProperty]
    private bool _isEditorOpen;

    [ObservableProperty]
    private string _actionText = string.Empty;

    [ObservableProperty]
    private string _actionState = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesCreditViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    /// <param name="changed">Called after a limit or an override changed, to refresh every view.</param>
    public ReceivablesCreditViewModel(ISender sender, ILocalizationService localization, TimeProvider time, Func<Task>? changed)
    {
        _sender = sender;
        _localization = localization;
        _time = time;
        _changed = changed;

        foreach (var scope in new[] { CreditScope.All, CreditScope.Order, CreditScope.DeliveryNote })
        {
            OverrideScopeChoices.Add(new ReceivablesChoice(scope.ToWire(), _localization["Receivables_Scope_" + scope.ToWire()]));
        }

        foreach (var scope in new[] { CreditScope.Order, CreditScope.DeliveryNote })
        {
            CheckScopeChoices.Add(new ReceivablesChoice(scope.ToWire(), _localization["Receivables_Scope_" + scope.ToWire()]));
        }

        _selectedOverrideScope = OverrideScopeChoices[0];
        _selectedCheckScope = CheckScopeChoices[0];
    }

    /// <summary>
    /// Gets the scopes an override can cover.
    /// </summary>
    public ObservableCollection<ReceivablesChoice> OverrideScopeChoices { get; } = [];

    /// <summary>
    /// Gets the operations a credit check can be run for.
    /// </summary>
    public ObservableCollection<ReceivablesChoice> CheckScopeChoices { get; } = [];

    /// <summary>
    /// Gets the factors of the risk score.
    /// </summary>
    public ObservableCollection<ReceivablesFactorRow> Factors { get; } = [];

    /// <summary>
    /// Gets the credit limit history, newest first.
    /// </summary>
    public ObservableCollection<ReceivablesLimitRow> Limits { get; } = [];

    /// <summary>
    /// Gets the overrides of the customer.
    /// </summary>
    public ObservableCollection<ReceivablesOverrideRow> Overrides { get; } = [];

    /// <summary>
    /// Loads the credit position of a customer.
    /// </summary>
    /// <param name="customerId">The customer, null for none.</param>
    /// <param name="customerLabel">The customer name.</param>
    /// <param name="currency">The ISO currency for a new limit.</param>
    /// <returns>A task representing the load.</returns>
    public async Task LoadAsync(Guid? customerId, string customerLabel, string? currency)
    {
        _customerId = customerId;
        _customerLabel = customerLabel;
        _currency = currency;
        HasCustomer = customerId is not null;
        Editor = null;

        if (customerId is not { } id)
        {
            Clear();

            return;
        }

        IsBusy = true;

        try
        {
            var status = await _sender.Send(new GetCustomerCreditStatusQuery(id));
            var risk = await _sender.Send(new GetCustomerRiskQuery(id));
            var limits = await _sender.Send(new GetCreditLimitsQuery(id));
            ApplyStatus(status);
            ApplyRisk(risk);
            ApplyLimits(limits);
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

    partial void OnEditorChanged(ReceivablesLimitEditorViewModel? value) => IsEditorOpen = value is not null;

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync(_customerId, _customerLabel, _currency);

    [RelayCommand]
    private void NewLimit() => OpenEditor(limit: null);

    [RelayCommand]
    private void EditLimit(ReceivablesLimitRow? row)
    {
        if (row is not null)
        {
            OpenEditor(row.Limit);
        }
    }

    [RelayCommand]
    private void CloseEditor() => Editor = null;

    [RelayCommand]
    private async Task DeleteLimitAsync(ReceivablesLimitRow? row)
    {
        if (row is null || _customerId is not { } id)
        {
            return;
        }

        try
        {
            var result = await _sender.Send(new DeleteCreditLimitCommand(new DeleteCreditLimitRequest(Guid.NewGuid(), id, _customerLabel, row.Limit.Id, _time.GetUtcNow().UtcDateTime)));
            await ShowResultAsync(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            Fail(ex);
        }
    }

    [RelayCommand]
    private async Task GrantOverrideAsync()
    {
        if (_customerId is not { } id)
        {
            return;
        }

        try
        {
            GrantOverrideRequest request = new(
                Guid.NewGuid(),
                id,
                _customerLabel,
                ReceivablesWire.ParseScope(SelectedOverrideScope?.Key),
                OverrideReason,
                OverrideExpires is { } date ? DateTime.SpecifyKind(date.Date.AddDays(1).AddTicks(-1), DateTimeKind.Local).ToUniversalTime() : null,
                _time.GetUtcNow().UtcDateTime);
            var result = await _sender.Send(new GrantCreditOverrideCommand(request));
            await ShowResultAsync(result);
            if (result.IsAccepted)
            {
                OverrideReason = string.Empty;
                OverrideExpires = null;
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Fail(ex);
        }
    }

    [RelayCommand]
    private async Task RevokeOverrideAsync(ReceivablesOverrideRow? row)
    {
        if (row is null || _customerId is not { } id)
        {
            return;
        }

        try
        {
            var result = await _sender.Send(new RevokeCreditOverrideCommand(new RevokeOverrideRequest(Guid.NewGuid(), id, _customerLabel, row.Override.Id, RevokeReason: null, _time.GetUtcNow().UtcDateTime)));
            await ShowResultAsync(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            Fail(ex);
        }
    }

    [RelayCommand]
    private async Task CheckCreditAsync()
    {
        if (_customerId is not { } id)
        {
            return;
        }

        var culture = _localization.CurrentCulture;
        if (!ReceivablesFormat.TryParseAmount(CheckAmountText, culture, out var amount) || amount < 0m)
        {
            CheckText = _localization[ReconciliationRules.AmountKey];
            CheckState = ReceivablesFormat.Bad;

            return;
        }

        try
        {
            var load = await _sender.Send(new CheckCustomerCreditQuery(id, ReceivablesWire.ParseScope(SelectedCheckScope?.Key), amount));
            if (load is null)
            {
                CheckText = _localization["Receivables_NoData"];
                CheckState = ReceivablesFormat.Warn;

                return;
            }

            var decision = load.Data;
            var breaches = decision.Breaches.Count == 0 ? string.Empty : " " + string.Join(", ", decision.Breaches.Select(breach => _localization["Receivables_Breach_" + breach]));
            var overridden = decision.Overridden ? " " + _localization["Receivables_Overridden"] : string.Empty;
            var cached = load.FromCache ? " " + _localization["Receivables_CheckLocal"] : string.Empty;
            CheckText = string.Format(culture, _localization["Receivables_CheckResult"], _localization["Receivables_Outcome_" + decision.Outcome.ToWire()], ReceivablesFormat.Money(decision.Exposure, culture), ReceivablesFormat.Money(decision.Limit, culture)) + breaches + overridden + cached;
            CheckState = ReceivablesFormat.OutcomeState(decision.Outcome);
        }
        catch (UnauthorizedAccessException ex)
        {
            CheckText = ex.Message;
            CheckState = ReceivablesFormat.Bad;
        }
    }

    private void OpenEditor(CustomerCreditLimit? limit)
    {
        if (_customerId is not { } id)
        {
            return;
        }

        Editor = new ReceivablesLimitEditorViewModel(
            _sender,
            _localization,
            _time,
            id,
            _customerLabel,
            limit,
            string.IsNullOrWhiteSpace(_currency) ? "EUR" : _currency,
            CompleteEditorAsync);
    }

    private async Task CompleteEditorAsync()
    {
        Editor = null;
        if (_changed is not null)
        {
            await _changed();
        }
    }

    private async Task ShowResultAsync(ReceivablesActionResult result)
    {
        var (text, state) = ReceivablesFormat.Describe(result, _localization);
        ActionText = text;
        ActionState = state;

        if (result.IsAccepted && _changed is not null)
        {
            await _changed();
        }
    }

    private void Fail(UnauthorizedAccessException ex)
    {
        ActionText = ex.Message;
        ActionState = ReceivablesFormat.Bad;
    }

    private void Clear()
    {
        BlockState = Ok;
        BlockText = string.Empty;
        LimitText = string.Empty;
        UtilizationText = string.Empty;
        AvailableText = string.Empty;
        InsuredText = string.Empty;
        OpenInvoicesText = string.Empty;
        DeliveryNotesText = string.Empty;
        OrdersText = string.Empty;
        PaymentsText = string.Empty;
        ExposureText = string.Empty;
        OverdueText = string.Empty;
        HasRisk = false;
        RiskText = string.Empty;
        NoticeText = string.Empty;
        StatusMessage = _localization["Receivables_SelectCustomer"];
        Factors.Clear();
        Limits.Clear();
        Overrides.Clear();
    }

    private void ApplyStatus(ReceivablesLoad<CustomerCreditStatus>? load)
    {
        Overrides.Clear();

        if (load is null)
        {
            Clear();
            StatusMessage = _localization["Receivables_NoData"];

            return;
        }

        var status = load.Data;
        var culture = _localization.CurrentCulture;
        var exposure = status.Exposure;
        var none = _localization["Receivables_None"];
        StatusMessage = string.Empty;
        NoticeText = load.FromCache ? _localization["Receivables_OfflineCache"] : string.Empty;

        (BlockState, BlockText) = Indicator(status);
        LimitText = status.Limit is { } limit
            ? string.Format(culture, _localization["Receivables_LimitText"], ReceivablesFormat.Money(limit.GrantedLimit, culture), limit.Currency, _localization["Receivables_Policy_" + limit.ReactionPolicy.ToWire()])
            : _localization["Receivables_NoLimit"];
        UtilizationText = ReceivablesFormat.Percent(status.UtilizationPercent, culture, none);
        AvailableText = status.AvailableCredit is { } available ? ReceivablesFormat.Money(available, culture) : none;
        InsuredText = string.Format(culture, _localization["Receivables_InsuredText"], ReceivablesFormat.Money(status.InsuredCover, culture), ReceivablesFormat.Money(status.UninsuredExposure, culture));
        OpenInvoicesText = ReceivablesFormat.Money(exposure.OpenInvoices, culture);
        DeliveryNotesText = ReceivablesFormat.Money(exposure.UnbilledDeliveryNotes, culture);
        OrdersText = ReceivablesFormat.Money(exposure.ConfirmedOrders, culture);
        PaymentsText = ReceivablesFormat.Money(exposure.UnreconciledPayments, culture);
        ExposureText = ReceivablesFormat.Money(exposure.TotalExposure, culture);
        OverdueText = string.Format(culture, _localization["Receivables_OverdueText"], ReceivablesFormat.Money(exposure.OverdueAmount, culture), exposure.OverdueItems.ToString(culture), exposure.MaxDaysPastDue.ToString(culture));

        var now = _time.GetUtcNow().UtcDateTime;
        foreach (var item in status.Overrides)
        {
            var until = item.ExpiresAt is { } expires ? expires.ToLocalTime().ToString("g", culture) : _localization["Receivables_NoExpiry"];
            Overrides.Add(new ReceivablesOverrideRow(
                item,
                string.Format(culture, _localization["Receivables_OverrideSummary"], _localization["Receivables_Scope_" + item.Scope.ToWire()], item.Reason),
                string.Format(culture, _localization["Receivables_OverrideDetail"], item.AuthorizedBy ?? none, until),
                item.IsActiveAt(now)));
        }
    }

    private (string State, string Text) Indicator(CustomerCreditStatus status)
    {
        var breaches = status.Decision is { } decision && decision.Breaches.Count > 0
            ? string.Join(", ", decision.Breaches.Select(breach => _localization["Receivables_Breach_" + breach]))
            : string.Empty;

        if (status.IsBlocked)
        {
            return (Blocked, string.IsNullOrEmpty(breaches) ? _localization["Receivables_Block_Blocked"] : _localization["Receivables_Block_Blocked"] + " " + breaches);
        }

        if (status.IsCreditHold)
        {
            return (Hold, _localization["Receivables_Block_Hold"]);
        }

        if (status.Decision is { Outcome: CreditOutcome.Warning })
        {
            return (Warning, string.IsNullOrEmpty(breaches) ? _localization["Receivables_Block_Warning"] : _localization["Receivables_Block_Warning"] + " " + breaches);
        }

        return (Ok, _localization["Receivables_Block_Ok"]);
    }

    private void ApplyRisk(ReceivablesLoad<CreditRiskScore>? load)
    {
        Factors.Clear();
        HasRisk = load is not null;

        if (load is null)
        {
            RiskText = string.Empty;
            RiskState = string.Empty;

            return;
        }

        var culture = _localization.CurrentCulture;
        var risk = load.Data;
        RiskText = string.Format(culture, _localization["Receivables_RiskText"], risk.Score.ToString(culture), _localization["Receivables_Rating_" + risk.Rating.ToWire()]);
        RiskState = risk.Rating switch
        {
            RiskRating.Low => ReceivablesFormat.Good,
            RiskRating.Medium => ReceivablesFormat.Warn,
            RiskRating.High => ReceivablesFormat.Bad,
            RiskRating.Critical => ReceivablesFormat.Bad,
            _ => ReceivablesFormat.Bad,
        };

        foreach (var factor in risk.Factors)
        {
            Factors.Add(new ReceivablesFactorRow(_localization["Receivables_Factor_" + factor.Key], factor.Value ?? "-", factor.Points.ToString("0.##", culture)));
        }
    }

    private void ApplyLimits(ReceivablesLoad<IReadOnlyList<CustomerCreditLimit>>? load)
    {
        Limits.Clear();

        if (load is null)
        {
            return;
        }

        var culture = _localization.CurrentCulture;
        var today = DateOnly.FromDateTime(_time.GetLocalNow().Date);
        var none = _localization["Receivables_None"];

        foreach (var limit in load.Data)
        {
            var validity = limit.ValidTo is { } to
                ? ReceivablesFormat.Day(limit.ValidFrom, culture) + " - " + ReceivablesFormat.Day(to, culture)
                : string.Format(culture, _localization["Receivables_ValidFrom"], ReceivablesFormat.Day(limit.ValidFrom, culture));
            Limits.Add(new ReceivablesLimitRow(
                limit,
                string.Format(culture, _localization["Receivables_LimitSummary"], ReceivablesFormat.Money(limit.GrantedLimit, culture), limit.Currency, validity),
                string.Format(
                    culture,
                    _localization["Receivables_LimitDetail"],
                    _localization["Receivables_Policy_" + limit.ReactionPolicy.ToWire()],
                    limit.InsurerName ?? none,
                    ReceivablesFormat.Money(limit.InsuredCover, culture),
                    limit.MaxOverdueDays?.ToString(culture) ?? none),
                limit.IsEffectiveOn(today)));
        }
    }
}
