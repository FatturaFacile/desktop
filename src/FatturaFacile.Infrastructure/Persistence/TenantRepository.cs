// <copyright file="TenantRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core repository implementation for tenant organizations, access mappings, and workspace queries.
/// </summary>
public class TenantRepository : RepositoryBase<Tenant>, ITenantRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TenantRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Tenant>> GetTenantsForWindowsUserAsync(string windowsUsername, CancellationToken cancellationToken = default)
    {
        var username = windowsUsername.Trim();
        var normalized = username.Contains('\\') ? username.Split('\\')[1] : username;

        return await Context.Tenants
            .IgnoreQueryFilters()
            .Where(t => !t.IsDeleted && t.TenantUsers.Any(tu => 
                tu.User != null && 
                !tu.User.IsDeleted && 
                (tu.User.WindowsUsername.ToLower() == username.ToLower() || tu.User.WindowsUsername.ToLower() == normalized.ToLower())))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }
}
