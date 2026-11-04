// <copyright file="AuditAndSoftDeleteInterceptor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Primitives;
using FatturaFacile.Infrastructure.Common.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FatturaFacile.Infrastructure.Common.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit metadata (creation/modification timestamps and users)
/// and transforms delete operations on <see cref="ISoftDeletable"/> entities into soft-delete updates.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuditAndSoftDeleteInterceptor"/> class.
/// </remarks>
/// <param name="tenantContext">The active tenant context accessor.</param>
public sealed class AuditAndSoftDeleteInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext = tenantContext;

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        ApplyAuditAndSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        ApplyAuditAndSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditAndSoftDelete(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var currentUserId = _tenantContext.UserId ?? "system";
        var currentTenantId = _tenantContext.TenantId;

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            // Multi-tenant auto assignment
            if (entry.State == EntityState.Added && entry.Entity is IHasTenant tenantEntity && tenantEntity.TenantId == Guid.Empty && currentTenantId != Guid.Empty)
            {
                var tenantProp = entry.Metadata.FindProperty("TenantId");
                if (tenantProp != null)
                {
                    entry.Property(tenantProp).CurrentValue = currentTenantId;
                }
            }

            // Soft-delete interception
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable)
            {
                entry.State = EntityState.Modified;
                var isDeletedProp = entry.Metadata.FindProperty("IsDeleted");
                if (isDeletedProp != null)
                {
                    entry.Property(isDeletedProp).CurrentValue = true;
                }

                var deletedAtProp = entry.Metadata.FindProperty("DeletedAt");
                if (deletedAtProp != null)
                {
                    entry.Property(deletedAtProp).CurrentValue = now;
                }
            }

            // Auditable Entity timestamps
            if (entry.Entity is IAuditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    var createdAtProp = entry.Metadata.FindProperty("CreatedAt");
                    if (createdAtProp != null)
                    {
                        entry.Property(createdAtProp).CurrentValue = now;
                    }

                    var createdByProp = entry.Metadata.FindProperty("CreatedBy");
                    if (createdByProp != null)
                    {
                        entry.Property(createdByProp).CurrentValue = currentUserId;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    var updatedAtProp = entry.Metadata.FindProperty("UpdatedAt");
                    if (updatedAtProp != null)
                    {
                        entry.Property(updatedAtProp).CurrentValue = now;
                    }

                    var updatedByProp = entry.Metadata.FindProperty("UpdatedBy");
                    if (updatedByProp != null)
                    {
                        entry.Property(updatedByProp).CurrentValue = currentUserId;
                    }
                }
            }
        }
    }
}
