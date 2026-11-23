// <copyright file="IProcurementStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Port to the local cache and outbox of the purchase order fulfilment.
/// </summary>
public interface IProcurementStore
{
    /// <summary>
    /// Finds a cached answer.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The row, or null.</returns>
    public Task<LocalProcurementCache?> FindCacheAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Saves an answer, replacing the one of the same key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="json">The body of the answer.</param>
    /// <param name="syncedAtUtc">The instant it was pulled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveCacheAsync(string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Keeps the most recent rows of a key prefix and deletes the others.
    /// </summary>
    /// <param name="prefix">The key prefix.</param>
    /// <param name="keep">The rows to keep.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the trim.</returns>
    public Task TrimCacheAsync(string prefix, int keep, CancellationToken cancellationToken);

    /// <summary>
    /// Queues an action; when an entry with the same idempotency key exists it is returned instead of a duplicate.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The queued entry.</returns>
    public Task<ProcurementOutboxEntry> EnqueueAsync(ProcurementOutboxEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the entries waiting to be sent, oldest first.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entries.</returns>
    public Task<IReadOnlyList<ProcurementOutboxEntry>> PendingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the entries the server refused.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entries.</returns>
    public Task<IReadOnlyList<ProcurementOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the changes of the tracked entries.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the save.</returns>
    public Task SaveAsync(CancellationToken cancellationToken);
}
