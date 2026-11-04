// <copyright file="UserRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Domain.Aggregates;
using FatturaFacile.Identity.Domain.Repositories;
using FatturaFacile.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Identity.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the <see cref="IUserRepository"/> interface.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserRepository"/> class.
/// </remarks>
/// <param name="context">The identity database context.</param>
public sealed class UserRepository(IdentityDbContext context) : IUserRepository
{
    private readonly IdentityDbContext _context = context;

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
           .IgnoreQueryFilters()
           .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByNormalizedEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return await _context.Users
           .IgnoreQueryFilters()
           .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _ = await _context.Users.AddAsync(user, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _ = _context.Users.Update(user);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }
}
