// <copyright file="FieldServiceApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.FieldService;

/// <summary>
/// <see cref="IFieldServiceApiClient"/> over the authorized HTTP client of the sync engine (bearer token, tenant header and one
/// token refresh on 401).
/// </summary>
/// <param name="api">The authorized sync client.</param>
public sealed class FieldServiceApiClient(ISyncApiClient api) : IFieldServiceApiClient
{
    private const string Base = "fsm/";
    private const string InstantFormat = "o";
    private static readonly byte[] PdfSignature = Encoding.ASCII.GetBytes("%PDF-");

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<string> GetBoardAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
        DataAsync(
            Base + "board?from=" + Uri.EscapeDataString(from.ToString(InstantFormat, CultureInfo.InvariantCulture)) + "&to=" + Uri.EscapeDataString(to.ToString(InstantFormat, CultureInfo.InvariantCulture)),
            cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetWorkOrdersAsync(CancellationToken cancellationToken) => DataAsync(Base + "work-orders", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetWorkOrderAsync(string workOrderId, CancellationToken cancellationToken) =>
        DataAsync(Base + "work-orders/" + Uri.EscapeDataString(workOrderId), cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetContractsAsync(CancellationToken cancellationToken) => DataAsync(Base + "maintenance-contracts", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetEquipmentAsync(CancellationToken cancellationToken) => DataAsync(Base + "equipment", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetEquipmentHistoryAsync(string equipmentId, CancellationToken cancellationToken) =>
        DataAsync(Base + "equipment/" + Uri.EscapeDataString(equipmentId) + "/service-history", cancellationToken);

    /// <inheritdoc/>
    public Task<string> GetTechnicianBundleAsync(string technicianId, CancellationToken cancellationToken) =>
        DataAsync(Base + "technicians/" + Uri.EscapeDataString(technicianId) + "/offline-bundle", cancellationToken);

    /// <inheritdoc/>
    public async Task<FieldServicePdf> DownloadReportPdfAsync(string reportId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportId);

        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, Base + "service-reports/" + Uri.EscapeDataString(reportId) + "/pdf", jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw Failure(reply.Status);
        }

        if (!reply.Content.Span.StartsWith(PdfSignature))
        {
            throw Failure(HttpStatusCode.BadGateway);
        }

        return new FieldServicePdf("service-report-" + reportId + ".pdf", reply.Content);
    }

    /// <inheritdoc/>
    public async Task<FieldServiceApiReply> SendAsync(HttpMethod method, string route, string? jsonBody, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(route);

        var reply = await api.SendAuthorizedAsync(method, Base + route, jsonBody, cancellationToken).ConfigureAwait(false);

        return new FieldServiceApiReply(reply.Status, reply.Text);
    }

    private static HttpRequestException Failure(HttpStatusCode status) =>
        new("Field service request failed with HTTP " + ((int)status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, status);

    private async Task<string> DataAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw Failure(reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Text);

        return document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The field service answer has no data.");
    }
}
