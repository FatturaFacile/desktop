// <copyright file="ProcurementStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Procurement;

/// <summary>
/// SQLite store of the procurement cache and outbox, scoped to the active tenant by the query filters of the context.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class ProcurementStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IProcurementStore
{
    /// <inheritdoc/>
    public Task<LocalProcurementCache?> FindCacheAsync(string key, CancellationToken cancellationToken) =>
        db.Set<LocalProcurementCache>().AsNoTracking().FirstOrDefaultAsync(x => x.CacheKey == key, cancellationToken);

    /// <inheritdoc/>
    public async Task SaveCacheAsync(string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var row = await db.Set<LocalProcurementCache>().FirstOrDefaultAsync(x => x.CacheKey == key, cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new LocalProcurementCache { TenantId = tenant.TenantId, CacheKey = key, CreatedAt = syncedAtUtc };
            _ = db.Set<LocalProcurementCache>().Add(row);
        }
        else
        {
            row.UpdatedAt = syncedAtUtc;
        }

        row.Json = json;
        row.SyncedAtUtc = syncedAtUtc;
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task TrimCacheAsync(string prefix, int keep, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        var rows = await db.Set<LocalProcurementCache>()
            .Where(x => x.CacheKey.StartsWith(prefix))
            .OrderByDescending(x => x.SyncedAtUtc)
            .Skip(keep)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (rows.Count == 0)
        {
            return;
        }

        db.Set<LocalProcurementCache>().RemoveRange(rows);
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ProcurementOutboxEntry> EnqueueAsync(ProcurementOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var existing = await db.Set<ProcurementOutboxEntry>().FirstOrDefaultAsync(x => x.IdempotencyKey == entry.IdempotencyKey, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return existing;
        }

        _ = db.Set<ProcurementOutboxEntry>().Add(entry);
        await SaveAsync(cancellationToken).ConfigureAwait(false);

        return entry;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProcurementOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
        await db.Set<ProcurementOutboxEntry>()
            .Where(x => x.Status == ProcurementOutboxStatus.Pending)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProcurementOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken) =>
        await db.Set<ProcurementOutboxEntry>()
            .Where(x => x.Status == ProcurementOutboxStatus.Conflict)
            .OrderBy(x => x.OccurredAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task SaveAsync(CancellationToken cancellationToken) =>
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
