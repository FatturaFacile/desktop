// <copyright file="IPermissionCache.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Security.Iam;

/// <summary>
/// Local, encrypted store of permission snapshots, one per tenant, user and company, so authorization keeps working
/// offline. A snapshot revoked by the server is never returned.
/// </summary>
public interface IPermissionCache
{
    /// <summary>
    /// Stores the snapshot, replacing the previous one of the same tenant, user and company.
    /// </summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task.</returns>
    public Task SaveAsync(PermissionSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the snapshot of the tenant, user and company, or null when there is none or it was revoked.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="companyId">The company, or null for the whole tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The snapshot, or null.</returns>
    public Task<PermissionSnapshot?> LoadAsync(Guid tenantId, Guid userId, Guid? companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks every snapshot of the user as revoked (server answered 401 or revoked the company access).
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task.</returns>
    public Task RevokeUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes every snapshot (sign-out on a shared machine).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task.</returns>
    public Task PurgeAsync(CancellationToken cancellationToken = default);
}
