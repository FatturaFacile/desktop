// <copyright file="IUserRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Exposes specialized persistence and query operations for <see cref="User"/> identity and access accounts within the administrative boundary.
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{

    /// <summary>
    /// Asynchronously retrieves an administrative user account by unique username identifier.
    /// </summary>
    /// <param name="username">The unique username or login name to query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous lookup; contains the matching <see cref="User"/> if found, or <see langword="null"/> otherwise.</returns>
    public Task<User?> GetByUsernameAsync(
        string            username,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves an administrative user account by canonical email address.
    /// </summary>
    /// <param name="email">The primary email address associated with the user account.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous lookup; contains the matching <see cref="User"/> if found, or <see langword="null"/> otherwise.</returns>
    public Task<User?> GetByEmailAsync(
        string            email,
        CancellationToken cancellationToken = default);

}
