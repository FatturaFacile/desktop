// <copyright file="ProjectEvmSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// Default <see cref="IProjectEvmSyncService"/>: pulls the three views of a project and saves them together, so a reader sees the
/// old snapshot or the new one, never a mix. A payload that does not parse never replaces a good cache.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="store">The local cache.</param>
/// <param name="time">The clock.</param>
public sealed class ProjectEvmSyncService(IProjectEvmApiClient api, IProjectEvmSnapshotStore store, TimeProvider time) : IProjectEvmSyncService
{
    private static readonly Dictionary<HttpStatusCode, ProjectEvmSyncStatus> ByStatus = new()
    {
        [HttpStatusCode.Unauthorized] = ProjectEvmSyncStatus.Unauthorized,
        [HttpStatusCode.Forbidden] = ProjectEvmSyncStatus.Forbidden,
    };

    /// <inheritdoc/>
    public Task<ProjectEvmSyncResult> SyncAsync(string projectId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);

        return api.IsAuthenticated
            ? RunAsync(() => PullAsync(projectId, cancellationToken), cancellationToken)
            : Task.FromResult(new ProjectEvmSyncResult(ProjectEvmSyncStatus.Unauthorized));
    }

    /// <inheritdoc/>
    public Task<ProjectEvmSyncResult> SyncProjectsAsync(CancellationToken cancellationToken) =>
        api.IsAuthenticated
            ? RunAsync(() => PullProjectsAsync(cancellationToken), cancellationToken)
            : Task.FromResult(new ProjectEvmSyncResult(ProjectEvmSyncStatus.Unauthorized));

    private static ProjectEvmSyncResult Classify(HttpRequestException ex)
    {
        // No status code means the server never answered.
        var status = ex.StatusCode is { } code ? ByStatus.GetValueOrDefault(code, ProjectEvmSyncStatus.Failed) : ProjectEvmSyncStatus.Offline;

        return new ProjectEvmSyncResult(status, Detail: ex.Message);
    }

    private static async Task<ProjectEvmSyncResult> RunAsync(Func<Task<ProjectEvmSyncResult>> pull, CancellationToken cancellationToken)
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
            return new ProjectEvmSyncResult(ProjectEvmSyncStatus.Failed, Detail: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // A timeout: the server did not answer in time.
            return new ProjectEvmSyncResult(ProjectEvmSyncStatus.Offline, Detail: ex.Message);
        }
    }

    private async Task<ProjectEvmSyncResult> PullAsync(string projectId, CancellationToken cancellationToken)
    {
        var breakdown = await api.GetBreakdownAsync(projectId, cancellationToken).ConfigureAwait(false);
        var waterfall = await api.GetWaterfallAsync(projectId, cancellationToken).ConfigureAwait(false);
        var scurve = await api.GetScurveAsync(projectId, cancellationToken).ConfigureAwait(false);
        var now = time.GetUtcNow().UtcDateTime;
        ProjectEvmPayload payload = new(breakdown, waterfall, scurve);

        // A payload that does not parse must never replace a good cache.
        _ = ProjectEvmParser.ParseSnapshot(projectId, payload, now);

        await store.SaveAsync(projectId, payload, now, cancellationToken).ConfigureAwait(false);

        return new ProjectEvmSyncResult(ProjectEvmSyncStatus.Synced, now);
    }

    private async Task<ProjectEvmSyncResult> PullProjectsAsync(CancellationToken cancellationToken)
    {
        var projects = await api.GetProjectsAsync(cancellationToken).ConfigureAwait(false);
        _ = ProjectEvmParser.ParseProjects(projects);

        var now = time.GetUtcNow().UtcDateTime;
        await store.SaveProjectsAsync(projects, now, cancellationToken).ConfigureAwait(false);

        return new ProjectEvmSyncResult(ProjectEvmSyncStatus.Synced, now);
    }
}
