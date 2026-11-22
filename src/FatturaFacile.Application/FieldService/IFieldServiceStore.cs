// <copyright file="IFieldServiceStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Local persistence of the Field Service module for the active tenant: the cache of the read models and the outbox of the
/// operations queued while offline.
/// </summary>
public interface IFieldServiceStore
{
    /// <summary>
    /// Finds a cached payload.
    /// </summary>
    /// <param name="kind">The kind (see <see cref="FieldServiceCacheKinds"/>).</param>
    /// <param name="key">The key inside the kind.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored payload, or null when it was never pulled.</returns>
    public Task<StoredFieldServicePayload?> FindAsync(string kind, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a payload, replacing the previous one of the same kind and key in a single save.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <param name="key">The key inside the kind.</param>
    /// <param name="json">The payload as received.</param>
    /// <param name="syncedAtUtc">When it was pulled (UTC).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveAsync(string kind, string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an operation to the outbox.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task EnqueueAsync(FieldServiceOutboxEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the operations waiting to be sent, in the order the technician acted.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pending entries.</returns>
    public Task<IReadOnlyList<FieldServiceOutboxEntry>> PendingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists the operations the server rejected and the user has not dismissed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conflicting entries.</returns>
    public Task<IReadOnlyList<FieldServiceOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the changes made to entries returned by this store.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveChangesAsync(CancellationToken cancellationToken);
}
