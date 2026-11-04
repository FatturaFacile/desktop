using Microsoft.EntityFrameworkCore;
using FatturaFacile.Identity.Domain.Aggregates;
using FatturaFacile.Identity.Domain.Entities;
using FatturaFacile.Identity.Infrastructure.Configurations;
using FatturaFacile.Infrastructure.Common.Tenancy;

namespace FatturaFacile.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Identity bounded context.
/// </summary>
public class IdentityDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SecurityAuditLog> SecurityAuditLogs => Set<SecurityAuditLog>();

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, ITenantContext? tenantContext = null)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new SecurityAuditLogConfiguration());

        if (_tenantContext != null)
        {
            modelBuilder.Entity<User>().HasQueryFilter(u =>
                _tenantContext.IsTenantFilterIgnored || u.TenantId == _tenantContext.TenantId);
        }
    }
}
