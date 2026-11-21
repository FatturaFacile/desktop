// <copyright file="ManufacturingSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// Sync orchestrator of Mod 061: pulls work centres, machines, working calendars and routings, then the OEE summary of every work
/// centre over the last <see cref="OeeWindowDays"/> days, and replaces the local cache in one transaction. The master data decides the
/// outcome; an OEE summary the server refuses (for example a work centre without logs) is simply left out.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="store">The local cache.</param>
/// <param name="time">The clock.</param>
public sealed class ManufacturingSyncService(IManufacturingApiClient api, IManufacturingStore store, TimeProvider time) : IManufacturingSyncService
{
    /// <summary>
    /// The length in days of the period the cached OEE summaries cover, ending at the sync instant.
    /// </summary>
    public const int OeeWindowDays = 30;

    private static readonly Dictionary<HttpStatusCode, ManufacturingSyncStatus> ByStatus = new()
    {
        [HttpStatusCode.Unauthorized] = ManufacturingSyncStatus.Unauthorized,
        [HttpStatusCode.Forbidden] = ManufacturingSyncStatus.Forbidden,
    };

    /// <inheritdoc/>
    public Task<ManufacturingSyncResult> SyncAsync(CancellationToken cancellationToken) =>
        api.IsAuthenticated
            ? PullAsync(cancellationToken)
            : Task.FromResult(new ManufacturingSyncResult(ManufacturingSyncStatus.Unauthorized));

    private static ManufacturingSyncResult Classify(HttpRequestException ex)
    {
        // No status code means the server never answered.
        var status = ex.StatusCode is { } code ? ByStatus.GetValueOrDefault(code, ManufacturingSyncStatus.Failed) : ManufacturingSyncStatus.Offline;

        return new ManufacturingSyncResult(status, Detail: ex.Message);
    }

    private async Task<ManufacturingSyncResult> PullAsync(CancellationToken cancellationToken)
    {
        try
        {
            var now = time.GetUtcNow().UtcDateTime;
            var workCenters = ManufacturingParser.ParseWorkCenters(await api.GetListAsync("work-centers", cancellationToken).ConfigureAwait(false), now);
            var machines = ManufacturingParser.ParseMachines(await api.GetListAsync("machines", cancellationToken).ConfigureAwait(false), now);
            var calendars = ManufacturingParser.ParseCalendars(await api.GetListAsync("working-calendars", cancellationToken).ConfigureAwait(false), now);
            var routings = ManufacturingParser.ParseRoutings(await api.GetListAsync("routings", cancellationToken).ConfigureAwait(false), now);
            var oee = await PullOeeAsync(workCenters, now, cancellationToken).ConfigureAwait(false);

            await store.ReplaceAsync(new ManufacturingMasterData(workCenters, machines, calendars, routings, oee), cancellationToken).ConfigureAwait(false);

            return new ManufacturingSyncResult(ManufacturingSyncStatus.Synced, now);
        }
        catch (HttpRequestException ex)
        {
            return Classify(ex);
        }
        catch (JsonException ex)
        {
            return new ManufacturingSyncResult(ManufacturingSyncStatus.Failed, Detail: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // A timeout: the server did not answer in time.
            return new ManufacturingSyncResult(ManufacturingSyncStatus.Offline, Detail: ex.Message);
        }
    }

    private async Task<List<MfgOeeSnapshot>> PullOeeAsync(IReadOnlyList<MfgWorkCenter> workCenters, DateTime now, CancellationToken cancellationToken)
    {
        List<MfgOeeSnapshot> snapshots = [];
        var from = now.AddDays(-OeeWindowDays);

        foreach (var workCenter in workCenters)
        {
            try
            {
                var json = await api.GetOeeSummaryAsync(workCenter.Id, from, now, cancellationToken).ConfigureAwait(false);
                snapshots.Add(ManufacturingParser.ParseOeeSnapshot(json, now));
            }
            catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound or HttpStatusCode.UnprocessableEntity)
            {
                // Optional piece: the work centre keeps working without an OEE figure.
            }
            catch (JsonException)
            {
                // A summary that does not parse is left out rather than failing the whole sync.
            }
        }

        return snapshots;
    }
}
