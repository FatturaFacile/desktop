// <copyright file="IProjectEvmApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// Read access to the profitability and earned value endpoints of the web-api (`/api/v1/projects/{id}/evm`). Every method
/// returns the raw `data` object of the answer.
/// </summary>
public interface IProjectEvmApiClient
{
    /// <summary>
    /// Gets a value indicating whether a user is signed in to the web-api.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Reads the WBS cost breakdown of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetBreakdownAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the profitability waterfall of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetWaterfallAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the monthly S-curve of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetScurveAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the projects of the picker.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetProjectsAsync(CancellationToken cancellationToken);
}
