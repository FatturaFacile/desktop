// <copyright file="ReceivablesViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The accounts receivable and credit management hub (Mod 013): the dashboard figures, the customers blocked for credit, the
/// queue of the actions waiting for the server, and the tabs of the open items, the aging, the provision and DSO, the credit
/// position and the ledger with the statement. It works from the cache offline and queues every write until the server is reachable.
/// </summary>
internal sealed partial class ReceivablesViewModel : ObservableObject
{
    private const string DateTimeFormat = "g";
    private const int OpenItemsTab = 0;
    private const int CreditTab = 3;

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;

    private Guid? _customerId;
    private bool _suppressCustomer;

    [ObservableProperty]
    private ReceivablesChoice? _selectedCustomer;

    [ObservableProperty]
    private string _customerIdText = string.Empty;

    [ObservableProperty]
    private string _currencyText = string.Empty;

    [ObservableProperty]
    private int _selectedTab;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private bool _hasPending;

    [ObservableProperty]
    private bool _hasConflicts;

    [ObservableProperty]
    private bool _hasBlocked;

    [ObservableProperty]
    private string _totalExposureText = string.Empty;

    [ObservableProperty]
    private string _totalOpenText = string.Empty;

    [ObservableProperty]
    private string _overdueText = string.Empty;

    [ObservableProperty]
    private string _over90Text = string.Empty;

    [ObservableProperty]
    private string _dsoText = string.Empty;

    [ObservableProperty]
    private string _provisionText = string.Empty;

    [ObservableProperty]
    private string _netRealizableText = string.Empty;

    [ObservableProperty]
    private string _overridesText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public ReceivablesViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        _sender = sender;
        _localization = localization;
        OpenItems = new ReceivablesOpenItemsViewModel(sender, localization, time, LoadAsync);
        Aging = new ReceivablesAgingViewModel(sender, localization, DrillDownAsync);
        Provision = new ReceivablesProvisionViewModel(sender, localization, time, LoadAsync);
        Credit = new ReceivablesCreditViewModel(sender, localization, time, LoadAsync);
        Ledger = new ReceivablesLedgerViewModel(sender, localization, time);
        CustomerChoices.Add(new ReceivablesChoice(string.Empty, _localization["Receivables_AllCustomers"]));
        _selectedCustomer = CustomerChoices[0];
        LastSyncedText = _localization["Receivables_NeverSynced"];
    }

    /// <summary>
    /// Gets the open items tab.
    /// </summary>
    public ReceivablesOpenItemsViewModel OpenItems { get; }

    /// <summary>
    /// Gets the aging tab.
    /// </summary>
    public ReceivablesAgingViewModel Aging { get; }

    /// <summary>
    /// Gets the provision and DSO tab.
    /// </summary>
    public ReceivablesProvisionViewModel Provision { get; }

    /// <summary>
    /// Gets the credit position tab.
    /// </summary>
    public ReceivablesCreditViewModel Credit { get; }

    /// <summary>
    /// Gets the ledger and statement tab.
    /// </summary>
    public ReceivablesLedgerViewModel Ledger { get; }

    /// <summary>
    /// Gets the customers that can be selected: all, then those of the aging and the blocked ones.
    /// </summary>
    public ObservableCollection<ReceivablesChoice> CustomerChoices { get; } = [];

    /// <summary>
    /// Gets the customers blocked for credit.
    /// </summary>
    public ObservableCollection<ReceivablesBlockedRow> Blocked { get; } = [];

    /// <summary>
    /// Gets the actions waiting for the server.
    /// </summary>
    public ObservableCollection<ReceivablesQueueRow> Queue { get; } = [];

    /// <summary>
    /// Gets the actions the server refused.
    /// </summary>
    public ObservableCollection<ReceivablesQueueRow> Conflicts { get; } = [];

    /// <summary>
    /// Sends the queued actions, then pulls the dashboard, the aging, the provision and the views of the selected customer;
    /// offline, the last synced figures stay on screen.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            _ = await _sender.Send(new FlushReceivablesOutboxCommand());
            var currency = CurrencyOrNull();
            var dashboard = await _sender.Send(new GetReceivablesDashboardQuery(currency));
            await Aging.LoadAsync(currency);
            OpenItems.SetBuckets(Aging.Buckets);
            ApplyDashboard(dashboard);
            RebuildCustomers();
            await Provision.LoadAsync(currency);
            await ApplyQueueAsync();
            await LoadCustomerPartsAsync();
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

    partial void OnSelectedCustomerChanged(ReceivablesChoice? value)
    {
        if (_suppressCustomer)
        {
            return;
        }

        _customerId = Guid.TryParse(value?.Key, out var id) ? id : null;
        _ = LoadCustomerPartsAsync();
    }

    [RelayCommand]
    private void SelectCustomerId()
    {
        if (!Guid.TryParse(CustomerIdText.Trim(), out var id))
        {
            StatusMessage = _localization["Receivables_Err_Customer"];

            return;
        }

        var key = id.ToString("D", CultureInfo.InvariantCulture);
        var choice = CustomerChoices.FirstOrDefault(candidate => string.Equals(candidate.Key, key, StringComparison.Ordinal));
        if (choice is null)
        {
            choice = new ReceivablesChoice(key, key);
            CustomerChoices.Add(choice);
        }

        SelectedCustomer = choice;
    }

    [RelayCommand]
    private async Task FlushAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _sender.Send(new FlushReceivablesOutboxCommand());
            StatusMessage = result.Offline
                ? _localization["Receivables_OfflineCache"]
                : string.Format(_localization.CurrentCulture, _localization["Receivables_FlushResult"], result.Sent.ToString(_localization.CurrentCulture));
            await LoadAsync();
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

    [RelayCommand]
    private async Task DismissAsync(ReceivablesQueueRow? row)
    {
        if (row is null)
        {
            return;
        }

        try
        {
            await _sender.Send(new DismissReceivablesConflictCommand(row.Id));
            await ApplyQueueAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void ShowBlocked(ReceivablesBlockedRow? row)
    {
        if (row is not null)
        {
            _ = DrillIntoCustomerAsync(row.CustomerId, CreditTab);
        }
    }

    private string? CurrencyOrNull() => string.IsNullOrWhiteSpace(CurrencyText) ? null : CurrencyText.Trim().ToUpperInvariant();

    private string CustomerLabel() => _customerId is null ? string.Empty : SelectedCustomer?.Label ?? string.Empty;

    private async Task DrillDownAsync(string? bucketKey, Guid? customerId)
    {
        SelectedTab = OpenItemsTab;
        SelectCustomerSilently(customerId);
        await OpenItems.DrillDownAsync(customerId, bucketKey);
        await LoadCreditAndLedgerAsync();
    }

    private Task DrillIntoCustomerAsync(Guid customerId, int tab)
    {
        SelectedTab = tab;
        SelectCustomerSilently(customerId);

        return LoadCustomerPartsAsync();
    }

    private void SelectCustomerSilently(Guid? customerId)
    {
        _suppressCustomer = true;
        try
        {
            _customerId = customerId;
            var key = customerId?.ToString("D", CultureInfo.InvariantCulture) ?? string.Empty;
            var choice = CustomerChoices.FirstOrDefault(candidate => string.Equals(candidate.Key, key, StringComparison.Ordinal));
            if (choice is null && customerId is { } id)
            {
                choice = new ReceivablesChoice(key, Aging.CustomerNames.GetValueOrDefault(id, key));
                CustomerChoices.Add(choice);
            }

            SelectedCustomer = choice ?? CustomerChoices[0];
        }
        finally
        {
            _suppressCustomer = false;
        }
    }

    private async Task LoadCustomerPartsAsync()
    {
        await OpenItems.LoadAsync(_customerId, CurrencyOrNull());
        await LoadCreditAndLedgerAsync();
    }

    private async Task LoadCreditAndLedgerAsync()
    {
        var currency = CurrencyOrNull();
        await Credit.LoadAsync(_customerId, CustomerLabel(), currency);
        await Ledger.LoadAsync(_customerId, CustomerLabel(), currency);
    }

    private void ApplyDashboard(ReceivablesLoad<ReceivablesDashboard>? load)
    {
        var culture = _localization.CurrentCulture;
        Blocked.Clear();
        HasBlocked = false;
        IsOffline = load?.FromCache != false;
        StatusMessage = IsOffline ? _localization["Receivables_OfflineCache"] : string.Empty;

        if (load is null)
        {
            LastSyncedText = _localization["Receivables_NeverSynced"];
            TotalExposureText = string.Empty;
            TotalOpenText = string.Empty;
            OverdueText = string.Empty;
            Over90Text = string.Empty;
            DsoText = string.Empty;
            ProvisionText = string.Empty;
            NetRealizableText = string.Empty;
            OverridesText = string.Empty;

            return;
        }

        var dashboard = load.Data;
        var kpis = dashboard.Kpis;
        LastSyncedText = string.Format(culture, _localization["Receivables_LastSynced"], load.SyncedAtUtc.ToLocalTime().ToString(DateTimeFormat, culture));
        TotalExposureText = ReceivablesFormat.Money(kpis.TotalExposure, culture);
        TotalOpenText = ReceivablesFormat.Money(kpis.TotalOpen, culture);
        OverdueText = ReceivablesFormat.Money(kpis.OverdueAmount, culture);
        Over90Text = ReceivablesFormat.Money(kpis.Over90, culture);
        DsoText = kpis.Dso is { } dso
            ? string.Format(culture, _localization["Receivables_DsoDays"], dso.ToString("0.##", culture))
            : _localization["Receivables_DsoUndefined"];
        ProvisionText = ReceivablesFormat.Money(kpis.BadDebtProvision, culture);
        NetRealizableText = ReceivablesFormat.Money(kpis.NetRealizableValue, culture);
        OverridesText = string.Format(culture, _localization["Receivables_OverridesCount"], kpis.ActiveOverrides.ToString(culture), kpis.BlockedCustomers.ToString(culture));

        foreach (var customer in dashboard.Blocked)
        {
            var name = Aging.CustomerNames.GetValueOrDefault(customer.CustomerId, customer.CustomerId.ToString("D", CultureInfo.InvariantCulture));
            Blocked.Add(new ReceivablesBlockedRow(
                customer.CustomerId,
                name,
                customer.Reason,
                string.Format(culture, _localization["Receivables_ExposureOfLimit"], ReceivablesFormat.Money(customer.Exposure, culture), ReceivablesFormat.Money(customer.Limit, culture))));
        }

        HasBlocked = Blocked.Count > 0;
    }

    private void RebuildCustomers()
    {
        _suppressCustomer = true;
        try
        {
            var keep = SelectedCustomer?.Key ?? string.Empty;
            CustomerChoices.Clear();
            CustomerChoices.Add(new ReceivablesChoice(string.Empty, _localization["Receivables_AllCustomers"]));
            foreach (var (id, name) in Aging.CustomerNames.OrderBy(pair => pair.Value, StringComparer.CurrentCultureIgnoreCase))
            {
                CustomerChoices.Add(new ReceivablesChoice(id.ToString("D", CultureInfo.InvariantCulture), name));
            }

            foreach (var row in Blocked.Where(row => CustomerChoices.All(choice => !string.Equals(choice.Key, row.CustomerId.ToString("D", CultureInfo.InvariantCulture), StringComparison.Ordinal))))
            {
                CustomerChoices.Add(new ReceivablesChoice(row.CustomerId.ToString("D", CultureInfo.InvariantCulture), row.Name));
            }

            if (_customerId is { } selected && CustomerChoices.All(choice => !string.Equals(choice.Key, selected.ToString("D", CultureInfo.InvariantCulture), StringComparison.Ordinal)))
            {
                var key = selected.ToString("D", CultureInfo.InvariantCulture);
                CustomerChoices.Add(new ReceivablesChoice(key, key));
            }

            SelectedCustomer = CustomerChoices.FirstOrDefault(choice => string.Equals(choice.Key, keep, StringComparison.Ordinal)) ?? CustomerChoices[0];
        }
        finally
        {
            _suppressCustomer = false;
        }
    }

    private async Task ApplyQueueAsync()
    {
        var queue = await _sender.Send(new GetReceivablesQueueQuery());
        Show(Queue, queue.Pending);
        Show(Conflicts, queue.Conflicts);
        HasPending = Queue.Count > 0;
        HasConflicts = Conflicts.Count > 0;
    }

    private void Show(ObservableCollection<ReceivablesQueueRow> target, IReadOnlyList<ReceivablesPendingAction> actions)
    {
        target.Clear();
        foreach (var action in actions)
        {
            target.Add(new ReceivablesQueueRow(
                action.Id,
                _localization["Receivables_Action_" + action.Action],
                action.Reference,
                action.OccurredAtUtc.ToLocalTime().ToString(DateTimeFormat, _localization.CurrentCulture),
                action.Error));
        }
    }
}
