// <copyright file="ISupplierRebatesCache.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Local cache of the supplier rebate and blanket agreement payloads of the active tenant, so the cockpit reads offline.
/// </summary>
public interface ISupplierRebatesCache
{
    /// <summary>
    /// Finds a cached payload.
    /// </summary>
    /// <param name="kind">The kind of payload (see <see cref="RebateCacheKinds"/>).</param>
    /// <param name="key">The key of the record.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored payload, or null when nothing was pulled yet.</returns>
    public Task<StoredRebatePayload?> FindAsync(string kind, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Stores payloads, replacing the previous ones, in a single save so a reader sees the old set or the new one, never a mix.
    /// </summary>
    /// <param name="entries">The payloads.</param>
    /// <param name="syncedAtUtc">When they were pulled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveAsync(IReadOnlyList<RebateCacheEntry> entries, DateTime syncedAtUtc, CancellationToken cancellationToken);
}
