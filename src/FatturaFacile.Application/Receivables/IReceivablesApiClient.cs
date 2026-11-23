// <copyright file="IReceivablesApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Port to the receivables endpoints of the web-api (/api/v1/invoicing/receivables).
/// </summary>
public interface IReceivablesApiClient
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
    /// Downloads a file.
    /// </summary>
    /// <param name="route">The route relative to the API root.</param>
    /// <param name="fileName">The name to give the file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The file.</returns>
    /// <exception cref="System.Net.Http.HttpRequestException">The server could not be reached or refused the request.</exception>
    public Task<ReceivablesFile> DownloadAsync(string route, string fileName, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a write.
    /// </summary>
    /// <param name="method">The HTTP method: POST, PUT or DELETE.</param>
    /// <param name="route">The route relative to the API root.</param>
    /// <param name="jsonBody">The JSON body, null for none.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The raw answer.</returns>
    public Task<ReceivablesApiReply> SendAsync(string method, string route, string? jsonBody, CancellationToken cancellationToken);
}
