// <copyright file="TenantRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Domain.Aggregates;
using FatturaFacile.Identity.Domain.Repositories;
using FatturaFacile.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Identity.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the <see cref="ITenantRepository"/> interface.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TenantRepository"/> class.
/// </remarks>
/// <param name="context">The identity database context.</param>
public sealed class TenantRepository(IdentityDbContext context) : ITenantRepository
{
    private readonly IdentityDbContext _context = context;

    /// <inheritdoc />
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _context.Tenants.FirstOrDefaultAsync(t => t.Slug == slug.ToLower(), cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _ = await _context.Tenants.AddAsync(tenant, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _ = _context.Tenants.Update(tenant);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }
}
