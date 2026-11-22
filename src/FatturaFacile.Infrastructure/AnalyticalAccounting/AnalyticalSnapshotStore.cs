// <copyright file="AnalyticalSnapshotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AnalyticalAccounting;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.AnalyticalAccounting;

/// <summary>
/// EF Core implementation of <see cref="IAnalyticalSnapshotStore"/> for the active tenant (the tenant filter of the context keeps
/// tenants apart).
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class AnalyticalSnapshotStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IAnalyticalSnapshotStore
{
    /// <inheritdoc/>
    public async Task<StoredAnalyticalPayload?> FindAsync(string key, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var row = await db.Set<LocalAnalyticalSnapshot>().AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken).ConfigureAwait(false);

        return row is null ? null : new StoredAnalyticalPayload(row.PayloadJson, row.SyncedAtUtc);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(json);

        var row = await db.Set<LocalAnalyticalSnapshot>().FirstOrDefaultAsync(x => x.Key == key, cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new LocalAnalyticalSnapshot { TenantId = tenant.TenantId, Key = key, CreatedAt = syncedAtUtc };
            _ = db.Set<LocalAnalyticalSnapshot>().Add(row);
        }
        else
        {
            row.UpdatedAt = syncedAtUtc;
        }

        row.PayloadJson = json;
        row.SyncedAtUtc = syncedAtUtc;
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
