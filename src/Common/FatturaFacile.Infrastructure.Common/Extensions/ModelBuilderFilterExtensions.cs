using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Domain.Shared.Primitives;
using FatturaFacile.Infrastructure.Common.Tenancy;

namespace FatturaFacile.Infrastructure.Common.Extensions;

/// <summary>
/// Extension methods for registering universal multi-tenant and soft-delete global query filters on ModelBuilder.
/// </summary>
public static class ModelBuilderFilterExtensions
{
    /// <summary>
    /// Automatically applies multi-tenant and soft-delete global query filters to all eligible entities.
    /// </summary>
    /// <param name="modelBuilder">The EF Core ModelBuilder instance.</param>
    /// <param name="tenantContext">The active tenant context accessor.</param>
    public static void ApplyGlobalTenantAndSoftDeleteFilters(this ModelBuilder modelBuilder, ITenantContext tenantContext)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            var isMustHaveTenant = typeof(IMustHaveTenant).IsAssignableFrom(clrType);
            var isSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(clrType);

            if (!isMustHaveTenant && !isSoftDeletable)
                continue;

            if (isMustHaveTenant && !isSoftDeletable)
            {
                // Filter: tenantContext.IsTenantFilterIgnored || EF.Property<Guid>(e, "TenantId") == tenantContext.TenantId
                var param = Expression.Parameter(clrType, "e");
                var tenantProp = Expression.Call(typeof(EF), nameof(EF.Property), [typeof(Guid)], param, Expression.Constant("TenantId"));
                var contextExpr = Expression.Constant(tenantContext);
                var isIgnored = Expression.Property(contextExpr, nameof(ITenantContext.IsTenantFilterIgnored));
                var currentTenant = Expression.Property(contextExpr, nameof(ITenantContext.TenantId));
                var equals = Expression.Equal(tenantProp, currentTenant);
                var combined = Expression.OrElse(isIgnored, equals);

                entityType.SetQueryFilter(Expression.Lambda(combined, param));
            }
            else if (!isMustHaveTenant && isSoftDeletable)
            {
                var param = Expression.Parameter(clrType, "e");
                var isDeletedProp = Expression.Call(typeof(EF), nameof(EF.Property), [typeof(bool)], param, Expression.Constant("IsDeleted"));
                var notDeleted = Expression.Not(isDeletedProp);

                entityType.SetQueryFilter(Expression.Lambda(notDeleted, param));
            }
            else
            {
                // Both MustHaveTenant and SoftDeletable
                var param = Expression.Parameter(clrType, "e");
                var tenantProp = Expression.Call(typeof(EF), nameof(EF.Property), [typeof(Guid)], param, Expression.Constant("TenantId"));
                var contextExpr = Expression.Constant(tenantContext);
                var isIgnored = Expression.Property(contextExpr, nameof(ITenantContext.IsTenantFilterIgnored));
                var currentTenant = Expression.Property(contextExpr, nameof(ITenantContext.TenantId));
                var tenantFilter = Expression.OrElse(isIgnored, Expression.Equal(tenantProp, currentTenant));

                var isDeletedProp = Expression.Call(typeof(EF), nameof(EF.Property), [typeof(bool)], param, Expression.Constant("IsDeleted"));
                var softDeleteFilter = Expression.Not(isDeletedProp);

                var combined = Expression.AndAlso(tenantFilter, softDeleteFilter);
                entityType.SetQueryFilter(Expression.Lambda(combined, param));
            }
        }
    }
}
