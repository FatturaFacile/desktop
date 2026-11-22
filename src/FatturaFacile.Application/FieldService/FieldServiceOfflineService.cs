// <copyright file="FieldServiceOfflineService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Default <see cref="IFieldServiceOfflineService"/>. Operations are saved in the outbox with their own client identifier and
/// the instant the technician acted, then sent to `POST /api/v1/fsm/mobile-sync` in batches: an operation the server reports as
/// `applied` or `duplicate` is done, one it reports as `rejected` becomes a conflict, and a network failure keeps it pending
/// with an exponential back-off. A lost answer is therefore harmless: the replay is a `duplicate`.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="store">The local outbox.</param>
/// <param name="time">The clock.</param>
public sealed class FieldServiceOfflineService(IFieldServiceApiClient api, IFieldServiceStore store, TimeProvider time) : IFieldServiceOfflineService
{
    private const string Route = "mobile-sync";
    private const string TooManyAttempts = "Too many failed delivery attempts.";

    /// <inheritdoc/>
    public Task<FieldServiceQueueResult> QueueStatusAsync(Guid workOrderId, string status, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(status);

        var error = FieldServiceValidation.CheckStatus(status);
        if (error is not null)
        {
            return Task.FromResult(Invalid(error));
        }

        var now = Now();

        return QueueAsync(NewEntry(FieldServiceOperationType.StatusTransition, workOrderId, FieldServiceRequests.StatusPayload(status, now), now), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<FieldServiceQueueResult> QueuePartAsync(FieldServicePartInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var error = FieldServiceValidation.CheckPart(input);

        return error is not null
            ? Task.FromResult(Invalid(error))
            : QueueAsync(NewEntry(FieldServiceOperationType.PartConsumption, input.WorkOrderId, FieldServiceRequests.PartPayload(input), Now()), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<FieldServiceQueueResult> QueueReportAsync(FieldServiceReportInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var error = FieldServiceValidation.CheckReport(input);
        if (error is not null)
        {
            return Task.FromResult(Invalid(error));
        }

        var now = Now();
        var safe = input with { Locale = FieldServiceValidation.SupportedLocale(input.Locale) };

        return QueueAsync(NewEntry(FieldServiceOperationType.ServiceReport, input.WorkOrderId, FieldServiceRequests.ReportPayload(safe, now), now), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<FieldServiceFlushResult> FlushAsync(CancellationToken cancellationToken) => FlushCoreAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<FieldServiceOutboxSummary> SummaryAsync(CancellationToken cancellationToken)
    {
        var pending = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        var conflicts = await store.ConflictsAsync(cancellationToken).ConfigureAwait(false);

        return new FieldServiceOutboxSummary(
            pending.Count,
            [.. conflicts.Select(entry => new FieldServiceConflict(entry.Id, entry.Operation, entry.WorkOrderId, DateTime.SpecifyKind(entry.OccurredAtUtc, DateTimeKind.Utc), entry.LastError ?? string.Empty))]);
    }

    /// <inheritdoc/>
    public async Task DismissConflictAsync(Guid entryId, CancellationToken cancellationToken)
    {
        var conflicts = await store.ConflictsAsync(cancellationToken).ConfigureAwait(false);
        var entry = conflicts.FirstOrDefault(candidate => candidate.Id == entryId);
        if (entry is null)
        {
            return;
        }

        entry.Status = FieldServiceOutboxStatus.Dismissed;
        entry.UpdatedAt = Now();
        await store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static FieldServiceQueueResult Invalid(string errorKey) => new(errorKey, Sent: false, Conflict: false, ClientUuid: string.Empty);

    private static FieldServiceOutboxEntry NewEntry(FieldServiceOperationType operation, Guid workOrderId, string payload, DateTime occurredAtUtc) => new()
    {
        Operation = operation,
        WorkOrderId = workOrderId,
        PayloadJson = payload,
        ClientUuid = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
        OccurredAtUtc = occurredAtUtc,
        NextAttemptAt = occurredAtUtc,
    };

    private static Dictionary<string, JsonElement> ResultsByClient(string body)
    {
        using var document = JsonDocument.Parse(body);
        Dictionary<string, JsonElement> results = [with(StringComparer.Ordinal)];
        var data = FieldServiceJson.Child(document.RootElement, "data") ?? default;

        foreach (var result in FieldServiceJson.Items(data, "results"))
        {
            results[FieldServiceJson.Text(result, "client_uuid")] = result.Clone();
        }

        return results;
    }

    private static bool IsDone(JsonElement result) =>
        FieldServiceJson.NullableText(result, "status") is { } status
        && (string.Equals(status, "applied", StringComparison.Ordinal) || string.Equals(status, "duplicate", StringComparison.Ordinal));

    private DateTime Now() => time.GetUtcNow().UtcDateTime;

    private async Task<FieldServiceQueueResult> QueueAsync(FieldServiceOutboxEntry entry, CancellationToken cancellationToken)
    {
        await store.EnqueueAsync(entry, cancellationToken).ConfigureAwait(false);
        _ = await FlushCoreAsync(cancellationToken).ConfigureAwait(false);

        return new FieldServiceQueueResult(
            ErrorKey: null,
            Sent: entry.Status == FieldServiceOutboxStatus.Sent,
            Conflict: entry.Status == FieldServiceOutboxStatus.Conflict,
            entry.ClientUuid);
    }

    private async Task<FieldServiceFlushResult> FlushCoreAsync(CancellationToken cancellationToken)
    {
        var entries = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        if (entries.Count == 0)
        {
            return new FieldServiceFlushResult(0, 0, 0, Offline: false);
        }

        if (!api.IsAuthenticated)
        {
            return new FieldServiceFlushResult(0, 0, entries.Count, Offline: true);
        }

        var now = Now();
        var due = entries.Where(entry => entry.NextAttemptAt <= now).ToList();
        if (due.Count == 0)
        {
            return new FieldServiceFlushResult(0, 0, entries.Count, Offline: true);
        }

        var sent = 0;
        var conflicts = 0;
        var offline = false;

        foreach (var chunk in due.Chunk(FieldServiceBatch.MaxOperations))
        {
            var (batchSent, batchConflicts, batchOffline) = await SendBatchAsync(chunk, cancellationToken).ConfigureAwait(false);
            await store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            sent += batchSent;
            conflicts += batchConflicts;

            if (batchOffline)
            {
                offline = true;

                break;
            }
        }

        var remaining = (await store.PendingAsync(cancellationToken).ConfigureAwait(false)).Count;

        return new FieldServiceFlushResult(sent, conflicts, remaining, offline);
    }

    private async Task<(int Sent, int Conflicts, bool Offline)> SendBatchAsync(FieldServiceOutboxEntry[] chunk, CancellationToken cancellationToken)
    {
        FieldServiceApiReply reply;

        try
        {
            reply = await api.SendAsync(HttpMethod.Post, Route, FieldServiceBatch.Build(chunk), cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            Backoff(chunk, ex.Message);

            return (0, 0, true);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            Backoff(chunk, ex.Message);

            return (0, 0, true);
        }

        return FieldServiceCallRunner.Classify(reply.Status) switch
        {
            FieldServiceCallStatus.Ok => Apply(chunk, reply.Body),
            FieldServiceCallStatus.Unauthorized or FieldServiceCallStatus.Forbidden => (0, 0, true),
            FieldServiceCallStatus.Rejected => RefuseAll(chunk, FieldServiceErrorReader.Describe(reply.Body)),
            FieldServiceCallStatus.Offline or FieldServiceCallStatus.Failed => BackoffAll(chunk, reply.Status.ToString()),
            _ => BackoffAll(chunk, reply.Status.ToString()),
        };
    }

    private (int Sent, int Conflicts, bool Offline) Apply(FieldServiceOutboxEntry[] chunk, string body)
    {
        Dictionary<string, JsonElement> results;

        try
        {
            results = ResultsByClient(body);
        }
        catch (JsonException ex)
        {
            return BackoffAll(chunk, ex.Message);
        }

        var sent = 0;
        var conflicts = 0;

        foreach (var entry in chunk)
        {
            if (!results.TryGetValue(entry.ClientUuid, out var result))
            {
                // The server did not report this operation: it stays pending and is replayed (the client identifier protects it).
                Backoff([entry], "The server did not report the operation.");

                continue;
            }

            if (IsDone(result))
            {
                Finish(entry, FieldServiceOutboxStatus.Sent, error: null);
                sent++;
            }
            else
            {
                Finish(entry, FieldServiceOutboxStatus.Conflict, FieldServiceErrorReader.Describe(result));
                conflicts++;
            }
        }

        return (sent, conflicts, false);
    }

    private (int Sent, int Conflicts, bool Offline) RefuseAll(FieldServiceOutboxEntry[] chunk, string error)
    {
        foreach (var entry in chunk)
        {
            Finish(entry, FieldServiceOutboxStatus.Conflict, error);
        }

        return (0, chunk.Length, false);
    }

    private (int Sent, int Conflicts, bool Offline) BackoffAll(FieldServiceOutboxEntry[] chunk, string error)
    {
        Backoff(chunk, error);

        return (0, 0, true);
    }

    private void Finish(FieldServiceOutboxEntry entry, FieldServiceOutboxStatus status, string? error)
    {
        entry.Status = status;
        entry.LastError = error;
        entry.UpdatedAt = Now();
    }

    private void Backoff(IEnumerable<FieldServiceOutboxEntry> entries, string error)
    {
        var now = Now();

        foreach (var entry in entries)
        {
            entry.Attempts++;
            entry.LastError = error;
            entry.UpdatedAt = now;

            if (SyncRetryPolicy.CanRetry(entry.Attempts))
            {
                entry.NextAttemptAt = now + SyncRetryPolicy.NextDelay(entry.Attempts);
            }
            else
            {
                entry.Status = FieldServiceOutboxStatus.Conflict;
                entry.LastError = TooManyAttempts + " " + error;
            }
        }
    }
}
