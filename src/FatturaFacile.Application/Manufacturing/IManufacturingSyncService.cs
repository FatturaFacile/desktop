// <copyright file="IManufacturingSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// Pulls the Mod 061 manufacturing master data and OEE summaries from the web-api into the local cache.
/// </summary>
public interface IManufacturingSyncService
{
    /// <summary>
    /// Pulls and caches the data. It never throws for network or server failures; they are reported in the result.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<ManufacturingSyncResult> SyncAsync(CancellationToken cancellationToken);
}
