// <copyright file="IProcurementApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Port to the purchase order fulfilment endpoints of the web-api (/api/v1/procurement, Mod 046).
/// </summary>
public interface IProcurementApiClient
{
    /// <summary>
    /// Gets a value indicating whether a session exists.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Reads a route.
    /// </summary>
    /// <param name="route">The route relative to the API root, with its query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The body of the answer.</returns>
    /// <exception cref="System.Net.Http.HttpRequestException">The server could not be reached or refused the request.</exception>
    public Task<string> GetAsync(string route, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a write.
    /// </summary>
    /// <param name="method">The HTTP method: POST, PUT or DELETE.</param>
    /// <param name="route">The route relative to the API root.</param>
    /// <param name="jsonBody">The JSON body, null for none.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The raw answer.</returns>
    public Task<ProcurementApiReply> SendAsync(string method, string route, string? jsonBody, CancellationToken cancellationToken);
}
