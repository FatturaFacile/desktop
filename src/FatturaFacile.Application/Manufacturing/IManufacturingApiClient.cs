// <copyright file="IManufacturingApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// Typed client of the web-api manufacturing endpoints (`/api/v1/manufacturing`). Every method returns the `data` payload as
/// received. Network and HTTP failures surface as <see cref="HttpRequestException"/> (with the status code when the server answered).
/// </summary>
public interface IManufacturingApiClient
{
    /// <summary>
    /// Gets a value indicating whether a server session exists.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Fetches every page of a list endpoint and merges the items.
    /// </summary>
    /// <param name="resource">The resource path under `manufacturing` (work-centers, machines, working-calendars or routings).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A JSON array with every item.</returns>
    public Task<string> GetListAsync(string resource, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the OEE summary of a work centre over a period.
    /// </summary>
    /// <param name="workCenterId">The work centre.</param>
    /// <param name="fromUtc">The start of the period.</param>
    /// <param name="toUtc">The end of the period.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetOeeSummaryAsync(Guid workCenterId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
}
