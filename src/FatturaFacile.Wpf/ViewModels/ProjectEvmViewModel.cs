// <copyright file="ProjectEvmViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.ProjectEvm;
using FatturaFacile.Application.ProjectEvm;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the project profitability and earned value view (Mod 082): the CPI/SPI traffic lights, EAC/ETC, margin and budget
/// absorption, the S-curve, the profitability waterfall, the WBS cost table and the reconciliation with accounting. It reads the
/// local cache first, so it shows the last synced figures offline; the sync pulls fresh ones when the server is reachable.
/// </summary>
internal sealed partial class ProjectEvmViewModel : ObservableObject
{
    private const double PercentScale = 100.0;
    private const string NumberFormat = "N2";
    private const string DateTimeFormat = "g";
    private const string NoValue = "-";
    private const string Green = "Green";

    private static readonly Dictionary<ProjectEvmSyncStatus, string> StatusKeys = new()
    {
        [ProjectEvmSyncStatus.Synced] = "ProjectEvm_Synced",
        [ProjectEvmSyncStatus.Offline] = "ProjectEvm_OfflineUsingCache",
        [ProjectEvmSyncStatus.Unauthorized] = "ProjectEvm_SignInRequired",
        [ProjectEvmSyncStatus.Forbidden] = "ProjectEvm_Forbidden",
    };

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;

    private bool _suppressSelection;

    [ObservableProperty]
    private ProjectEvmProject? _selectedProject;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasSnapshot;

    [ObservableProperty]
    private bool _hasFigures;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _bacText = NoValue;

    [ObservableProperty]
    private string _cpiText = NoValue;

    [ObservableProperty]
    private string _cpiLight = Green;

    [ObservableProperty]
    private string _spiText = NoValue;

    [ObservableProperty]
    private string _spiLight = Green;

    [ObservableProperty]
    private string _eacText = NoValue;

    [ObservableProperty]
    private string _vacText = NoValue;

    [ObservableProperty]
    private string _marginText = NoValue;

    [ObservableProperty]
    private double _absorptionPercent;

    [ObservableProperty]
    private string _absorptionText = NoValue;

    [ObservableProperty]
    private bool _absorptionWarning;

    [ObservableProperty]
    private bool _hasManualEtc;

    [ObservableProperty]
    private string _alertText = string.Empty;

    [ObservableProperty]
    private bool _hasBreaches;

    [ObservableProperty]
    private bool _hasReconciliation;

    [ObservableProperty]
    private string _reconciliationText = string.Empty;

    [ObservableProperty]
    private string _reconciliationState = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectEvmViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public ProjectEvmViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _localization = localization;
        LastSyncedText = _localization["ProjectEvm_NeverSynced"];
    }

    /// <summary>
    /// Gets the projects of the picker.
    /// </summary>
    public ObservableCollection<ProjectEvmProject> Projects { get; } = [];

    /// <summary>
    /// Gets the cumulative PV, EV and AC of the S-curve.
    /// </summary>
    public ObservableCollection<ProjectEvmScurvePoint> ScurvePoints { get; } = [];

    /// <summary>
    /// Gets the EAC projection of the S-curve.
    /// </summary>
    public ObservableCollection<ProjectEvmEacPoint> ForecastPoints { get; } = [];

    /// <summary>
    /// Gets the WBS cost table: the project total first, then the nodes in tree order.
    /// </summary>
    internal ObservableCollection<ProjectEvmRow> Rows { get; } = [];

    /// <summary>
    /// Gets the steps of the profitability waterfall.
    /// </summary>
    internal ObservableCollection<ProjectEvmStepRow> WaterfallSteps { get; } = [];

    /// <summary>
    /// Loads the project picker and the snapshot of the selected project from the cache, pulling from the server when the cache
    /// is empty.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            if (!await ShowProjectsAsync())
            {
                await PullAsync(project: null);
                _ = await ShowProjectsAsync();
            }

            await ShowCachedAsync(syncIfMissing: true);
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
    /// Pulls fresh data from the server and shows it; offline, the cached snapshot stays on screen.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public async Task SyncAsync()
    {
        IsBusy = true;

        try
        {
            await PullAsync(project: null);
            _ = await ShowProjectsAsync();

            if (SelectedProject is { } selected)
            {
                await PullAsync(selected.Id);
            }

            await ShowCachedAsync(syncIfMissing: false);
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

    private static ProjectEvmRow EmptyRow(string code, string title, int depth) =>
        new(code, title, depth, HasFigures: false, NoValue, NoValue, NoValue, NoValue, NoValue, NoValue, NoValue, NoValue, NoValue, NoValue, NoValue, NoValue, NoValue, NoValue, Green, Green);

    private static string Light(ProjectEvmLight value) => value.ToString();

    private static string ReconciliationKey(ProjectEvmReconciliation reconciliation) => (reconciliation.HasValuation, reconciliation.IsReconciled) switch
    {
        (false, _) => "ProjectEvm_ReconNone",
        (true, true) => "ProjectEvm_ReconOk",
        (true, false) => "ProjectEvm_ReconGap",
    };

    partial void OnSelectedProjectChanged(ProjectEvmProject? value)
    {
        if (_suppressSelection || value is null)
        {
            return;
        }

        _ = ReloadSelectedAsync();
    }

    private async Task ReloadSelectedAsync()
    {
        IsBusy = true;

        try
        {
            await ShowCachedAsync(syncIfMissing: true);
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

    private async Task<bool> ShowProjectsAsync()
    {
        var projects = await _sender.Send(new GetProjectEvmProjectsQuery());
        var keep = SelectedProject?.Id;

        _suppressSelection = true;
        try
        {
            Projects.Clear();
            foreach (var project in projects)
            {
                Projects.Add(project);
            }

            SelectedProject = Projects.FirstOrDefault(p => string.Equals(p.Id, keep, StringComparison.Ordinal)) ?? Projects.FirstOrDefault();
        }
        finally
        {
            _suppressSelection = false;
        }

        return projects.Count > 0;
    }

    private async Task PullAsync(string? project)
    {
        var result = await _sender.Send(new SyncProjectEvmSnapshotCommand(project));
        IsOffline = result.Status != ProjectEvmSyncStatus.Synced;
        StatusMessage = result.Status == ProjectEvmSyncStatus.Failed
            ? Format("ProjectEvm_SyncFailed", result.Detail ?? string.Empty)
            : _localization[StatusKeys.GetValueOrDefault(result.Status, "ProjectEvm_Synced")];
    }

    private async Task ShowCachedAsync(bool syncIfMissing)
    {
        if (SelectedProject is not { } project)
        {
            Apply(snapshot: null);

            return;
        }

        var snapshot = await _sender.Send(new GetProjectEvmSnapshotQuery(project.Id));
        if (snapshot is null && syncIfMissing)
        {
            await PullAsync(project.Id);
            snapshot = await _sender.Send(new GetProjectEvmSnapshotQuery(project.Id));
        }

        Apply(snapshot);
    }

    private void Apply(ProjectEvmSnapshot? snapshot)
    {
        HasSnapshot = snapshot is not null;
        Rows.Clear();
        WaterfallSteps.Clear();
        ScurvePoints.Clear();
        ForecastPoints.Clear();

        if (snapshot is null)
        {
            ShowFigures(figures: null, string.Empty);
            HasReconciliation = false;
            LastSyncedText = _localization["ProjectEvm_NeverSynced"];
            if (!IsOffline && StatusMessage.Length == 0)
            {
                StatusMessage = _localization[Projects.Count == 0 ? "ProjectEvm_NoProjects" : "ProjectEvm_NoSnapshot"];
            }

            return;
        }

        var culture = _localization.CurrentCulture;
        LastSyncedText = Format("ProjectEvm_LastSynced", snapshot.SyncedAtUtc.ToLocalTime().ToString(DateTimeFormat, culture));
        ShowFigures(snapshot.Project, snapshot.Currency);

        if (snapshot.Project is { } total)
        {
            Rows.Add(RowOf(_localization["ProjectEvm_ProjectTotal"], string.Empty, 0, total, snapshot.Currency));
        }

        AddNodes(snapshot.Nodes, 0, snapshot.Currency);
        BuildWaterfall(snapshot);

        foreach (var point in snapshot.Points)
        {
            ScurvePoints.Add(point);
        }

        foreach (var point in snapshot.Forecast)
        {
            ForecastPoints.Add(point);
        }

        ShowReconciliation(snapshot.Reconciliation, snapshot.Currency);
    }

    private void ShowFigures(ProjectEvmFigures? figures, string currency)
    {
        HasFigures = figures is not null;
        BacText = figures is null ? NoValue : Money(figures.Bac, currency);
        CpiText = figures is null ? NoValue : Index(figures.Cpi);
        CpiLight = figures is null ? Green : Light(figures.CpiLight);
        SpiText = figures is null ? NoValue : Index(figures.Spi);
        SpiLight = figures is null ? Green : Light(figures.SpiLight);
        EacText = figures is null ? NoValue : Money(figures.Eac, currency);
        VacText = figures is null ? NoValue : Money(figures.Vac, currency);
        MarginText = figures is null ? NoValue : Money(figures.GrossMargin, currency) + " (" + Percent(figures.GrossMarginPercent) + ")";
        AbsorptionPercent = Math.Clamp(figures?.AbsorptionPercent ?? 0, 0, PercentScale);
        AbsorptionText = figures is null ? NoValue : Percent(figures.AbsorptionPercent);
        AbsorptionWarning = figures?.AbsorptionWarning ?? false;
        HasManualEtc = figures?.ManualEtc is not null;
        HasBreaches = figures?.Breaches > 0;
        AlertText = AlertOf(figures);
    }

    private string AlertOf(ProjectEvmFigures? figures) => figures?.Breaches switch
    {
        null => string.Empty,
        0 => _localization["ProjectEvm_AlertsOk"],
        { } count => Format("ProjectEvm_AlertsBreached", count.ToString(_localization.CurrentCulture)),
    };

    private void AddNodes(IReadOnlyList<ProjectEvmNode> nodes, int depth, string currency)
    {
        foreach (var node in nodes)
        {
            Rows.Add(RowOf(node.Code, node.Title, depth, node.Figures, currency));
            AddNodes(node.Children, depth + 1, currency);
        }
    }

    private ProjectEvmRow RowOf(string code, string title, int depth, ProjectEvmFigures? f, string currency) =>
        f is null ? EmptyRow(code, title, depth) : FullRow(code, title, depth, f, currency);

    private ProjectEvmRow FullRow(string code, string title, int depth, ProjectEvmFigures f, string currency)
    {
        return new ProjectEvmRow(
            code,
            title,
            depth,
            HasFigures: true,
            Money(f.Bac, currency),
            Money(f.Committed, currency),
            Money(f.Actual, currency),
            Money(f.Exposure, currency),
            Money(f.Ev, currency),
            Money(f.Pv, currency),
            Money(f.Cv, currency),
            Money(f.Sv, currency),
            Index(f.Cpi),
            Index(f.Spi),
            Money(f.Eac, currency),
            Money(f.Etc, currency) + (f.ManualEtc is null ? string.Empty : " *"),
            Money(f.Vac, currency),
            Money(f.GrossMargin, currency) + " (" + Percent(f.GrossMarginPercent) + ")",
            Light(f.CpiLight),
            Light(f.SpiLight));
    }

    private void BuildWaterfall(ProjectEvmSnapshot snapshot)
    {
        var largest = snapshot.Waterfall.Count == 0 ? 1.0 : Math.Max(1.0, snapshot.Waterfall.Max(step => Math.Abs(step.Amount)));
        foreach (var step in snapshot.Waterfall)
        {
            WaterfallSteps.Add(new ProjectEvmStepRow(StepLabel(step.Key), Money(step.Amount, snapshot.Currency), Math.Abs(step.Amount) / largest * PercentScale, step.IsTotal));
        }
    }

    private string StepLabel(string key) => key switch
    {
        "revenue" => _localization["ProjectEvm_Step_Revenue"],
        "gross_margin" => _localization["ProjectEvm_Step_GrossMargin"],
        "TimesheetLabor" => _localization["ProjectEvm_Src_TimesheetLabor"],
        "EmployeeExpense" => _localization["ProjectEvm_Src_EmployeeExpense"],
        "VendorInvoice" => _localization["ProjectEvm_Src_VendorInvoice"],
        "WarehouseMaterialIssue" => _localization["ProjectEvm_Src_WarehouseMaterialIssue"],
        "OverheadAllocation" => _localization["ProjectEvm_Src_OverheadAllocation"],
        "Adjustment" => _localization["ProjectEvm_Src_Adjustment"],
        _ => key,
    };

    private void ShowReconciliation(ProjectEvmReconciliation? reconciliation, string currency)
    {
        HasReconciliation = reconciliation is not null;
        if (reconciliation is null)
        {
            ReconciliationText = string.Empty;
            ReconciliationState = string.Empty;

            return;
        }

        ReconciliationText = Format(
            "ProjectEvm_ReconciliationLine",
            reconciliation.Standard ?? NoValue,
            Money(reconciliation.ManagerialCost, currency),
            Money(reconciliation.StatutoryCost, currency),
            reconciliation.Difference is { } difference ? Money(difference, currency) : NoValue,
            Money(reconciliation.Overhead, currency));
        ReconciliationState = _localization[ReconciliationKey(reconciliation)];
    }

    private string Money(double value, string currency) => currency + " " + value.ToString(NumberFormat, _localization.CurrentCulture);

    private string Index(double? value) => value is { } v ? v.ToString(NumberFormat, _localization.CurrentCulture) : NoValue;

    private string Percent(double? value) => value is { } v ? v.ToString(NumberFormat, _localization.CurrentCulture) + "%" : NoValue;

    private string Format(string key, params string[] arguments) => string.Format(_localization.CurrentCulture, _localization[key], arguments);
}
