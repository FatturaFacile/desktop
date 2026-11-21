// <copyright file="WorkbenchViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Manufacturing.Bom;
using FatturaFacile.Application.Manufacturing.Features;
using FatturaFacile.Application.Manufacturing.Scheduling;
using FatturaFacile.Domain.Manufacturing;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The offline-first Engineering and Shop-Floor Supervisor workbench: cockpit, orders and shop-floor postings,
/// multi-level BOM costing, local finite-capacity Gantt and the sync queue. Everything is read from the local store; mutations are queued.
/// </summary>
internal sealed partial class WorkbenchViewModel : PayrollSectionViewModel
{
    private readonly INetworkStatus _network;

    [ObservableProperty]
    private ProductionOrder? _selectedOrder;

    [ObservableProperty]
    private ProductionOperation? _selectedOperation;

    [ObservableProperty]
    private BomHeader? _selectedBom;

    [ObservableProperty]
    private decimal _explosionQuantity = 1m;

    [ObservableProperty]
    private decimal _explosionTotal;

    [ObservableProperty]
    private string _wipSummary = string.Empty;

    [ObservableProperty]
    private string _cockpitUpdated = string.Empty;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private decimal _setupMinutes;

    [ObservableProperty]
    private decimal _runMinutes;

    [ObservableProperty]
    private decimal _laborMinutes;

    [ObservableProperty]
    private decimal _downtimeMinutes;

    [ObservableProperty]
    private decimal _goodQuantity;

    [ObservableProperty]
    private decimal _scrapQuantity;

    [ObservableProperty]
    private string _scrapReason = string.Empty;

    [ObservableProperty]
    private bool _isFinal;

    [ObservableProperty]
    private decimal _newOrderQuantity = 1m;

    [ObservableProperty]
    private DateTime _newOrderStart = DateTime.Today;

    [ObservableProperty]
    private DateTime _newOrderEnd = DateTime.Today.AddDays(7);

    private IReadOnlyList<RoutingOperation> _routing = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkbenchViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="localizationService">The localization service.</param>
    /// <param name="network">The network status.</param>
    public WorkbenchViewModel(ISender sender, ILocalizationService localizationService, INetworkStatus network)
        : base(sender, localizationService)
    {
        ArgumentNullException.ThrowIfNull(network);
        _network = network;
        _isOffline = !network.IsOnline;
        network.Changed += (_, _) => IsOffline = !_network.IsOnline;
    }

    /// <summary>
    /// Gets the production orders.
    /// </summary>
    public ObservableCollection<ProductionOrder> Orders { get; } = [];

    /// <summary>
    /// Gets the operations of the selected order.
    /// </summary>
    public ObservableCollection<ProductionOperation> Operations { get; } = [];

    /// <summary>
    /// Gets the BOMs.
    /// </summary>
    public ObservableCollection<BomHeader> Boms { get; } = [];

    /// <summary>
    /// Gets the rows of the multi-level BOM explosion.
    /// </summary>
    public ObservableCollection<BomExplosionRow> ExplosionRows { get; } = [];

    /// <summary>
    /// Gets the WIP costing rows of the selected order.
    /// </summary>
    public ObservableCollection<CostLineRow> CostLines { get; } = [];

    /// <summary>
    /// Gets the Gantt rows (orders with their operations).
    /// </summary>
    public ObservableCollection<GanttTaskRow> GanttTasks { get; } = [];

    /// <summary>
    /// Gets the flattened cached cockpit metrics.
    /// </summary>
    public ObservableCollection<CockpitMetricRow> CockpitRows { get; } = [];

    /// <summary>
    /// Gets the outbox entries.
    /// </summary>
    public ObservableCollection<ManufacturingOutboxEntry> Outbox { get; } = [];

    /// <summary>
    /// Gets the MRP planned orders.
    /// </summary>
    public ObservableCollection<MrpPlan> Plans { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var selectedId = SelectedOrder?.Id;
        var selectedBomId = SelectedBom?.Id;
        var workbench = await Sender.Send(new GetManufacturingWorkbenchQuery());

        _routing = workbench.RoutingOperations;
        Replace(Orders, workbench.Orders);
        Replace(Boms, workbench.Boms);
        Replace(Outbox, workbench.Outbox);
        Replace(Plans, workbench.Plans);
        Replace(CockpitRows, CockpitMetricRow.Flatten(workbench.Telemetry?.PayloadJson));
        CockpitUpdated = workbench.Telemetry is { } telemetry
            ? string.Format(CultureInfo.CurrentCulture, Localization["Wb_CockpitUpdated"], telemetry.CapturedAt.ToLocalTime().ToString("g", CultureInfo.CurrentCulture))
            : Localization["Wb_NoCockpit"];

        SelectedOrder = Orders.FirstOrDefault(o => o.Id == selectedId) ?? Orders.FirstOrDefault();
        SelectedBom = Boms.FirstOrDefault(b => b.Id == selectedBomId) ?? Boms.FirstOrDefault();
        await RebuildScheduleAsync();
    });

    /// <summary>
    /// Loads the workbench (used by the shell navigation).
    /// </summary>
    /// <returns>A task representing the load.</returns>
    public Task LoadWorkbenchAsync() => LoadAsync();

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private static double Progress(ProductionOrder? order, ScheduledTask task)
    {
        var operation = order?.Operations.FirstOrDefault(o => o.Id == task.OperationId);
        return operation is null || order is null || order.TargetQuantity <= 0m
            ? 0d
            : (double)Math.Min(100m, operation.ConfirmedGoodQuantity / order.TargetQuantity * 100m);
    }

    partial void OnSelectedOrderChanged(ProductionOrder? value)
    {
        Replace(Operations, value?.Operations ?? []);
        SelectedOperation = Operations.FirstOrDefault();
        _ = RefreshCostingAsync(value);
    }

    partial void OnSelectedBomChanged(BomHeader? value) => _ = ExplodeAsync(value);

    partial void OnExplosionQuantityChanged(decimal value) => _ = ExplodeAsync(SelectedBom);

    private async Task ExplodeAsync(BomHeader? bom)
    {
        ExplosionRows.Clear();
        ExplosionTotal = 0m;
        if (bom is null || ExplosionQuantity <= 0m)
        {
            return;
        }

        try
        {
            var result = await Sender.Send(new ExplodeBomQuery(bom.Id, ExplosionQuantity));
            Replace(ExplosionRows, result.Rows);
            ExplosionTotal = result.TotalCost;
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task RefreshCostingAsync(ProductionOrder? order)
    {
        CostLines.Clear();
        WipSummary = string.Empty;
        if (order is null || order.TargetQuantity <= 0m)
        {
            return;
        }

        try
        {
            var cost = await Sender.Send(new GetOrderCostingQuery(order.Id));
            CostLines.Add(new CostLineRow(Localization["Wb_Material"], cost.Standard.Material, cost.StandardForOutput.Material, cost.Actual.Material, cost.Variance.Material));
            CostLines.Add(new CostLineRow(Localization["Wb_Labor"], cost.Standard.Labor, cost.StandardForOutput.Labor, cost.Actual.Labor, cost.Variance.Labor));
            CostLines.Add(new CostLineRow(Localization["Wb_Overhead"], cost.Standard.Overhead, cost.StandardForOutput.Overhead, cost.Actual.Overhead, cost.Variance.Overhead));
            CostLines.Add(new CostLineRow(Localization["Wb_Total"], cost.Standard.Total, cost.StandardForOutput.Total, cost.Actual.Total, cost.Variance.Total));
            WipSummary = string.Format(CultureInfo.CurrentCulture, "{0}: {1:N2}", Localization["Wb_Wip"], cost.WipBalance)
                + (cost.IsVarianceFinal ? string.Empty : " - " + Localization["Wb_VarianceProvisional"]);
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task RebuildScheduleAsync()
    {
        var schedule = await Sender.Send(new BuildScheduleQuery(DateTime.Today));
        GanttTasks.Clear();
        foreach (var group in schedule.Tasks.GroupBy(t => t.OrderId))
        {
            var order = Orders.FirstOrDefault(o => o.Id == group.Key);
            GanttTaskRow summary = new()
            {
                TaskName = group.First().OrderNumber,
                StartDate = group.Min(t => t.Start),
                FinishDate = group.Max(t => t.End),
                Progress = order is { TargetQuantity: > 0m } ? (double)Math.Min(100m, order.CompletedQuantity / order.TargetQuantity * 100m) : 0d,
            };

            foreach (var task in group.OrderBy(t => t.Sequence))
            {
                summary.Children.Add(new GanttTaskRow
                {
                    TaskName = string.Format(CultureInfo.InvariantCulture, "{0} {1} [{2}]", task.Sequence, task.OperationName, task.WorkCenterCode),
                    StartDate = task.Start,
                    FinishDate = task.End > task.Start ? task.End : task.Start.AddMinutes(1),
                    Progress = Progress(order, task),
                });
            }

            GanttTasks.Add(summary);
        }
    }

}
