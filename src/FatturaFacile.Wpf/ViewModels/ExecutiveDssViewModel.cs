// <copyright file="ExecutiveDssViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Dss;
using FatturaFacile.Application.Features.Dss;
using FatturaFacile.Domain.Dss;
using FatturaFacile.Wpf.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the Executive DSS cockpit: the strategic filter, the Balanced Scorecard, the alerts, the variance waterfall, the
/// profitability matrix with the Whale Curve and the what-if simulator. It reads the local cache first, so the cockpit shows the
/// last synced data and the simulator keeps working without a connection; the sync pulls fresh data when the server is reachable.
/// </summary>
internal sealed partial class ExecutiveDssViewModel : ObservableObject
{
    private const string FormatPdf = "pdf";
    private const string FormatXlsx = "xlsx";
    private const double Tolerance = 1e-9;

    private static readonly Dictionary<DssSyncStatus, string> StatusKeys = new()
    {
        [DssSyncStatus.Synced] = "Dss_Synced",
        [DssSyncStatus.Offline] = "Dss_OfflineUsingCache",
        [DssSyncStatus.Unauthorized] = "Dss_SignInRequired",
        [DssSyncStatus.Forbidden] = "Dss_Forbidden",
    };

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly IFileDialogService _fileDialog;

    private DssSnapshot? _snapshot;
    private bool _suppressReload;

    [ObservableProperty]
    private DssChoice? _selectedEntity;

    [ObservableProperty]
    private DssChoice? _selectedCurrency;

    [ObservableProperty]
    private DssChoice? _selectedPeriod;

    [ObservableProperty]
    private DssChoice? _selectedBaseline;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasSnapshot;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _showRevenueBridge;

    [ObservableProperty]
    private double _pricePct;

    [ObservableProperty]
    private double _cogsPct;

    [ObservableProperty]
    private double _opexPct;

    [ObservableProperty]
    private double _dsoDaysDelta;

    [ObservableProperty]
    private double _fxShiftPct;

    [ObservableProperty]
    private double _fteDelta;

    [ObservableProperty]
    private bool _hasSimulation;

    [ObservableProperty]
    private DssMatrixRow? _selectedMatrixRow;

    [ObservableProperty]
    private bool _isDetailOpen;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutiveDssViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="fileDialog">The file dialog service.</param>
    public ExecutiveDssViewModel(ISender sender, ILocalizationService localization, IFileDialogService fileDialog)
    {
        _sender = sender;
        _localization = localization;
        _fileDialog = fileDialog;
        LastSyncedText = _localization["Dss_NeverSynced"];
        BuildChoices();
    }

    /// <summary>
    /// Gets the legal entities of the filter (the first one means every entity).
    /// </summary>
    public ObservableCollection<DssChoice> Entities { get; } = [];

    /// <summary>
    /// Gets the reporting currencies.
    /// </summary>
    public ObservableCollection<DssChoice> Currencies { get; } = [];

    /// <summary>
    /// Gets the period presets.
    /// </summary>
    public ObservableCollection<DssChoice> Periods { get; } = [];

    /// <summary>
    /// Gets the comparison baselines.
    /// </summary>
    public ObservableCollection<DssChoice> Baselines { get; } = [];

    /// <summary>
    /// Gets the financial health indicators.
    /// </summary>
    public ObservableCollection<DssKpiCard> FinancialKpis { get; } = [];

    /// <summary>
    /// Gets the customer and commercial indicators.
    /// </summary>
    public ObservableCollection<DssKpiCard> CustomerKpis { get; } = [];

    /// <summary>
    /// Gets the internal efficiency indicators.
    /// </summary>
    public ObservableCollection<DssKpiCard> EfficiencyKpis { get; } = [];

    /// <summary>
    /// Gets the strategic alerts, most severe first.
    /// </summary>
    public ObservableCollection<DssAlertItem> Alerts { get; } = [];

    /// <summary>
    /// Gets the bars of the variance waterfall on screen.
    /// </summary>
    public ObservableCollection<DssWaterfallBar> BridgeBars { get; } = [];

    /// <summary>
    /// Gets the profitability matrix rows.
    /// </summary>
    public ObservableCollection<DssMatrixRow> MatrixRows { get; } = [];

    /// <summary>
    /// Gets the Whale Curve points.
    /// </summary>
    public ObservableCollection<DssWhalePoint> WhalePoints { get; } = [];

    /// <summary>
    /// Gets the lines of the what-if result.
    /// </summary>
    public ObservableCollection<DssSimulationRow> SimulationRows { get; } = [];

    /// <summary>
    /// Gets the lines of the detail dialog of the selected profitability row.
    /// </summary>
    public ObservableCollection<DssDetailLine> DetailLines { get; } = [];

    /// <summary>
    /// Gets the filter currently selected.
    /// </summary>
    public DssFilter CurrentFilter => new(
        SelectedEntity?.Value ?? string.Empty,
        SelectedCurrency?.Value ?? DssFilter.Default.Currency,
        SelectedPeriod?.Value ?? DssFilter.Default.PeriodPreset,
        SelectedBaseline?.Value ?? DssFilter.Default.ComparisonBaseline);

    /// <summary>
    /// Loads the cockpit: the cached snapshot first, then a pull from the server when nothing is cached for the filter.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task LoadAsync() => ReloadAsync(syncIfMissing: true);

    /// <summary>
    /// Pulls fresh data from the server for the current filter and shows it; offline, the cached snapshot stays on screen.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public async Task SyncAsync()
    {
        IsBusy = true;

        try
        {
            await PullAsync(CurrentFilter);
            _ = await ShowCachedAsync();
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
    /// Generates the board pack on the server and saves it. It needs a connection.
    /// </summary>
    /// <param name="format">pdf or xlsx.</param>
    /// <returns>A task representing the export.</returns>
    [RelayCommand]
    public async Task ExportBoardPackAsync(string? format)
    {
        var isXlsx = string.Equals(format, FormatXlsx, StringComparison.OrdinalIgnoreCase);
        var chosen = isXlsx ? FormatXlsx : FormatPdf;
        if (IsOffline)
        {
            StatusMessage = _localization["Dss_ExportOnlineOnly"];
            return;
        }

        var filter = CurrentFilter;
        var path = _fileDialog.ShowSaveFile(
            _localization["Dss_ExportDialogTitle"],
            _localization[isXlsx ? "Dss_ExportFileXlsx" : "Dss_ExportFilePdf"],
            $"board-pack-{filter.PeriodPreset.ToLowerInvariant()}-{filter.Currency.ToLowerInvariant()}.{chosen}");
        if (path is null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var pack = await _sender.Send(new ExportDssBoardPackQuery(filter, chosen, BoardPackLocale(filter.Currency)));
            await File.WriteAllBytesAsync(path, pack.Content);
            StatusMessage = Format("Dss_ExportSaved", path);
        }
        catch (HttpRequestException ex)
        {
            IsOffline = ex.StatusCode is null;
            StatusMessage = Format("Dss_SyncFailed", ex.Message);
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
    /// Closes the detail dialog of a profitability row.
    /// </summary>
    [RelayCommand]
    public void CloseDetail() => SelectedMatrixRow = null;

    /// <summary>
    /// Sets every what-if slider back to zero.
    /// </summary>
    [RelayCommand]
    public void ResetScenario()
    {
        PricePct = 0;
        CogsPct = 0;
        OpexPct = 0;
        DsoDaysDelta = 0;
        FxShiftPct = 0;
        FteDelta = 0;
    }

    private static string DeltaState(bool? favorable)
    {
        var state = "Neutral";
        if (favorable is { } improving)
        {
            state = improving ? "Good" : "Bad";
        }

        return state;
    }

    private static string StateOf(double delta, bool higherIsBetter)
    {
        if (Math.Abs(delta) < Tolerance)
        {
            return "Neutral";
        }

        var improving = (delta > 0) == higherIsBetter;

        return improving ? "Good" : "Bad";
    }

    private static string Signed(double value, string text) => value > 0 ? "+" + text : text;

    private static string Money(double value, string currency, CultureInfo culture) => value.ToString("N0", culture) + " " + currency;

    private static string FormatKpi(double value, string unit, string currency, CultureInfo culture) => unit switch
    {
        "currency" => Money(value, currency, culture),
        "percent" => value.ToString("0.#", culture) + "%",
        "days" => value.ToString("0.#", culture),
        _ => value.ToString("0.##", culture),
    };

    private static string DeltaText(DssKpi kpi, string currency, CultureInfo culture) =>
        kpi.DeltaPct is { } pct ? Signed(pct, pct.ToString("0.#", culture) + "%") : DeltaAmount(kpi, currency, culture);

    private static string DeltaAmount(DssKpi kpi, string currency, CultureInfo culture) =>
        kpi.Delta is { } delta ? Signed(delta, FormatKpi(delta, kpi.Unit, currency, culture)) : string.Empty;

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    partial void OnSelectedEntityChanged(DssChoice? value) => FilterChanged();

    partial void OnSelectedCurrencyChanged(DssChoice? value) => FilterChanged();

    partial void OnSelectedPeriodChanged(DssChoice? value) => FilterChanged();

    partial void OnSelectedBaselineChanged(DssChoice? value) => FilterChanged();

    partial void OnShowRevenueBridgeChanged(bool value) => BuildBridge();

    partial void OnPricePctChanged(double value) => Recalculate();

    partial void OnCogsPctChanged(double value) => Recalculate();

    partial void OnOpexPctChanged(double value) => Recalculate();

    partial void OnDsoDaysDeltaChanged(double value) => Recalculate();

    partial void OnFxShiftPctChanged(double value) => Recalculate();

    partial void OnFteDeltaChanged(double value) => Recalculate();

    partial void OnSelectedMatrixRowChanged(DssMatrixRow? value)
    {
        DetailLines.Clear();
        if (value is not null)
        {
            foreach (var line in DetailOf(value))
            {
                DetailLines.Add(line);
            }
        }

        IsDetailOpen = value is not null;
    }

    private void FilterChanged()
    {
        if (!_suppressReload)
        {
            LoadCommand.Execute(parameter: null);
        }
    }

    private void BuildChoices()
    {
        _suppressReload = true;
        try
        {
            Replace(Currencies, DssFilter.Currencies.Select(value => new DssChoice(value, value)));
            Replace(Periods, DssFilter.PeriodPresets.Select(value => new DssChoice(value, _localization["Dss_PERIOD_" + value])));
            Replace(Baselines, DssFilter.Baselines.Select(value => new DssChoice(value, _localization["Dss_BASELINE_" + value])));
            Replace(Entities, [new DssChoice(string.Empty, _localization["Dss_ALL_ENTITIES"])]);

            SelectedEntity = Entities[0];
            SelectedCurrency = Currencies[0];
            SelectedPeriod = Periods.First(choice => string.Equals(choice.Value, DssFilter.Default.PeriodPreset, StringComparison.Ordinal));
            SelectedBaseline = Baselines.First(choice => string.Equals(choice.Value, DssFilter.Default.ComparisonBaseline, StringComparison.Ordinal));
        }
        finally
        {
            _suppressReload = false;
        }
    }

    private async Task ReloadAsync(bool syncIfMissing)
    {
        IsBusy = true;

        try
        {
            var shown = await ShowCachedAsync();
            if (!shown && syncIfMissing)
            {
                await PullAsync(CurrentFilter);
                _ = await ShowCachedAsync();
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

    private async Task PullAsync(DssFilter filter)
    {
        var result = await _sender.Send(new SyncDssSnapshotCommand(filter));
        IsOffline = result.Status != DssSyncStatus.Synced;

        StatusMessage = StatusText(result);
    }

    private string StatusText(DssSyncResult result)
    {
        var key = StatusKeys.GetValueOrDefault(result.Status, string.Empty);
        if (result.Status == DssSyncStatus.Offline && _snapshot is null)
        {
            key = "Dss_OfflineNoCache";
        }

        return result.Status == DssSyncStatus.Failed ? Format("Dss_SyncFailed", result.Detail ?? string.Empty) : _localization[key];
    }

    private async Task<bool> ShowCachedAsync()
    {
        var snapshot = await _sender.Send(new GetDssSnapshotQuery(CurrentFilter));
        Apply(snapshot);

        return snapshot is not null;
    }

    private void Apply(DssSnapshot? snapshot)
    {
        _snapshot = snapshot;
        HasSnapshot = snapshot is not null;

        if (snapshot is null)
        {
            Clear();
            LastSyncedText = _localization["Dss_NeverSynced"];
            if (!IsOffline && StatusMessage.Length == 0)
            {
                StatusMessage = _localization["Dss_NoSnapshot"];
            }

            return;
        }

        var culture = _localization.CurrentCulture;
        LastSyncedText = Format("Dss_LastSynced", snapshot.SyncedAtUtc.ToLocalTime().ToString("g", culture));
        UpdateEntities(snapshot.LegalEntities);

        Replace(FinancialKpis, Cards(snapshot, "financial"));
        Replace(CustomerKpis, Cards(snapshot, "customer"));
        Replace(EfficiencyKpis, Cards(snapshot, "efficiency"));
        Replace(Alerts, snapshot.Overview.Alerts.Select(AlertOf));
        Replace(MatrixRows, snapshot.Profitability.Rows);
        Replace(WhalePoints, snapshot.Profitability.WhaleCurve);
        BuildBridge();
        Recalculate();
    }

    private void Clear()
    {
        FinancialKpis.Clear();
        CustomerKpis.Clear();
        EfficiencyKpis.Clear();
        Alerts.Clear();
        MatrixRows.Clear();
        WhalePoints.Clear();
        BridgeBars.Clear();
        SimulationRows.Clear();
        HasSimulation = false;
    }

    private void UpdateEntities(IReadOnlyList<DssLegalEntity> entities)
    {
        var selected = SelectedEntity?.Value ?? string.Empty;
        List<DssChoice> choices = [new DssChoice(string.Empty, _localization["Dss_ALL_ENTITIES"]), .. entities.Select(entity => new DssChoice(entity.Id, entity.Name))];
        if (choices.Count == Entities.Count && choices.Zip(Entities).All(pair => pair.First == pair.Second))
        {
            return;
        }

        _suppressReload = true;
        try
        {
            Replace(Entities, choices);
            SelectedEntity = Entities.FirstOrDefault(choice => string.Equals(choice.Value, selected, StringComparison.Ordinal)) ?? Entities[0];
        }
        finally
        {
            _suppressReload = false;
        }
    }

    private void BuildBridge()
    {
        if (_snapshot is null)
        {
            return;
        }

        var steps = ShowRevenueBridge ? _snapshot.VarianceBridge.RevenueSteps : _snapshot.VarianceBridge.EbitdaSteps;
        Replace(BridgeBars, DssWaterfallBuilder.Build(steps, key => _localization["Dss_STEP_" + key.ToUpperInvariant()]));
    }

    private void Recalculate()
    {
        SimulationRows.Clear();
        var scenario = new WhatIfScenario(PricePct, CogsPct, OpexPct, DsoDaysDelta, FxShiftPct, FteDelta);
        HasSimulation = _snapshot is not null && !scenario.IsNeutral;
        if (_snapshot is null || scenario.IsNeutral)
        {
            return;
        }

        var result = WhatIfScenarioCalculator.Simulate(_snapshot.Baseline, scenario);
        var currency = _snapshot.Filter.Currency;

        SimulationRows.Add(Row("Dss_SIM_EBITDA", result.Baseline.Ebitda, result.Simulated.Ebitda, result.Delta.Ebitda, currency));
        SimulationRows.Add(Row("Dss_SIM_NET_CASH_FLOW", result.Baseline.NetCashFlow, result.Simulated.NetCashFlow, result.Delta.NetCashFlow, currency));
        SimulationRows.Add(BreakEven(result, currency));
    }

    private DssSimulationRow Row(string titleKey, double baseline, double simulated, double delta, string currency)
    {
        var culture = _localization.CurrentCulture;

        return new DssSimulationRow(
            _localization[titleKey],
            Money(baseline, currency, culture),
            Money(simulated, currency, culture),
            Signed(delta, Money(delta, currency, culture)),
            StateOf(delta, higherIsBetter: true));
    }

    private DssSimulationRow BreakEven(WhatIfResult result, string currency)
    {
        var culture = _localization.CurrentCulture;
        const string Dash = "–";
        var before = result.Baseline.BreakEvenRevenue is { } b ? Money(b, currency, culture) : Dash;
        var after = result.Simulated.BreakEvenRevenue is { } a ? Money(a, currency, culture) : Dash;
        var change = Dash;
        var state = "Neutral";
        if (result.Delta.BreakEvenRevenue is { } delta)
        {
            change = Signed(delta, Money(delta, currency, culture));

            // A lower break-even is better.
            state = StateOf(delta, higherIsBetter: false);
        }

        return new DssSimulationRow(_localization["Dss_SIM_BREAK_EVEN"], before, after, change, state);
    }

    private List<DssKpiCard> Cards(DssSnapshot snapshot, string group)
    {
        var culture = _localization.CurrentCulture;
        List<DssKpiCard> cards = [];

        foreach (var kpi in snapshot.Overview.Kpis.Where(kpi => string.Equals(kpi.Group, group, StringComparison.Ordinal)))
        {
            var value = kpi.Available && kpi.Value is { } current ? FormatKpi(current, kpi.Unit, snapshot.Filter.Currency, culture) : "–";
            double? progress = null;
            if (kpi.Target is { } target && kpi.Value is { } reached && Math.Abs(target) > Tolerance)
            {
                progress = Math.Clamp(reached / target * 100, 0, 100);
            }

            List<DssSparkPoint> trend = [.. kpi.Sparkline.Select((point, index) => new DssSparkPoint(index, point))];
            cards.Add(new DssKpiCard(
                kpi.Key,
                _localization["Dss_KPI_" + kpi.Key.ToUpperInvariant()],
                value,
                DeltaText(kpi, snapshot.Filter.Currency, culture),
                DeltaState(kpi.Favorable),
                kpi.Available,
                trend,
                progress));
        }

        return cards;
    }

    private DssAlertItem AlertOf(DssAlert alert)
    {
        var culture = _localization.CurrentCulture;
        var detail = string.Format(culture, _localization["Dss_ALERT_DETAIL"], alert.Value.ToString("0.##", culture), alert.Threshold.ToString("0.##", culture));

        return new DssAlertItem(
            alert.Severity,
            _localization["Dss_SEVERITY_" + alert.Severity.ToUpperInvariant()],
            _localization["Dss_ALERT_" + alert.Code.ToUpperInvariant()],
            string.IsNullOrEmpty(alert.Label) ? detail : $"{alert.Label} · {detail}");
    }

    private IEnumerable<DssDetailLine> DetailOf(DssMatrixRow row)
    {
        var culture = _localization.CurrentCulture;
        var currency = _snapshot?.Filter.Currency ?? DssFilter.Default.Currency;

        yield return new DssDetailLine(_localization["Dss_COL_ENTITY"], row.Label);
        yield return new DssDetailLine(_localization["Dss_COL_NET_REVENUE"], Money(row.NetRevenue, currency, culture));
        yield return new DssDetailLine(_localization["Dss_COL_GROSS_MARGIN"], Money(row.GrossMargin, currency, culture) + " (" + row.GrossMarginPct.ToString("0.#", culture) + "%)");
        yield return new DssDetailLine(_localization["Dss_COL_CM1"], Money(row.ContributionMargin1, currency, culture));
        yield return new DssDetailLine(_localization["Dss_COL_CM2"], Money(row.ContributionMargin2, currency, culture));
        yield return new DssDetailLine(_localization["Dss_COL_EBITDA"], Money(row.Ebitda, currency, culture));
        yield return new DssDetailLine(_localization["Dss_COL_NET_PROFIT"], Money(row.NetOperatingProfit, currency, culture));
        yield return new DssDetailLine(_localization["Dss_COL_MARGIN_PCT"], row.NetMarginPct.ToString("0.#", culture) + "%");
    }

    private string BoardPackLocale(string currency) => _localization.CurrentCulture.TwoLetterISOLanguageName switch
    {
        "de" => "de-CH",
        "fr" => "fr-CH",
        "it" => string.Equals(currency, "CHF", StringComparison.Ordinal) ? "it-CH" : "it-IT",
        _ => "en",
    };

    private string Format(string key, string argument) => string.Format(_localization.CurrentCulture, _localization[key], argument);
}
