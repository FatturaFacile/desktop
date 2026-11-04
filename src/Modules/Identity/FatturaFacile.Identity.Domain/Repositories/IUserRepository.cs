// <copyright file="IUserRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Repositories;
using FatturaFacile.Identity.Domain.Aggregates;

namespace FatturaFacile.Identity.Domain.Repositories;

/// <summary>
/// Domain repository contract for managing <see cref="User"/> entities.
/// </summary>
public interface IUserRepository : IRepository
{
    /// <summary>
    /// Retrieves a user by their primary key identifier across all tenants.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching <see cref="User"/> if found; otherwise, null.</returns>
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user within a specific tenant by their blind-index normalized email hash.
    /// </summary>
    /// <param name="tenantId">The owning tenant identifier.</param>
    /// <param name="normalizedEmail">The SHA-256 blind-index hash of the email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching <see cref="User"/> if found; otherwise, null.</returns>
    public Task<User?> GetByNormalizedEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user aggregate root to the store.
    /// </summary>
    /// <param name="user">The user instance to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user in the store.
    /// </summary>
    /// <param name="user">The modified user instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
