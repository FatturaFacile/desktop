// <copyright file="ManufacturingApiGateway.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Application.Manufacturing.Sync;
using FatturaFacile.Application.Sync;
using FatturaFacile.Infrastructure.Sync;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <summary>
/// HTTP gateway to /api/v1/manufacturing, riding on the typed sync client (session, token refresh, tenant header, idempotency header).
/// </summary>
/// <param name="api">The sync client.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class ManufacturingApiGateway(SyncApiClient api, TimeProvider timeProvider) : IManufacturingApiGateway
{
    private const string Prefix = "manufacturing/";
    private const int PageSize = 100;

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<ManufacturingSendOutcome> SendAsync(ManufacturingRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendCoreAsync(request, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ManufacturingPage> GetPageAsync(string resource, DateTime? updatedSince, int page, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        return GetPageCoreAsync(resource, updatedSince, page, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<string?> GetJsonAsync(string route, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        return GetJsonCoreAsync(route, cancellationToken);
    }

    private static ManufacturingSendOutcome Classify(SyncHttpReply reply)
    {
        var code = (int)reply.Status;
        if (reply.IsSuccess)
        {
            return new ManufacturingSendOutcome(SyncSendResult.Success, code, Error: null, ResponseBody: reply.Body);
        }

        var error = "HTTP " + code.ToString(CultureInfo.InvariantCulture) + ": " + (reply.Body.Length > 300 ? reply.Body[..300] : reply.Body);
        if (reply.Status == HttpStatusCode.Unauthorized)
        {
            return new ManufacturingSendOutcome(SyncSendResult.Unauthorized, code, "Session expired.", ResponseBody: null);
        }

        // 409/412 are state conflicts; 422 is how the module reports a lifecycle rule violation. The resolver reads the server state to decide.
        if (reply.Status is HttpStatusCode.Conflict or HttpStatusCode.PreconditionFailed or HttpStatusCode.UnprocessableEntity)
        {
            return new ManufacturingSendOutcome(SyncSendResult.Conflict, code, error, ResponseBody: reply.Body);
        }

        var transient = reply.Status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || code >= 500;
        return new ManufacturingSendOutcome(transient ? SyncSendResult.Transient : SyncSendResult.Rejected, code, error, ResponseBody: reply.Body);
    }

    private static string ReadData(SyncHttpReply reply) =>
        reply.IsSuccess ? UnwrapData(reply.Body) : throw new HttpRequestException("Manufacturing request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);

    private static string UnwrapData(string body)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        return root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data) ? data.GetRawText() : root.GetRawText();
    }

    private async Task<ManufacturingSendOutcome> SendCoreAsync(ManufacturingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var reply = await api.SendIdempotentAsync(HttpMethod.Post, Prefix + request.Route, request.JsonPayload ?? "{}", request.IdempotencyKey, cancellationToken).ConfigureAwait(false);
            return Classify(reply);
        }
        catch (HttpRequestException ex)
        {
            return new ManufacturingSendOutcome(SyncSendResult.Transient, 0, ex.Message, ResponseBody: null);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new ManufacturingSendOutcome(SyncSendResult.Transient, 0, ex.Message, ResponseBody: null);
        }
    }

    private async Task<ManufacturingPage> GetPageCoreAsync(string resource, DateTime? updatedSince, int page, CancellationToken cancellationToken)
    {
        StringBuilder url = new(Prefix);
        _ = url.Append(resource).Append("?per_page=").Append(PageSize.ToString(CultureInfo.InvariantCulture)).Append("&page=").Append(page.ToString(CultureInfo.InvariantCulture));
        if (updatedSince is { } since)
        {
            _ = url.Append("&updated_since=").Append(Uri.EscapeDataString(DateTime.SpecifyKind(since, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture)));
        }

        // The instant before the request is the safe checkpoint: rows changed while the pull runs are fetched again next time.
        var serverTime = timeProvider.GetUtcNow().UtcDateTime;
        var reply = await api.SendJsonAsync(HttpMethod.Get, url.ToString(), jsonBody: null, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Manufacturing list request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Body);
        var root = document.RootElement;
        var envelope = root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object && data.TryGetProperty("data", out _) ? data : root;
        var rows = envelope.TryGetProperty("data", out var list) ? WorkbenchJson.Items(list) : [];

        var hasMore = false;
        if (envelope.TryGetProperty("meta", out var meta) && meta.ValueKind == JsonValueKind.Object)
        {
            hasMore = WorkbenchJson.IntOrZero(meta, "current_page") < WorkbenchJson.IntOrZero(meta, "last_page");
        }

        return new ManufacturingPage(rows, hasMore, serverTime);
    }

    private async Task<string?> GetJsonCoreAsync(string route, CancellationToken cancellationToken)
    {
        var reply = await api.SendJsonAsync(HttpMethod.Get, Prefix + route, jsonBody: null, cancellationToken).ConfigureAwait(false);
        return reply.Status == HttpStatusCode.NotFound ? null : ReadData(reply);
    }
}
