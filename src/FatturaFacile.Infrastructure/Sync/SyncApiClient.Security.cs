// <copyright file="SyncApiClient.Security.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using FatturaFacile.Application.Security;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Infrastructure.Sync;

/// <content>
/// Security surface of the web-api client: offline audit batches and PII unmasking. It reuses the session, the
/// refresh-on-401 behavior and the outcome classification of the sync client.
/// </content>
public sealed partial class SyncApiClient : ISecurityApiClient
{
    /// <inheritdoc/>
    public Task<AuditBatchOutcome> UploadAuditBatchAsync(AuditBatchRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return UploadAuditBatchCoreAsync(request, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<PiiUnmaskOutcome> UnmaskPiiAsync(PiiUnmaskRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return UnmaskPiiCoreAsync(request, cancellationToken);
    }

    /// <summary>
    /// Builds the JSON body of an offline audit batch. Entries carry their signed fields verbatim plus the offline origin.
    /// </summary>
    /// <param name="request">The batch.</param>
    /// <returns>The JSON text.</returns>
    internal static string BuildBatchBody(AuditBatchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entries = new JsonArray();
        foreach (var entry in request.Entries)
        {
            entries.Add(new JsonObject
            {
                ["sequence_number"] = entry.SequenceNumber,
                ["previous_hash"] = entry.PreviousHash,
                ["payload_hash"] = entry.PayloadHash,
                ["record_hash"] = entry.RecordHash,
                ["payload"] = JsonNode.Parse(entry.PayloadJson),
                ["occurred_at"] = AuditChain.FormatInstant(entry.OccurredAt),
                ["entity_type"] = entry.EntityType,
                ["entity_id"] = entry.EntityId,
                ["action"] = entry.Action,
                ["origin"] = "offline",
            });
        }

        return new JsonObject
        {
            ["batch_id"] = request.BatchId.ToString("D", CultureInfo.InvariantCulture),
            ["device_id"] = request.DeviceId,
            ["origin"] = SecurityApiRoutes.DesktopOfflineOrigin,
            ["entries"] = entries,
        }.ToJsonString();
    }

    /// <summary>
    /// Parses the acknowledgement of a batch.
    /// </summary>
    /// <param name="body">The response body.</param>
    /// <returns>The batch timestamp and the per-entry acknowledgements.</returns>
    internal static (DateTime? ReceivedAt, IReadOnlyList<AuditAcknowledgement> Entries) ParseBatchReply(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return (null, []);
        }

        using var document = JsonDocument.Parse(body);
        var data = document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty("data", out var inner) && inner.ValueKind == JsonValueKind.Object
            ? inner
            : document.RootElement;

        if (data.ValueKind != JsonValueKind.Object)
        {
            return (null, []);
        }

        var batchTime = ReadInstant(data, "received_at") ?? ReadInstant(data, "server_time");
        List<AuditAcknowledgement> acknowledgements = [];

        if (data.TryGetProperty("entries", out var entries) && entries.ValueKind == JsonValueKind.Array)
        {
            for (var index = 0; index < entries.GetArrayLength(); index++)
            {
                var item = entries[index];
                if (item.ValueKind == JsonValueKind.Object
                    && item.TryGetProperty("sequence_number", out var sequence)
                    && sequence.TryGetInt64(out var number)
                    && (ReadInstant(item, "server_received_at") ?? batchTime) is { } received)
                {
                    var reference = item.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String ? id.GetString() : null;
                    acknowledgements.Add(new AuditAcknowledgement(number, received, reference));
                }
            }
        }

        return (batchTime, acknowledgements);
    }

    private static DateTime? ReadInstant(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.String
        && DateTimeOffset.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed.UtcDateTime
            : null;

    private static string? ReadString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static string? ParseUnmaskedValue(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var data = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var inner) ? inner : root;
            return data.ValueKind == JsonValueKind.Object
                ? ReadString(data, "value") ?? ReadString(data, "unmasked_value")
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string RelativeRoute(string route) => route.TrimStart(SyncOptions.UrlSeparator);

    private async Task<AuditBatchOutcome> UploadAuditBatchCoreAsync(AuditBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var reply = await SendWithRefreshAsync(
                Spec(HttpMethod.Post, RelativeRoute(SecurityApiRoutes.OfflineAuditBatches), BuildBatchBody(request), request.BatchId),
                cancellationToken).ConfigureAwait(false);
            var outcome = Classify(reply, SyncOperation.Create);
            if (outcome.Result != SyncSendResult.Success)
            {
                return new AuditBatchOutcome(outcome.Result, Error: outcome.Error);
            }

            try
            {
                var (receivedAt, entries) = ParseBatchReply(reply.Body);
                return new AuditBatchOutcome(SyncSendResult.Success, receivedAt, entries);
            }
            catch (JsonException)
            {
                // The batch was accepted: an unreadable acknowledgement only loses the server timestamps.
                return new AuditBatchOutcome(SyncSendResult.Success);
            }
        }
        catch (HttpRequestException ex)
        {
            return new AuditBatchOutcome(SyncSendResult.Transient, Error: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new AuditBatchOutcome(SyncSendResult.Transient, Error: ex.Message);
        }
    }

    private async Task<PiiUnmaskOutcome> UnmaskPiiCoreAsync(PiiUnmaskRequest request, CancellationToken cancellationToken)
    {
        var body = new JsonObject
        {
            ["entity_type"] = request.EntityType,
            ["entity_id"] = request.EntityId,
            ["field"] = request.Field,
            ["justification"] = request.Justification,
        }.ToJsonString();

        try
        {
            var reply = await SendWithRefreshAsync(Spec(HttpMethod.Post, RelativeRoute(SecurityApiRoutes.PiiUnmask), body), cancellationToken).ConfigureAwait(false);
            var outcome = Classify(reply, SyncOperation.Create);
            return outcome.Result == SyncSendResult.Success
                ? new PiiUnmaskOutcome(SyncSendResult.Success, ParseUnmaskedValue(reply.Body))
                : new PiiUnmaskOutcome(outcome.Result, Error: outcome.Error);
        }
        catch (HttpRequestException ex)
        {
            return new PiiUnmaskOutcome(SyncSendResult.Transient, Error: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new PiiUnmaskOutcome(SyncSendResult.Transient, Error: ex.Message);
        }
    }
}
