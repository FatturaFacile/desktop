// <copyright file="ManufacturingStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <summary>
/// EF Core implementation of <see cref="IManufacturingStore"/> for the active tenant (the tenant filters of the context keep tenants apart).
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class ManufacturingStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IManufacturingStore
{
    /// <inheritdoc/>
    public Task ReplaceAsync(ManufacturingMasterData data, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(data);

        return WriteAsync(data, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ManufacturingOverview> GetOverviewAsync(CancellationToken cancellationToken) => ReadAsync(cancellationToken);

    private static void Stamp(ITenantScopedEntity entity, Guid tenantId) => entity.TenantId = tenantId;

    private async Task WriteAsync(ManufacturingMasterData data, CancellationToken cancellationToken)
    {
        var tenantId = tenant.TenantId;
        var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await using (transaction.ConfigureAwait(false))
        {
            // Children first, then parents: the cache of the tenant is rebuilt from what the server returned.
            _ = await db.Set<MfgRoutingOperationBomLink>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            _ = await db.Set<MfgRoutingOperation>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            _ = await db.Set<MfgRouting>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            _ = await db.Set<MfgCalendarShift>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            _ = await db.Set<MfgCalendarClosure>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            _ = await db.Set<MfgWorkingCalendar>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            _ = await db.Set<MfgMachineCenter>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            _ = await db.Set<MfgWorkCenter>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            _ = await db.Set<MfgOeeSnapshot>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

            AddAll(data.WorkCenters, tenantId);
            AddAll(data.Machines, tenantId);
            AddAll(data.OeeSnapshots, tenantId);

            foreach (var calendar in data.Calendars)
            {
                Stamp(calendar, tenantId);
                calendar.Shifts.ToList().ForEach(s => Stamp(s, tenantId));
                calendar.Closures.ToList().ForEach(c => Stamp(c, tenantId));
                _ = db.Set<MfgWorkingCalendar>().Add(calendar);
            }

            foreach (var routing in data.Routings)
            {
                Stamp(routing, tenantId);
                foreach (var operation in routing.Operations)
                {
                    Stamp(operation, tenantId);
                    operation.BomComponents.ToList().ForEach(l => Stamp(l, tenantId));
                }

                _ = db.Set<MfgRouting>().Add(routing);
            }

            // One SaveChanges inside the transaction: a reader sees the old cache or the new one, never a mix.
            _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        db.ChangeTracker.Clear();
    }

    private async Task<ManufacturingOverview> ReadAsync(CancellationToken cancellationToken)
    {
        var workCenters = await db.Set<MfgWorkCenter>().AsNoTracking().OrderBy(x => x.WorkCenterCode).ToListAsync(cancellationToken).ConfigureAwait(false);
        var machines = await db.Set<MfgMachineCenter>().AsNoTracking().OrderBy(x => x.MachineCode).ToListAsync(cancellationToken).ConfigureAwait(false);
        var calendars = await db.Set<MfgWorkingCalendar>().AsNoTracking().Include(x => x.Shifts).Include(x => x.Closures).AsSplitQuery()
            .OrderBy(x => x.Code).ToListAsync(cancellationToken).ConfigureAwait(false);
        var routings = await db.Set<MfgRouting>().AsNoTracking().Include(x => x.Operations).ThenInclude(o => o.BomComponents).AsSplitQuery()
            .OrderBy(x => x.RoutingNumber).ThenBy(x => x.Revision).ToListAsync(cancellationToken).ConfigureAwait(false);
        var oee = await db.Set<MfgOeeSnapshot>().AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var calendar in calendars)
        {
            calendar.Shifts = [.. calendar.Shifts.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime, StringComparer.Ordinal)];
            calendar.Closures = [.. calendar.Closures.OrderBy(c => c.StartsOn)];
        }

        foreach (var routing in routings)
        {
            routing.Operations = [.. routing.Operations.OrderBy(o => o.Sequence)];
        }

        List<DateTime> stamps = [];
        stamps.AddRange(workCenters.Select(x => x.SyncedAtUtc));
        stamps.AddRange(machines.Select(x => x.SyncedAtUtc));
        stamps.AddRange(calendars.Select(x => x.SyncedAtUtc));
        stamps.AddRange(routings.Select(x => x.SyncedAtUtc));
        DateTime? lastSynced = stamps.Count > 0 ? stamps.Max() : null;

        return new ManufacturingOverview(workCenters, machines, calendars, routings, oee, lastSynced);
    }

    private void AddAll<T>(IEnumerable<T> entities, Guid tenantId)
        where T : class, ITenantScopedEntity
    {
        foreach (var entity in entities)
        {
            Stamp(entity, tenantId);
            _ = db.Set<T>().Add(entity);
        }
    }
}
