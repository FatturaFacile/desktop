// <copyright file="TenantRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository managing company tenant configurations, multi-branch setups, and tenant activation states.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class TenantRepository(FatturaFacileDbContext dbContext)
    : Repository<Tenant, Guid>(dbContext), ITenantRepository
{
    /// <summary>
    /// Finds a tenant by its unique alphanumeric organization code (e.g., "TENANT-IT", "TENANT-CH").
    /// </summary>
    /// <param name="code">The tenant code to match.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="Tenant"/> entity if found; otherwise, <see langword="null"/>.</returns>
    public Task<Tenant?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(
            t => t.Code.Equals(
                code,
                StringComparison.OrdinalIgnoreCase),
            cancellationToken);

    /// <summary>
    /// Retrieves all active tenant organizations registered in the system.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of active <see cref="Tenant"/> entities.</returns>
    public async Task<IReadOnlyList<Tenant>> GetActiveTenantsAsync(
        CancellationToken cancellationToken = default) =>
        await DbSet.Where(t => t.IsActive).ToListAsync(cancellationToken);
}
