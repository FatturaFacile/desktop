// <copyright file="IDssSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// Pulls the Executive DSS snapshots of the web-api into the local cache.
/// </summary>
public interface IDssSyncService
{
    /// <summary>
    /// Pulls the overview, the profitability matrix and the variance bridge of a filter and stores them together. A failure
    /// leaves the cached snapshot untouched, so the cockpit keeps working offline on the last data.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the attempt ended; never throws for HTTP or network failures.</returns>
    public Task<DssSyncResult> SyncAsync(DssFilter filter, CancellationToken cancellationToken);
}
