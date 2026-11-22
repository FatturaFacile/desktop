// <copyright file="LedgerApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.Accounting;

/// <summary>
/// Calls the General Ledger endpoints of the web-api with the authenticated sync session.
/// </summary>
/// <param name="api">The authenticated web-api client.</param>
public sealed class LedgerApiClient(ISyncApiClient api) : ILedgerApiClient
{
    private const string Base = "general-ledger/";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetTemplatesAsync(CancellationToken cancellationToken) =>
        DataAsync(HttpMethod.Get, Base + "accounting-templates", body: null, cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetAccountsAsync(CancellationToken cancellationToken) =>
        DataAsync(HttpMethod.Get, Base + "accounts", body: null, cancellationToken);

    /// <inheritdoc/>
    public Task<string> PreviewTemplateAsync(Guid templateId, string body, CancellationToken cancellationToken) =>
        DataAsync(HttpMethod.Post, Base + "accounting-templates/" + templateId + "/preview", body, cancellationToken);

    /// <inheritdoc/>
    public Task<string> PostTemplateAsync(Guid templateId, string body, CancellationToken cancellationToken) =>
        DataAsync(HttpMethod.Post, Base + "accounting-templates/" + templateId + "/post", body, cancellationToken);

    /// <inheritdoc/>
    public Task<string> CreateJournalAsync(string body, CancellationToken cancellationToken) =>
        DataAsync(HttpMethod.Post, Base + "journals", body, cancellationToken);

    /// <inheritdoc/>
    public Task<string> ReverseJournalAsync(Guid journalId, string body, CancellationToken cancellationToken) =>
        DataAsync(HttpMethod.Post, Base + "journals/" + journalId + "/reverse", body, cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetTrialBalanceAsync(string query, CancellationToken cancellationToken) =>
        DataAsync(HttpMethod.Get, Base + "reports/trial-balance?" + query, body: null, cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetAccountLedgerAsync(string query, CancellationToken cancellationToken) =>
        DataAsync(HttpMethod.Get, Base + "reports/account-ledger?" + query, body: null, cancellationToken);

    private async Task<string> DataAsync(HttpMethod method, string url, string? body, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(method, url, body, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("General ledger request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);
        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The general ledger answer has no data.");
    }
}
