// <copyright file="IManufacturingStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// Local cache of the manufacturing data of the active tenant.
/// </summary>
public interface IManufacturingStore
{
    /// <summary>
    /// Replaces the cache of the active tenant with freshly pulled data in a single transaction (a reader sees the old or the new
    /// data, never a mix; records the server no longer returns disappear).
    /// </summary>
    /// <param name="data">The pulled data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task ReplaceAsync(ManufacturingMasterData data, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the cache.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached data (empty before the first sync).</returns>
    public Task<ManufacturingOverview> GetOverviewAsync(CancellationToken cancellationToken);
}
