using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Infrastructure.Common.Tenancy;

namespace FatturaFacile.Infrastructure.Common.Interceptors;

/// <summary>
/// EF Core SaveChangesInterceptor that strictly enforces tenant assignment and validation
/// on all entities implementing <see cref="IMustHaveTenant"/>.
/// </summary>
public sealed class TenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;

    public TenantSaveChangesInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        EnforceTenantRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        EnforceTenantRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EnforceTenantRules(DbContext? context)
    {
        if (context == null) return;

        var currentTenantId = _tenantContext.TenantId;
        var entries = context.ChangeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is IMustHaveTenant)
            {
                var tenantProp = entry.Property("TenantId");
                var entityTenantId = (Guid)(tenantProp.CurrentValue ?? Guid.Empty);

                if (entry.State == EntityState.Added)
                {
                    if (entityTenantId == Guid.Empty)
                    {
                        if (currentTenantId == Guid.Empty)
                        {
                            throw new InvalidOperationException(
                                $"Cannot insert entity '{entry.Entity.GetType().Name}' because no TenantId was supplied and no active TenantContext is set.");
                        }

                        tenantProp.CurrentValue = currentTenantId;
                    }
                    else if (currentTenantId != Guid.Empty && entityTenantId != currentTenantId)
                    {
                        throw new InvalidOperationException(
                            $"TenantId mismatch: Entity '{entry.Entity.GetType().Name}' has TenantId '{entityTenantId}' but active TenantContext is '{currentTenantId}'.");
                    }
                }
                else if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    if (currentTenantId != Guid.Empty && entityTenantId != currentTenantId && !_tenantContext.IsTenantFilterIgnored)
                    {
                        throw new InvalidOperationException(
                            $"Cross-tenant mutation denied: Cannot modify entity '{entry.Entity.GetType().Name}' of tenant '{entityTenantId}' from tenant '{currentTenantId}'.");
                    }
                }
            }
        }
    }
}
