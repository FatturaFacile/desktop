// <copyright file="HelpdeskStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Helpdesk;

/// <summary>
/// SQLite store of the helpdesk cache and outbox, scoped to the active tenant by the query filters of the context.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class HelpdeskStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IHelpdeskStore
{
    /// <inheritdoc/>
    public Task<LocalHelpdeskCache?> FindCacheAsync(string key, CancellationToken cancellationToken) =>
        db.Set<LocalHelpdeskCache>().AsNoTracking().FirstOrDefaultAsync(x => x.CacheKey == key, cancellationToken);

    /// <inheritdoc/>
    public async Task SaveCacheAsync(string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var row = await db.Set<LocalHelpdeskCache>().FirstOrDefaultAsync(x => x.CacheKey == key, cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new LocalHelpdeskCache { TenantId = tenant.TenantId, CacheKey = key, CreatedAt = syncedAtUtc };
            _ = db.Set<LocalHelpdeskCache>().Add(row);
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

        var rows = await db.Set<LocalHelpdeskCache>()
            .Where(x => x.CacheKey.StartsWith(prefix))
            .OrderByDescending(x => x.SyncedAtUtc)
            .Skip(keep)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (rows.Count == 0)
        {
            return;
        }

        db.Set<LocalHelpdeskCache>().RemoveRange(rows);
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task EnqueueAsync(HelpdeskOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _ = db.Set<HelpdeskOutboxEntry>().Add(entry);

        return SaveAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HelpdeskOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
        await db.Set<HelpdeskOutboxEntry>()
            .Where(x => x.Status == HelpdeskOutboxStatus.Pending)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HelpdeskOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken) =>
        await db.Set<HelpdeskOutboxEntry>()
            .Where(x => x.Status == HelpdeskOutboxStatus.Conflict)
            .OrderBy(x => x.OccurredAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task SaveAsync(CancellationToken cancellationToken) =>
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
