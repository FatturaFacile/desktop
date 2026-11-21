// <copyright file="IManufacturingApiGateway.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Outbox;

namespace FatturaFacile.Application.Manufacturing.Sync;

/// <summary>
/// Port to the /api/v1/manufacturing endpoints of the web-api.
/// </summary>
public interface IManufacturingApiGateway
{
    /// <summary>
    /// Gets a value indicating whether a session token is available.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Sends one write (always POST) with the idempotency key. Never throws for HTTP or network failures.
    /// </summary>
    /// <param name="request">The write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The classified outcome.</returns>
    public Task<ManufacturingSendOutcome> SendAsync(ManufacturingRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches one page of a list resource. Network failures surface as <see cref="System.Net.Http.HttpRequestException"/>.
    /// </summary>
    /// <param name="resource">The resource route.</param>
    /// <param name="updatedSince">Only rows updated at or after this instant (the server may ignore it).</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page.</returns>
    public Task<ManufacturingPage> GetPageAsync(string resource, DateTime? updatedSince, int page, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the "data" object of a single resource. Network failures surface as <see cref="System.Net.Http.HttpRequestException"/>.
    /// </summary>
    /// <param name="route">The relative route.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The JSON text, or null when the server answered 404.</returns>
    public Task<string?> GetJsonAsync(string route, CancellationToken cancellationToken);
}
