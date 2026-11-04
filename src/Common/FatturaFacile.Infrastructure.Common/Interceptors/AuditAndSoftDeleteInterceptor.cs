using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using FatturaFacile.Domain.Shared.Primitives;
using FatturaFacile.Infrastructure.Common.Audit;
using FatturaFacile.Infrastructure.Common.Tenancy;

namespace FatturaFacile.Infrastructure.Common.Interceptors;

public sealed class AuditAndSoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;

    public AuditAndSoftDeleteInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditAndSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditAndSoftDelete(DbContext? context)
    {
        if (context == null) return;

        var now = DateTimeOffset.UtcNow;
        var currentUserId = _tenantContext.UserId ?? "system";
        var currentTenantId = _tenantContext.TenantId;

        var entries = context.ChangeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            // Multi-tenant auto assignment
            if (entry.State == EntityState.Added && entry.Entity is IHasTenant tenantEntity)
            {
                if (tenantEntity.TenantId == Guid.Empty && currentTenantId != Guid.Empty)
                {
                    entry.Property("TenantId").CurrentValue = currentTenantId;
                }
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
