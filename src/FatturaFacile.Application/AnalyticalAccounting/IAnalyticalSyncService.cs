// <copyright file="IAnalyticalSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// Synchronizes the analytical cost accounting module with the web-api: sends the entries queued offline, then pulls the read models
/// into the local cache.
/// </summary>
public interface IAnalyticalSyncService
{
    /// <summary>
    /// Flushes the queue and refreshes the cache.
    /// </summary>
    /// <param name="from">The first posting date of the reports, YYYY-MM-DD, or null.</param>
    /// <param name="to">The last posting date of the reports, YYYY-MM-DD, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<AnalyticalSyncResult> SyncAsync(string? from, string? to, CancellationToken cancellationToken);
}
