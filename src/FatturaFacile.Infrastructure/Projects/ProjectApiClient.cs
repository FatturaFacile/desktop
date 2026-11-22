// <copyright file="ProjectApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Projects;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.Projects;

/// <summary>
/// <see cref="IProjectApiClient"/> over the authorized HTTP client of the sync engine.
/// </summary>
/// <param name="api">The authorized sync client.</param>
public sealed class ProjectApiClient(ISyncApiClient api) : IProjectApiClient
{
    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetProjectsAsync(CancellationToken cancellationToken) =>
        DataAsync(ProjectRoutes.Projects + "?per_page=100", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetNodesAsync(Guid projectId, CancellationToken cancellationToken) =>
        DataAsync(ProjectRoutes.Nodes(projectId), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetDependenciesAsync(Guid projectId, CancellationToken cancellationToken) =>
        DataAsync(ProjectRoutes.Dependencies(projectId), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetBaselinesAsync(Guid projectId, CancellationToken cancellationToken) =>
        DataAsync(ProjectRoutes.Baselines(projectId), cancellationToken);

    /// <inheritdoc/>
    public Task<SyncApiReply> SendAsync(HttpMethod method, string route, string? jsonBody, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(route);

        return api.SendAuthorizedAsync(method, route, jsonBody, cancellationToken);
    }

    private async Task<string> DataAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Project request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The project answer has no data.");
    }
}
