// <copyright file="IAnalyticalSnapshotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// The local cache of the analytical read models for the active tenant, so they can be read offline.
/// </summary>
public interface IAnalyticalSnapshotStore
{
    /// <summary>
    /// Finds a cached read model.
    /// </summary>
    /// <param name="key">One of the snapshot keys of <see cref="AnalyticalContract"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The payload, or null when it was never pulled.</returns>
    public Task<StoredAnalyticalPayload?> FindAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a read model, replacing the previous one.
    /// </summary>
    /// <param name="key">The snapshot key.</param>
    /// <param name="json">The payload.</param>
    /// <param name="syncedAtUtc">When it was pulled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveAsync(string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken);
}
