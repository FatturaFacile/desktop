// <copyright file="IdentityDbContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Domain.Aggregates;
using FatturaFacile.Identity.Domain.Entities;
using FatturaFacile.Identity.Infrastructure.Configurations;
using FatturaFacile.Infrastructure.Common.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Identity module: tenants, users, refresh tokens, and security audits.
/// </summary>
/// <param name="options">The DbContext options.</param>
/// <param name="tenantContext">The tenant context for multitenant filtering.</param>
public sealed class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options,
    ITenantContext? tenantContext = null)
    : DbContext(options)
{
    private readonly ITenantContext? _tenantContext = tenantContext;

    /// <summary>
    /// Gets the database set for tenant organizations.
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>
    /// Gets the database set for user profiles.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets the database set for refresh tokens.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Gets the database set for security audit logs.
    /// </summary>
    public DbSet<SecurityAuditLog> SecurityAuditLogs => Set<SecurityAuditLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.ApplyConfiguration(new TenantConfiguration());
        _ = modelBuilder.ApplyConfiguration(new UserConfiguration());
        _ = modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        _ = modelBuilder.ApplyConfiguration(new SecurityAuditLogConfiguration());

        if (_tenantContext != null)
        {
            _ = modelBuilder.Entity<User>().HasQueryFilter(u =>
                _tenantContext.IsTenantFilterIgnored || u.TenantId == _tenantContext.TenantId);
        }
    }
}
