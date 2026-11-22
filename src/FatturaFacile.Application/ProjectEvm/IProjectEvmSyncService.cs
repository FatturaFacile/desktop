// <copyright file="IProjectEvmSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// Pulls the profitability snapshots from the web-api into the local cache.
/// </summary>
public interface IProjectEvmSyncService
{
    /// <summary>
    /// Pulls the snapshot of a project; a failure leaves the cache untouched.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the attempt ended.</returns>
    public Task<ProjectEvmSyncResult> SyncAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Pulls the project picker.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the attempt ended.</returns>
    public Task<ProjectEvmSyncResult> SyncProjectsAsync(CancellationToken cancellationToken);
}
