// <copyright file="IProjectApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The web-api calls of the project portfolio.
/// </summary>
public interface IProjectApiClient
{
    /// <summary>
    /// Gets a value indicating whether a user is signed in to the web-api.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Reads the project list.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` JSON.</returns>
    public Task<string> GetProjectsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the WBS nodes of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` JSON.</returns>
    public Task<string> GetNodesAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the dependencies of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` JSON.</returns>
    public Task<string> GetDependenciesAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the baselines of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` JSON.</returns>
    public Task<string> GetBaselinesAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Sends one queued change.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="route">The relative route.</param>
    /// <param name="jsonBody">The body, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The answer.</returns>
    public Task<Sync.SyncApiReply> SendAsync(HttpMethod method, string route, string? jsonBody, CancellationToken cancellationToken);
}
