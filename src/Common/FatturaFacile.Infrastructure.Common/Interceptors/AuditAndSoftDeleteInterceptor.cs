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
                entry.Property("TenantId").CurrentValue = currentTenantId;
            }

            // Soft-delete interception
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable)
            {
                entry.State = EntityState.Modified;
                entry.Property("IsDeleted").CurrentValue = true;
                entry.Property("DeletedAt").CurrentValue = now;
            }

            // Auditable Entity timestamps
            if (entry.Entity is IAuditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = now;
                    entry.Property("CreatedBy").CurrentValue = currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = now;
                    entry.Property("UpdatedBy").CurrentValue = currentUserId;
                }
            }
        }
    }
}
