// <copyright file="TenantSaveChangesInterceptor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// EF Core SaveChangesInterceptor that enforces multi-tenant rules, automatic TenantId assignment, and read-only protection for global records.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TenantSaveChangesInterceptor"/> class.
/// </remarks>
/// <param name="currentTenantService">The current active tenant service.</param>
public sealed class TenantSaveChangesInterceptor(ICurrentTenantService currentTenantService) : SaveChangesInterceptor
{

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        ApplyTenantRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        ApplyTenantRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyTenantRules(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var activeTenantId = currentTenantService.TenantId;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Read-only protection: prevent editing or deleting global records (TenantId == null)
            if (entry.Entity is IOptionalTenantEntity { TenantId: null } && (entry.State == EntityState.Modified || entry.State == EntityState.Deleted))
            {
                throw new InvalidOperationException(
                    $"Cannot modify or delete global/shared system default record of type '{entry.Entity.GetType().Name}'. Global records are strictly read-only.");
            }

            // Working time records are evidence for the labour inspectorate (art. 73 OLL 1): the audit trail is
            // append-only and clock events are only deleted logically.
            if ((entry.Entity is TimeAuditEntry && entry.State is EntityState.Modified or EntityState.Deleted)
                || (entry.Entity is TimeClockEvent && entry.State == EntityState.Deleted))
            {
                throw new InvalidOperationException("Le registrazioni della durata del lavoro e il loro storico non possono essere eliminati.");
            }

            // Assign active TenantId to newly created tenant-scoped entities
            if (entry.Entity is ITenantScopedEntity { TenantId: var tenantId } tenantScopedEntity && entry.State == EntityState.Added && tenantId == Guid.Empty)
            {
                tenantScopedEntity.TenantId = activeTenantId;
            }

            // Audit timestamps
            if (entry.Entity is IBaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added && baseEntity.CreatedAt == null)
                {
                    baseEntity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    baseEntity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
