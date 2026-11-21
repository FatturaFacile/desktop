// <copyright file="DssSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;

namespace FatturaFacile.Application.Dss;

/// <summary>
/// Sync orchestrator of the Executive DSS: pulls the three views (and the optional what-if inputs and legal entities) and saves
/// them in one piece. The pieces the cockpit cannot work without decide the outcome; the optional ones are skipped when the
/// server refuses them (for example a user who may read the cockpit but not simulate).
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="store">The local cache.</param>
/// <param name="time">The clock.</param>
public sealed class DssSyncService(IDssApiClient api, IDssSnapshotStore store, TimeProvider time) : IDssSyncService
{
    private static readonly Dictionary<HttpStatusCode, DssSyncStatus> ByStatus = new()
    {
        [HttpStatusCode.Unauthorized] = DssSyncStatus.Unauthorized,
        [HttpStatusCode.Forbidden] = DssSyncStatus.Forbidden,
    };

    /// <inheritdoc/>
    public Task<DssSyncResult> SyncAsync(DssFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return api.IsAuthenticated
            ? PullAsync(filter, cancellationToken)
            : Task.FromResult(new DssSyncResult(DssSyncStatus.Unauthorized));
    }

    private static DssSyncResult Classify(HttpRequestException ex)
    {
        // No status code means the server never answered.
        var status = ex.StatusCode is { } code ? ByStatus.GetValueOrDefault(code, DssSyncStatus.Failed) : DssSyncStatus.Offline;

        return new DssSyncResult(status, Detail: ex.Message);
    }

    private static async Task<string?> TryAsync(Func<Task<string>> fetch)
    {
        try
        {
            return await fetch().ConfigureAwait(false);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound or HttpStatusCode.UnprocessableEntity)
        {
            return null;
        }
    }

    private async Task<DssSyncResult> PullAsync(DssFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var overview = await api.GetOverviewAsync(filter, cancellationToken).ConfigureAwait(false);
            var profitability = await api.GetProfitabilityAsync(filter, cancellationToken).ConfigureAwait(false);
            var bridge = await api.GetVarianceBridgeAsync(filter, cancellationToken).ConfigureAwait(false);
            var inputs = await TryAsync(() => api.GetSimulationInputsAsync(filter, cancellationToken)).ConfigureAwait(false);
            var entities = await TryAsync(() => api.GetLegalEntitiesAsync(cancellationToken)).ConfigureAwait(false);

            // A payload that does not parse must never replace a good cache.
            _ = DssSnapshotParser.ParseOverview(overview);
            _ = DssSnapshotParser.ParseProfitability(profitability);
            _ = DssSnapshotParser.ParseVarianceBridge(bridge);

            var now = time.GetUtcNow().UtcDateTime;
            await store.SaveAsync(filter, new DssPayload(overview, profitability, bridge, inputs, entities), now, cancellationToken).ConfigureAwait(false);

            return new DssSyncResult(DssSyncStatus.Synced, now);
        }
        catch (HttpRequestException ex)
        {
            return Classify(ex);
        }
        catch (JsonException ex)
        {
            return new DssSyncResult(DssSyncStatus.Failed, Detail: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // A timeout: the server did not answer in time.
            return new DssSyncResult(DssSyncStatus.Offline, Detail: ex.Message);
        }
    }
}
