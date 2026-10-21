// <copyright file="UserRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents user repository.
/// </summary>
/// <param name="dbContext">The db context.</param>
public sealed class UserRepository(FatturaFacileDbContext dbContext)
    : Repository<User, Guid>(dbContext), IUserRepository
{
    /// <summary>
    /// Get By Username Async.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<User?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default) => DbSet
        .IgnoreQueryFilters().FirstOrDefaultAsync(
            u => u.Username.Equals(
                username,
                StringComparison.OrdinalIgnoreCase),
            cancellationToken);

    /// <summary>
    /// Get By Email Async.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default) => DbSet
        .IgnoreQueryFilters().FirstOrDefaultAsync(
            u => u.Email.Equals(
                email,
                StringComparison.OrdinalIgnoreCase),
            cancellationToken);
}
