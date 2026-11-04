// <copyright file="ITenantRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Repositories;
using FatturaFacile.Identity.Domain.Aggregates;

namespace FatturaFacile.Identity.Domain.Repositories;

/// <summary>
/// Domain repository contract for managing <see cref="Tenant"/> entities.
/// </summary>
public interface ITenantRepository : IRepository
{
    /// <summary>
    /// Retrieves a tenant by its unique primary key identifier.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching <see cref="Tenant"/> if found; otherwise, null.</returns>
    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tenant by its unique URL slug.
    /// </summary>
    /// <param name="slug">The tenant slug.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching <see cref="Tenant"/> if found; otherwise, null.</returns>
    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new tenant aggregate root to the store.
    /// </summary>
    /// <param name="tenant">The tenant aggregate root.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing tenant in the store.
    /// </summary>
    /// <param name="tenant">The modified tenant aggregate root.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
