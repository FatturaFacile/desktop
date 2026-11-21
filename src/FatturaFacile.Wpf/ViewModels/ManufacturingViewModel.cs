// <copyright file="ManufacturingViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Manufacturing;
using FatturaFacile.Application.Manufacturing;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Manufacturing;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the Mod 061 production engineering view: work centres with their machines, routings with their operation steps and an
/// offline lead time and cost calculation, the cached OEE summaries with the six big losses, and the working calendars. Everything
/// is read from the local cache, so the view works without a connection; the sync pulls fresh data when the server is reachable.
/// </summary>
internal sealed partial class ManufacturingViewModel : ObservableObject
{
    private static readonly Dictionary<ManufacturingSyncStatus, string> StatusKeys = new()
    {
        [ManufacturingSyncStatus.Synced] = "Mfg_Synced",
        [ManufacturingSyncStatus.Offline] = "Mfg_OfflineUsingCache",
        [ManufacturingSyncStatus.Unauthorized] = "Mfg_SignInRequired",
        [ManufacturingSyncStatus.Forbidden] = "Mfg_Forbidden",
    };

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;

    private ManufacturingOverview? _overview;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private MfgWorkCenterRow? _selectedWorkCenter;

    [ObservableProperty]
    private MfgRoutingRow? _selectedRouting;

    [ObservableProperty]
    private decimal _quantity = 1m;

    [ObservableProperty]
    private bool _includeOverlap = true;

    [ObservableProperty]
    private string _calculationSummary = string.Empty;

    [ObservableProperty]
    private bool _hasOee;

    [ObservableProperty]
    private bool _oeeMeetsTarget;

    [ObservableProperty]
    private string _oeeHeadline = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManufacturingViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public ManufacturingViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _localization = localization;
        LastSyncedText = _localization["Mfg_NeverSynced"];
    }

    /// <summary>
    /// Gets the work centres.
    /// </summary>
    public ObservableCollection<MfgWorkCenterRow> WorkCenters { get; } = [];

    /// <summary>
    /// Gets the machines of the selected work centre.
    /// </summary>
    public ObservableCollection<MfgMachineRow> Machines { get; } = [];

    /// <summary>
    /// Gets the routings.
    /// </summary>
    public ObservableCollection<MfgRoutingRow> Routings { get; } = [];

    /// <summary>
    /// Gets the operation steps of the selected routing.
    /// </summary>
    public ObservableCollection<MfgOperationRow> Operations { get; } = [];

    /// <summary>
    /// Gets the cached OEE summaries.
    /// </summary>
    public ObservableCollection<MfgOeeRow> OeeRows { get; } = [];

    /// <summary>
    /// Gets the six big losses of the selected work centre.
    /// </summary>
    public ObservableCollection<MfgLossRow> Losses { get; } = [];

    /// <summary>
    /// Gets the working calendars.
    /// </summary>
    public ObservableCollection<MfgCalendarRow> Calendars { get; } = [];

    /// <summary>
    /// Loads the cached data, pulling from the server first when nothing is cached yet.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            await ShowCachedAsync();
            if (!HasData)
            {
                await PullAsync();
                await ShowCachedAsync();
            }
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
    /// Pulls fresh data from the server and shows it; offline, the cached data stays on screen.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public async Task SyncAsync()
    {
        IsBusy = true;

        try
        {
            await PullAsync();
            await ShowCachedAsync();
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
    /// Recalculates the lead time and cost of the selected routing for the quantity, offline.
    /// </summary>
    /// <returns>A task representing the calculation.</returns>
    [RelayCommand]
    public async Task RecalculateAsync()
    {
        // Each branch replaces the list in one synchronous step, so overlapping runs never leave duplicate rows.
        var routing = _overview?.Routings.FirstOrDefault(r => r.Id == SelectedRouting?.Id);
        if (routing is null)
        {
            Operations.Clear();
            CalculationSummary = string.Empty;
            return;
        }

        if (Quantity <= 0m)
        {
            CalculationSummary = _localization["Mfg_QuantityInvalid"];
            Fill(Operations, routing.Operations.OrderBy(operation => operation.Sequence).Select(o => OperationRow(o, step: null)));
            return;
        }

        try
        {
            var calculation = await _sender.Send(new CalculateRoutingQuery(routing.Id, Quantity, IncludeOverlap));
            Fill(Operations, routing.Operations.OrderBy(operation => operation.Sequence).Select(o => OperationRow(o, calculation?.Steps.FirstOrDefault(s => s.OperationId == o.Id))));
            CalculationSummary = calculation is null ? _localization["Mfg_NoOperations"] : Summary(calculation, routing);
        }
        catch (InvalidOperationException)
        {
            Fill(Operations, routing.Operations.OrderBy(operation => operation.Sequence).Select(o => OperationRow(o, step: null)));
            CalculationSummary = _localization["Mfg_CalcMissingData"];
        }
    }

    private static string Number(double value, CultureInfo culture) => value.ToString("0.##", culture);

    private static string Percent(double value, CultureInfo culture) => value.ToString("0.#", culture) + " %";

    private static void Fill<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private static Dictionary<Guid, MfgOeeSnapshot> LatestOee(ManufacturingOverview overview) =>
        overview.OeeSnapshots
            .Where(s => s.MachineCenterId is null)
            .GroupBy(s => s.WorkCenterId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.ToUtc).First());

    private static MfgCalendarRow CalendarRow(MfgWorkingCalendar c, CultureInfo culture) => new(
        c.Code,
        c.Name,
        c.Timezone,
        string.Join(", ", c.Shifts.GroupBy(s => s.DayOfWeek).Select(g => g.Key.ToString(culture) + ": " + string.Join('/', g.Select(s => s.StartTime + "–" + s.EndTime)))),
        c.Closures.Count.ToString(culture));

    private static MfgOeeRow OeeRow(MfgWorkCenter w, MfgOeeSnapshot s, CultureInfo culture) => new(
        w.Id,
        w.WorkCenterCode + " " + w.Name,
        s.FromUtc.ToLocalTime().ToString("d", culture) + " – " + s.ToUtc.ToLocalTime().ToString("d", culture),
        Percent(s.AvailabilityPercent, culture),
        Percent(s.PerformancePercent, culture),
        Percent(s.QualityPercent, culture),
        Percent(s.OeePercent, culture),
        Percent(s.OeeTargetPercent, culture),
        (s.GapToTargetPercent > 0 ? "+" : string.Empty) + Number(s.GapToTargetPercent, culture),
        s.MeetsTarget);

    partial void OnSelectedWorkCenterChanged(MfgWorkCenterRow? value) => ShowWorkCenter(value);

    partial void OnSelectedRoutingChanged(MfgRoutingRow? value) => RecalculateCommand.Execute(parameter: null);

    partial void OnQuantityChanged(decimal value) => RecalculateCommand.Execute(parameter: null);

    partial void OnIncludeOverlapChanged(bool value) => RecalculateCommand.Execute(parameter: null);

    private async Task PullAsync()
    {
        var result = await _sender.Send(new SyncManufacturingCommand());
        IsOffline = result.Status != ManufacturingSyncStatus.Synced;
        StatusMessage = result.Status == ManufacturingSyncStatus.Failed
            ? string.Format(_localization.CurrentCulture, _localization["Mfg_SyncFailed"], result.Detail ?? string.Empty)
            : _localization[StatusKeys.GetValueOrDefault(result.Status, string.Empty)];
        if (result.Status == ManufacturingSyncStatus.Offline && !HasData)
        {
            StatusMessage = _localization["Mfg_OfflineNoCache"];
        }
    }

    private async Task ShowCachedAsync() => Apply(await _sender.Send(new GetManufacturingOverviewQuery()));

    private void Apply(ManufacturingOverview overview)
    {
        var culture = _localization.CurrentCulture;
        _overview = overview;
        HasData = overview.WorkCenters.Count > 0 || overview.Routings.Count > 0;
        LastSyncedText = overview.LastSyncedUtc is { } at
            ? string.Format(culture, _localization["Mfg_LastSynced"], at.ToLocalTime().ToString("g", culture))
            : _localization["Mfg_NeverSynced"];
        if (!HasData && !IsOffline && StatusMessage.Length == 0)
        {
            StatusMessage = _localization["Mfg_NoData"];
        }

        var selectedWorkCenter = SelectedWorkCenter?.Id;
        var selectedRouting = SelectedRouting?.Id;
        var latestOee = LatestOee(overview);

        Fill(WorkCenters, overview.WorkCenters.Select(w => WorkCenterRow(w, overview, latestOee.GetValueOrDefault(w.Id), culture)));
        Fill(Routings, overview.Routings.Select(RoutingRow));
        Fill(Calendars, overview.Calendars.Select(c => CalendarRow(c, culture)));
        Fill(OeeRows, overview.WorkCenters.Where(w => latestOee.ContainsKey(w.Id)).Select(w => OeeRow(w, latestOee[w.Id], culture)));

        SelectedWorkCenter = WorkCenters.FirstOrDefault(w => w.Id == selectedWorkCenter) ?? WorkCenters.FirstOrDefault();
        SelectedRouting = Routings.FirstOrDefault(r => r.Id == selectedRouting) ?? Routings.FirstOrDefault();
    }

    private void ShowWorkCenter(MfgWorkCenterRow? row)
    {
        Machines.Clear();
        Losses.Clear();
        HasOee = false;
        OeeMeetsTarget = false;
        OeeHeadline = string.Empty;
        if (row is null || _overview is null)
        {
            return;
        }

        var culture = _localization.CurrentCulture;
        Fill(Machines, _overview.Machines.Where(m => m.WorkCenterId == row.Id).Select(m => MachineRow(m, culture)));

        var snapshot = LatestOee(_overview).GetValueOrDefault(row.Id);
        if (snapshot is null)
        {
            return;
        }

        HasOee = true;
        OeeMeetsTarget = snapshot.MeetsTarget;
        OeeHeadline = string.Format(culture, _localization["Mfg_OeeHeadline"], Percent(snapshot.OeePercent, culture), Percent(snapshot.OeeTargetPercent, culture));
        Fill(Losses, ManufacturingParser.ParseLosses(snapshot.SixBigLossesJson).Select(l => new MfgLossRow(
            _localization["Mfg_LOSS_" + l.Category.ToUpperInvariant()],
            _localization["Mfg_FACTOR_" + l.Factor.ToUpperInvariant()],
            Number(l.Minutes, culture))));
    }

    private MfgWorkCenterRow WorkCenterRow(MfgWorkCenter w, ManufacturingOverview overview, MfgOeeSnapshot? oee, CultureInfo culture) => new(
        w.Id,
        w.WorkCenterCode,
        w.Name,
        Label("Mfg_STATUS_", w.Status),
        Number((double)w.CapacityHoursPerDay, culture),
        Percent((double)w.EfficiencyRate, culture),
        oee is null ? "–" : Percent(oee.OeePercent, culture),
        Percent((double)w.OeeTargetPercent, culture),
        overview.Machines.Count(m => m.WorkCenterId == w.Id).ToString(culture));

    private MfgMachineRow MachineRow(MfgMachineCenter m, CultureInfo culture)
    {
        List<string> flags = [];
        if (m.IsIndustria40Enabled)
        {
            flags.Add(_localization["Mfg_FLAG_INDUSTRIA40"]);
        }

        if (m.IsTransizione50Enabled)
        {
            flags.Add(_localization["Mfg_FLAG_TRANSIZIONE50"]);
        }

        return new MfgMachineRow(
            m.MachineCode,
            m.Name,
            Label("Mfg_STATUS_", m.Status),
            Number((double)m.IdealCycleTimeSecondsPerUnit, culture),
            Label("Mfg_TELEMETRY_", OeeCalculator.TelemetryStatus(m.TelemetryLastSeenAt, DateTime.UtcNow)),
            string.Join(" · ", flags));
    }

    private MfgRoutingRow RoutingRow(MfgRouting r) => new(
        r.Id,
        r.RoutingNumber,
        r.Revision,
        string.IsNullOrEmpty(r.ItemCode) ? (r.ItemName ?? string.Empty) : r.ItemCode + " " + r.ItemName,
        r.Name,
        Label("Mfg_ROUTING_", r.Status),
        r.Operations.Count,
        r.IsDefault ? _localization["Mfg_Yes"] : string.Empty);

    private MfgOperationRow OperationRow(MfgRoutingOperation o, RoutingStepResult? step)
    {
        var culture = _localization.CurrentCulture;
        var workCenter = _overview?.WorkCenters.FirstOrDefault(w => w.Id == o.WorkCenterId)?.WorkCenterCode ?? string.Empty;
        var machine = o.MachineCenterId is { } machineId ? _overview?.Machines.FirstOrDefault(m => m.Id == machineId)?.MachineCode ?? string.Empty : string.Empty;

        return new MfgOperationRow(
            o.Sequence,
            o.OperationCode,
            o.Name,
            workCenter,
            machine,
            Label("Mfg_OPTYPE_", o.OperationType),
            Number((double)o.SetupTimeMinutes, culture),
            Number((double)o.RunTimeMinutesPerUnit, culture),
            step is null ? "–" : Number(step.DurationMinutes, culture),
            step is null ? "–" : Number(step.StartOffsetMinutes, culture),
            step is null ? "–" : Number(step.EndOffsetMinutes, culture),
            Links(o));
    }

    private string Links(MfgRoutingOperation o)
    {
        List<string> links = [];
        if (o.BomComponents.Count > 0)
        {
            links.Add(string.Format(_localization.CurrentCulture, _localization["Mfg_LinkBom"], o.BomComponents.Count));
        }

        if (o.LinkedQualityControlPointId is not null)
        {
            links.Add(_localization["Mfg_LinkQc"]);
        }

        if (o.WorkInstructionDocId is not null)
        {
            links.Add(_localization["Mfg_LinkWorkInstruction"]);
        }

        return string.Join(" · ", links);
    }

    private string Summary(RoutingCalculation calculation, MfgRouting routing)
    {
        var culture = _localization.CurrentCulture;
        var currency = _overview?.WorkCenters.FirstOrDefault(w => routing.Operations.Any(o => o.WorkCenterId == w.Id))?.CurrencyCode ?? string.Empty;

        return string.Format(
            culture,
            _localization["Mfg_CalcSummary"],
            Number(calculation.TotalMinutes, culture),
            Number(calculation.SerialMinutes, culture),
            Number(calculation.OverlapSavingMinutes, culture),
            calculation.Cost.Total.ToString("N2", culture) + " " + currency,
            calculation.UnitCost.ToString("N4", culture) + " " + currency);
    }

    private string Label(string prefix, string value)
    {
        var key = prefix + value.ToUpperInvariant();
        var text = _localization[key];

        return string.Equals(text, key, StringComparison.Ordinal) ? value : text;
    }
}
