// <copyright file="AnalyticalApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.AnalyticalAccounting;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.AnalyticalAccounting;

/// <summary>
/// <see cref="IAnalyticalApiClient"/> over the authorized HTTP client of the sync engine.
/// </summary>
/// <param name="api">The authorized sync client.</param>
public sealed class AnalyticalApiClient(ISyncApiClient api) : IAnalyticalApiClient
{
    private const string Base = "analytical/";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetCentersAsync(CancellationToken cancellationToken) =>
        DataAsync(Base + "centers/tree", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetRunsAsync(CancellationToken cancellationToken) =>
        DataAsync(Base + "runs?per_page=50", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetMarginAsync(string? from, string? to, CancellationToken cancellationToken) =>
        DataAsync(Base + "reports/contribution-margin" + Query(from, to, includeDescendants: false), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetComparisonAsync(string? from, string? to, CancellationToken cancellationToken) =>
        DataAsync(Base + "reports/direct-vs-apportioned" + Query(from, to, includeDescendants: true), cancellationToken);

    /// <inheritdoc/>
    public async Task<AnalyticalPostReply> PostEntryAsync(string json, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Post, Base + "entries", json, cancellationToken).ConfigureAwait(false);
        if (reply.IsSuccess)
        {
            return new AnalyticalPostReply(Accepted: true, Refused: false);
        }

        // An authentication problem or a server error is retried later; a refusal of the entry itself is not.
        var refused = reply.Status is HttpStatusCode.UnprocessableEntity or HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict;

        return new AnalyticalPostReply(Accepted: false, refused, refused ? Detail(reply.Text) : null);
    }

    private static string Query(string? from, string? to, bool includeDescendants)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(from))
        {
            parts.Add("from=" + Uri.EscapeDataString(from));
        }

        if (!string.IsNullOrWhiteSpace(to))
        {
            parts.Add("to=" + Uri.EscapeDataString(to));
        }

        if (includeDescendants)
        {
            parts.Add("include_descendants=1");
        }

        return parts.Count == 0 ? string.Empty : "?" + string.Join('&', parts);
    }

    private static string? Detail(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);

            return document.RootElement.TryGetProperty("message", out var message) ? message.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<string> DataAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Analytical accounting request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The analytical accounting answer has no data.");
    }
}
