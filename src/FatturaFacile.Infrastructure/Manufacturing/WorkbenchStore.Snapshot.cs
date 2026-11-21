// <copyright file="WorkbenchStore.Snapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.Manufacturing.Sync;
using FatturaFacile.Domain.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <content>
/// <see cref="IManufacturingSnapshotStore"/> implementation: checkpoints, version stamps and the upsert of the pulled rows.
/// </content>
public sealed partial class WorkbenchStore
{
    private static readonly IReadOnlyDictionary<Guid, string> NoVersions = new Dictionary<Guid, string>();

    /// <inheritdoc/>
    public async Task<DateTime?> GetCheckpointAsync(string resource, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        var checkpoint = await db.Checkpoints.AsNoTracking().FirstOrDefaultAsync(c => c.Resource == resource, cancellationToken).ConfigureAwait(false);
        return checkpoint?.LastServerTime;
    }

    /// <inheritdoc/>
    public async Task SaveCheckpointAsync(string resource, DateTime serverTime, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        var checkpoint = await db.Checkpoints.FirstOrDefaultAsync(c => c.Resource == resource, cancellationToken).ConfigureAwait(false);
        if (checkpoint is null)
        {
            _ = db.Checkpoints.Add(new ManufacturingCheckpoint { TenantId = tenant.TenantId, Resource = resource, LastServerTime = serverTime });
        }
        else
        {
            checkpoint.LastServerTime = serverTime;
            checkpoint.UpdatedAt = Now;
        }

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<Guid, string>> GetKnownVersionsAsync(string resource, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        return resource switch
        {
            ManufacturingResources.Items => await db.ItemCosts.AsNoTracking().Where(e => e.ServerVersion != null).ToDictionaryAsync(e => e.Id, e => e.ServerVersion!, cancellationToken).ConfigureAwait(false),
            ManufacturingResources.WorkCenters => await db.WorkCenters.AsNoTracking().Where(e => e.ServerVersion != null).ToDictionaryAsync(e => e.Id, e => e.ServerVersion!, cancellationToken).ConfigureAwait(false),
            ManufacturingResources.Boms => await db.BomHeaders.AsNoTracking().Where(e => e.ServerVersion != null).ToDictionaryAsync(e => e.Id, e => e.ServerVersion!, cancellationToken).ConfigureAwait(false),
            ManufacturingResources.PlannedOrders => await db.MrpPlans.AsNoTracking().Where(e => e.ServerVersion != null).ToDictionaryAsync(e => e.Id, e => e.ServerVersion!, cancellationToken).ConfigureAwait(false),
            ManufacturingResources.ProductionOrders => await db.ProductionOrders.AsNoTracking().Where(e => e.ServerVersion != null).ToDictionaryAsync(e => e.ServerId ?? e.Id, e => e.ServerVersion!, cancellationToken).ConfigureAwait(false),
            ManufacturingResources.Routings => (await db.RoutingOperations.AsNoTracking().Where(e => e.ServerVersion != null).Select(e => new { e.RoutingId, e.ServerVersion }).Distinct().ToListAsync(cancellationToken).ConfigureAwait(false))
                .GroupBy(e => e.RoutingId)
                .ToDictionary(g => g.Key, g => g.First().ServerVersion!),
            _ => NoVersions,
        };
    }

    /// <inheritdoc/>
    public async Task<IReadOnlySet<Guid>> GetProtectedOrderIdsAsync(CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        var rows = await db.ProductionOrders.AsNoTracking().Where(o => o.HasPendingChanges).Select(o => new { o.Id, o.ServerId }).ToListAsync(cancellationToken).ConfigureAwait(false);
        HashSet<Guid> ids = [];
        foreach (var row in rows)
        {
            _ = ids.Add(row.Id);
            if (row.ServerId is { } serverId)
            {
                _ = ids.Add(serverId);
            }
        }

        return ids;
    }

    /// <inheritdoc/>
    public Task<(int Applied, int Skipped)> ApplyAsync(string resource, IReadOnlyList<JsonElement> rows, IReadOnlySet<Guid> protectedOrderIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(protectedOrderIds);
        return ApplyCoreAsync(resource, rows, protectedOrderIds, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveTelemetryAsync(string payloadJson, DateTime capturedAt, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        _ = db.TelemetrySnapshots.Add(new TelemetrySnapshot { TenantId = tenant.TenantId, CapturedAt = capturedAt, PayloadJson = payloadJson });
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var stale = await db.TelemetrySnapshots.OrderByDescending(t => t.CapturedAt).Skip(MaxTelemetrySnapshots).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (stale.Count > 0)
        {
            db.TelemetrySnapshots.RemoveRange(stale);
            _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static string? Version(JsonElement row) => WorkbenchJson.StringOrNull(row, "updated_at") ?? WorkbenchJson.StringOrNull(row, "created_at");

    private static DateTime DateOnly(JsonElement row, string name, DateTime fallback) => WorkbenchJson.DateOrNull(row, name)?.Date ?? fallback;

    private static decimal Rate(decimal raw) => raw > 1m ? raw / 100m : raw;

    private async Task<(int Applied, int Skipped)> ApplyItemsAsync(ManufacturingDbContext db, IReadOnlyList<JsonElement> rows, CancellationToken cancellationToken)
    {
        var applied = 0;
        foreach (var row in rows)
        {
            if (WorkbenchJson.GuidOrNull(row, "id") is not { } id)
            {
                continue;
            }

            var item = await db.ItemCosts.FirstOrDefaultAsync(i => i.Id == id, cancellationToken).ConfigureAwait(false);
            if (item is null)
            {
                item = new ItemCost { Id = id, TenantId = tenant.TenantId };
                _ = db.ItemCosts.Add(item);
            }

            item.ItemCode = WorkbenchJson.StringOrNull(row, "item_code") ?? item.ItemCode;
            item.Name = WorkbenchJson.StringOrNull(row, "name") ?? item.Name;
            item.StandardCost = WorkbenchJson.DecimalOrZero(row, "standard_cost");
            item.ServerVersion = Version(row);
            item.UpdatedAt = Now;

            // The BOMs already cached are repriced right away.
            foreach (var line in await db.BomLines.Where(l => l.ComponentItemId == id).ToListAsync(cancellationToken).ConfigureAwait(false))
            {
                line.StandardUnitCost = item.StandardCost;
            }

            applied++;
        }

        return (applied, rows.Count - applied);
    }

    private async Task<(int Applied, int Skipped)> ApplyWorkCentersAsync(ManufacturingDbContext db, IReadOnlyList<JsonElement> rows, CancellationToken cancellationToken)
    {
        var applied = 0;
        foreach (var row in rows)
        {
            if (WorkbenchJson.GuidOrNull(row, "id") is not { } id)
            {
                continue;
            }

            var center = await db.WorkCenters.FirstOrDefaultAsync(c => c.Id == id, cancellationToken).ConfigureAwait(false);
            if (center is null)
            {
                center = new WorkCenter { Id = id, TenantId = tenant.TenantId };
                _ = db.WorkCenters.Add(center);
            }

            center.Code = WorkbenchJson.StringOrNull(row, "work_center_code") ?? center.Code;
            center.Name = WorkbenchJson.StringOrNull(row, "name") ?? center.Name;
            center.CapacityHoursPerDay = WorkbenchJson.DecimalOrZero(row, "capacity_hours_per_day");
            center.EfficiencyRate = Rate(WorkbenchJson.DecimalOrZero(row, "efficiency_rate"));
            center.AvailabilityRate = Rate(WorkbenchJson.DecimalOrZero(row, "availability_rate"));
            center.HourlyCostRate = WorkbenchJson.DecimalOrZero(row, "hourly_cost_rate");
            center.HourlyOverheadRate = WorkbenchJson.DecimalOrZero(row, "hourly_overhead_rate");
            center.Status = WorkbenchJson.StringOrNull(row, "status") ?? center.Status;
            center.ServerVersion = Version(row);
            center.UpdatedAt = Now;
            applied++;
        }

        return (applied, rows.Count - applied);
    }

    private async Task<(int Applied, int Skipped)> ApplyBomsAsync(ManufacturingDbContext db, IReadOnlyList<JsonElement> rows, CancellationToken cancellationToken)
    {
        var costs = (await db.ItemCosts.AsNoTracking().Select(i => new { i.Id, i.StandardCost }).ToListAsync(cancellationToken).ConfigureAwait(false)).ToDictionary(i => i.Id, i => i.StandardCost);
        var applied = 0;
        foreach (var row in rows)
        {
            if (WorkbenchJson.GuidOrNull(row, "id") is not { } id)
            {
                continue;
            }

            var bom = await db.BomHeaders.Include(b => b.Lines).FirstOrDefaultAsync(b => b.Id == id, cancellationToken).ConfigureAwait(false);
            if (bom is null)
            {
                bom = new BomHeader { Id = id, TenantId = tenant.TenantId };
                _ = db.BomHeaders.Add(bom);
            }

            bom.BomNumber = WorkbenchJson.StringOrNull(row, "bom_number") ?? bom.BomNumber;
            bom.ItemId = WorkbenchJson.GuidOrNull(row, "mfg_item_id") ?? bom.ItemId;
            bom.ItemCode = WorkbenchJson.StringOrNull(row, "item_code") ?? bom.ItemCode;
            bom.ItemName = WorkbenchJson.StringOrNull(row, "item_name") ?? bom.ItemName;
            bom.Revision = WorkbenchJson.StringOrNull(row, "revision") ?? bom.Revision;
            bom.Status = WorkbenchJson.StringOrNull(row, "status") ?? bom.Status;
            bom.BaseQuantity = WorkbenchJson.DecimalOrZero(row, "base_quantity") is var baseQuantity and > 0m ? baseQuantity : 1m;
            bom.Uom = WorkbenchJson.StringOrNull(row, "uom") ?? bom.Uom;
            bom.ServerVersion = Version(row);
            bom.UpdatedAt = Now;

            if (row.TryGetProperty("components", out var components) && components.ValueKind == JsonValueKind.Array)
            {
                var existingLines = bom.Lines.ToDictionary(l => l.Id);
                HashSet<Guid> seen = [];
                foreach (var component in WorkbenchJson.Items(components))
                {
                    var lineId = WorkbenchJson.GuidOrNull(component, "id") ?? Guid.NewGuid();
                    _ = seen.Add(lineId);
                    if (!existingLines.TryGetValue(lineId, out var line))
                    {
                        line = new BomLine { Id = lineId, TenantId = tenant.TenantId, BomHeaderId = id };
                        bom.Lines.Add(line);
                    }

                    var componentItemId = WorkbenchJson.GuidOrNull(component, "component_item_id") ?? Guid.Empty;
                    line.ComponentItemId = componentItemId;
                    line.ComponentItemCode = WorkbenchJson.StringOrNull(component, "component_item_code") ?? string.Empty;
                    line.Sequence = WorkbenchJson.IntOrZero(component, "sequence");
                    line.QuantityPer = WorkbenchJson.DecimalOrZero(component, "quantity_per");
                    line.ScrapPercentage = WorkbenchJson.DecimalOrZero(component, "scrap_percentage");
                    line.IsPhantom = component.TryGetProperty("is_phantom", out var phantom) && phantom.ValueKind == JsonValueKind.True;
                    line.StandardUnitCost = costs.GetValueOrDefault(componentItemId);
                }

                db.BomLines.RemoveRange(bom.Lines.Where(l => !seen.Contains(l.Id)).ToList());
            }

            applied++;
        }

        return (applied, rows.Count - applied);
    }

    private async Task<(int Applied, int Skipped)> ApplyRoutingsAsync(ManufacturingDbContext db, IReadOnlyList<JsonElement> rows, CancellationToken cancellationToken)
    {
        var applied = 0;
        foreach (var row in rows)
        {
            if (WorkbenchJson.GuidOrNull(row, "id") is not { } routingId || !row.TryGetProperty("operations", out var operations) || operations.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var existing = (await db.RoutingOperations.Where(r => r.RoutingId == routingId).ToListAsync(cancellationToken).ConfigureAwait(false)).ToDictionary(r => r.Id);
            HashSet<Guid> seen = [];
            var number = WorkbenchJson.StringOrNull(row, "routing_number") ?? string.Empty;
            var itemId = WorkbenchJson.GuidOrNull(row, "mfg_item_id") ?? Guid.Empty;
            var version = Version(row);
            foreach (var operation in WorkbenchJson.Items(operations))
            {
                var operationId = WorkbenchJson.GuidOrNull(operation, "id") ?? Guid.NewGuid();
                _ = seen.Add(operationId);
                if (!existing.TryGetValue(operationId, out var step))
                {
                    step = new RoutingOperation { Id = operationId, TenantId = tenant.TenantId };
                    _ = db.RoutingOperations.Add(step);
                }

                step.RoutingId = routingId;
                step.RoutingNumber = number;
                step.ItemId = itemId;
                step.Sequence = WorkbenchJson.IntOrZero(operation, "sequence");
                step.OperationCode = WorkbenchJson.StringOrNull(operation, "operation_code") ?? string.Empty;
                step.Name = WorkbenchJson.StringOrNull(operation, "name") ?? string.Empty;
                step.WorkCenterId = WorkbenchJson.GuidOrNull(operation, "work_center_id") ?? Guid.Empty;
                step.SetupMinutes = WorkbenchJson.DecimalOrZero(operation, "setup_time_minutes");
                step.RunMinutesPerUnit = WorkbenchJson.DecimalOrZero(operation, "run_time_minutes_per_unit");
                step.LaborMinutesPerUnit = WorkbenchJson.DecimalOrZero(operation, "labor_time_minutes_per_unit");
                step.ServerVersion = version;
            }

            db.RoutingOperations.RemoveRange(existing.Values.Where(r => !seen.Contains(r.Id)).ToList());

            applied++;
        }

        return (applied, rows.Count - applied);
    }

    private async Task<(int Applied, int Skipped)> ApplyOrdersAsync(ManufacturingDbContext db, IReadOnlyList<JsonElement> rows, IReadOnlySet<Guid> protectedOrderIds, CancellationToken cancellationToken)
    {
        var applied = 0;
        var skipped = 0;
        foreach (var row in rows)
        {
            if (WorkbenchJson.GuidOrNull(row, "id") is not { } serverId)
            {
                skipped++;
                continue;
            }

            // An order created offline keeps its local identifier; the server identifier is matched through ServerId.
            var order = await db.ProductionOrders.Include(o => o.Operations).FirstOrDefaultAsync(o => o.Id == serverId || o.ServerId == serverId, cancellationToken).ConfigureAwait(false);
            if (protectedOrderIds.Contains(serverId) || order is { HasPendingChanges: true })
            {
                skipped++;
                continue;
            }

            if (order is null)
            {
                order = new ProductionOrder { Id = serverId, TenantId = tenant.TenantId };
                _ = db.ProductionOrders.Add(order);
            }

            ApplyOrderFields(order, row, serverId);
            if (row.TryGetProperty("operations", out var operations) && operations.ValueKind == JsonValueKind.Array)
            {
                var existingOps = order.Operations.ToDictionary(o => o.Id);
                HashSet<Guid> seenOps = [];
                foreach (var operation in WorkbenchJson.Items(operations))
                {
                    var mapped = MapOperation(operation, order);
                    _ = seenOps.Add(mapped.Id);
                    if (existingOps.TryGetValue(mapped.Id, out var current))
                    {
                        db.Entry(current).CurrentValues.SetValues(mapped);
                    }
                    else
                    {
                        order.Operations.Add(mapped);
                    }
                }

                db.ProductionOperations.RemoveRange(order.Operations.Where(o => !seenOps.Contains(o.Id)).ToList());
            }

            applied++;
        }

        return (applied, skipped);
    }

    private void ApplyOrderFields(ProductionOrder order, JsonElement row, Guid serverId)
    {
        order.ServerId = serverId;
        order.OrderNumber = WorkbenchJson.StringOrNull(row, "order_number") ?? order.OrderNumber;
        order.ItemId = WorkbenchJson.GuidOrNull(row, "mfg_item_id") ?? order.ItemId;
        order.ItemCode = WorkbenchJson.StringOrNull(row, "item_code") ?? order.ItemCode;
        order.ItemName = WorkbenchJson.StringOrNull(row, "item_name") ?? order.ItemName;
        order.BomId = WorkbenchJson.GuidOrNull(row, "bom_id");
        order.RoutingId = WorkbenchJson.GuidOrNull(row, "routing_id");
        order.Status = WorkbenchJson.ParseStatus(WorkbenchJson.StringOrNull(row, "status")) ?? order.Status;
        order.TargetQuantity = WorkbenchJson.DecimalOrZero(row, "target_quantity");
        order.CompletedQuantity = WorkbenchJson.DecimalOrZero(row, "completed_quantity");
        order.ScrappedQuantity = WorkbenchJson.DecimalOrZero(row, "scrapped_quantity");
        order.Uom = WorkbenchJson.StringOrNull(row, "uom") ?? order.Uom;
        order.PlannedStartDate = DateOnly(row, "planned_start_date", order.PlannedStartDate);
        order.PlannedEndDate = DateOnly(row, "planned_end_date", order.PlannedEndDate);
        order.ActualMaterialCost = WorkbenchJson.DecimalOrZero(row, "actual_material_cost");
        order.ActualLaborCost = WorkbenchJson.DecimalOrZero(row, "actual_labor_cost");
        order.ActualOverheadCost = WorkbenchJson.DecimalOrZero(row, "actual_overhead_cost");
        order.ReceivedValue = WorkbenchJson.DecimalOrZero(row, "received_value");
        order.ServerVersion = Version(row);
        order.HasPendingChanges = false;
        order.UpdatedAt = Now;
    }

    private ProductionOperation MapOperation(JsonElement operation, ProductionOrder order) => new()
    {
        Id = WorkbenchJson.GuidOrNull(operation, "id") ?? Guid.NewGuid(),
        TenantId = tenant.TenantId,
        ProductionOrderId = order.Id,
        Sequence = WorkbenchJson.IntOrZero(operation, "sequence"),
        OperationCode = WorkbenchJson.StringOrNull(operation, "operation_code") ?? string.Empty,
        Name = WorkbenchJson.StringOrNull(operation, "name") ?? string.Empty,
        WorkCenterId = WorkbenchJson.GuidOrNull(operation, "work_center_id") ?? Guid.Empty,
        PlannedSetupMinutes = WorkbenchJson.DecimalOrZero(operation, "planned_setup_minutes"),
        PlannedRunMinutes = WorkbenchJson.DecimalOrZero(operation, "planned_run_minutes"),
        PlannedLaborMinutes = WorkbenchJson.DecimalOrZero(operation, "planned_labor_minutes"),
        ConfirmedGoodQuantity = WorkbenchJson.DecimalOrZero(operation, "good_quantity"),
        ConfirmedScrapQuantity = WorkbenchJson.DecimalOrZero(operation, "scrap_quantity"),
        ActualMinutes = WorkbenchJson.DecimalOrZero(operation, "actual_setup_minutes") + WorkbenchJson.DecimalOrZero(operation, "actual_run_minutes"),
    };

    private async Task<(int Applied, int Skipped)> ApplyPlansAsync(ManufacturingDbContext db, IReadOnlyList<JsonElement> rows, CancellationToken cancellationToken)
    {
        var applied = 0;
        foreach (var row in rows)
        {
            if (WorkbenchJson.GuidOrNull(row, "id") is not { } id)
            {
                continue;
            }

            var plan = await db.MrpPlans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
            if (plan is null)
            {
                plan = new MrpPlan { Id = id, TenantId = tenant.TenantId };
                _ = db.MrpPlans.Add(plan);
            }

            plan.PlannedOrderNumber = WorkbenchJson.StringOrNull(row, "planned_order_number") ?? plan.PlannedOrderNumber;
            plan.MrpRunId = WorkbenchJson.GuidOrNull(row, "mrp_run_id") ?? plan.MrpRunId;
            plan.ItemId = WorkbenchJson.GuidOrNull(row, "mfg_item_id") ?? plan.ItemId;
            plan.ItemCode = WorkbenchJson.StringOrNull(row, "item_code") ?? plan.ItemCode;
            plan.ItemName = WorkbenchJson.StringOrNull(row, "item_name") ?? plan.ItemName;
            plan.Quantity = WorkbenchJson.DecimalOrZero(row, "quantity");
            plan.StartDate = DateOnly(row, "start_date", plan.StartDate);
            plan.DueDate = DateOnly(row, "due_date", plan.DueDate);
            plan.Status = WorkbenchJson.StringOrNull(row, "status") ?? plan.Status;
            plan.BomId = WorkbenchJson.GuidOrNull(row, "bom_id");
            plan.RoutingId = WorkbenchJson.GuidOrNull(row, "routing_id");
            plan.ServerVersion = Version(row);
            plan.UpdatedAt = Now;
            applied++;
        }

        return (applied, rows.Count - applied);
    }

    private async Task<(int Applied, int Skipped)> ApplyCoreAsync(string resource, IReadOnlyList<JsonElement> rows, IReadOnlySet<Guid> protectedOrderIds, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        var counts = resource switch
        {
            ManufacturingResources.Items => await ApplyItemsAsync(db, rows, cancellationToken).ConfigureAwait(false),
            ManufacturingResources.WorkCenters => await ApplyWorkCentersAsync(db, rows, cancellationToken).ConfigureAwait(false),
            ManufacturingResources.Boms => await ApplyBomsAsync(db, rows, cancellationToken).ConfigureAwait(false),
            ManufacturingResources.Routings => await ApplyRoutingsAsync(db, rows, cancellationToken).ConfigureAwait(false),
            ManufacturingResources.ProductionOrders => await ApplyOrdersAsync(db, rows, protectedOrderIds, cancellationToken).ConfigureAwait(false),
            ManufacturingResources.PlannedOrders => await ApplyPlansAsync(db, rows, cancellationToken).ConfigureAwait(false),
            _ => (Applied: 0, Skipped: rows.Count),
        };

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return counts;
    }
}
