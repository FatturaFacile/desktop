// <copyright file="DssApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.Dss;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.Dss;

/// <summary>
/// HTTP client of the Executive DSS endpoints. It rides on the authenticated sync session (bearer token, tenant header and token refresh).
/// </summary>
/// <param name="api">The authenticated web-api client.</param>
public sealed class DssApiClient(ISyncApiClient api) : IDssApiClient
{
    private const string Base = "analytics/dss/";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetOverviewAsync(DssFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return DataAsync(HttpMethod.Get, Base + "overview?" + Query(filter), body: null, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<string> GetProfitabilityAsync(DssFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return DataAsync(HttpMethod.Get, Base + "profitability-matrix?" + Query(filter) + "&dimension=customer", body: null, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<string> GetVarianceBridgeAsync(DssFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return DataAsync(HttpMethod.Get, Base + "variance-bridge?" + Query(filter), body: null, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<string> GetSimulationInputsAsync(DssFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var body = Body(filter);
        body["scenario"] = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["price_pct"] = 0,
            ["cogs_pct"] = 0,
            ["opex_pct"] = 0,
            ["dso_days_delta"] = 0,
            ["fx_shift_pct"] = 0,
            ["fte_delta"] = 0,
        };

        return DataAsync(HttpMethod.Post, Base + "simulate", JsonSerializer.Serialize(body), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<string> GetLegalEntitiesAsync(CancellationToken cancellationToken) =>
        DataAsync(HttpMethod.Get, Base + "legal-entities", body: null, cancellationToken);

    /// <inheritdoc/>
    public Task<byte[]> ExportBoardPackAsync(DssFilter filter, string format, string locale, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var body = Body(filter);
        body["format"] = format;
        body["locale"] = locale;

        return DownloadAsync(JsonSerializer.Serialize(body), cancellationToken);
    }

    private static string Query(DssFilter filter)
    {
        StringBuilder query = new();
        _ = query.Append("currency=").Append(Uri.EscapeDataString(filter.Currency))
            .Append("&period_preset=").Append(Uri.EscapeDataString(filter.PeriodPreset))
            .Append("&comparison_baseline=").Append(Uri.EscapeDataString(filter.ComparisonBaseline));

        if (filter.LegalEntityId.Length > 0)
        {
            _ = query.Append("&legal_entity_ids%5B%5D=").Append(Uri.EscapeDataString(filter.LegalEntityId));
        }

        return query.ToString();
    }

    private static Dictionary<string, object> Body(DssFilter filter)
    {
        Dictionary<string, object> body = new(StringComparer.Ordinal)
        {
            ["currency"] = filter.Currency,
            ["period_preset"] = filter.PeriodPreset,
            ["comparison_baseline"] = filter.ComparisonBaseline,
        };

        if (filter.LegalEntityId.Length > 0)
        {
            body["legal_entity_ids"] = new[] { filter.LegalEntityId };
        }

        return body;
    }

    private static void EnsureSuccess(SyncApiReply reply)
    {
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Executive DSS request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }
    }

    private async Task<byte[]> DownloadAsync(string body, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Post, Base + "export-board-pack", body, cancellationToken).ConfigureAwait(false);
        EnsureSuccess(reply);

        return reply.Content.ToArray();
    }

    private async Task<string> DataAsync(HttpMethod method, string url, string? body, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(method, url, body, cancellationToken).ConfigureAwait(false);
        EnsureSuccess(reply);

        using var document = JsonDocument.Parse(reply.Text);
        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The Executive DSS answer has no data.");
    }
}
