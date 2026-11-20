// <copyright file="SyncStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// EF Core implementation of <see cref="ISyncStore"/> for the active tenant.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class SyncStore(FatturaFacileDbContext db, ICurrentTenantService tenant, TimeProvider timeProvider) : ISyncStore
{
    /// <inheritdoc/>
    public async Task<DateTime?> GetCheckpointAsync(string entityName, CancellationToken cancellationToken)
    {
        var checkpoint = await db.Set<SyncCheckpoint>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EntityName == entityName, cancellationToken)
            .ConfigureAwait(false);
        return checkpoint?.LastServerTime;
    }

    /// <inheritdoc/>
    public async Task SaveCheckpointAsync(string entityName, DateTime serverTime, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var checkpoint = await db.Set<SyncCheckpoint>().FirstOrDefaultAsync(x => x.EntityName == entityName, cancellationToken).ConfigureAwait(false);
        if (checkpoint is null)
        {
            checkpoint = new SyncCheckpoint { TenantId = tenant.TenantId, EntityName = entityName, CreatedAt = now };
            _ = db.Set<SyncCheckpoint>().Add(checkpoint);
        }

        checkpoint.LastServerTime = serverTime;
        checkpoint.UpdatedAt = now;
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SyncOutboxEntry>> GetDueEntriesAsync(DateTime nowUtc, int max, CancellationToken cancellationToken)
    {
        return await db.Set<SyncOutboxEntry>()
            .Where(x => x.Status == SyncOutboxStatus.Pending && x.NextAttemptAt <= nowUtc)
            .OrderBy(x => x.CreatedAt)
            .Take(max)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlySet<Guid>> GetProtectedEntityIdsAsync(string entityName, CancellationToken cancellationToken)
    {
        var ids = await db.Set<SyncOutboxEntry>()
            .AsNoTracking()
            .Where(x => x.EntityName == entityName && (x.Status == SyncOutboxStatus.Pending || x.Status == SyncOutboxStatus.Conflict))
            .Select(x => x.EntityId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return ids.ToHashSet();
    }

    /// <inheritdoc/>
    public Task SaveEntryAsync(SyncOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return SaveEntryCoreAsync(entry, cancellationToken);
    }

    private async Task SaveEntryCoreAsync(SyncOutboxEntry entry, CancellationToken cancellationToken)
    {
        var state = await db.Set<SyncEntityState>()
            .FirstOrDefaultAsync(x => x.EntityName == entry.EntityName && x.EntityId == entry.EntityId, cancellationToken)
            .ConfigureAwait(false);
        SyncStateWriter.Apply(db, state, (entry.TenantId, entry.EntityName, entry.EntityId), entry.Status, serverVersion: null, timeProvider.GetUtcNow().UtcDateTime);
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
