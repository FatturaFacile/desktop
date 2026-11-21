// <copyright file="WorkbenchStore.Read.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Features;
using FatturaFacile.Domain.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <content>
/// <see cref="Application.Manufacturing.IManufacturingReadStore"/> implementation.
/// </content>
public sealed partial class WorkbenchStore
{
    /// <inheritdoc/>
    public async Task<ManufacturingWorkbench> GetWorkbenchAsync(CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);

        var centers = await db.WorkCenters.AsNoTracking().OrderBy(c => c.Code).ToListAsync(cancellationToken).ConfigureAwait(false);
        var boms = await db.BomHeaders.AsNoTracking().Include(b => b.Lines).OrderBy(b => b.BomNumber).ToListAsync(cancellationToken).ConfigureAwait(false);
        var routing = await db.RoutingOperations.AsNoTracking().OrderBy(r => r.RoutingNumber).ThenBy(r => r.Sequence).ToListAsync(cancellationToken).ConfigureAwait(false);
        var orders = await db.ProductionOrders.AsNoTracking().Include(o => o.Operations).OrderBy(o => o.PlannedStartDate).ThenBy(o => o.OrderNumber).ToListAsync(cancellationToken).ConfigureAwait(false);
        var plans = await db.MrpPlans.AsNoTracking().OrderBy(p => p.DueDate).ToListAsync(cancellationToken).ConfigureAwait(false);
        var logs = await db.ShopFloorLogs.AsNoTracking().OrderByDescending(l => l.CreatedAt).Take(MaxLogs).ToListAsync(cancellationToken).ConfigureAwait(false);
        var outbox = await db.Outbox.AsNoTracking().OrderByDescending(e => e.Sequence).Take(MaxOutbox).ToListAsync(cancellationToken).ConfigureAwait(false);
        var telemetry = await db.TelemetrySnapshots.AsNoTracking().OrderByDescending(t => t.CapturedAt).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        foreach (var order in orders)
        {
            order.Operations = [.. order.Operations.OrderBy(o => o.Sequence)];
        }

        foreach (var bom in boms)
        {
            bom.Lines = [.. bom.Lines.OrderBy(l => l.Sequence)];
        }

        return new ManufacturingWorkbench(centers, boms, routing, orders, plans, logs, outbox, telemetry);
    }

    /// <inheritdoc/>
    public async Task<ProductionOrder?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        var order = await db.ProductionOrders.AsNoTracking().Include(o => o.Operations).FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken).ConfigureAwait(false);
        order?.Operations = [.. order.Operations.OrderBy(o => o.Sequence)];
        return order;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ShopFloorLog>> GetLogsAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        return await db.ShopFloorLogs.AsNoTracking().Where(l => l.ProductionOrderId == orderId).OrderBy(l => l.CreatedAt).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RoutingOperation>> GetRoutingOperationsAsync(Guid routingId, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        return await db.RoutingOperations.AsNoTracking().Where(r => r.RoutingId == routingId).OrderBy(r => r.Sequence).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
