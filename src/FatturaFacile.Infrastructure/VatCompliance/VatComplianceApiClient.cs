// <copyright file="VatComplianceApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Sync;
using FatturaFacile.Application.VatCompliance;

namespace FatturaFacile.Infrastructure.VatCompliance;

/// <summary>
/// <see cref="IVatComplianceApiClient"/> over the authorized HTTP client of the sync engine.
/// </summary>
/// <param name="api">The authorized sync client.</param>
public sealed class VatComplianceApiClient(ISyncApiClient api) : IVatComplianceApiClient
{
    private const string Ledger = "general-ledger/";
    private const string Letters = "invoicing/declarations-of-intent";
    private const string Slash = "/";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetIntentLettersAsync(int year, CancellationToken cancellationToken) =>
        DataAsync(Letters + "?year=" + Number(year), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetSettlementsAsync(int year, CancellationToken cancellationToken) =>
        DataAsync(Ledger + "tax-settlements?per_page=100&year=" + Number(year), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetWithholdingSettlementsAsync(int year, CancellationToken cancellationToken) =>
        DataAsync(Ledger + "withholding-settlements?year=" + Number(year), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetWithholdingSettlementAsync(Guid settlementId, CancellationToken cancellationToken) =>
        DataAsync(Ledger + "withholding-settlements/" + settlementId.ToString("D"), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetRegisterAsync(string type, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        return DataAsync(
            Ledger + "vat-registers/" + Uri.EscapeDataString(type) + "?from=" + from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "&to=" + to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SyncHttpReply> CreateIntentLetterAsync(string json, CancellationToken cancellationToken) =>
        api.SendJsonAsync(HttpMethod.Post, Letters, json, cancellationToken);

    /// <inheritdoc/>
    public Task<SyncHttpReply> UpdateIntentLetterAsync(Guid serverId, string json, CancellationToken cancellationToken) =>
        api.SendJsonAsync(HttpMethod.Put, Letters + Slash + serverId.ToString("D"), json, cancellationToken);

    /// <inheritdoc/>
    public Task<SyncHttpReply> DeleteIntentLetterAsync(Guid serverId, CancellationToken cancellationToken) =>
        api.SendJsonAsync(HttpMethod.Delete, Letters + Slash + serverId.ToString("D"), jsonBody: null, cancellationToken);

    /// <inheritdoc/>
    public Task<SyncHttpReply> PayWithholdingSettlementAsync(Guid serverId, string json, CancellationToken cancellationToken) =>
        api.SendJsonAsync(HttpMethod.Post, Ledger + "withholding-settlements/" + serverId.ToString("D") + "/pay", json, cancellationToken);

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private async Task<string> DataAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("VAT compliance request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The VAT compliance answer has no data.");
    }
}
