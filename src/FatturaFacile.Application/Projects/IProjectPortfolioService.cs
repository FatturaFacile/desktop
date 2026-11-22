// <copyright file="IProjectPortfolioService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The offline-first project portfolio and WBS service (Mod 078): every change is validated, applied to the local database, rolled
/// up, scheduled with CPM and queued for the web-api; reads never need the network.
/// </summary>
public interface IProjectPortfolioService
{
    /// <summary>
    /// Replays the queue, optionally refreshes from the web-api, and reads the portfolio.
    /// </summary>
    /// <param name="refresh">Whether to pull the web-api first.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The portfolio.</returns>
    public Task<ProjectPortfolio> LoadPortfolioAsync(bool refresh, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the plan of a project from the local database.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The plan, or null for an unknown project.</returns>
    public Task<ProjectPlan?> LoadPlanAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Compares the plan of a project with one of its baselines.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="baselineId">The baseline identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The variance, or null when the project or the baseline is unknown.</returns>
    public Task<BaselineVariance?> LoadVarianceAsync(Guid projectId, Guid baselineId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a project.
    /// </summary>
    /// <param name="project">The project; the rolled-up figures are recomputed.</param>
    /// <param name="commandId">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ProjectActionResult> SaveProjectAsync(Project project, Guid commandId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a WBS node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="commandId">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ProjectActionResult> SaveNodeAsync(WbsNode node, Guid commandId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a WBS node with its descendants and the dependencies that touch them.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="nodeId">The node identifier.</param>
    /// <param name="commandId">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ProjectActionResult> DeleteNodeAsync(Guid projectId, Guid nodeId, Guid commandId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a dependency; a link that would close a cycle is rejected.
    /// </summary>
    /// <param name="dependency">The dependency.</param>
    /// <param name="commandId">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ProjectActionResult> SaveDependencyAsync(TaskDependency dependency, Guid commandId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a dependency.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="dependencyId">The dependency identifier.</param>
    /// <param name="commandId">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ProjectActionResult> DeleteDependencyAsync(Guid projectId, Guid dependencyId, Guid commandId, CancellationToken cancellationToken);

    /// <summary>
    /// Freezes the current plan of a project as a new baseline.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="name">The baseline name; a numbered default is used when empty.</param>
    /// <param name="commandId">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result, whose entity is the baseline.</returns>
    public Task<ProjectActionResult> CaptureBaselineAsync(Guid projectId, string name, Guid commandId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a project from a template or from an existing project (#186).
    /// </summary>
    /// <param name="request">The choices of the user.</param>
    /// <param name="commandId">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result, whose entity is the new project.</returns>
    public Task<ProjectActionResult> InstantiateTemplateAsync(TemplateInstantiationRequest request, Guid commandId, CancellationToken cancellationToken);

    /// <summary>
    /// Replays the queued changes in order; offline they stay queued.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<ProjectFlushResult> FlushAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes the local portfolio from the web-api, keeping projects that have undelivered changes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<ProjectPullResult> PullAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Discards a refused change.
    /// </summary>
    /// <param name="entryId">The outbox entry identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task DismissConflictAsync(Guid entryId, CancellationToken cancellationToken);
}
