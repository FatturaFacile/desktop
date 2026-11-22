// <copyright file="HelpdeskApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.Helpdesk;

/// <summary>
/// Calls the helpdesk endpoints of the web-api (esm module) with the authorized sync session.
/// </summary>
/// <param name="api">The authorized web-api client.</param>
public sealed class HelpdeskApiClient(ISyncApiClient api) : IHelpdeskApiClient
{
    private const string Base = "esm/";
    private const string DateFormat = "yyyy-MM-dd";
    private const int BreachRiskLimit = 100;

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetBreachRiskAsync(int riskPercent, CancellationToken cancellationToken) =>
        DataAsync(Base + "helpdesk/breach-risk?risk_percent=" + Number(riskPercent) + "&limit=" + Number(BreachRiskLimit), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetMetricsAsync(DateOnly from, DateOnly to, string groupBy, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);

        return DataAsync(
            Base + "helpdesk/metrics?from=" + from.ToString(DateFormat, CultureInfo.InvariantCulture) + "&to=" + to.ToString(DateFormat, CultureInfo.InvariantCulture) + "&group_by=" + Uri.EscapeDataString(groupBy),
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<string> GetCustomerContextAsync(Guid ticketId, CancellationToken cancellationToken) =>
        DataAsync(Base + "tickets/" + ticketId.ToString("D", CultureInfo.InvariantCulture) + "/customer-context", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetEscalationsAsync(Guid ticketId, CancellationToken cancellationToken) =>
        DataAsync(Base + "tickets/" + ticketId.ToString("D", CultureInfo.InvariantCulture) + "/escalations", cancellationToken);

    /// <inheritdoc/>
    public Task<string> SearchKnowledgeAsync(string text, string locale, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);

        var query = string.IsNullOrWhiteSpace(text) ? string.Empty : "&q=" + Uri.EscapeDataString(text);

        return DataAsync(Base + "kb/articles?is_published=1&locale=" + Uri.EscapeDataString(locale) + query, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<HelpdeskApiReply> SendAsync(string route, string jsonBody, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonBody);

        var reply = await api.SendAuthorizedAsync(HttpMethod.Post, route, jsonBody, cancellationToken).ConfigureAwait(false);

        return new HelpdeskApiReply(reply.Status, reply.Text);
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private async Task<string> DataAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Helpdesk request failed with HTTP " + Number((int)reply.Status) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The helpdesk answer has no data.");
    }
}
