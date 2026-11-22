// <copyright file="IAnalyticalApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// Access to the analytical cost accounting endpoints of the web-api (`/api/v1/analytical`). The read methods return the raw `data`
/// element of the answer.
/// </summary>
public interface IAnalyticalApiClient
{
    /// <summary>
    /// Gets a value indicating whether a user is signed in to the web-api.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Reads the center tree.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetCentersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the apportionment run history.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` array.</returns>
    public Task<string> GetRunsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the contribution margin income statement.
    /// </summary>
    /// <param name="from">The first posting date, YYYY-MM-DD, or null.</param>
    /// <param name="to">The last posting date, YYYY-MM-DD, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetMarginAsync(string? from, string? to, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the direct-versus-apportioned cost comparison.
    /// </summary>
    /// <param name="from">The first posting date, YYYY-MM-DD, or null.</param>
    /// <param name="to">The last posting date, YYYY-MM-DD, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The `data` object.</returns>
    public Task<string> GetComparisonAsync(string? from, string? to, CancellationToken cancellationToken);

    /// <summary>
    /// Posts a manual analytical entry.
    /// </summary>
    /// <param name="json">The JSON body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome of the request.</returns>
    public Task<AnalyticalPostReply> PostEntryAsync(string json, CancellationToken cancellationToken);
}
