// <copyright file="InventoryCountingApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FatturaFacile.Application.InventoryCounting;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.InventoryCounting;

/// <summary>
/// HTTP client of the web-api physical inventory surface (/api/v1/inventory); authentication, tenant header and
/// token refresh come from the sync client. Network and HTTP failures are returned as failed results.
/// </summary>
/// <param name="transport">The authenticated web-api transport.</param>
public sealed class InventoryCountingApiClient(ISyncApiClient transport) : IInventoryCountingApiClient
{
    private const string Root = "inventory/";

    private static readonly JsonSerializerOptions ReadOptions = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private static readonly JsonSerializerOptions WriteOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <inheritdoc/>
    public bool IsAuthenticated => transport.IsAuthenticated;

    /// <inheritdoc/>
    public Task<InventoryCountingResult<IReadOnlyList<AbcRunDto>>> GetAbcRunsAsync(CancellationToken cancellationToken) =>
        ReadAsync(HttpMethod.Get, "abc-runs", body: null, ReadList<AbcRunDto>, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<AbcRunDto>> GetAbcRunAsync(string runId, CancellationToken cancellationToken) =>
        ReadAsync(HttpMethod.Get, "abc-runs/" + Uri.EscapeDataString(runId), body: null, ReadObject<AbcRunDto>, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<AbcRunDto>> RunAbcAsync(AbcRunRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ReadAsync(HttpMethod.Post, "abc-runs", JsonSerializer.Serialize(request, WriteOptions), ReadObject<AbcRunDto>, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<InventoryCountingResult<IReadOnlyList<AbcOverrideDto>>> GetAbcOverridesAsync(CancellationToken cancellationToken) =>
        ReadAsync(HttpMethod.Get, "abc-overrides", body: null, ReadList<AbcOverrideDto>, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> OverrideAbcClassAsync(string stockItemId, string? abcClass, string reason, CancellationToken cancellationToken)
    {
        // The class is serialized as an explicit null when the override is cleared.
        var body = new JsonObject { ["stock_item_id"] = stockItemId, ["abc_class"] = abcClass, ["reason"] = reason };
        return ActionAsync(HttpMethod.Post, "abc-overrides", body, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<InventoryCountingResult<IReadOnlyList<CycleCountPlanDto>>> GetPlansAsync(CancellationToken cancellationToken) =>
        ReadAsync(HttpMethod.Get, "cycle-count-plans", body: null, ReadList<CycleCountPlanDto>, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<IReadOnlyList<CycleCountPlanDto>>> GeneratePlansAsync(string warehouseId, string? warehouseZoneId, bool? freezesStock, CancellationToken cancellationToken)
    {
        var body = new JsonObject { ["warehouse_id"] = warehouseId };
        if (!string.IsNullOrWhiteSpace(warehouseZoneId))
        {
            body["warehouse_zone_id"] = warehouseZoneId;
        }

        if (freezesStock is { } freezes)
        {
            body["freezes_stock"] = freezes;
        }

        return ReadAsync(HttpMethod.Post, "cycle-count-plans/generate", body.ToJsonString(), ReadList<CycleCountPlanDto>, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> RunPlanAsync(string planId, CancellationToken cancellationToken) =>
        ActionAsync(HttpMethod.Post, "cycle-count-plans/" + Uri.EscapeDataString(planId) + "/run", body: null, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<int>> RunDuePlansAsync(CancellationToken cancellationToken) =>
        ReadAsync(HttpMethod.Post, "cycle-count-plans/run-due", body: null, ReadOpened, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> DeletePlanAsync(string planId, CancellationToken cancellationToken) =>
        ActionAsync(HttpMethod.Delete, "cycle-count-plans/" + Uri.EscapeDataString(planId), body: null, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<IReadOnlyList<CountSessionDto>>> GetSessionsAsync(CancellationToken cancellationToken) =>
        ReadAsync(HttpMethod.Get, "counts?per_page=100", body: null, ReadList<CountSessionDto>, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<CountSessionDto>> CreateSessionAsync(CreateCountSessionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ReadAsync(HttpMethod.Post, "counts", JsonSerializer.Serialize(request, WriteOptions), ReadObject<CountSessionDto>, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> FreezeAsync(string sessionId, CancellationToken cancellationToken) =>
        ActionAsync(HttpMethod.Post, SessionPath(sessionId, "freeze"), body: null, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> UnfreezeAsync(string sessionId, string reason, CancellationToken cancellationToken) =>
        ActionAsync(HttpMethod.Post, SessionPath(sessionId, "unfreeze"), new JsonObject { ["reason"] = reason }, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> AssignAsync(string sessionId, string? assignedTo, CancellationToken cancellationToken) =>
        ActionAsync(HttpMethod.Post, SessionPath(sessionId, "assign"), new JsonObject { ["assigned_to"] = assignedTo }, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> CancelAsync(string sessionId, CancellationToken cancellationToken) =>
        ActionAsync(HttpMethod.Post, SessionPath(sessionId, "cancel"), body: null, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<CountVarianceReportDto>> GetVarianceAsync(string sessionId, CancellationToken cancellationToken) =>
        ReadAsync(HttpMethod.Get, SessionPath(sessionId, "variance"), body: null, ReadObject<CountVarianceReportDto>, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> RecountAsync(string sessionId, IReadOnlyList<string> lineIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lineIds);

        JsonArray ids = [];
        foreach (var lineId in lineIds)
        {
            ids.Add(lineId);
        }

        return ActionAsync(HttpMethod.Post, SessionPath(sessionId, "recount"), new JsonObject { ["line_ids"] = ids }, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> SubmitDifferencesAsync(string sessionId, CancellationToken cancellationToken) =>
        ActionAsync(HttpMethod.Post, SessionPath(sessionId, "reconcile"), body: null, cancellationToken);

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> ApproveAsync(string sessionId, int level, string? notes, CancellationToken cancellationToken)
    {
        var body = new JsonObject();
        if (!string.IsNullOrWhiteSpace(notes))
        {
            body["notes"] = notes;
        }

        return ActionAsync(HttpMethod.Post, ApprovalPath(sessionId, level, "approve"), body, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<InventoryCountingResult<bool>> RejectAsync(string sessionId, int level, string reason, CancellationToken cancellationToken) =>
        ActionAsync(HttpMethod.Post, ApprovalPath(sessionId, level, "reject"), new JsonObject { ["reason"] = reason }, cancellationToken);

    /// <summary>
    /// Returns the payload of a response: the "data" member of the envelope when present, otherwise the root itself.
    /// </summary>
    /// <param name="root">The parsed response.</param>
    /// <returns>The payload.</returns>
    private static JsonElement Unwrap(JsonElement root) =>
        root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data) ? data : root;

    private static string SessionPath(string sessionId, string action) => "counts/" + Uri.EscapeDataString(sessionId) + "/" + action;

    private static string ApprovalPath(string sessionId, int level, string action) =>
        "counts/" + Uri.EscapeDataString(sessionId) + "/approvals/" + level.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/" + action;

    private static IReadOnlyList<T> ReadList<T>(JsonElement root)
    {
        var payload = Unwrap(root);

        // Paginated lists nest the rows once more in {data: {data: [...]}}.
        if (payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("data", out var inner))
        {
            payload = inner;
        }

        return payload.ValueKind == JsonValueKind.Array ? payload.Deserialize<List<T>>(ReadOptions) ?? [] : [];
    }

    private static T ReadObject<T>(JsonElement root)
        where T : class, new() =>
        Unwrap(root).Deserialize<T>(ReadOptions) ?? new T();

    private static int ReadOpened(JsonElement root)
    {
        var payload = Unwrap(root);
        foreach (var candidate in new[] { payload, root })
        {
            if (candidate.ValueKind == JsonValueKind.Object && candidate.TryGetProperty("opened", out var opened) && opened.ValueKind == JsonValueKind.Number)
            {
                return opened.GetInt32();
            }
        }

        return 0;
    }

    private static InventoryCountingResult<T> Failure<T>(SyncHttpReply reply)
    {
        var message = string.Empty;
        List<string> errors = [];

        try
        {
            using var document = JsonDocument.Parse(reply.Body);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                message = ReadString(root, "message") ?? ReadString(root, "detail") ?? ReadString(root, "title") ?? string.Empty;

                if (root.TryGetProperty("errors", out var bag) && bag.ValueKind == JsonValueKind.Object)
                {
                    var fields = bag.Deserialize<Dictionary<string, JsonElement>>() ?? [with(StringComparer.Ordinal)];
                    foreach (var field in fields.Values)
                    {
                        AddMessages(field, errors);
                    }
                }
            }
        }
        catch (JsonException)
        {
            // The body is not JSON: the status code is enough.
        }

        return InventoryCountingResults.Fail<T>((int)reply.Status, message, errors);
    }

    private static void AddMessages(JsonElement value, List<string> errors)
    {
        if (value.ValueKind == JsonValueKind.String)
        {
            errors.Add(value.GetString() ?? string.Empty);
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            for (var index = 0; index < value.GetArrayLength(); index++)
            {
                AddMessages(value[index], errors);
            }
        }
    }

    private static string? ReadString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private Task<InventoryCountingResult<bool>> ActionAsync(HttpMethod method, string path, JsonObject? body, CancellationToken cancellationToken) =>
        ReadAsync(method, path, body?.ToJsonString(), static _ => true, cancellationToken);

    private async Task<InventoryCountingResult<T>> ReadAsync<T>(HttpMethod method, string path, string? body, Func<JsonElement, T> parse, CancellationToken cancellationToken)
    {
        SyncHttpReply reply;

        try
        {
            reply = await transport.SendJsonAsync(method, Root + path, body, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            return InventoryCountingResults.Fail<T>(statusCode: 0, ex.Message);
        }

        if (!reply.IsSuccess)
        {
            return Failure<T>(reply);
        }

        try
        {
            if (string.IsNullOrWhiteSpace(reply.Body))
            {
                return InventoryCountingResults.Ok(parse(default), (int)reply.Status);
            }

            using var document = JsonDocument.Parse(reply.Body);
            return InventoryCountingResults.Ok(parse(document.RootElement), (int)reply.Status);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            return InventoryCountingResults.Fail<T>((int)reply.Status, "Unexpected inventory response: " + ex.Message);
        }
    }
}
