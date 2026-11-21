// <copyright file="ISyncStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Sync;

/// <summary>
/// Persistence port of the sync engine (outbox, checkpoints and per-record state), scoped to the active tenant.
/// </summary>
public interface ISyncStore
{
    /// <summary>
    /// Gets the last completed pull position of an entity.
    /// </summary>
    /// <param name="entityName">The sync entity name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The server time, or null before the first pass.</returns>
    public Task<DateTime?> GetCheckpointAsync(string entityName, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the pull position of an entity.
    /// </summary>
    /// <param name="entityName">The sync entity name.</param>
    /// <param name="serverTime">The server time.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the checkpoint is saved.</returns>
    public Task SaveCheckpointAsync(string entityName, DateTime serverTime, CancellationToken cancellationToken);

    /// <summary>
    /// Gets pending outbox entries that are due, oldest first.
    /// </summary>
    /// <param name="nowUtc">The current UTC instant.</param>
    /// <param name="max">The maximum number of entries.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The due entries.</returns>
    public Task<IReadOnlyList<SyncOutboxEntry>> GetDueEntriesAsync(DateTime nowUtc, int max, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the identifiers of records with a pending or conflicting local change.
    /// </summary>
    /// <param name="entityName">The sync entity name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The identifiers.</returns>
    public Task<IReadOnlySet<Guid>> GetProtectedEntityIdsAsync(string entityName, CancellationToken cancellationToken);

    /// <summary>
    /// Saves an outbox entry and mirrors its status on the record state.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the entry is saved.</returns>
    public Task SaveEntryAsync(SyncOutboxEntry entry, CancellationToken cancellationToken);
}
