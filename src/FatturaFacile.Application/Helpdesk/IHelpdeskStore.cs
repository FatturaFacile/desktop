// <copyright file="IHelpdeskStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// Local persistence of the helpdesk: the cache of the last answers and the outbox of queued actions, scoped to the active tenant.
/// </summary>
public interface IHelpdeskStore
{
    /// <summary>
    /// Finds a cached answer.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The row, or null.</returns>
    public Task<LocalHelpdeskCache?> FindCacheAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Saves an answer, replacing the one of the same key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="json">The JSON.</param>
    /// <param name="syncedAtUtc">The instant it was pulled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the answer is saved.</returns>
    public Task SaveCacheAsync(string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Keeps only the most recent rows whose key starts with a prefix.
    /// </summary>
    /// <param name="prefix">The key prefix.</param>
    /// <param name="keep">The number of rows to keep.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the old rows are removed.</returns>
    public Task TrimCacheAsync(string prefix, int keep, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a new outbox entry.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the entry is saved.</returns>
    public Task EnqueueAsync(HelpdeskOutboxEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the entries waiting to be sent, oldest first.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entries.</returns>
    public Task<IReadOnlyList<HelpdeskOutboxEntry>> PendingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the entries the server refused and the user has not dismissed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entries.</returns>
    public Task<IReadOnlyList<HelpdeskOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the changes of the tracked entries.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the changes are saved.</returns>
    public Task SaveAsync(CancellationToken cancellationToken);
}
