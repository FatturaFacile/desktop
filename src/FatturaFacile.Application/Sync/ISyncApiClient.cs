// <copyright file="ISyncApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Sync;

/// <summary>
/// Typed client of the web-api sync surface.
/// </summary>
public interface ISyncApiClient
{
    /// <summary>
    /// Gets a value indicating whether a session token is available.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Signs in against POST /api/v1/auth/login and selects the first tenant.
    /// </summary>
    /// <param name="email">The user e-mail.</param>
    /// <param name="password">The user password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when the credentials were accepted.</returns>
    public Task<bool> LoginAsync(string email, string password, CancellationToken cancellationToken);

    /// <summary>
    /// Ends the local session.
    /// </summary>
    public void Logout();

    /// <summary>
    /// Fetches one page of the delta feed. Network failures surface as <see cref="HttpRequestException"/>.
    /// </summary>
    /// <param name="entity">The sync entity name.</param>
    /// <param name="updatedSince">Only records updated at or after this UTC instant.</param>
    /// <param name="cursor">The cursor of the previous page.</param>
    /// <param name="limit">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page.</returns>
    public Task<SyncDeltaPage> GetDeltaAsync(string entity, DateTime? updatedSince, string? cursor, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Replays one write with an idempotency key.
    /// </summary>
    /// <param name="request">The write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The classified outcome; never throws for HTTP or network failures.</returns>
    public Task<SyncSendOutcome> SendAsync(SyncRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an authenticated JSON request under /api/v1/ (bearer token, tenant header and one token refresh on 401).
    /// Network failures surface as <see cref="HttpRequestException"/>; HTTP error statuses are returned, not thrown.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="relativeUrl">The URL relative to /api/v1/ (for example "ai/suggestions").</param>
    /// <param name="jsonBody">The JSON body, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The status and body of the reply.</returns>
    public Task<SyncHttpReply> SendJsonAsync(HttpMethod method, string relativeUrl, string? jsonBody, CancellationToken cancellationToken);
}
