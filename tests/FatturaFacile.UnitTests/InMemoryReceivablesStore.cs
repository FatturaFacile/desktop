// <copyright file="InMemoryReceivablesStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.UnitTests;

/// <summary>
/// An in-memory <see cref="IReceivablesStore"/> for the tests of the receivables service.
/// </summary>
internal sealed class InMemoryReceivablesStore : IReceivablesStore
{
    /// <summary>
    /// Gets the cached answers by key.
    /// </summary>
    public Dictionary<string, LocalReceivablesCache> Cache { get; } = [with(StringComparer.Ordinal)];

    /// <summary>
    /// Gets the queued actions.
    /// </summary>
    public List<ReceivablesOutboxEntry> Entries { get; } = [];

    /// <summary>
    /// Gets the prefixes the cache was asked to trim.
    /// </summary>
    public List<string> TrimmedPrefixes { get; } = [];

    /// <inheritdoc/>
    public Task<LocalReceivablesCache?> FindCacheAsync(string key, CancellationToken cancellationToken) =>
        Task.FromResult(Cache.GetValueOrDefault(key));

    /// <inheritdoc/>
    public Task SaveCacheAsync(string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        Cache[key] = new LocalReceivablesCache { CacheKey = key, Json = json, SyncedAtUtc = syncedAtUtc };

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task TrimCacheAsync(string prefix, int keep, CancellationToken cancellationToken)
    {
        TrimmedPrefixes.Add(prefix);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<ReceivablesOutboxEntry> EnqueueAsync(ReceivablesOutboxEntry entry, CancellationToken cancellationToken)
    {
        var existing = Entries.Find(candidate => candidate.IdempotencyKey == entry.IdempotencyKey);
        if (existing is not null)
        {
            return Task.FromResult(existing);
        }

        Entries.Add(entry);

        return Task.FromResult(entry);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ReceivablesOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ReceivablesOutboxEntry>>([.. Entries.Where(entry => entry.Status == ReceivablesOutboxStatus.Pending).OrderBy(entry => entry.OccurredAtUtc)]);

    /// <inheritdoc/>
    public Task<IReadOnlyList<ReceivablesOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ReceivablesOutboxEntry>>([.. Entries.Where(entry => entry.Status == ReceivablesOutboxStatus.Conflict)]);

    /// <inheritdoc/>
    public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
