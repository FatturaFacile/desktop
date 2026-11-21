// <copyright file="IDssSnapshotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// Local cache of the Executive DSS snapshots of the active tenant, one per filter.
/// </summary>
public interface IDssSnapshotStore
{
    /// <summary>
    /// Finds the snapshot cached for a filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored payload, or null when nothing was pulled for the filter yet.</returns>
    public Task<StoredDssPayload?> FindAsync(DssFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the snapshot of a filter, replacing the previous one in a single save (a reader sees the old or the new, never a mix).
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <param name="payload">The payload.</param>
    /// <param name="syncedAtUtc">When it was pulled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveAsync(DssFilter filter, DssPayload payload, DateTime syncedAtUtc, CancellationToken cancellationToken);
}
