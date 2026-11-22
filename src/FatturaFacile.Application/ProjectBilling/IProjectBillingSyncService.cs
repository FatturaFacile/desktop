// <copyright file="IProjectBillingSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// Pulls the project billing snapshots from the web-api into the local cache.
/// </summary>
public interface IProjectBillingSyncService
{
    /// <summary>
    /// Pulls the billing snapshot of a project; a failure leaves the cache untouched.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the attempt ended.</returns>
    public Task<ProjectBillingSyncResult> SyncAsync(string projectId, CancellationToken cancellationToken);
}
