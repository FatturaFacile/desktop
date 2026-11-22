// <copyright file="MaintenanceContractsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FluentValidation;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the maintenance contracts view: the list and the editor of a contract (coverage type, preventive frequency by calendar
/// and by meter, included visits, call-out fee, covered equipment). It reads the cache first; saving needs the server.
/// </summary>
internal sealed partial class MaintenanceContractsViewModel : ObservableObject
{
    private const string NoValue = "-";
    private const string DefaultCurrency = "EUR";

    private readonly ISender _sender;
    private readonly FieldServiceLabels _labels;

    private IReadOnlyList<MaintenanceContract> _contracts = [];
    private string? _editingId;
    private bool _suppressSelection;

    [ObservableProperty]
    private MaintenanceContractRow? _selectedContract;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _isNew = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _editNumber = string.Empty;

    [ObservableProperty]
    private string _editTitle = string.Empty;

    [ObservableProperty]
    private string _editCustomerId = string.Empty;

    [ObservableProperty]
    private DateTime _editStartDate = DateTime.Today;

    [ObservableProperty]
    private DateTime? _editEndDate;

    [ObservableProperty]
    private FieldServiceChoice<string>? _selectedStatus;

    [ObservableProperty]
    private FieldServiceChoice<MaintenanceCoverageType>? _selectedCoverage;

    [ObservableProperty]
    private string _calendarEvery = string.Empty;

    [ObservableProperty]
    private FieldServiceChoice<string>? _selectedUnit;

    [ObservableProperty]
    private string _operatingHours = string.Empty;

    [ObservableProperty]
    private string _cycles = string.Empty;

    [ObservableProperty]
    private string _leadPercent = string.Empty;

    [ObservableProperty]
    private string _leadDays = string.Empty;

    [ObservableProperty]
    private string _includedVisits = string.Empty;

    [ObservableProperty]
    private string _usedVisitsText = NoValue;

    [ObservableProperty]
    private string _calloutFee = "0";

    [ObservableProperty]
    private string _hourlyRate = string.Empty;

    [ObservableProperty]
    private string _currency = DefaultCurrency;

    [ObservableProperty]
    private string _notes = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceContractsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public MaintenanceContractsViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _labels = new FieldServiceLabels(localization);
        LastSyncedText = _labels.Key("FieldService_NeverSynced");

        foreach (var coverage in Enum.GetValues<MaintenanceCoverageType>())
        {
            Coverages.Add(new FieldServiceChoice<MaintenanceCoverageType>(coverage, _labels.Coverage(coverage)));
        }

        foreach (var status in new[] { "draft", "active", "suspended", "expired", "cancelled" })
        {
            Statuses.Add(new FieldServiceChoice<string>(status, _labels.ContractStatus(status)));
        }

        foreach (var unit in new[] { "day", "week", "month", "year" })
        {
            Units.Add(new FieldServiceChoice<string>(unit, _labels.Key("FieldService_Unit_" + unit)));
        }

        StartNew();
    }

    /// <summary>
    /// Gets the contracts of the list.
    /// </summary>
    public ObservableCollection<MaintenanceContractRow> Contracts { get; } = [];

    /// <summary>
    /// Gets the coverage types the editor offers.
    /// </summary>
    public ObservableCollection<FieldServiceChoice<MaintenanceCoverageType>> Coverages { get; } = [];

    /// <summary>
    /// Gets the contract statuses the editor offers.
    /// </summary>
    public ObservableCollection<FieldServiceChoice<string>> Statuses { get; } = [];

    /// <summary>
    /// Gets the calendar units of the preventive frequency.
    /// </summary>
    public ObservableCollection<FieldServiceChoice<string>> Units { get; } = [];

    /// <summary>
    /// Gets the customer assets the user can assign to the contract.
    /// </summary>
    public ObservableCollection<FieldServiceEquipmentPick> Equipment { get; } = [];

    /// <summary>
    /// Loads the contracts and the equipment from the cache, pulling them from the server when the cache is empty.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task LoadAsync() => GuardAsync(async () =>
    {
        if (!await ShowContractsAsync())
        {
            await PullAsync(FieldServiceSyncScope.Contracts);
            _ = await ShowContractsAsync();
        }

        await ShowEquipmentAsync();
    });

    /// <summary>
    /// Pulls the contracts and the equipment from the server; offline, the cache stays on screen.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public Task SyncAsync() => GuardAsync(async () =>
    {
        await PullAsync(FieldServiceSyncScope.Equipment);
        await PullAsync(FieldServiceSyncScope.Contracts);
        _ = await ShowContractsAsync();
        await ShowEquipmentAsync();
    });

    /// <summary>
    /// Clears the editor to create a new contract.
    /// </summary>
    [RelayCommand]
    public void NewContract()
    {
        _suppressSelection = true;
        SelectedContract = null;
        _suppressSelection = false;
        StartNew();
    }

    /// <summary>
    /// Saves the contract of the editor (a new one or the edited one) on the server and refreshes the list.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task SaveAsync()
    {
        if (BuildInput() is not { } input)
        {
            StatusMessage = _labels.Key("FieldService_Err_Number");

            return;
        }

        await GuardAsync(async () =>
        {
            var result = await _sender.Send(new SaveMaintenanceContractCommand(input));
            IsOffline = result.Status == FieldServiceCallStatus.Offline;

            if (result.Status != FieldServiceCallStatus.Ok)
            {
                StatusMessage = _labels.Call(result.Status, result.Detail);

                return;
            }

            await PullAsync(FieldServiceSyncScope.Contracts);
            _ = await ShowContractsAsync(result.Id);
            StatusMessage = _labels.Key("FieldService_ContractSaved");
        });
    }

    partial void OnSelectedContractChanged(MaintenanceContractRow? value)
    {
        if (_suppressSelection || value is null)
        {
            return;
        }

        var contract = _contracts.FirstOrDefault(candidate => string.Equals(candidate.Id, value.Id, StringComparison.Ordinal));
        if (contract is not null)
        {
            Fill(contract);
        }
    }

    private async Task GuardAsync(Func<Task> action)
    {
        IsBusy = true;

        try
        {
            await action();
        }
        catch (ValidationException ex)
        {
            StatusMessage = ex.Errors.Select(error => _labels.Key(error.ErrorMessage)).FirstOrDefault() ?? ex.Message;
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

    private async Task PullAsync(FieldServiceSyncScope scope)
    {
        var result = await _sender.Send(new SyncFieldServiceCommand(scope, Key: null, From: null, To: null));
        IsOffline = result.Status != FieldServiceCallStatus.Ok;
        StatusMessage = _labels.Call(result.Status, result.Detail);
    }

    private async Task<bool> ShowContractsAsync(string? focusId = null)
    {
        var cached = await _sender.Send(new GetMaintenanceContractsQuery());
        var keep = focusId ?? SelectedContract?.Id;
        _contracts = cached?.Value ?? [];

        _suppressSelection = true;
        try
        {
            Contracts.Clear();
            foreach (var contract in _contracts)
            {
                Contracts.Add(RowOf(contract));
            }

            SelectedContract = Contracts.FirstOrDefault(row => string.Equals(row.Id, keep, StringComparison.Ordinal));
        }
        finally
        {
            _suppressSelection = false;
        }

        LastSyncedText = cached is null
            ? _labels.Key("FieldService_NeverSynced")
            : _labels.Format("FieldService_LastSynced", cached.SyncedAtUtc.ToLocalTime().ToString("g", _labels.Culture));

        if (SelectedContract is not null && _contracts.FirstOrDefault(c => string.Equals(c.Id, SelectedContract.Id, StringComparison.Ordinal)) is { } selected)
        {
            Fill(selected);
        }

        return _contracts.Count > 0;
    }

    private async Task ShowEquipmentAsync()
    {
        var cached = await _sender.Send(new GetFieldServiceEquipmentQuery());
        var ticked = Equipment.Where(pick => pick.IsSelected).Select(pick => pick.Id).ToHashSet(StringComparer.Ordinal);

        Equipment.Clear();
        foreach (var asset in cached?.Value ?? [])
        {
            Equipment.Add(new FieldServiceEquipmentPick(asset.Id, asset.SerialNumber + (string.IsNullOrEmpty(asset.SiteAddress) ? string.Empty : " - " + asset.SiteAddress)) { IsSelected = ticked.Contains(asset.Id) });
        }

        TickEquipment(_contracts.FirstOrDefault(c => string.Equals(c.Id, _editingId, StringComparison.Ordinal))?.EquipmentIds ?? [.. ticked]);
    }

    private MaintenanceContractRow RowOf(MaintenanceContract contract)
    {
        var visits = contract.IncludedVisits is { } included
            ? _labels.Format("FieldService_VisitsOf", contract.UsedVisits.ToString(_labels.Culture), included.ToString(_labels.Culture))
            : contract.UsedVisits.ToString(_labels.Culture);

        return new(
            contract.Id,
            contract.Number,
            contract.Title,
            _labels.ContractStatus(contract.Status),
            _labels.Coverage(contract.Coverage),
            _labels.Day(contract.StartDate) + " - " + _labels.Day(contract.EndDate),
            visits,
            _labels.Money(contract.CalloutFee, contract.Currency),
            FrequencyText(contract.Frequency));
    }

    private string FrequencyText(PreventiveFrequency? rule)
    {
        if (rule is null)
        {
            return NoValue;
        }

        List<string> parts = [];
        if (rule.CalendarEvery is { } every && rule.CalendarUnit is { } unit)
        {
            parts.Add(_labels.Format("FieldService_FrequencyCalendar", every.ToString(_labels.Culture), _labels.Key("FieldService_Unit_" + unit)));
        }

        if (rule.OperatingHours is { } hours)
        {
            parts.Add(_labels.Format("FieldService_FrequencyHours", _labels.Number(hours)));
        }

        if (rule.Cycles is { } cycles)
        {
            parts.Add(_labels.Format("FieldService_FrequencyCycles", cycles.ToString(_labels.Culture)));
        }

        return parts.Count == 0 ? NoValue : string.Join("; ", parts);
    }

    private void StartNew()
    {
        _editingId = null;
        IsNew = true;
        EditNumber = string.Empty;
        EditTitle = string.Empty;
        EditCustomerId = string.Empty;
        EditStartDate = DateTime.Today;
        EditEndDate = null;
        SelectedStatus = Statuses.FirstOrDefault();
        SelectedCoverage = Coverages.FirstOrDefault();
        CalendarEvery = string.Empty;
        SelectedUnit = Units.FirstOrDefault(choice => string.Equals(choice.Value, "month", StringComparison.Ordinal));
        OperatingHours = string.Empty;
        Cycles = string.Empty;
        LeadPercent = string.Empty;
        LeadDays = string.Empty;
        IncludedVisits = string.Empty;
        UsedVisitsText = NoValue;
        CalloutFee = "0";
        HourlyRate = string.Empty;
        Currency = DefaultCurrency;
        Notes = string.Empty;
        TickEquipment([]);
    }

    private void Fill(MaintenanceContract contract)
    {
        var culture = _labels.Culture;
        _editingId = contract.Id;
        IsNew = false;
        EditNumber = contract.Number;
        EditTitle = contract.Title;
        EditCustomerId = contract.CustomerId;
        EditStartDate = contract.StartDate.ToDateTime(TimeOnly.MinValue);
        EditEndDate = contract.EndDate?.ToDateTime(TimeOnly.MinValue);
        SelectedStatus = Statuses.FirstOrDefault(choice => string.Equals(choice.Value, contract.Status, StringComparison.Ordinal));
        SelectedCoverage = Coverages.FirstOrDefault(choice => choice.Value == contract.Coverage);
        CalendarEvery = contract.Frequency?.CalendarEvery?.ToString(culture) ?? string.Empty;
        SelectedUnit = Units.FirstOrDefault(choice => string.Equals(choice.Value, contract.Frequency?.CalendarUnit, StringComparison.Ordinal)) ?? SelectedUnit;
        OperatingHours = contract.Frequency?.OperatingHours is { } hours ? _labels.Number(hours) : string.Empty;
        Cycles = contract.Frequency?.Cycles?.ToString(culture) ?? string.Empty;
        LeadPercent = contract.Frequency?.LeadPercent is { } percent ? _labels.Number(percent) : string.Empty;
        LeadDays = contract.Frequency?.LeadDays?.ToString(culture) ?? string.Empty;
        IncludedVisits = contract.IncludedVisits?.ToString(culture) ?? string.Empty;
        UsedVisitsText = contract.UsedVisits.ToString(culture);
        CalloutFee = _labels.Number(contract.CalloutFee);
        HourlyRate = contract.HourlyRate is { } rate ? _labels.Number(rate) : string.Empty;
        Currency = contract.Currency;
        Notes = contract.Notes ?? string.Empty;
        TickEquipment(contract.EquipmentIds);
    }

    private void TickEquipment(IReadOnlyCollection<string> ids)
    {
        foreach (var pick in Equipment)
        {
            pick.IsSelected = ids.Contains(pick.Id, StringComparer.Ordinal);
        }
    }

    private FieldServiceContractInput? BuildInput()
    {
        var culture = _labels.Culture;

        if (!FieldServiceNumbers.TryAmount(CalloutFee, culture, out var callout)
            || !FieldServiceNumbers.TryOptional(HourlyRate, culture, out var rate)
            || !FieldServiceNumbers.TryWhole(IncludedVisits, culture, out var visits))
        {
            return null;
        }

        if (!FieldServiceNumbers.TryWhole(CalendarEvery, culture, out var every)
            || !FieldServiceNumbers.TryOptional(OperatingHours, culture, out var hours)
            || !FieldServiceNumbers.TryWhole(Cycles, culture, out var cycles))
        {
            return null;
        }

        if (!FieldServiceNumbers.TryOptional(LeadPercent, culture, out var percent)
            || !FieldServiceNumbers.TryWhole(LeadDays, culture, out var days))
        {
            return null;
        }

        var unit = every is null ? null : SelectedUnit?.Value;
        var rule = (every, hours, cycles, days) is (null, null, null, null)
            ? null
            : new PreventiveFrequency(every, unit, hours, cycles, percent, days);

        return new FieldServiceContractInput(
            _editingId,
            EditCustomerId.Trim(),
            EditNumber.Trim(),
            EditTitle.Trim(),
            DateOnly.FromDateTime(EditStartDate),
            EditEndDate is { } end ? DateOnly.FromDateTime(end) : null,
            SelectedStatus?.Value ?? "draft",
            SelectedCoverage?.Value ?? MaintenanceCoverageType.LaborOnly,
            rule,
            visits,
            callout,
            rate,
            Currency.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            [.. Equipment.Where(pick => pick.IsSelected).Select(pick => pick.Id)]);
    }
}
