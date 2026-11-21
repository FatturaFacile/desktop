// <copyright file="AiApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.AiCopilot;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.AiCopilot;

/// <summary>
/// HTTP client of the web-api AI surface (/api/v1/ai); authentication, tenant header and token refresh come from the sync client.
/// </summary>
/// <param name="transport">The authenticated web-api transport.</param>
public sealed class AiApiClient(ISyncApiClient transport) : IAiApiClient
{
    private const string Route = "ai/suggestions";

    /// <inheritdoc/>
    public bool IsAuthenticated => transport.IsAuthenticated;

    /// <inheritdoc/>
    public async Task<AiSuggestionPage> GetSuggestionsAsync(int perPage, CancellationToken cancellationToken)
    {
        var body = await GetAsync(Route + "?per_page=" + perPage.ToString(CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
        return Parse(body, ParsePage);
    }

    /// <inheritdoc/>
    public async Task<AiInsightsDto> GetInsightsAsync(CancellationToken cancellationToken)
    {
        var body = await GetAsync("ai/insights", cancellationToken).ConfigureAwait(false);
        return Parse(body, root => ParseInsights(root.GetProperty("data")));
    }

    /// <inheritdoc/>
    public Task<AiDecisionOutcome> ApproveAsync(Guid id, string? editedPayloadJson, string? feedback, CancellationToken cancellationToken)
    {
        JsonElement? edited = null;
        if (!string.IsNullOrWhiteSpace(editedPayloadJson))
        {
            try
            {
                using var parsed = JsonDocument.Parse(editedPayloadJson);
                edited = parsed.RootElement.Clone();
            }
            catch (JsonException)
            {
                return Task.FromResult(new AiDecisionOutcome(AiApiStatus.Refused, "The edited payload is not valid JSON."));
            }
        }

        var body = JsonSerializer.Serialize(new Dictionary<string, object?>(StringComparer.Ordinal) { ["edited_payload"] = edited, ["feedback"] = feedback });
        return PostAsync(id, "approve", body, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<AiDecisionOutcome> RejectAsync(Guid id, string reason, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.Serialize(new Dictionary<string, string>(StringComparer.Ordinal) { ["reason"] = reason });
        return PostAsync(id, "reject", body, cancellationToken);
    }

    private static T Parse<T>(string body, Func<JsonElement, T> parser)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            return parser(document.RootElement);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
        {
            throw new HttpRequestException("Unexpected AI response: " + ex.Message, ex);
        }
    }

    private static List<T> Map<T>(JsonElement array, Func<JsonElement, T> selector)
    {
        List<T> result = [];
        var count = array.GetArrayLength();
        for (var index = 0; index < count; index++)
        {
            result.Add(selector(array[index]));
        }

        return result;
    }

    private static List<T> MapProperty<T>(JsonElement parent, string name, Func<JsonElement, T> selector) =>
        parent.TryGetProperty(name, out var array) && array.ValueKind == JsonValueKind.Array ? Map(array, selector) : [];

    private static AiSuggestionPage ParsePage(JsonElement root)
    {
        var items = Map(root.GetProperty("data"), ParseSuggestion);
        var total = root.TryGetProperty("meta", out var meta) && meta.TryGetProperty("total", out var count) && count.ValueKind == JsonValueKind.Number
            ? count.GetInt32()
            : items.Count;
        return new AiSuggestionPage(items, total);
    }

    private static AiInsightsDto ParseInsights(JsonElement data)
    {
        AiCashFlowDto? cashFlow = null;
        if (data.TryGetProperty("cash_flow", out var flow) && flow.ValueKind == JsonValueKind.Object)
        {
            cashFlow = new AiCashFlowDto(
                OptionalString(flow, "currency"),
                flow.TryGetProperty("horizon_days", out var horizon) && horizon.ValueKind == JsonValueKind.Number ? horizon.GetInt32() : 0,
                MapProperty(flow, "dates", v => v.ValueKind == JsonValueKind.String ? v.GetString() ?? string.Empty : v.GetRawText()),
                MapProperty(flow, "p10", v => ToDecimal(v) ?? 0m),
                MapProperty(flow, "p50", v => ToDecimal(v) ?? 0m),
                MapProperty(flow, "p90", v => ToDecimal(v) ?? 0m));
        }

        var risks = data.TryGetProperty("payment_risk", out var payment) && payment.ValueKind == JsonValueKind.Array ? payment.GetArrayLength() : 0;
        return new AiInsightsDto(
            cashFlow,
            risks,
            MapProperty(data, "recommendations", ParseSuggestion),
            MapProperty(data, "anomalies", ParseSuggestion));
    }

    private static AiSuggestionDto ParseSuggestion(JsonElement e) => new(
        Guid.Parse(e.GetProperty("id").GetString()!),
        OptionalString(e, "kind") ?? string.Empty,
        OptionalString(e, "workflow") ?? string.Empty,
        OptionalString(e, "source_type") ?? string.Empty,
        OptionalString(e, "source_reference") ?? string.Empty,
        OptionalString(e, "source_text"),
        RawObject(e, "proposed_payload"),
        RawObject(e, "field_confidences"),
        OptionalNumber(e, "confidence") is { } confidence ? (double)confidence : 0d,
        OptionalNumber(e, "amount"),
        OptionalString(e, "reasoning"),
        OptionalString(e, "status") ?? string.Empty,
        OptionalString(e, "priority") ?? "normal",
        OptionalString(e, "provider_used"),
        OptionalString(e, "model_used"),
        OptionalDate(e, "expires_at"),
        OptionalDate(e, "created_at"));

    private static string? OptionalString(JsonElement e, string name) =>
        e.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static decimal? OptionalNumber(JsonElement e, string name) =>
        e.TryGetProperty(name, out var value) ? ToDecimal(value) : null;

    private static decimal? ToDecimal(JsonElement value)
    {
        decimal? result = null;
        if (value.ValueKind == JsonValueKind.Number)
        {
            result = value.GetDecimal();
        }
        else if (value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            result = parsed;
        }

        return result;
    }

    private static DateTime? OptionalDate(JsonElement e, string name) =>
        OptionalString(e, name) is { } text && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date) ? date.UtcDateTime : null;

    private static string RawObject(JsonElement e, string name) =>
        e.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object ? value.GetRawText() : "{}";

    private static string ProblemDetail(SyncHttpReply reply)
    {
        try
        {
            using var document = JsonDocument.Parse(reply.Body);
            var root = document.RootElement;
            var text = OptionalString(root, "detail") ?? OptionalString(root, "title") ?? OptionalString(root, "message");
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }
        catch (JsonException)
        {
            // Not a JSON problem document: fall back to the status code.
        }

        return "HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture);
    }

    private static AiDecisionOutcome Classify(SyncHttpReply reply)
    {
        if (reply.IsSuccess)
        {
            return new AiDecisionOutcome(AiApiStatus.Success);
        }

        var detail = ProblemDetail(reply);
        var status = (int)reply.Status;
        if (status == (int)HttpStatusCode.Conflict)
        {
            return new AiDecisionOutcome(AiApiStatus.AlreadyDecided, detail);
        }

        if (status == (int)HttpStatusCode.UnprocessableEntity)
        {
            return new AiDecisionOutcome(AiApiStatus.Incomplete, detail);
        }

        if (status == (int)HttpStatusCode.NotFound)
        {
            return new AiDecisionOutcome(AiApiStatus.NotFound, detail);
        }

        if (status == (int)HttpStatusCode.Unauthorized)
        {
            return new AiDecisionOutcome(AiApiStatus.Unauthorized, detail);
        }

        var transient = status is (int)HttpStatusCode.RequestTimeout or (int)HttpStatusCode.TooManyRequests or >= 500;
        return new AiDecisionOutcome(transient ? AiApiStatus.Transient : AiApiStatus.Refused, detail);
    }

    private async Task<string> GetAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await transport.SendJsonAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);
        return reply.IsSuccess ? reply.Body : throw new HttpRequestException(ProblemDetail(reply), inner: null, reply.Status);
    }

    private async Task<AiDecisionOutcome> PostAsync(Guid id, string action, string body, CancellationToken cancellationToken)
    {
        var url = $"{Route}/{id:D}/{action}";
        try
        {
            var reply = await transport.SendJsonAsync(HttpMethod.Post, url, body, cancellationToken).ConfigureAwait(false);
            return Classify(reply);
        }
        catch (HttpRequestException ex)
        {
            return new AiDecisionOutcome(AiApiStatus.Transient, ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new AiDecisionOutcome(AiApiStatus.Transient, ex.Message);
        }
    }
}
