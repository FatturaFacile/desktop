// <copyright file="IProjectEvmSnapshotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// Local cache of the profitability snapshots of the active tenant, one per project, and of the project picker.
/// </summary>
public interface IProjectEvmSnapshotStore
{
    /// <summary>
    /// Finds the snapshot cached for a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored payload, or null when nothing was pulled for the project yet.</returns>
    public Task<StoredProjectEvmPayload?> FindAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the snapshot of a project, replacing the previous one in a single save.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="payload">The payload.</param>
    /// <param name="syncedAtUtc">When it was pulled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveAsync(string projectId, ProjectEvmPayload payload, DateTime syncedAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Finds the cached project picker.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array of the projects, or null when it was never pulled.</returns>
    public Task<string?> FindProjectsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stores the project picker.
    /// </summary>
    /// <param name="projectsJson">The `data` array of the projects.</param>
    /// <param name="syncedAtUtc">When it was pulled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveProjectsAsync(string projectsJson, DateTime syncedAtUtc, CancellationToken cancellationToken);
}
