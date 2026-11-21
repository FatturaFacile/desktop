// <copyright file="IMesApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// Typed client of the web-api MES operator board (`/api/v1/manufacturing/mes`). Network failures surface as
/// <see cref="HttpRequestException"/> without a status code.
/// </summary>
public interface IMesApiClient
{
    /// <summary>
    /// Gets a value indicating whether a server session exists.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Fetches the board of a work centre.
    /// </summary>
    /// <param name="workCenterId">The work centre.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object of the answer.</returns>
    public Task<string> GetBoardAsync(Guid workCenterId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the work centres of the company.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array of the answer.</returns>
    public Task<string> GetWorkCentersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sends a command: a POST of the body to the route, whatever the answer.
    /// </summary>
    /// <param name="route">The route relative to the API prefix.</param>
    /// <param name="jsonBody">The body, which carries the idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The answer.</returns>
    public Task<MesApiReply> SendCommandAsync(string route, string jsonBody, CancellationToken cancellationToken);
}
