// <copyright file="OrderCostingService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Bom;
using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Application.Manufacturing.Costing;

/// <summary>
/// Assembles the standard and actual cost of an order from the locally cached data, so the costing works offline.
/// </summary>
public static class OrderCostingService
{
    /// <summary>
    /// Gets the standard cost of an order: the server rollup when it was pulled, otherwise BOM explosion plus routing time at the work-centre rates.
    /// </summary>
    /// <param name="order">The order with its operations.</param>
    /// <param name="boms">The cached BOMs.</param>
    /// <param name="centers">The cached work centres by identifier.</param>
    /// <returns>The unrounded standard cost of the whole order quantity.</returns>
    public static CostBreakdown StandardFor(ProductionOrder order, IReadOnlyCollection<BomHeader> boms, IReadOnlyDictionary<Guid, WorkCenter> centers)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(boms);
        ArgumentNullException.ThrowIfNull(centers);

        CostBreakdown pulled = new(order.StandardMaterialCost, order.StandardLaborCost, order.StandardOverheadCost);
        if (pulled.Total > 0m || order.TargetQuantity <= 0m)
        {
            return pulled;
        }

        var materialPerUnit = 0m;
        if (order.BomId is { } bomId && boms.Any(b => b.Id == bomId))
        {
            materialPerUnit = BomExplosionService.Explode(boms, bomId, order.TargetQuantity).TotalCost / order.TargetQuantity;
        }

        var operations = order.Operations.Select(o =>
        {
            _ = centers.TryGetValue(o.WorkCenterId, out var center);
            return new RoutingCostInput(
                o.PlannedSetupMinutes,
                o.PlannedRunMinutes / order.TargetQuantity,
                o.PlannedLaborMinutes / order.TargetQuantity,
                center?.HourlyCostRate ?? 0m,
                center?.HourlyOverheadRate ?? 0m);
        });

        return WipCostingCalculator.CalculateStandard(order.TargetQuantity, materialPerUnit, operations);
    }

    /// <summary>
    /// Gets the actual cost of an order: the server actuals plus the postings still waiting in the outbox.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="logs">The shop-floor logs of the order.</param>
    /// <param name="centers">The cached work centres by identifier.</param>
    /// <returns>The unrounded actual cost.</returns>
    public static CostBreakdown ActualFor(ProductionOrder order, IEnumerable<ShopFloorLog> logs, IReadOnlyDictionary<Guid, WorkCenter> centers)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(logs);
        ArgumentNullException.ThrowIfNull(centers);

        var serverActual = new CostBreakdown(order.ActualMaterialCost, order.ActualLaborCost, order.ActualOverheadCost);
        var operationCenters = order.Operations.ToDictionary(o => o.Id, o => o.WorkCenterId);

        // Only unsynchronised postings are added: the synchronised ones are already part of the server actuals.
        var pending = logs
            .Where(l => l.SyncStatus == ManufacturingOutboxStatus.Pending)
            .Select(l =>
            {
                WorkCenter? center = null;
                if (operationCenters.TryGetValue(l.OperationId, out var centerId))
                {
                    _ = centers.TryGetValue(centerId, out center);
                }

                return new PostedTimeInput(l.SetupMinutes, l.RunMinutes, l.LaborMinutes, center?.HourlyCostRate ?? 0m, center?.HourlyOverheadRate ?? 0m);
            });

        return serverActual.Plus(WipCostingCalculator.CalculateActual(0m, pending));
    }
}
