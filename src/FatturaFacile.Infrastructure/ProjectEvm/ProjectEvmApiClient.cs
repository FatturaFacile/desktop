// <copyright file="ProjectEvmApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.ProjectEvm;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.ProjectEvm;

/// <summary>
/// <see cref="IProjectEvmApiClient"/> over the authorized HTTP client of the sync engine.
/// </summary>
/// <param name="api">The authorized sync client.</param>
public sealed class ProjectEvmApiClient(ISyncApiClient api) : IProjectEvmApiClient
{
    private const string Base = "projects/";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetBreakdownAsync(string projectId, CancellationToken cancellationToken) =>
        DataAsync(Base + Uri.EscapeDataString(projectId) + "/evm/wbs-breakdown", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetWaterfallAsync(string projectId, CancellationToken cancellationToken) =>
        DataAsync(Base + Uri.EscapeDataString(projectId) + "/evm/waterfall", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetScurveAsync(string projectId, CancellationToken cancellationToken) =>
        DataAsync(Base + "evm/scurve?granularity=month&project_ids%5B%5D=" + Uri.EscapeDataString(projectId), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetProjectsAsync(CancellationToken cancellationToken) =>
        DataAsync("projects?per_page=100", cancellationToken);

    private async Task<string> DataAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Project profitability request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The project profitability answer has no data.");
    }
}
