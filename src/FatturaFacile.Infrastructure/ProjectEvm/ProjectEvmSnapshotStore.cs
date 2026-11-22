// <copyright file="ProjectEvmSnapshotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ProjectEvm;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.ProjectEvm;

/// <summary>
/// EF Core implementation of <see cref="IProjectEvmSnapshotStore"/> for the active tenant (the tenant filter of the context keeps
/// tenants apart).
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class ProjectEvmSnapshotStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IProjectEvmSnapshotStore
{
    private const string EmptyObject = "{}";

    /// <inheritdoc/>
    public async Task<StoredProjectEvmPayload?> FindAsync(string projectId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);

        var row = await Matching(projectId).AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return row is null ? null : new StoredProjectEvmPayload(new ProjectEvmPayload(row.BreakdownJson, row.WaterfallJson, row.ScurveJson), row.SyncedAtUtc);
    }

    /// <inheritdoc/>
    public Task SaveAsync(string projectId, ProjectEvmPayload payload, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);
        ArgumentNullException.ThrowIfNull(payload);

        return WriteAsync(projectId, payload.BreakdownJson, payload.WaterfallJson, payload.ScurveJson, syncedAtUtc, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string?> FindProjectsAsync(CancellationToken cancellationToken)
    {
        var row = await Matching(LocalProjectEvmSnapshot.ProjectIndexKey).AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return row?.BreakdownJson;
    }

    /// <inheritdoc/>
    public Task SaveProjectsAsync(string projectsJson, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(projectsJson);

        return WriteAsync(LocalProjectEvmSnapshot.ProjectIndexKey, projectsJson, EmptyObject, EmptyObject, syncedAtUtc, cancellationToken);
    }

    private async Task WriteAsync(string key, string breakdown, string waterfall, string scurve, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        var row = await Matching(key).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new LocalProjectEvmSnapshot { TenantId = tenant.TenantId, ProjectId = key, CreatedAt = syncedAtUtc };
            _ = db.Set<LocalProjectEvmSnapshot>().Add(row);
        }
        else
        {
            row.UpdatedAt = syncedAtUtc;
        }

        row.BreakdownJson = breakdown;
        row.WaterfallJson = waterfall;
        row.ScurveJson = scurve;
        row.SyncedAtUtc = syncedAtUtc;

        // One SaveChanges: the three views and their timestamp replace the previous snapshot together.
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private IQueryable<LocalProjectEvmSnapshot> Matching(string key) => db.Set<LocalProjectEvmSnapshot>().Where(x => x.ProjectId == key);
}
