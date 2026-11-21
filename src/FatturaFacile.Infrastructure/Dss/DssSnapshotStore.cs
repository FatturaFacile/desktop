// <copyright file="DssSnapshotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Dss;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Dss;

/// <summary>
/// EF Core implementation of <see cref="IDssSnapshotStore"/> for the active tenant (the tenant filter of the context keeps tenants apart).
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class DssSnapshotStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IDssSnapshotStore
{
    /// <inheritdoc/>
    public Task<StoredDssPayload?> FindAsync(DssFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return ReadAsync(filter, cancellationToken);
    }

    /// <inheritdoc/>
    public Task SaveAsync(DssFilter filter, DssPayload payload, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(payload);

        return WriteAsync(filter, payload, syncedAtUtc, cancellationToken);
    }

    private async Task<StoredDssPayload?> ReadAsync(DssFilter filter, CancellationToken cancellationToken)
    {
        var row = await Matching(filter).AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return row is null
            ? null
            : new StoredDssPayload(new DssPayload(row.OverviewJson, row.ProfitabilityJson, row.VarianceBridgeJson, row.SimulationInputsJson, row.LegalEntitiesJson), row.SyncedAtUtc);
    }

    private async Task WriteAsync(DssFilter filter, DssPayload payload, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        var row = await Matching(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new LocalDssSnapshot
            {
                TenantId = tenant.TenantId,
                LegalEntityId = filter.LegalEntityId,
                Currency = filter.Currency,
                PeriodPreset = filter.PeriodPreset,
                ComparisonBaseline = filter.ComparisonBaseline,
                CreatedAt = syncedAtUtc,
            };
            _ = db.Set<LocalDssSnapshot>().Add(row);
        }
        else
        {
            row.UpdatedAt = syncedAtUtc;
        }

        row.OverviewJson = payload.OverviewJson;
        row.ProfitabilityJson = payload.ProfitabilityJson;
        row.VarianceBridgeJson = payload.VarianceBridgeJson;
        row.SimulationInputsJson = payload.SimulationInputsJson;
        row.LegalEntitiesJson = payload.LegalEntitiesJson;
        row.SyncedAtUtc = syncedAtUtc;

        // One SaveChanges: the three views and their timestamp replace the previous snapshot together.
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private IQueryable<LocalDssSnapshot> Matching(DssFilter filter) =>
        db.Set<LocalDssSnapshot>().Where(x =>
            x.LegalEntityId == filter.LegalEntityId
            && x.Currency == filter.Currency
            && x.PeriodPreset == filter.PeriodPreset
            && x.ComparisonBaseline == filter.ComparisonBaseline);
}
