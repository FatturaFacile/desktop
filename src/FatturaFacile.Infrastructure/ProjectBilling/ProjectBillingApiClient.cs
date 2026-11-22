// <copyright file="ProjectBillingApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.ProjectBilling;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.ProjectBilling;

/// <summary>
/// <see cref="IProjectBillingApiClient"/> over the authorized HTTP client of the sync engine.
/// </summary>
/// <param name="api">The authorized sync client.</param>
public sealed class ProjectBillingApiClient(ISyncApiClient api) : IProjectBillingApiClient
{
    private const string Base = "projects/";
    private const string Absent = "null";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetSummaryAsync(string projectId, CancellationToken cancellationToken) =>
        DataAsync(Path(projectId, "/billing/unbilled-summary"), optional: false, cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetContractAsync(string projectId, CancellationToken cancellationToken) =>
        DataAsync(Path(projectId, "/billing-contract"), optional: true, cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetCertificatesAsync(string projectId, CancellationToken cancellationToken) =>
        DataAsync(Path(projectId, "/sal-certificates"), optional: false, cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetRetentionAsync(string projectId, CancellationToken cancellationToken) =>
        DataAsync(Path(projectId, "/retention"), optional: true, cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetRetainerAsync(string projectId, CancellationToken cancellationToken) =>
        DataAsync(Path(projectId, "/retainer"), optional: true, cancellationToken);

    private static string Path(string projectId, string suffix) => Base + Uri.EscapeDataString(projectId) + suffix;

    private async Task<string> DataAsync(string url, bool optional, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (optional && reply.Status == HttpStatusCode.NotFound)
        {
            return Absent;
        }

        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Project billing request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The project billing answer has no data.");
    }
}
