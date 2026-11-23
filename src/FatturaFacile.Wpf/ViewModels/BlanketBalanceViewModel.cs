// <copyright file="BlanketBalanceViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.SupplierRebates;
using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the blanket purchase agreement balance view (Mod 049): the ceiling (plafond) and the guaranteed minimum spend against what
/// was ordered, received and invoiced, the alerts (ceiling near or exceeded, minimum at risk or not met) and the items. It reads the
/// local cache first, so it shows the last synced balance offline.
/// </summary>
internal sealed partial class BlanketBalanceViewModel : ObservableObject
{
    private const string DateTimeFormat = "g";
    private const decimal PercentScale = 100m;

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;

    private bool _suppressSelection;

    [ObservableProperty]
    private RebateChoice? _selectedContract;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _hasBalance;

    [ObservableProperty]
    private bool _hasAlerts;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _ceilingText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _minimumText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _orderedText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _receivedText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _invoicedText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _remainingText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _shortfallText = RebateFormat.NoValue;

    [ObservableProperty]
    private double _usedPercent;

    [ObservableProperty]
    private string _usedText = RebateFormat.NoValue;

    [ObservableProperty]
    private double _reachedPercent;

    [ObservableProperty]
    private string _reachedText = RebateFormat.NoValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlanketBalanceViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public BlanketBalanceViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _localization = localization;
        LastSyncedText = _localization["SupplierRebates_NeverSynced"];
    }

    /// <summary>
    /// Gets the blanket agreements of the picker.
    /// </summary>
    public ObservableCollection<RebateChoice> Contracts { get; } = [];

    /// <summary>
    /// Gets the alerts of the balance of the selected agreement.
    /// </summary>
    public ObservableCollection<BlanketAlertRow> Alerts { get; } = [];

    /// <summary>
    /// Gets the items of the selected agreement.
    /// </summary>
    public ObservableCollection<BlanketLineRow> Lines { get; } = [];

    private CultureInfo Culture => _localization.CurrentCulture;

    /// <summary>
    /// Loads the blanket agreements and the balance of the selected one from the cache, pulling from the server when the cache is
    /// empty.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            if (!await ShowContractsAsync())
            {
                await PullAsync(RebateSyncScope.BlanketContracts, id: null);
                _ = await ShowContractsAsync();
            }

            await ShowBalanceAsync(syncIfMissing: true);
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
    /// Pulls fresh data from the server and shows it; offline, the cached balance stays on screen.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand(CanExecute = nameof(CanSync))]
    public async Task SyncAsync()
    {
        IsBusy = true;

        try
        {
            await PullAsync(RebateSyncScope.BlanketContracts, id: null);
            _ = await ShowContractsAsync();

            if (SelectedContract is { } selected)
            {
                await PullAsync(RebateSyncScope.BlanketBalance, selected.Id);
            }

            await ShowBalanceAsync(syncIfMissing: false);
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

    partial void OnSelectedContractChanged(RebateChoice? value)
    {
        if (_suppressSelection || value is null)
        {
            return;
        }

        _ = ReloadSelectedAsync();
    }

    partial void OnIsBusyChanged(bool value) => SyncCommand.NotifyCanExecuteChanged();

    private bool CanSync() => !IsBusy;

    private async Task ReloadSelectedAsync()
    {
        IsBusy = true;

        try
        {
            await ShowBalanceAsync(syncIfMissing: true);
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

    private async Task<bool> ShowContractsAsync()
    {
        var cached = await _sender.Send(new GetBlanketContractsQuery());
        var keep = SelectedContract?.Id;

        _suppressSelection = true;
        try
        {
            Contracts.Clear();
            foreach (var contract in cached.Value)
            {
                Contracts.Add(new RebateChoice(contract.Id, contract.Label));
            }

            SelectedContract = Contracts.FirstOrDefault(choice => string.Equals(choice.Id, keep, StringComparison.Ordinal)) ?? Contracts.FirstOrDefault();
        }
        finally
        {
            _suppressSelection = false;
        }

        return cached.SyncedAtUtc is not null;
    }

    private async Task PullAsync(RebateSyncScope scope, string? id)
    {
        var result = await _sender.Send(new SyncSupplierRebatesCommand(scope, id));
        IsOffline = result.Status != SupplierRebatesStatus.Synced;
        StatusMessage = result.Status switch
        {
            SupplierRebatesStatus.Synced => _localization["SupplierRebates_Synced"],
            SupplierRebatesStatus.Offline => _localization["SupplierRebates_OfflineUsingCache"],
            SupplierRebatesStatus.Unauthorized => _localization["SupplierRebates_SignInRequired"],
            SupplierRebatesStatus.Forbidden => _localization["SupplierRebates_Forbidden"],
            _ => string.Format(Culture, _localization["SupplierRebates_SyncFailed"], result.Detail ?? string.Empty),
        };
    }

    private async Task ShowBalanceAsync(bool syncIfMissing)
    {
        if (SelectedContract is not { } contract)
        {
            Apply(balance: null);

            return;
        }

        var cached = await _sender.Send(new GetBlanketBalanceQuery(contract.Id));
        if (cached is null && syncIfMissing)
        {
            await PullAsync(RebateSyncScope.BlanketBalance, contract.Id);
            cached = await _sender.Send(new GetBlanketBalanceQuery(contract.Id));
        }

        Apply(cached);
    }

    private void Apply(RebateCached<BlanketBalance>? cached)
    {
        Alerts.Clear();
        Lines.Clear();
        HasBalance = cached is not null;
        HasAlerts = false;

        if (cached is null)
        {
            ShowFigures(balance: null);
            LastSyncedText = _localization["SupplierRebates_NeverSynced"];
            if (!IsOffline && StatusMessage.Length == 0)
            {
                StatusMessage = _localization[Contracts.Count == 0 ? "SupplierRebates_Empty" : "SupplierRebates_LoadError"];
            }

            return;
        }

        var balance = cached.Value;
        LastSyncedText = string.Format(Culture, _localization["SupplierRebates_LastSynced"], cached.SyncedAtUtc?.ToLocalTime().ToString(DateTimeFormat, Culture) ?? string.Empty);
        ShowFigures(balance);

        foreach (var alert in balance.Alerts)
        {
            Alerts.Add(new BlanketAlertRow(alert.Code, AlertText(alert.Code), alert.Severity, SeverityText(alert.Severity)));
        }

        HasAlerts = Alerts.Count > 0;

        foreach (var line in balance.Lines)
        {
            Lines.Add(new BlanketLineRow(
                line.Description,
                RebateFormat.Money(line.UnitPrice, balance.Currency, Culture),
                Quantity(line.CommittedQuantity),
                Quantity(line.ReleasedQuantity),
                Quantity(line.RemainingQuantity),
                RebateFormat.Money(line.OrderedAmount, balance.Currency, Culture)));
        }
    }

    private void ShowFigures(BlanketBalance? balance)
    {
        if (balance is null)
        {
            CeilingText = RebateFormat.NoValue;
            MinimumText = RebateFormat.NoValue;
            OrderedText = RebateFormat.NoValue;
            ReceivedText = RebateFormat.NoValue;
            InvoicedText = RebateFormat.NoValue;
            RemainingText = RebateFormat.NoValue;
            ShortfallText = RebateFormat.NoValue;
            UsedPercent = 0;
            UsedText = RebateFormat.NoValue;
            ReachedPercent = 0;
            ReachedText = RebateFormat.NoValue;

            return;
        }

        var currency = balance.Currency;
        CeilingText = RebateFormat.Money(balance.CeilingAmount, currency, Culture);
        MinimumText = RebateFormat.Money(balance.MinimumSpendAmount, currency, Culture);
        OrderedText = RebateFormat.Money(balance.OrderedAmount, currency, Culture);
        ReceivedText = RebateFormat.Money(balance.ReceivedAmount, currency, Culture);
        InvoicedText = RebateFormat.Money(balance.InvoicedAmount, currency, Culture);
        RemainingText = RebateFormat.Money(balance.RemainingCeiling, currency, Culture);
        ShortfallText = RebateFormat.Money(balance.MinimumShortfall, currency, Culture);
        UsedPercent = (double)Math.Clamp(balance.CeilingUsedPercent ?? 0m, 0m, PercentScale);
        UsedText = RebateFormat.Percent(balance.CeilingUsedPercent, Culture);
        ReachedPercent = (double)Math.Clamp(balance.MinimumReachedPercent ?? 0m, 0m, PercentScale);
        ReachedText = RebateFormat.Percent(balance.MinimumReachedPercent, Culture);
    }

    private string Quantity(decimal? value) => value is { } quantity ? quantity.ToString("0.####", Culture) : RebateFormat.NoValue;

    private string AlertText(string code) => code switch
    {
        RebateCodes.AlertCeilingNear => _localization["SupplierRebates_AlertCeilingNear"],
        RebateCodes.AlertCeilingExceeded => _localization["SupplierRebates_AlertCeilingExceeded"],
        RebateCodes.AlertMinimumAtRisk => _localization["SupplierRebates_AlertMinimumAtRisk"],
        RebateCodes.AlertMinimumNotMet => _localization["SupplierRebates_AlertMinimumNotMet"],
        _ => code,
    };

    private string SeverityText(string severity) => string.Equals(severity, "critical", StringComparison.Ordinal)
        ? _localization["SupplierRebates_SeverityCritical"]
        : _localization["SupplierRebates_SeverityWarning"];
}
