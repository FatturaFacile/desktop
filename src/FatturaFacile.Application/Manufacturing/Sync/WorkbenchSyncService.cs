// <copyright file="WorkbenchSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.Manufacturing.Outbox;

namespace FatturaFacile.Application.Manufacturing.Sync;

/// <summary>
/// Reconnect procedure: first push the outbox (idempotent, conflicts resolved by policy), then pull the delta of every resource.
/// </summary>
/// <param name="queue">The outbox queue.</param>
/// <param name="gateway">The web-api gateway.</param>
/// <param name="store">The local snapshot store.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class WorkbenchSyncService(
    OutboxSyncQueue queue,
    IManufacturingApiGateway gateway,
    IManufacturingSnapshotStore store,
    TimeProvider timeProvider)
{
    /// <summary>
    /// Runs one synchronisation. Offline or failing resources are reported in the result, never thrown.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public async Task<WorkbenchSyncResult> SyncAsync(CancellationToken cancellationToken)
    {
        if (!gateway.IsAuthenticated)
        {
            return new WorkbenchSyncResult(new OutboxFlushResult(0, 0, 0, 0, 0, 0, AuthRequired: true, []), 0, 0, Offline: false, []);
        }

        var push = await queue.FlushAsync(cancellationToken).ConfigureAwait(false);
        if (push.AuthRequired)
        {
            return new WorkbenchSyncResult(push, 0, 0, Offline: false, []);
        }

        List<string> errors = [.. push.Errors];
        var pulled = 0;
        var skipped = 0;
        var offline = push.Errors.Count > 0;

        if (!offline)
        {
            try
            {
                var protectedIds = await store.GetProtectedOrderIdsAsync(cancellationToken).ConfigureAwait(false);
                foreach (var resource in ManufacturingResources.PullOrder)
                {
                    var (applied, skippedRows) = await PullAsync(resource, protectedIds, cancellationToken).ConfigureAwait(false);
                    pulled += applied;
                    skipped += skippedRows;
                }

                var cockpit = await gateway.GetJsonAsync(ManufacturingResources.Cockpit, cancellationToken).ConfigureAwait(false);
                if (cockpit is not null)
                {
                    await store.SaveTelemetryAsync(cockpit, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (HttpRequestException ex)
            {
                offline = true;
                errors.Add(ex.Message);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                offline = true;
                errors.Add(ex.Message);
            }
        }

        return new WorkbenchSyncResult(push, pulled, skipped, offline, errors);
    }

    private async Task<(int Applied, int Skipped)> PullAsync(string resource, IReadOnlySet<Guid> protectedIds, CancellationToken cancellationToken)
    {
        var since = await store.GetCheckpointAsync(resource, cancellationToken).ConfigureAwait(false);
        var known = await store.GetKnownVersionsAsync(resource, cancellationToken).ConfigureAwait(false);
        DateTime? firstServerTime = null;
        var applied = 0;
        var skipped = 0;
        var pageNumber = 1;

        while (true)
        {
            var page = await gateway.GetPageAsync(resource, since, pageNumber, cancellationToken).ConfigureAwait(false);
            firstServerTime ??= page.ServerTime;

            List<JsonElement> changed = [];
            foreach (var row in page.Items)
            {
                var id = WorkbenchJson.GuidOrNull(row, "id");
                var version = WorkbenchJson.StringOrNull(row, "updated_at");
                if (id is { } rowId && version is not null && known.TryGetValue(rowId, out var stored) && string.Equals(stored, version, StringComparison.Ordinal))
                {
                    skipped++;
                    continue;
                }

                changed.Add(await WithDetailAsync(resource, row, id, cancellationToken).ConfigureAwait(false));
            }

            var (appliedRows, skippedRows) = await store.ApplyAsync(resource, changed, protectedIds, cancellationToken).ConfigureAwait(false);
            applied += appliedRows;
            skipped += skippedRows;

            if (!page.HasMore)
            {
                break;
            }

            pageNumber++;
        }

        if (firstServerTime is { } checkpoint)
        {
            await store.SaveCheckpointAsync(resource, checkpoint, cancellationToken).ConfigureAwait(false);
        }

        return (applied, skipped);
    }

    private async Task<JsonElement> WithDetailAsync(string resource, JsonElement row, Guid? id, CancellationToken cancellationToken)
    {
        if (!ManufacturingResources.NeedsDetail(resource) || id is null)
        {
            return row;
        }

        var detail = await gateway.GetJsonAsync($"{resource}/{id.Value:D}", cancellationToken).ConfigureAwait(false);
        if (detail is null)
        {
            return row;
        }

        using var document = JsonDocument.Parse(detail);
        return document.RootElement.Clone();
    }
}
