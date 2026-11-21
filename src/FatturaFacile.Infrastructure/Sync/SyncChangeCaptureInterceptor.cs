// <copyright file="SyncChangeCaptureInterceptor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// Captures local changes of synchronized entities into the outbox in the same transaction as the change itself.
/// </summary>
/// <param name="timeProvider">The clock.</param>
public sealed class SyncChangeCaptureInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static SyncOperation? Classify(EntityEntry entry, ISyncableEntity entity)
    {
        return entry.State switch
        {
            EntityState.Added => SyncOperation.Create,
            EntityState.Deleted => SyncOperation.Delete,
            EntityState.Modified => entity.DeletedAt is null ? SyncOperation.Update : SyncOperation.Delete,
            EntityState.Detached or EntityState.Unchanged => null,
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry.State, "Unknown entity state."),
        };
    }

    private static bool HasPendingWrite(DbContext context, List<Change> changes, Change change)
    {
        return changes.Exists(c => string.Equals(c.Name, change.Name, StringComparison.Ordinal) && c.Id == change.Id && c.Operation == SyncOperation.Create)
            || context.Set<SyncOutboxEntry>().Any(x =>
                x.EntityName == change.Name && x.EntityId == change.Id && x.Status == SyncOutboxStatus.Pending && x.Operation != SyncOperation.Delete);
    }

    private void Capture(DbContext? context)
    {
        if (context is null || SyncSuppression.IsSuppressed)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        List<Change> changes = [];

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not ISyncableEntity { TenantId: { } tenantId } entity
                || !SyncCatalog.TryGetWritable(entry.Entity.GetType(), out var name)
                || Classify(entry, entity) is not { } operation)
            {
                continue;
            }

            changes.Add(new Change(name, entity.Id, tenantId, operation));
        }

        foreach (var change in changes)
        {
            // A pending create or update already carries the latest state: the payload is built at send time.
            if (change.Operation == SyncOperation.Update && HasPendingWrite(context, changes, change))
            {
                continue;
            }

            _ = context.Set<SyncOutboxEntry>().Add(new SyncOutboxEntry
            {
                TenantId = change.TenantId,
                EntityName = change.Name,
                EntityId = change.Id,
                Operation = change.Operation,
                NextAttemptAt = now,
                CreatedAt = now,
            });

            var state = context.Set<SyncEntityState>().FirstOrDefault(s => s.EntityName == change.Name && s.EntityId == change.Id);
            SyncStateWriter.Apply(context, state, (change.TenantId, change.Name, change.Id), SyncOutboxStatus.Pending, serverVersion: null, now);
        }
    }

    private readonly record struct Change(string Name, Guid Id, Guid TenantId, SyncOperation Operation);
}
