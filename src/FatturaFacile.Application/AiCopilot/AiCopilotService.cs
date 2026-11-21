// <copyright file="AiCopilotService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Offline-first AI copilot logic: refreshes the local cache of suggestions and insights, takes human decisions online
/// or queues them while the server is unreachable, and pushes the queue when the connection is back.
/// </summary>
/// <remarks>
/// Queue policy: 409 and 404 drop the queued decision (the suggestion is removed from the cache), 422 and other
/// refusals keep the suggestion pending and surface the server explanation, and network failures keep the decision queued.
/// </remarks>
/// <param name="api">The web-api client.</param>
/// <param name="store">The cache persistence port.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class AiCopilotService(IAiApiClient api, IAiCopilotStore store, TimeProvider timeProvider)
{
    private const int PerPage = 100;

    private static readonly JsonSerializerOptions CacheOptions = new();

    /// <summary>
    /// Reads the local cache.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached state.</returns>
    public async Task<AiCopilotSnapshot> LoadAsync(CancellationToken cancellationToken)
    {
        var suggestions = await store.ListOpenAsync(cancellationToken).ConfigureAwait(false);
        var queued = await store.CountQueuedAsync(cancellationToken).ConfigureAwait(false);
        var cache = await store.GetInsightsAsync(cancellationToken).ConfigureAwait(false);
        return new AiCopilotSnapshot(suggestions, ReadInsights(cache), cache?.FetchedAt, queued);
    }

    /// <summary>
    /// Pushes the queued decisions, then refreshes suggestions and insights from the server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result; the cached data is returned when the server is unreachable.</returns>
    public async Task<AiRefreshResult> RefreshAsync(CancellationToken cancellationToken)
    {
        if (!api.IsAuthenticated)
        {
            return await ResultAsync(AiRefreshStatus.AuthRequired, AiFlushResult.Empty, cancellationToken).ConfigureAwait(false);
        }

        var flush = await FlushAsync(cancellationToken).ConfigureAwait(false);
        if (flush.AuthRequired)
        {
            return await ResultAsync(AiRefreshStatus.AuthRequired, flush, cancellationToken).ConfigureAwait(false);
        }

        if (flush.Remaining > 0)
        {
            // The network is most likely down: keep the cache and the queue as they are.
            return await ResultAsync(AiRefreshStatus.Offline, flush, cancellationToken).ConfigureAwait(false);
        }

        var status = await PullSuggestionsAsync(cancellationToken).ConfigureAwait(false);
        if (status == AiRefreshStatus.Ok)
        {
            status = await PullInsightsAsync(cancellationToken).ConfigureAwait(false);
        }

        return await ResultAsync(status, flush, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Pushes the queued decisions oldest first and stops at the first network failure.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The summary of the flush.</returns>
    public async Task<AiFlushResult> FlushAsync(CancellationToken cancellationToken)
    {
        var queued = await store.GetQueuedAsync(cancellationToken).ConfigureAwait(false);
        var sent = 0;
        var dropped = 0;
        var refused = 0;
        var authRequired = false;

        foreach (var suggestion in queued)
        {
            var outcome = await SendQueuedAsync(suggestion, cancellationToken).ConfigureAwait(false);
            if (outcome.Status == AiApiStatus.Success)
            {
                await store.RemoveAsync(suggestion.Id, cancellationToken).ConfigureAwait(false);
                sent++;
            }
            else if (outcome.Status is AiApiStatus.AlreadyDecided or AiApiStatus.NotFound)
            {
                await store.RemoveAsync(suggestion.Id, cancellationToken).ConfigureAwait(false);
                dropped++;
            }
            else if (outcome.Status == AiApiStatus.Unauthorized)
            {
                authRequired = true;
                break;
            }
            else if (outcome.Status == AiApiStatus.Transient)
            {
                // The network is most likely down: keep this and the remaining decisions queued, in order.
                suggestion.QueuedAttempts++;
                suggestion.LastError = outcome.Detail;
                await store.SaveAsync(suggestion, cancellationToken).ConfigureAwait(false);
                break;
            }
            else
            {
                // 422 and other refusals: the suggestion is pending again and carries the server explanation.
                await DiscardQueuedAsync(suggestion, outcome.Detail, cancellationToken).ConfigureAwait(false);
                refused++;
            }
        }

        var remaining = await store.CountQueuedAsync(cancellationToken).ConfigureAwait(false);
        return new AiFlushResult(sent, dropped, refused, remaining, authRequired);
    }

    /// <summary>
    /// Approves or rejects a suggestion now, or queues the decision when the server cannot be reached.
    /// </summary>
    /// <param name="id">The suggestion identifier.</param>
    /// <param name="request">The decision.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<AiDecisionResult> DecideAsync(Guid id, AiDecisionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return DecideCoreAsync(id, request, cancellationToken);
    }

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static AiInsightsDto? ReadInsights(AiInsightCache? cache)
    {
        if (cache is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AiInsightsDto>(cache.PayloadJson, CacheOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static AiRefreshStatus Classify(HttpRequestException exception) => (int?)exception.StatusCode switch
    {
        (int)HttpStatusCode.Forbidden => AiRefreshStatus.Forbidden,
        (int)HttpStatusCode.Unauthorized => AiRefreshStatus.AuthRequired,
        _ => AiRefreshStatus.Offline,
    };

    private Task<AiDecisionOutcome> SendQueuedAsync(AiSuggestion suggestion, CancellationToken cancellationToken) =>
        suggestion.QueuedDecision == AiQueuedDecision.Approve
            ? api.ApproveAsync(suggestion.Id, suggestion.QueuedEditedPayloadJson, suggestion.QueuedFeedback, cancellationToken)
            : api.RejectAsync(suggestion.Id, suggestion.QueuedFeedback ?? string.Empty, cancellationToken);

    private async Task<AiRefreshStatus> PullSuggestionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var page = await api.GetSuggestionsAsync(PerPage, cancellationToken).ConfigureAwait(false);
            var now = timeProvider.GetUtcNow().UtcDateTime;
            await store.UpsertAsync(page.Items, now, cancellationToken).ConfigureAwait(false);

            // Only a complete queue allows pruning: with more pages the missing rows may still be open on the server.
            if (page.Total <= page.Items.Count)
            {
                await store.RemoveMissingAsync(page.Items.Select(i => i.Id).ToHashSet(), cancellationToken).ConfigureAwait(false);
            }

            return AiRefreshStatus.Ok;
        }
        catch (HttpRequestException ex)
        {
            return Classify(ex);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return AiRefreshStatus.Offline;
        }
    }

    private async Task<AiRefreshStatus> PullInsightsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var insights = await api.GetInsightsAsync(cancellationToken).ConfigureAwait(false);
            var json = JsonSerializer.Serialize(insights, CacheOptions);
            await store.SaveInsightsAsync(json, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
            return AiRefreshStatus.Ok;
        }
        catch (HttpRequestException ex)
        {
            return Classify(ex);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return AiRefreshStatus.Offline;
        }
    }

    private async Task<AiDecisionResult> QueueAsync(AiSuggestion suggestion, AiDecisionRequest request, CancellationToken cancellationToken)
    {
        suggestion.QueuedDecision = request.Approve ? AiQueuedDecision.Approve : AiQueuedDecision.Reject;
        suggestion.QueuedFeedback = Clean(request.Text);
        suggestion.QueuedEditedPayloadJson = request.Approve ? request.EditedPayloadJson : null;
        suggestion.QueuedAt = timeProvider.GetUtcNow().UtcDateTime;
        suggestion.QueuedAttempts = 0;
        suggestion.LastError = null;
        await store.SaveAsync(suggestion, cancellationToken).ConfigureAwait(false);
        return await DecisionAsync(AiDecisionStatus.Queued, detail: null, cancellationToken).ConfigureAwait(false);
    }

    private Task DiscardQueuedAsync(AiSuggestion suggestion, string? detail, CancellationToken cancellationToken)
    {
        suggestion.QueuedDecision = AiQueuedDecision.None;
        suggestion.QueuedFeedback = null;
        suggestion.QueuedEditedPayloadJson = null;
        suggestion.QueuedAt = null;
        suggestion.QueuedAttempts = 0;
        suggestion.LastError = detail;
        return store.SaveAsync(suggestion, cancellationToken);
    }

    private async Task<AiDecisionResult> DecisionAsync(AiDecisionStatus status, string? detail, CancellationToken cancellationToken) =>
        new(status, detail, await LoadAsync(cancellationToken).ConfigureAwait(false));

    private async Task<AiRefreshResult> ResultAsync(AiRefreshStatus status, AiFlushResult flush, CancellationToken cancellationToken) =>
        new(status, flush, await LoadAsync(cancellationToken).ConfigureAwait(false));

    private async Task<AiDecisionResult> DecideCoreAsync(Guid id, AiDecisionRequest request, CancellationToken cancellationToken)
    {
        var suggestion = await store.FindAsync(id, cancellationToken).ConfigureAwait(false);
        if (suggestion is null)
        {
            return await DecisionAsync(AiDecisionStatus.NotFound, detail: null, cancellationToken).ConfigureAwait(false);
        }

        if (!AiSuggestionStatus.IsOpen(suggestion.Status))
        {
            return await DecisionAsync(AiDecisionStatus.AlreadyDecided, detail: null, cancellationToken).ConfigureAwait(false);
        }

        if (!api.IsAuthenticated)
        {
            return await QueueAsync(suggestion, request, cancellationToken).ConfigureAwait(false);
        }

        var outcome = request.Approve
            ? await api.ApproveAsync(id, request.EditedPayloadJson, Clean(request.Text), cancellationToken).ConfigureAwait(false)
            : await api.RejectAsync(id, Clean(request.Text) ?? string.Empty, cancellationToken).ConfigureAwait(false);

        if (outcome.Status == AiApiStatus.Success)
        {
            await store.RemoveAsync(id, cancellationToken).ConfigureAwait(false);
            return await DecisionAsync(AiDecisionStatus.Sent, detail: null, cancellationToken).ConfigureAwait(false);
        }

        if (outcome.Status is AiApiStatus.AlreadyDecided or AiApiStatus.NotFound)
        {
            // Decided elsewhere (409) or gone (404): the suggestion leaves the local queue too.
            await store.RemoveAsync(id, cancellationToken).ConfigureAwait(false);
            var gone = outcome.Status == AiApiStatus.NotFound ? AiDecisionStatus.NotFound : AiDecisionStatus.AlreadyDecided;
            return await DecisionAsync(gone, outcome.Detail, cancellationToken).ConfigureAwait(false);
        }

        if (outcome.Status is AiApiStatus.Unauthorized or AiApiStatus.Transient)
        {
            return await QueueAsync(suggestion, request, cancellationToken).ConfigureAwait(false);
        }

        // 422 and other refusals: the suggestion stays pending and carries the server explanation.
        await DiscardQueuedAsync(suggestion, outcome.Detail, cancellationToken).ConfigureAwait(false);
        var status = outcome.Status == AiApiStatus.Incomplete ? AiDecisionStatus.Incomplete : AiDecisionStatus.Refused;
        return await DecisionAsync(status, outcome.Detail, cancellationToken).ConfigureAwait(false);
    }
}
