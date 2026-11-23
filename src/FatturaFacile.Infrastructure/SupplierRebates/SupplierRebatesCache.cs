// <copyright file="SupplierRebatesCache.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.SupplierRebates;

/// <summary>
/// EF Core implementation of <see cref="ISupplierRebatesCache"/> for the active tenant (the tenant filter of the context keeps
/// tenants apart).
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class SupplierRebatesCache(FatturaFacileDbContext db, ICurrentTenantService tenant) : ISupplierRebatesCache
{
    /// <inheritdoc/>
    public async Task<StoredRebatePayload?> FindAsync(string kind, string key, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var row = await db.Set<LocalSupplierRebateSnapshot>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Kind == kind && x.RecordKey == key, cancellationToken)
            .ConfigureAwait(false);

        return row is null ? null : new StoredRebatePayload(row.PayloadJson, row.SyncedAtUtc);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(IReadOnlyList<RebateCacheEntry> entries, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entries);

        foreach (var entry in entries)
        {
            var row = await db.Set<LocalSupplierRebateSnapshot>()
                .FirstOrDefaultAsync(x => x.Kind == entry.Kind && x.RecordKey == entry.Key, cancellationToken)
                .ConfigureAwait(false);

            if (row is null)
            {
                row = new LocalSupplierRebateSnapshot { TenantId = tenant.TenantId, Kind = entry.Kind, RecordKey = entry.Key, CreatedAt = syncedAtUtc };
                _ = db.Set<LocalSupplierRebateSnapshot>().Add(row);
            }
            else
            {
                row.UpdatedAt = syncedAtUtc;
            }

            row.PayloadJson = entry.Json;
            row.SyncedAtUtc = syncedAtUtc;
        }

        // One SaveChanges: the payloads of a record and their timestamp replace the previous ones together.
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
