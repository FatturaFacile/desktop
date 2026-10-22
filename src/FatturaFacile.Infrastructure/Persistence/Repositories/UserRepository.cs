// <copyright file="UserRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository managing user authentication records and credentials, bypassing global tenant filters during authentication lookups.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class UserRepository(FatturaFacileDbContext dbContext)
    : Repository<User, Guid>(dbContext: dbContext), IUserRepository
{

    /// <summary>
    /// Finds a user across all tenants by their unique login username, ignoring multi-tenant global query filters.
    /// </summary>
    /// <param name="username">The case-insensitive username to match.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="User"/> entity if found; otherwise, <see langword="null"/>.</returns>
    public Task<User?> GetByUsernameAsync(
        string            username,
        CancellationToken cancellationToken = default) => DbSet
                                                         .IgnoreQueryFilters()
                                                         .FirstOrDefaultAsync(
                                                                              predicate: u => u.Username.Equals(
                                                                                                                username,
                                                                                                                StringComparison
                                                                                                                   .OrdinalIgnoreCase),
                                                                              cancellationToken: cancellationToken);

    /// <summary>
    /// Finds a user across all tenants by their registered email address, ignoring multi-tenant global query filters.
    /// </summary>
    /// <param name="email">The case-insensitive email address to match.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="User"/> entity if found; otherwise, <see langword="null"/>.</returns>
    public Task<User?> GetByEmailAsync(
        string            email,
        CancellationToken cancellationToken = default) => DbSet
                                                         .IgnoreQueryFilters()
                                                         .FirstOrDefaultAsync(
                                                                              predicate: u => u.Email.Equals(
                                                                                                             email,
                                                                                                             StringComparison
                                                                                                                .OrdinalIgnoreCase),
                                                                              cancellationToken: cancellationToken);

}
