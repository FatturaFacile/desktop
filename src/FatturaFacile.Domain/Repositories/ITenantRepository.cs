// <copyright file="ITenantRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Exposes administrative persistence and query operations for <see cref="Tenant"/> aggregates managing tenant workspace boundaries and configurations.
/// </summary>
public interface ITenantRepository : IRepository<Tenant, Guid>
{
    /// <summary>
    /// Asynchronously retrieves an administrative tenant partition matching the specified unique business code or slug.
    /// </summary>
    /// <param name="code">The alphanumeric unique tenant identifier or code key (e.g., company handle).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous lookup; contains the matching <see cref="Tenant"/> if found, or <see langword="null"/> otherwise.</returns>
    public Task<Tenant?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves all active, operational tenants eligible for user authentication and billing operations.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous query; contains a read-only list of active <see cref="Tenant"/> aggregates.</returns>
    public Task<IReadOnlyList<Tenant>> GetActiveTenantsAsync(
        CancellationToken cancellationToken = default);
}
