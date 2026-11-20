// <copyright file="SyncEngine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Sync;

/// <summary>
/// Offline-first sync pass: replays the outbox (push) and then pulls server deltas per entity.
/// </summary>
/// <remarks>
/// Conflict policy: a pulled record is never allowed to overwrite a record with a pending local change; the pending
/// change is pushed instead and the server arbitrates (409/412 marks the entry as a conflict for manual resolution).
/// </remarks>
/// <param name="client">The web-api client.</param>
/// <param name="store">The sync persistence port.</param>
/// <param name="handlers">The registered entity handlers.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class SyncEngine(
    ISyncApiClient client,
    ISyncStore store,
    IEnumerable<ISyncEntityHandler> handlers,
    TimeProvider timeProvider)
{
    private const int PushBatchSize = 100;
    private const int PageSize = 200;

    private readonly Dictionary<string, ISyncEntityHandler> handlersByName =
        handlers.ToDictionary(h => h.EntityName, StringComparer.Ordinal);

    /// <summary>
    /// Runs one push-then-pull pass.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The summary of the pass.</returns>
    public async Task<SyncRunResult> RunAsync(CancellationToken cancellationToken)
    {
        if (!client.IsAuthenticated)
        {
            return SyncRunResult.NotAuthenticated;
        }

        List<string> errors = [];
        var push = await PushAsync(errors, cancellationToken).ConfigureAwait(false);
        if (push.AuthRequired)
        {
            return new SyncRunResult(push.Pushed, push.Conflicts, push.Failed, Pulled: 0, SkippedPulls: 0, AuthRequired: true, errors);
        }

        var pulled = 0;
        var skipped = 0;
        foreach (var handler in handlersByName.Values)
        {
            try
            {
                var applied = await PullAsync(handler, cancellationToken).ConfigureAwait(false);
                pulled += applied.Applied;
                skipped += applied.Skipped;
            }
            catch (HttpRequestException ex)
            {
                errors.Add($"{handler.EntityName}: {ex.Message}");
            }
        }

        return new SyncRunResult(push.Pushed, push.Conflicts, push.Failed, pulled, skipped, !client.IsAuthenticated, errors);
    }

    private async Task<PushTotals> PushAsync(List<string> errors, CancellationToken cancellationToken)
    {
        PushTotals totals = new();
        var due = await store.GetDueEntriesAsync(timeProvider.GetUtcNow().UtcDateTime, PushBatchSize, cancellationToken).ConfigureAwait(false);

        foreach (var entry in due)
        {
            var outcome = await TrySendAsync(entry, cancellationToken).ConfigureAwait(false);
            var now = timeProvider.GetUtcNow().UtcDateTime;
            entry.UpdatedAt = now;

            switch (outcome.Result)
            {
                case SyncSendResult.Success:
                    entry.Status = SyncOutboxStatus.Synced;
                    entry.LastError = null;
                    totals.Pushed++;
                    break;

                case SyncSendResult.Conflict:
                    entry.Status = SyncOutboxStatus.Conflict;
                    entry.LastError = outcome.Error;
                    totals.Conflicts++;
                    break;

                case SyncSendResult.Rejected:
                    entry.Status = SyncOutboxStatus.Failed;
                    entry.LastError = outcome.Error;
                    totals.Failed++;
                    break;

                case SyncSendResult.Unauthorized:
                    totals.AuthRequired = true;
                    return totals;

                case SyncSendResult.Transient:
                    entry.Attempts++;
                    entry.LastError = outcome.Error;
                    if (SyncRetryPolicy.CanRetry(entry.Attempts))
                    {
                        entry.NextAttemptAt = now + SyncRetryPolicy.NextDelay(entry.Attempts);
                    }
                    else
                    {
                        entry.Status = SyncOutboxStatus.Failed;
                        totals.Failed++;
                    }

                    errors.Add($"{entry.EntityName}/{entry.EntityId}: {outcome.Error}");
                    await store.SaveEntryAsync(entry, cancellationToken).ConfigureAwait(false);

                    // The network is most likely down: stop here and keep the order of the remaining entries.
                    return totals;

                default:
                    throw new InvalidOperationException($"Unexpected send result {outcome.Result}.");
            }

            await store.SaveEntryAsync(entry, cancellationToken).ConfigureAwait(false);
        }

        return totals;
    }

    private async Task<SyncSendOutcome> TrySendAsync(SyncOutboxEntry entry, CancellationToken cancellationToken)
    {
        if (!handlersByName.TryGetValue(entry.EntityName, out var handler) || handler.WriteRoute is null)
        {
            return new SyncSendOutcome(SyncSendResult.Rejected, $"Entity '{entry.EntityName}' does not accept writes.");
        }

        string? payload = null;
        if (entry.Operation != SyncOperation.Delete)
        {
            payload = await handler.BuildPayloadAsync(entry.EntityId, cancellationToken).ConfigureAwait(false);
            if (payload is null)
            {
                // The record disappeared locally after it was queued: nothing left to send.
                return new SyncSendOutcome(SyncSendResult.Success);
            }
        }

        SyncRequest request = new(entry.Operation, handler.WriteRoute, entry.EntityId, entry.IdempotencyKey, payload);
        return await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<SyncApplyResult> PullAsync(ISyncEntityHandler handler, CancellationToken cancellationToken)
    {
        var since = await store.GetCheckpointAsync(handler.EntityName, cancellationToken).ConfigureAwait(false);
        var protectedIds = await store.GetProtectedEntityIdsAsync(handler.EntityName, cancellationToken).ConfigureAwait(false);

        string? cursor = null;
        DateTime? firstServerTime = null;
        var applied = 0;
        var skipped = 0;

        while (true)
        {
            var page = await client.GetDeltaAsync(handler.EntityName, since, cursor, PageSize, cancellationToken).ConfigureAwait(false);

            // The server time of the first page is the safe checkpoint: records changed during the pass are fetched again next time.
            firstServerTime ??= page.ServerTime;

            var result = await handler.ApplyAsync(page.Data, page.Deleted, protectedIds, cancellationToken).ConfigureAwait(false);
            applied += result.Applied;
            skipped += result.Skipped;

            if (!page.HasMore || page.NextCursor is null)
            {
                break;
            }

            cursor = page.NextCursor;
        }

        if (firstServerTime is { } checkpoint)
        {
            await store.SaveCheckpointAsync(handler.EntityName, checkpoint, cancellationToken).ConfigureAwait(false);
        }

        return new SyncApplyResult(applied, skipped);
    }

    private sealed class PushTotals
    {
        public int Pushed { get; set; }

        public int Conflicts { get; set; }

        public int Failed { get; set; }

        public bool AuthRequired { get; set; }
    }
}
