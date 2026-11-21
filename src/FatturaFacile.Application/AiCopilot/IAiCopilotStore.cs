// <copyright file="IAiCopilotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Persistence port of the AI copilot cache and offline decision queue, scoped to the active tenant.
/// </summary>
public interface IAiCopilotStore
{
    /// <summary>
    /// Gets the cached open suggestions, most urgent first.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The suggestions (not tracked).</returns>
    public Task<IReadOnlyList<AiSuggestion>> ListOpenAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Finds a cached suggestion.
    /// </summary>
    /// <param name="id">The suggestion identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tracked suggestion, or null.</returns>
    public Task<AiSuggestion?> FindAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the suggestions with a queued decision, oldest first.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tracked suggestions.</returns>
    public Task<IReadOnlyList<AiSuggestion>> GetQueuedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Counts the queued decisions.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of queued decisions.</returns>
    public Task<int> CountQueuedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Inserts or updates cached suggestions from server data, preserving queued decisions.
    /// </summary>
    /// <param name="items">The server suggestions.</param>
    /// <param name="nowUtc">The refresh instant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the rows are saved.</returns>
    public Task UpsertAsync(IReadOnlyList<AiSuggestionDto> items, DateTime nowUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Removes cached suggestions that are not in the server queue any more (rows with a queued decision are kept).
    /// </summary>
    /// <param name="serverIds">The identifiers of the open suggestions on the server.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the rows are removed.</returns>
    public Task RemoveMissingAsync(IReadOnlySet<Guid> serverIds, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a cached suggestion.
    /// </summary>
    /// <param name="id">The suggestion identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the row is removed.</returns>
    public Task RemoveAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Saves changes made to a tracked suggestion.
    /// </summary>
    /// <param name="suggestion">The suggestion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the row is saved.</returns>
    public Task SaveAsync(AiSuggestion suggestion, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the cached insights.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cache row, or null before the first fetch.</returns>
    public Task<AiInsightCache?> GetInsightsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Replaces the cached insights.
    /// </summary>
    /// <param name="payloadJson">The insights as JSON.</param>
    /// <param name="nowUtc">The fetch instant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the cache is saved.</returns>
    public Task SaveInsightsAsync(string payloadJson, DateTime nowUtc, CancellationToken cancellationToken);
}
