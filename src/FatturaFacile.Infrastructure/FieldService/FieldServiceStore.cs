// <copyright file="FieldServiceStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.FieldService;

/// <summary>
/// EF Core implementation of <see cref="IFieldServiceStore"/> for the active tenant (the tenant filters of the context keep
/// tenants apart).
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class FieldServiceStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IFieldServiceStore
{
    /// <inheritdoc/>
    public async Task<StoredFieldServicePayload?> FindAsync(string kind, string key, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var row = await Matching(kind, key).AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return row is null ? null : new StoredFieldServicePayload(row.PayloadJson, row.SyncedAtUtc);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(string kind, string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var row = await Matching(kind, key).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new LocalFieldServiceCache { TenantId = tenant.TenantId, Kind = kind, Key = key, CreatedAt = syncedAtUtc };
            _ = db.Set<LocalFieldServiceCache>().Add(row);
        }
        else
        {
            row.UpdatedAt = syncedAtUtc;
        }

        row.PayloadJson = json;
        row.SyncedAtUtc = syncedAtUtc;

        // One SaveChanges: the payload and its timestamp replace the previous ones together.
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task EnqueueAsync(FieldServiceOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _ = db.Set<FieldServiceOutboxEntry>().Add(entry);

        return SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FieldServiceOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
        await db.Set<FieldServiceOutboxEntry>()
            .Where(x => x.Status == FieldServiceOutboxStatus.Pending)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FieldServiceOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken) =>
        await db.Set<FieldServiceOutboxEntry>()
            .Where(x => x.Status == FieldServiceOutboxStatus.Conflict)
            .OrderBy(x => x.OccurredAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    private IQueryable<LocalFieldServiceCache> Matching(string kind, string key) =>
        db.Set<LocalFieldServiceCache>().Where(x => x.Kind == kind && x.Key == key);
}
