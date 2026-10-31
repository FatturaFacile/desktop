// <copyright file="ITenantRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Defines data access operations for tenant organizations, workspaces, and multi-tenant access mappings.
/// </summary>
public interface ITenantRepository : IRepository<Tenant>
{
    /// <summary>
    /// Retrieves all active tenant workspaces accessible to a specific Windows identity username.
    /// </summary>
    /// <param name="windowsUsername">The authenticated Windows user principal or account name.</param>
    /// <param name="cancellationToken">Cancellation token to observe during query execution.</param>
    /// <returns>A read-only collection of tenants the user is authorized to access.</returns>
    Task<IReadOnlyList<Tenant>> GetTenantsForWindowsUserAsync(string windowsUsername, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves tenant organization profile and configuration details by tenant identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tenant workspace.</param>
    /// <param name="cancellationToken">Cancellation token to observe during query execution.</param>
    /// <returns>The tenant entity if found; otherwise, null.</returns>
    Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
