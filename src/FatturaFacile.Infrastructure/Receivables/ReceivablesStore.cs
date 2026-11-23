// <copyright file="ReceivablesStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Receivables;

/// <summary>
/// SQLite store of the receivables cache and outbox, scoped to the active tenant by the query filters of the context.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class ReceivablesStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IReceivablesStore
{
    /// <inheritdoc/>
    public Task<LocalReceivablesCache?> FindCacheAsync(string key, CancellationToken cancellationToken) =>
        db.Set<LocalReceivablesCache>().AsNoTracking().FirstOrDefaultAsync(x => x.CacheKey == key, cancellationToken);

    /// <inheritdoc/>
    public async Task SaveCacheAsync(string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var row = await db.Set<LocalReceivablesCache>().FirstOrDefaultAsync(x => x.CacheKey == key, cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new LocalReceivablesCache { TenantId = tenant.TenantId, CacheKey = key, CreatedAt = syncedAtUtc };
            _ = db.Set<LocalReceivablesCache>().Add(row);
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

        var rows = await db.Set<LocalReceivablesCache>()
            .Where(x => x.CacheKey.StartsWith(prefix))
            .OrderByDescending(x => x.SyncedAtUtc)
            .Skip(keep)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (rows.Count == 0)
        {
            return;
        }

        db.Set<LocalReceivablesCache>().RemoveRange(rows);
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<ReceivablesOutboxEntry> EnqueueAsync(ReceivablesOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return EnqueueCoreAsync(entry, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReceivablesOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
        await db.Set<ReceivablesOutboxEntry>()
            .Where(x => x.Status == ReceivablesOutboxStatus.Pending)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ReceivablesOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken) =>
        await db.Set<ReceivablesOutboxEntry>()
            .Where(x => x.Status == ReceivablesOutboxStatus.Conflict)
            .OrderBy(x => x.OccurredAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task SaveAsync(CancellationToken cancellationToken) =>
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    private async Task<ReceivablesOutboxEntry> EnqueueCoreAsync(ReceivablesOutboxEntry entry, CancellationToken cancellationToken)
    {
        var existing = await db.Set<ReceivablesOutboxEntry>().FirstOrDefaultAsync(x => x.IdempotencyKey == entry.IdempotencyKey, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return existing;
        }

        _ = db.Set<ReceivablesOutboxEntry>().Add(entry);
        await SaveAsync(cancellationToken).ConfigureAwait(false);

        return entry;
    }
}
