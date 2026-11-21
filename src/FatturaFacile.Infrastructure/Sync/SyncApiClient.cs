// <copyright file="SyncApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// HTTP client of the web-api sync surface (login, refresh, delta feed and idempotent writes).
/// </summary>
/// <param name="http">The configured HTTP client (base address is the API root).</param>
/// <param name="session">The in-memory session.</param>
public sealed partial class SyncApiClient(HttpClient http, SyncSession session) : ISyncApiClient
{
    private const string ApiPrefix = "api/v1/";

    /// <inheritdoc/>
    public bool IsAuthenticated => session.IsAuthenticated;

    /// <inheritdoc/>
    public async Task<bool> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.Serialize(new Dictionary<string, string>(StringComparer.Ordinal) { ["email"] = email, ["password"] = password });
        var reply = await SendRawAsync(Spec(HttpMethod.Post, ApiPrefix + "auth/login", body), authorize: false, cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            return false;
        }

        using var document = JsonDocument.Parse(reply.Body);
        var data = document.RootElement.GetProperty("data");
        var token = data.GetProperty("token").GetString();
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        string? tenantId = null;
        if (data.TryGetProperty("active_tenant", out var tenant) && tenant.ValueKind == JsonValueKind.Object)
        {
            tenantId = tenant.GetProperty("id").GetString();
        }

        session.Set(token, tenantId);
        return true;
    }

    /// <inheritdoc/>
    public void Logout() => session.Clear();

    /// <inheritdoc/>
    public async Task<SyncDeltaPage> GetDeltaAsync(string entity, DateTime? updatedSince, string? cursor, int limit, CancellationToken cancellationToken)
    {
        StringBuilder url = new(ApiPrefix);
        _ = url.Append("mobile/sync/").Append(Uri.EscapeDataString(entity)).Append("?limit=").Append(limit.ToString(CultureInfo.InvariantCulture));
        if (updatedSince is { } since)
        {
            _ = url.Append("&updated_since=").Append(Uri.EscapeDataString(DateTime.SpecifyKind(since, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture)));
        }

        if (!string.IsNullOrEmpty(cursor))
        {
            _ = url.Append("&cursor=").Append(Uri.EscapeDataString(cursor));
        }

        var address = url.ToString();
        var reply = await SendWithRefreshAsync(Spec(HttpMethod.Get, address), cancellationToken).ConfigureAwait(false);
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Delta request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        using var document = JsonDocument.Parse(reply.Body);
        return ParseDelta(document.RootElement.GetProperty("data"));
    }

    /// <inheritdoc/>
    public Task<SyncSendOutcome> SendAsync(SyncRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendCoreAsync(request, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<SyncHttpReply> SendJsonAsync(HttpMethod method, string relativeUrl, string? jsonBody, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativeUrl);
        return SendJsonCoreAsync(method, relativeUrl, jsonBody, cancellationToken);
    }

    private static SyncDeltaPage ParseDelta(JsonElement data)
    {
        var rows = data.GetProperty("data").Deserialize<List<JsonElement>>() ?? [];
        var deletedRaw = data.GetProperty("deleted").Deserialize<List<string>>() ?? [];
        List<Guid> deleted = [.. deletedRaw.Where(raw => Guid.TryParse(raw, out _)).Select(Guid.Parse)];

        var cursor = data.TryGetProperty("next_cursor", out var next) && next.ValueKind == JsonValueKind.String ? next.GetString() : null;
        var serverTime = DateTimeOffset.Parse(data.GetProperty("server_time").GetString()!, CultureInfo.InvariantCulture).UtcDateTime;
        var hasMore = data.TryGetProperty("has_more", out var more) && more.ValueKind == JsonValueKind.True;

        return new SyncDeltaPage(rows, deleted, cursor, serverTime, hasMore);
    }

    private static RequestSpec Spec(HttpMethod method, string url, string? body = null, Guid? key = null) => new(method, url, body, key);

    private static HttpRequestMessage CreateMessage(RequestSpec spec)
    {
        HttpRequestMessage message = new(spec.Method, spec.Url);
        if (spec.IdempotencyKey is { } key)
        {
            _ = message.Headers.TryAddWithoutValidation("Idempotency-Key", key.ToString("D", CultureInfo.InvariantCulture));
        }

        if (spec.Body is not null)
        {
            message.Content = new StringContent(spec.Body, Encoding.UTF8, "application/json");
        }

        return message;
    }

    private static SyncSendOutcome Classify(HttpReply reply, SyncOperation operation)
    {
        var status = reply.Status;
        if (reply.IsSuccess || (operation == SyncOperation.Delete && status == HttpStatusCode.NotFound))
        {
            return new SyncSendOutcome(SyncSendResult.Success);
        }

        if (status == HttpStatusCode.Unauthorized)
        {
            return new SyncSendOutcome(SyncSendResult.Unauthorized, "Session expired.");
        }

        var error = "HTTP " + ((int)status).ToString(CultureInfo.InvariantCulture) + ": " + (reply.Body.Length > 300 ? reply.Body[..300] : reply.Body);
        if (status is HttpStatusCode.Conflict or HttpStatusCode.PreconditionFailed)
        {
            return new SyncSendOutcome(SyncSendResult.Conflict, error);
        }

        var transient = status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)status >= 500;
        return new SyncSendOutcome(transient ? SyncSendResult.Transient : SyncSendResult.Rejected, error);
    }

    private async Task<SyncHttpReply> SendJsonCoreAsync(HttpMethod method, string relativeUrl, string? jsonBody, CancellationToken cancellationToken)
    {
        var reply = await SendWithRefreshAsync(Spec(method, ApiPrefix + relativeUrl, jsonBody), cancellationToken).ConfigureAwait(false);
        return new SyncHttpReply(reply.Status, reply.Body);
    }

    private async Task<SyncSendOutcome> SendCoreAsync(SyncRequest request, CancellationToken cancellationToken)
    {
        var path = ApiPrefix + request.Route;
        var url = request.Operation == SyncOperation.Create ? path : string.Concat(path, SyncOptions.UrlSeparator, request.EntityId.ToString("D", CultureInfo.InvariantCulture));
        var method = request.Operation switch
        {
            SyncOperation.Create => HttpMethod.Post,
            SyncOperation.Update => HttpMethod.Put,
            SyncOperation.Delete => HttpMethod.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.Operation, "Unknown operation."),
        };

        try
        {
            var reply = await SendWithRefreshAsync(Spec(method, url, request.JsonPayload, request.IdempotencyKey), cancellationToken).ConfigureAwait(false);
            return Classify(reply, request.Operation);
        }
        catch (HttpRequestException ex)
        {
            return new SyncSendOutcome(SyncSendResult.Transient, ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new SyncSendOutcome(SyncSendResult.Transient, ex.Message);
        }
    }

    private async Task<HttpReply> SendWithRefreshAsync(RequestSpec spec, CancellationToken cancellationToken)
    {
        var reply = await SendRawAsync(spec, authorize: true, cancellationToken).ConfigureAwait(false);
        return reply.Status != HttpStatusCode.Unauthorized || !await TryRefreshAsync(cancellationToken).ConfigureAwait(false)
            ? reply
            : await SendRawAsync(spec, authorize: true, cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpReply> SendRawAsync(RequestSpec spec, bool authorize, CancellationToken cancellationToken)
    {
        using var message = CreateMessage(spec);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (authorize)
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
            if (session.ServerTenantId is { Length: > 0 } tenantId)
            {
                _ = message.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId);
            }
        }

        using var response = await http.SendAsync(message, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return new HttpReply(response.StatusCode, body);
    }

    private async Task<bool> TryRefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            var reply = await SendRawAsync(Spec(HttpMethod.Post, ApiPrefix + "auth/refresh"), authorize: true, cancellationToken).ConfigureAwait(false);
            if (reply.IsSuccess)
            {
                using var document = JsonDocument.Parse(reply.Body);
                var token = document.RootElement.GetProperty("data").GetProperty("token").GetString();
                if (!string.IsNullOrEmpty(token))
                {
                    session.Set(token, tenantId: null);
                    return true;
                }
            }
        }
        catch (HttpRequestException)
        {
            // Offline: keep the session, the caller reports the failure as transient.
            return false;
        }

        session.Clear();
        return false;
    }

    private readonly record struct RequestSpec(HttpMethod Method, string Url, string? Body, Guid? IdempotencyKey);

    private readonly record struct HttpReply(HttpStatusCode Status, string Body)
    {
        public bool IsSuccess => (int)Status is >= 200 and < 300;
    }
}
