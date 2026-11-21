// <copyright file="MesStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Mes;

/// <summary>
/// EF Core implementation of <see cref="IMesStore"/> for the active tenant (the tenant filters of the context keep tenants apart).
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class MesStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IMesStore
{
    /// <inheritdoc/>
    public Task<LocalMesBoard?> FindBoardAsync(Guid workCenterId, CancellationToken cancellationToken) =>
        db.Set<LocalMesBoard>().AsNoTracking().FirstOrDefaultAsync(x => x.WorkCenterId == workCenterId, cancellationToken);

    /// <inheritdoc/>
    public async Task SaveBoardAsync(Guid workCenterId, string json, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var row = await db.Set<LocalMesBoard>().FirstOrDefaultAsync(x => x.WorkCenterId == workCenterId, cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new LocalMesBoard { TenantId = tenant.TenantId, WorkCenterId = workCenterId, CreatedAt = syncedAtUtc };
            _ = db.Set<LocalMesBoard>().Add(row);
        }
        else
        {
            row.UpdatedAt = syncedAtUtc;
        }

        row.BoardJson = json;
        row.SyncedAtUtc = syncedAtUtc;
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task EnqueueAsync(MesOutboxEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _ = db.Set<MesOutboxEntry>().Add(entry);

        return SaveAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MesOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
        await db.Set<MesOutboxEntry>()
            .Where(x => x.Status == MesOutboxStatus.Pending)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MesOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken) =>
        await db.Set<MesOutboxEntry>()
            .Where(x => x.Status == MesOutboxStatus.Conflict)
            .OrderBy(x => x.OccurredAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task SaveAsync(CancellationToken cancellationToken) =>
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task AssignPhaseAsync(Guid operationId, Guid phaseId, CancellationToken cancellationToken)
    {
        var waiting = await db.Set<MesOutboxEntry>()
            .Where(x => x.OperationId == operationId && x.PhaseId == null && x.Status == MesOutboxStatus.Pending)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var entry in waiting)
        {
            entry.PhaseId = phaseId;
        }

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
