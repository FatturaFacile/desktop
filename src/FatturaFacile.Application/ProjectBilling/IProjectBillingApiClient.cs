// <copyright file="IProjectBillingApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// Read access to the project billing endpoints of the web-api (`/api/v1/projects/{id}`). Every method returns the raw `data`
/// of the answer; the optional ones return the JSON text `null` when the project has no billing contract yet.
/// </summary>
public interface IProjectBillingApiClient
{
    /// <summary>
    /// Gets a value indicating whether a user is signed in to the web-api.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Reads what the project has earned but not billed.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetSummaryAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the billing contract of the project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object, or `null`.</returns>
    public Task<string> GetContractAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the SAL certificates of the project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetCertificatesAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the guarantee retention of the project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object, or `null` when the project has no contract.</returns>
    public Task<string> GetRetentionAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the prepaid retainer of the project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object, or `null` when the project has no contract.</returns>
    public Task<string> GetRetainerAsync(string projectId, CancellationToken cancellationToken);
}
