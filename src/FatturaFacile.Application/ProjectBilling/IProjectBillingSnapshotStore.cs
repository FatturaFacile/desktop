// <copyright file="IProjectBillingSnapshotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// Local cache of the project billing snapshots of the active tenant, one per project.
/// </summary>
public interface IProjectBillingSnapshotStore
{
    /// <summary>
    /// Finds the snapshot cached for a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored payload, or null when nothing was pulled for the project yet.</returns>
    public Task<StoredProjectBillingPayload?> FindAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the snapshot of a project, replacing the previous one in a single save.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="payload">The payload.</param>
    /// <param name="syncedAtUtc">When it was pulled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveAsync(string projectId, ProjectBillingPayload payload, DateTime syncedAtUtc, CancellationToken cancellationToken);
}
