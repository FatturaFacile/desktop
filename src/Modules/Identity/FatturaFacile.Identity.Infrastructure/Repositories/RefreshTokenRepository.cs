// <copyright file="RefreshTokenRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Domain.Entities;
using FatturaFacile.Identity.Domain.Repositories;
using FatturaFacile.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Identity.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the <see cref="IRefreshTokenRepository"/> interface.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RefreshTokenRepository"/> class.
/// </remarks>
/// <param name="context">The identity database context.</param>
public sealed class RefreshTokenRepository(IdentityDbContext context) : IRefreshTokenRepository
{
    private readonly IdentityDbContext _context = context;

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
           .FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _ = await _context.RefreshTokens.AddAsync(token, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _ = _context.RefreshTokens.Update(token);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }
}
