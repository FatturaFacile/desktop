// <copyright file="SwissVatApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.SwissVat;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.SwissVat;

/// <summary>
/// <see cref="ISwissVatApiClient"/> implementation over the authorized sync HTTP client.
/// </summary>
/// <param name="api">Authorized sync HTTP client.</param>
public sealed class SwissVatApiClient(ISyncApiClient api) : ISwissVatApiClient
{
    private const string Prefix = "swiss-vat/";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetConfigAsync(CancellationToken cancellationToken) =>
        GetDataAsync(Prefix + "config", cancellationToken);

    /// <inheritdoc/>
    public async Task<string> UpdateConfigAsync(string json, CancellationToken cancellationToken)
    {
        var reply = await api.SendJsonAsync(HttpMethod.Put, Prefix + "config", json, cancellationToken).ConfigureAwait(false);
        return ExtractData(reply);
    }

    /// <inheritdoc/>
    public Task<string> GetReturnsAsync(int year, CancellationToken cancellationToken) =>
        GetDataAsync(Prefix + "returns?year=" + year.ToString(CultureInfo.InvariantCulture), cancellationToken);

    /// <inheritdoc/>
    public async Task<string> CalculateReturnAsync(string json, CancellationToken cancellationToken)
    {
        var reply = await api.SendJsonAsync(HttpMethod.Post, Prefix + "returns/calculate", json, cancellationToken).ConfigureAwait(false);
        return ExtractData(reply);
    }

    /// <inheritdoc/>
    public Task<string> GetBezugsteuerAsync(CancellationToken cancellationToken) =>
        GetDataAsync(Prefix + "bezugsteuer", cancellationToken);

    /// <inheritdoc/>
    public async Task<string> DetectBezugsteuerAsync(string periodFrom, string periodTo, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.Serialize(new { period_from = periodFrom, period_to = periodTo });
        var reply = await api.SendJsonAsync(HttpMethod.Post, Prefix + "bezugsteuer/detect", body, cancellationToken).ConfigureAwait(false);
        return ExtractData(reply);
    }

    /// <inheritdoc/>
    public async Task<string> ConfirmBezugsteuerAsync(Guid id, CancellationToken cancellationToken)
    {
        var reply = await api.SendJsonAsync(HttpMethod.Post, Prefix + "bezugsteuer/" + id.ToString("D") + "/confirm", "{}", cancellationToken).ConfigureAwait(false);
        return ExtractData(reply);
    }

    /// <inheritdoc/>
    public Task<string> GetAdjustmentsAsync(int year, CancellationToken cancellationToken) =>
        GetDataAsync(Prefix + "adjustments?year=" + year.ToString(CultureInfo.InvariantCulture), cancellationToken);

    /// <inheritdoc/>
    public async Task<string> ExportSuisseTaxXmlAsync(Guid returnId, CancellationToken cancellationToken)
    {
        var reply = await api.SendJsonAsync(HttpMethod.Post, Prefix + "returns/" + returnId.ToString("D") + "/export-xml", "{}", cancellationToken).ConfigureAwait(false);
        return ExtractData(reply);
    }

    private static string ExtractData(SyncHttpReply reply)
    {
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException(
                "Swiss VAT request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".",
                inner: null,
                reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);
        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The Swiss VAT API response contains no data element.");
    }

    private async Task<string> GetDataAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        return ExtractData(reply);
    }
}
