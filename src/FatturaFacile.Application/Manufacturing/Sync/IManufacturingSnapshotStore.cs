// <copyright file="IManufacturingSnapshotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.Manufacturing.Sync;

/// <summary>
/// Local store operations of the delta pull.
/// </summary>
public interface IManufacturingSnapshotStore
{
    /// <summary>
    /// Gets the delta checkpoint of a resource.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The server time of the last complete pull, or null.</returns>
    public Task<DateTime?> GetCheckpointAsync(string resource, CancellationToken cancellationToken);

    /// <summary>
    /// Saves the delta checkpoint of a resource.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="serverTime">The server time of the first page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveCheckpointAsync(string resource, DateTime serverTime, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the server version stamps already stored for a resource.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The map of server identifier to updated_at stamp.</returns>
    public Task<IReadOnlyDictionary<Guid, string>> GetKnownVersionsAsync(string resource, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the identifiers (local and server) of the orders that still have unsynchronised mutations; a pull never overwrites them.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The protected identifiers.</returns>
    public Task<IReadOnlySet<Guid>> GetProtectedOrderIdsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Upserts rows (already completed with their details) into the local tables.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="rows">The rows.</param>
    /// <param name="protectedOrderIds">The orders that must not be overwritten.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of rows applied and skipped.</returns>
    public Task<(int Applied, int Skipped)> ApplyAsync(string resource, IReadOnlyList<JsonElement> rows, IReadOnlySet<Guid> protectedOrderIds, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a cockpit snapshot.
    /// </summary>
    /// <param name="payloadJson">The cockpit JSON.</param>
    /// <param name="capturedAt">The fetch instant (UTC).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveTelemetryAsync(string payloadJson, DateTime capturedAt, CancellationToken cancellationToken);
}
