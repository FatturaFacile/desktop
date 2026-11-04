// <copyright file="IRefreshTokenRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Repositories;
using FatturaFacile.Identity.Domain.Entities;

namespace FatturaFacile.Identity.Domain.Repositories;

/// <summary>
/// Domain repository contract for persisting and rotating <see cref="RefreshToken"/> entities.
/// </summary>
public interface IRefreshTokenRepository : IRepository
{
    /// <summary>
    /// Retrieves a refresh token record by its SHA-256 token hash.
    /// </summary>
    /// <param name="tokenHash">The SHA-256 token hash.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching <see cref="RefreshToken"/> if found; otherwise, null.</returns>
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new refresh token to the store.
    /// </summary>
    /// <param name="token">The refresh token instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing refresh token in the store (e.g. to revoke or rotate).
    /// </summary>
    /// <param name="token">The modified refresh token instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default);
}
