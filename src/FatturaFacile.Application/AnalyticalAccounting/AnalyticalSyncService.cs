// <copyright file="AnalyticalSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// Default <see cref="IAnalyticalSyncService"/>. Queued entries go first, oldest first: an accepted one leaves the queue, a refused
/// one is parked as failed, and a server that cannot be reached stops the run with everything still queued. The four read models
/// are pulled and parsed before any of them is saved, so the cache is replaced together or not at all.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="store">The local cache.</param>
/// <param name="queue">The offline queue.</param>
/// <param name="time">The clock.</param>
public sealed class AnalyticalSyncService(IAnalyticalApiClient api, IAnalyticalSnapshotStore store, IAnalyticalOperationQueue queue, TimeProvider time) : IAnalyticalSyncService
{
    private static readonly Dictionary<HttpStatusCode, AnalyticalSyncStatus> ByStatus = new()
    {
        [HttpStatusCode.Unauthorized] = AnalyticalSyncStatus.Unauthorized,
        [HttpStatusCode.Forbidden] = AnalyticalSyncStatus.Forbidden,
    };

    /// <inheritdoc/>
    public Task<AnalyticalSyncResult> SyncAsync(string? from, string? to, CancellationToken cancellationToken) =>
        api.IsAuthenticated
            ? RunAsync(() => PullAsync(from, to, cancellationToken), cancellationToken)
            : Task.FromResult(new AnalyticalSyncResult(AnalyticalSyncStatus.Unauthorized));

    private static AnalyticalSyncResult Classify(HttpRequestException ex)
    {
        // No status code means the server never answered.
        var status = ex.StatusCode is { } code ? ByStatus.GetValueOrDefault(code, AnalyticalSyncStatus.Failed) : AnalyticalSyncStatus.Offline;

        return new AnalyticalSyncResult(status, Detail: ex.Message);
    }

    private static async Task<AnalyticalSyncResult> RunAsync(Func<Task<AnalyticalSyncResult>> pull, CancellationToken cancellationToken)
    {
        try
        {
            return await pull().ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            return Classify(ex);
        }
        catch (JsonException ex)
        {
            return new AnalyticalSyncResult(AnalyticalSyncStatus.Failed, Detail: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // A timeout: the server did not answer in time.
            return new AnalyticalSyncResult(AnalyticalSyncStatus.Offline, Detail: ex.Message);
        }
    }

    private async Task<AnalyticalSyncResult> PullAsync(string? from, string? to, CancellationToken cancellationToken)
    {
        var (sent, failed) = await FlushAsync(cancellationToken).ConfigureAwait(false);

        var centers = await api.GetCentersAsync(cancellationToken).ConfigureAwait(false);
        var runs = await api.GetRunsAsync(cancellationToken).ConfigureAwait(false);
        var margin = await api.GetMarginAsync(from, to, cancellationToken).ConfigureAwait(false);
        var comparison = await api.GetComparisonAsync(from, to, cancellationToken).ConfigureAwait(false);

        // A payload that does not parse must never replace a good cache.
        _ = AnalyticalParser.ParseCenters(centers);
        _ = AnalyticalParser.ParseRuns(runs);
        _ = AnalyticalParser.ParseMargin(margin);
        _ = AnalyticalParser.ParseComparison(comparison);

        var now = time.GetUtcNow().UtcDateTime;
        await store.SaveAsync(AnalyticalContract.CentersKey, centers, now, cancellationToken).ConfigureAwait(false);
        await store.SaveAsync(AnalyticalContract.RunsKey, runs, now, cancellationToken).ConfigureAwait(false);
        await store.SaveAsync(AnalyticalContract.MarginKey, margin, now, cancellationToken).ConfigureAwait(false);
        await store.SaveAsync(AnalyticalContract.ComparisonKey, comparison, now, cancellationToken).ConfigureAwait(false);

        return new AnalyticalSyncResult(AnalyticalSyncStatus.Synced, now, sent, failed);
    }

    private async Task<(int Sent, int Failed)> FlushAsync(CancellationToken cancellationToken)
    {
        var sent = 0;
        var failed = 0;
        foreach (var operation in await queue.PendingAsync(cancellationToken).ConfigureAwait(false))
        {
            var reply = await api.PostEntryAsync(operation.PayloadJson, cancellationToken).ConfigureAwait(false);
            if (reply.Accepted)
            {
                await queue.MarkSentAsync(operation.Id, cancellationToken).ConfigureAwait(false);
                sent++;
            }
            else if (reply.Refused)
            {
                await queue.MarkFailedAsync(operation.Id, reply.Detail, cancellationToken).ConfigureAwait(false);
                failed++;
            }
        }

        return (sent, failed);
    }
}
