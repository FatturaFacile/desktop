// <copyright file="ProjectBillingSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// Default <see cref="IProjectBillingSyncService"/>: pulls the views of a project and saves them together, so a reader sees the
/// old snapshot or the new one, never a mix. A payload that does not parse never replaces a good cache.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="store">The local cache.</param>
/// <param name="time">The clock.</param>
public sealed class ProjectBillingSyncService(IProjectBillingApiClient api, IProjectBillingSnapshotStore store, TimeProvider time) : IProjectBillingSyncService
{
    private const string Absent = "null";

    private static readonly Dictionary<HttpStatusCode, ProjectBillingSyncStatus> ByStatus = new()
    {
        [HttpStatusCode.Unauthorized] = ProjectBillingSyncStatus.Unauthorized,
        [HttpStatusCode.Forbidden] = ProjectBillingSyncStatus.Forbidden,
    };

    /// <inheritdoc/>
    public Task<ProjectBillingSyncResult> SyncAsync(string projectId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);

        return api.IsAuthenticated
            ? RunAsync(() => PullAsync(projectId, cancellationToken), cancellationToken)
            : Task.FromResult(new ProjectBillingSyncResult(ProjectBillingSyncStatus.Unauthorized));
    }

    private static ProjectBillingSyncResult Classify(HttpRequestException ex)
    {
        // No status code means the server never answered.
        var status = ex.StatusCode is { } code ? ByStatus.GetValueOrDefault(code, ProjectBillingSyncStatus.Failed) : ProjectBillingSyncStatus.Offline;

        return new ProjectBillingSyncResult(status, Detail: ex.Message);
    }

    private static async Task<ProjectBillingSyncResult> RunAsync(Func<Task<ProjectBillingSyncResult>> pull, CancellationToken cancellationToken)
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
            return new ProjectBillingSyncResult(ProjectBillingSyncStatus.Failed, Detail: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // A timeout: the server did not answer in time.
            return new ProjectBillingSyncResult(ProjectBillingSyncStatus.Offline, Detail: ex.Message);
        }
    }

    private async Task<ProjectBillingSyncResult> PullAsync(string projectId, CancellationToken cancellationToken)
    {
        var summary = await api.GetSummaryAsync(projectId, cancellationToken).ConfigureAwait(false);
        var contract = await api.GetContractAsync(projectId, cancellationToken).ConfigureAwait(false);
        var certificates = await api.GetCertificatesAsync(projectId, cancellationToken).ConfigureAwait(false);

        // Retention and retainer only exist under a contract.
        var hasContract = !string.Equals(contract, Absent, StringComparison.Ordinal);
        var retention = hasContract ? await api.GetRetentionAsync(projectId, cancellationToken).ConfigureAwait(false) : Absent;
        var retainer = hasContract ? await api.GetRetainerAsync(projectId, cancellationToken).ConfigureAwait(false) : Absent;
        var now = time.GetUtcNow().UtcDateTime;
        ProjectBillingPayload payload = new(summary, contract, certificates, retention, retainer);

        // A payload that does not parse must never replace a good cache.
        _ = ProjectBillingParser.ParseSnapshot(projectId, payload, now);

        await store.SaveAsync(projectId, payload, now, cancellationToken).ConfigureAwait(false);

        return new ProjectBillingSyncResult(ProjectBillingSyncStatus.Synced, now);
    }
}
