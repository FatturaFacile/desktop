// <copyright file="WorkbenchStore.Outbox.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;
using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Application.Manufacturing.Sync;
using FatturaFacile.Domain.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <content>
/// <see cref="IManufacturingOutboxStore"/> implementation.
/// </content>
public sealed partial class WorkbenchStore
{
    private const string OrderPlaceholder = "{order}";

    /// <inheritdoc/>
    public Task QueueNewOrderAsync(ProductionOrder order, ManufacturingOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(entry);
        return QueueNewOrderCoreAsync(order, entry, cancellationToken);
    }

    /// <inheritdoc/>
    public Task QueueAsync(ManufacturingOutboxEntry entry, ShopFloorLog? log, OptimisticOrderChange change, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(change);
        return QueueCoreAsync(entry, log, change, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ManufacturingOutboxEntry>> GetPendingAsync(CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        return await db.Outbox.AsNoTracking().Where(e => e.Status == ManufacturingOutboxStatus.Pending).OrderBy(e => e.Sequence).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlySet<Guid>> GetBlockedOrderIdsAsync(CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        var ids = await db.Outbox.AsNoTracking()
            .Where(e => e.Status == ManufacturingOutboxStatus.Conflict || e.Status == ManufacturingOutboxStatus.Failed)
            .Select(e => e.ProductionOrderId)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return ids.ToHashSet();
    }

    /// <inheritdoc/>
    public Task SaveAsync(ManufacturingOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return SaveCoreAsync(entry, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ResolvedRoute?> ResolveRouteAsync(ManufacturingOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return ResolveRouteCoreAsync(entry, cancellationToken);
    }

    /// <inheritdoc/>
    public Task ApplyOutcomeAsync(ManufacturingOutboxEntry entry, string? responseBody, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return ApplyOutcomeCoreAsync(entry, responseBody, cancellationToken);
    }

    private static void ApplyChange(ProductionOrder order, OptimisticOrderChange change, int sign)
    {
        if (change.NewStatus is { } status)
        {
            order.Status = status;
        }

        if (change.OperationId is { } operationId)
        {
            var operation = order.Operations.FirstOrDefault(o => o.Id == operationId);
            if (operation is not null)
            {
                operation.ConfirmedGoodQuantity = Math.Max(0m, operation.ConfirmedGoodQuantity + (sign * change.GoodDelta));
                operation.ConfirmedScrapQuantity = Math.Max(0m, operation.ConfirmedScrapQuantity + (sign * change.ScrapDelta));
                operation.ActualMinutes = Math.Max(0m, operation.ActualMinutes + (sign * change.MinutesDelta));
            }
        }

        if (change.CountsAsOutput)
        {
            order.CompletedQuantity = Math.Max(0m, order.CompletedQuantity + (sign * change.GoodDelta));
        }

        order.ScrappedQuantity = Math.Max(0m, order.ScrappedQuantity + (sign * change.ScrapDelta));
    }

    private static async Task<long> NextSequenceAsync(ManufacturingDbContext db, CancellationToken cancellationToken)
    {
        var max = await db.Outbox.IgnoreQueryFilters().MaxAsync(e => (long?)e.Sequence, cancellationToken).ConfigureAwait(false);
        return (max ?? 0L) + 1L;
    }

    private static async Task AdoptCreatedOrderAsync(ManufacturingDbContext db, ManufacturingOutboxEntry entry, string? responseBody, CancellationToken cancellationToken)
    {
        var order = await db.ProductionOrders.FirstOrDefaultAsync(o => o.Id == entry.ProductionOrderId, cancellationToken).ConfigureAwait(false);
        if (order is null || string.IsNullOrWhiteSpace(responseBody))
        {
            return;
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;
        var data = root.TryGetProperty("data", out var inner) && inner.ValueKind == JsonValueKind.Object ? inner : root;

        order.ServerId = WorkbenchJson.GuidOrNull(data, "id") ?? order.ServerId;
        order.OrderNumber = WorkbenchJson.StringOrNull(data, "order_number") ?? order.OrderNumber;
        order.ServerVersion = WorkbenchJson.StringOrNull(data, "updated_at") ?? order.ServerVersion;
    }

    private async Task CompleteConfirmationAsync(ManufacturingDbContext db, ManufacturingOutboxEntry entry, string? responseBody, CancellationToken cancellationToken)
    {
        var log = await db.ShopFloorLogs.FirstOrDefaultAsync(l => l.OperationPostingUuid == entry.IdempotencyKey, cancellationToken).ConfigureAwait(false);
        if (log is null)
        {
            return;
        }

        if (entry.Status == ManufacturingOutboxStatus.Synced && !string.IsNullOrWhiteSpace(responseBody))
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            var data = root.TryGetProperty("data", out var inner) && inner.ValueKind == JsonValueKind.Object ? inner : root;
            log.ServerConfirmationNumber = WorkbenchJson.StringOrNull(data, "confirmation_number");
        }
        else if (entry.Status == ManufacturingOutboxStatus.Discarded)
        {
            // The server state won: take the optimistic effect of the dropped posting back out of the local order.
            var order = await db.ProductionOrders.Include(o => o.Operations).FirstOrDefaultAsync(o => o.Id == log.ProductionOrderId, cancellationToken).ConfigureAwait(false);
            if (order is not null)
            {
                var isLast = order.Operations.Count > 0 && order.Operations.Any(o => o.Id == log.OperationId) && order.Operations.First(o => o.Id == log.OperationId).Sequence >= order.Operations.Max(o => o.Sequence);
                ApplyChange(order, new OptimisticOrderChange(order.Id, NewStatus: null, log.OperationId, log.GoodQuantity, log.ScrapQuantity, log.SetupMinutes + log.RunMinutes, isLast), sign: -1);
                order.UpdatedAt = Now;
            }
        }

        log.SyncStatus = entry.Status;
        log.UpdatedAt = Now;
    }

    private async Task QueueNewOrderCoreAsync(ProductionOrder order, ManufacturingOutboxEntry entry, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        order.TenantId = tenant.TenantId;
        foreach (var operation in order.Operations)
        {
            operation.TenantId = tenant.TenantId;
        }

        entry.TenantId = tenant.TenantId;
        entry.Sequence = await NextSequenceAsync(db, cancellationToken).ConfigureAwait(false);
        _ = db.ProductionOrders.Add(order);
        _ = db.Outbox.Add(entry);
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task QueueCoreAsync(ManufacturingOutboxEntry entry, ShopFloorLog? log, OptimisticOrderChange change, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        var order = await db.ProductionOrders.Include(o => o.Operations).FirstOrDefaultAsync(o => o.Id == change.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Production order {change.OrderId} is not in the local store.");

        ApplyChange(order, change, sign: 1);
        order.HasPendingChanges = true;
        order.UpdatedAt = Now;

        entry.TenantId = tenant.TenantId;
        entry.Sequence = await NextSequenceAsync(db, cancellationToken).ConfigureAwait(false);
        _ = db.Outbox.Add(entry);
        if (log is not null)
        {
            log.TenantId = tenant.TenantId;
            _ = db.ShopFloorLogs.Add(log);
        }

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SaveCoreAsync(ManufacturingOutboxEntry entry, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        var stored = await db.Outbox.FirstOrDefaultAsync(e => e.Id == entry.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Outbox entry {entry.Id} is not in the local store.");

        db.Entry(stored).CurrentValues.SetValues(entry);
        stored.UpdatedAt = Now;

        if (entry.Kind == ManufacturingMutationKind.ConfirmOperation)
        {
            var log = await db.ShopFloorLogs.FirstOrDefaultAsync(l => l.OperationPostingUuid == entry.IdempotencyKey, cancellationToken).ConfigureAwait(false);
            if (log is not null)
            {
                log.SyncStatus = entry.Status;
                log.UpdatedAt = Now;
            }
        }

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // The order is only protected from a pull while something about it is still unresolved.
        var unresolved = await db.Outbox.AnyAsync(
            e => e.ProductionOrderId == entry.ProductionOrderId
                && (e.Status == ManufacturingOutboxStatus.Pending || e.Status == ManufacturingOutboxStatus.Conflict || e.Status == ManufacturingOutboxStatus.Failed),
            cancellationToken).ConfigureAwait(false);
        var order = await db.ProductionOrders.FirstOrDefaultAsync(o => o.Id == entry.ProductionOrderId, cancellationToken).ConfigureAwait(false);
        if (order is not null && order.HasPendingChanges != unresolved)
        {
            order.HasPendingChanges = unresolved;
            _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<ResolvedRoute?> ResolveRouteCoreAsync(ManufacturingOutboxEntry entry, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        var serverId = await db.ProductionOrders.AsNoTracking()
            .Where(o => o.Id == entry.ProductionOrderId)
            .Select(o => o.ServerId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var template = entry.RouteTemplate;
        return (template.Contains(OrderPlaceholder, StringComparison.Ordinal), serverId) switch
        {
            (false, _) => new ResolvedRoute(template, serverId),
            (true, { } id) => new ResolvedRoute(template.Replace(OrderPlaceholder, id.ToString("D", CultureInfo.InvariantCulture), StringComparison.Ordinal), id),
            _ => null,
        };
    }

    private async Task ApplyOutcomeCoreAsync(ManufacturingOutboxEntry entry, string? responseBody, CancellationToken cancellationToken)
    {
        var db = await factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var dbScope = db.ConfigureAwait(false);
        if (entry.Kind == ManufacturingMutationKind.CreateOrder && entry.Status == ManufacturingOutboxStatus.Synced)
        {
            await AdoptCreatedOrderAsync(db, entry, responseBody, cancellationToken).ConfigureAwait(false);
        }
        else if (entry.Kind == ManufacturingMutationKind.ConfirmOperation)
        {
            await CompleteConfirmationAsync(db, entry, responseBody, cancellationToken).ConfigureAwait(false);
        }

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
