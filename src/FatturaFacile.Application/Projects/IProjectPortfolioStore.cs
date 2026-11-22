// <copyright file="IProjectPortfolioStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The local database of the project portfolio and its outbox. Reads return detached copies of the current tenant.
/// </summary>
public interface IProjectPortfolioStore
{
    /// <summary>
    /// Reads every project and template.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The projects.</returns>
    public Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Finds a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project, or null.</returns>
    public Task<Project?> FindProjectAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the nodes of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The nodes.</returns>
    public Task<IReadOnlyList<WbsNode>> GetNodesAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the dependencies of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dependencies.</returns>
    public Task<IReadOnlyList<TaskDependency>> GetDependenciesAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the baselines of a project, newest first.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The baselines.</returns>
    public Task<IReadOnlyList<ProjectBaseline>> GetBaselinesAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a project and replaces its nodes and dependencies with the given ones (rows that are not in the lists are deleted).
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="nodes">The complete node list.</param>
    /// <param name="dependencies">The complete dependency list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SavePlanAsync(Project project, IReadOnlyList<WbsNode> nodes, IReadOnlyList<TaskDependency> dependencies, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces the baselines of a project with the given ones.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="baselines">The complete baseline list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveBaselinesAsync(Guid projectId, IReadOnlyList<ProjectBaseline> baselines, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a baseline.
    /// </summary>
    /// <param name="baseline">The baseline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task AddBaselineAsync(ProjectBaseline baseline, CancellationToken cancellationToken);

    /// <summary>
    /// Queues a change.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task EnqueueAsync(ProjectOutboxEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the tracked entries waiting to be sent, oldest first.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entries.</returns>
    public Task<IReadOnlyList<ProjectOutboxEntry>> PendingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the tracked entries the web-api refused.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entries.</returns>
    public Task<IReadOnlyList<ProjectOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Tells whether a project has changes that were not delivered (waiting or refused), so a refresh must not overwrite it.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the project has undelivered changes.</returns>
    public Task<bool> HasUnsentChangesAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Saves the changes made to tracked outbox entries.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveAsync(CancellationToken cancellationToken);
}
