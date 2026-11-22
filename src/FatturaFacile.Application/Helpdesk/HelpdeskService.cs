// <copyright file="HelpdeskService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// Offline-first helpdesk (Mod 083): reads go to the server and fall back to the last cached answer; time logs and escalations
/// are saved in the outbox first and replayed in order when the server is reachable. The server recognises a replay by the
/// idempotency key, and the escalation endpoints answer with the existing record when they are repeated.
/// </summary>
/// <param name="api">The helpdesk endpoints.</param>
/// <param name="store">The local cache and outbox.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="time">The clock.</param>
public sealed class HelpdeskService(IHelpdeskApiClient api, IHelpdeskStore store, ICurrentTenantService tenant, TimeProvider time) : IHelpdeskService
{
    /// <summary>
    /// The localization key of a missing ticket.
    /// </summary>
    public const string TicketKey = "Helpdesk_Err_Ticket";

    /// <summary>
    /// The localization key of a time entry outside one day.
    /// </summary>
    public const string MinutesKey = "Helpdesk_Err_Minutes";

    /// <summary>
    /// The localization key of a text that is too long.
    /// </summary>
    public const string TextKey = "Helpdesk_Err_Text";

    /// <summary>
    /// The localization key of a visit that ends before it starts.
    /// </summary>
    public const string ScheduleKey = "Helpdesk_Err_Schedule";

    /// <summary>
    /// The longest time entry in minutes.
    /// </summary>
    public const int MaxMinutes = 1440;

    /// <summary>
    /// The longest free text in characters.
    /// </summary>
    public const int MaxText = 2000;

    private const string BreachRiskKey = "breach-risk";
    private const string KnowledgePrefix = "kb:";
    private const int KnowledgeKeep = 50;
    private const string TicketBase = "esm/tickets/";

    /// <inheritdoc/>
    public async Task<HelpdeskWorkbench> LoadWorkbenchAsync(int riskPercent, CancellationToken cancellationToken)
    {
        _ = await FlushAsync(cancellationToken).ConfigureAwait(false);

        var load = await ReadAsync(
            BreachRiskKey,
            token => api.GetBreachRiskAsync(riskPercent, token),
            HelpdeskParser.ParseBreachRisk,
            cancellationToken).ConfigureAwait(false);

        var pending = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        var conflicts = await store.ConflictsAsync(cancellationToken).ConfigureAwait(false);

        return new HelpdeskWorkbench(
            load is null ? [] : Sort(load.Data),
            load?.FromCache ?? false,
            load?.SyncedAtUtc,
            [.. pending.Select(ToPending)],
            [.. conflicts.Select(ToPending)]);
    }

    /// <inheritdoc/>
    public Task<HelpdeskLoad<HelpdeskMetrics>?> LoadMetricsAsync(DateOnly from, DateOnly to, string groupBy, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);

        var key = string.Join(':', "metrics", groupBy, from.ToString("O", CultureInfo.InvariantCulture), to.ToString("O", CultureInfo.InvariantCulture));

        return ReadAsync(key, token => api.GetMetricsAsync(from, to, groupBy, token), HelpdeskParser.ParseMetrics, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<HelpdeskLoad<HelpdeskCustomerContext>?> LoadCustomerContextAsync(Guid ticketId, CancellationToken cancellationToken) =>
        ReadAsync(ContextKey(ticketId), token => api.GetCustomerContextAsync(ticketId, token), HelpdeskParser.ParseCustomerContext, cancellationToken);

    /// <inheritdoc/>
    public Task<HelpdeskLoad<IReadOnlyList<HelpdeskEscalation>>?> LoadEscalationsAsync(Guid ticketId, CancellationToken cancellationToken) =>
        ReadAsync("escalations:" + ticketId.ToString("D", CultureInfo.InvariantCulture), token => api.GetEscalationsAsync(ticketId, token), HelpdeskParser.ParseEscalations, cancellationToken);

    /// <inheritdoc/>
    public async Task<HelpdeskLoad<IReadOnlyList<HelpdeskKbArticle>>?> SearchKnowledgeAsync(string text, string locale, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);

        var query = (text ?? string.Empty).Trim();
        var key = KnowledgePrefix + locale + ":" + query.ToUpperInvariant();
        var load = await ReadAsync(key, token => api.SearchKnowledgeAsync(query, locale, token), HelpdeskParser.ParseKnowledge, cancellationToken).ConfigureAwait(false);
        if (load is { FromCache: false })
        {
            await store.TrimCacheAsync(KnowledgePrefix, KnowledgeKeep, cancellationToken).ConfigureAwait(false);
        }

        return load;
    }

    /// <inheritdoc/>
    public Task<HelpdeskActionResult> LogTimeAsync(HelpdeskTimeLogRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = ValidateTicket(request.TicketId) ?? ValidateTime(request);
        if (invalid is not null)
        {
            return Task.FromResult(new HelpdeskActionResult(HelpdeskActionStatus.Rejected, invalid));
        }

        JsonObject body = new()
        {
            ["minutes"] = request.Minutes,
            ["is_billable"] = request.IsBillable,
        };
        Put(body, "technician_id", request.TechnicianId?.ToString("D", CultureInfo.InvariantCulture));
        Put(body, "description", request.Description);

        return QueueAsync(
            NewEntry(HelpdeskAction.LogTime, request.CommandId, request.TicketId, request.TicketNumber, body, request.OccurredAtUtc),
            HelpdeskTimeBilling.Estimate(request.Minutes, request.IsBillable, request.RemainingContractHours),
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<HelpdeskActionResult> EscalateToFieldServiceAsync(HelpdeskFieldServiceRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = ValidateTicket(request.TicketId) ?? ValidateVisit(request);
        if (invalid is not null)
        {
            return Task.FromResult(new HelpdeskActionResult(HelpdeskActionStatus.Rejected, invalid));
        }

        JsonObject body = [];
        Put(body, "work_type", request.WorkType);
        Put(body, "scheduled_start", Iso(request.ScheduledStart));
        Put(body, "scheduled_end", Iso(request.ScheduledEnd));
        Put(body, "location_address", request.LocationAddress);

        return QueueAsync(NewEntry(HelpdeskAction.EscalateFieldService, request.CommandId, request.TicketId, request.TicketNumber, body, request.OccurredAtUtc), fallback: null, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<HelpdeskActionResult> EscalateToRmaAsync(HelpdeskRmaRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = ValidateTicket(request.TicketId) ?? (TooLong(request.ReportedSymptom) ? TextKey : null);
        if (invalid is not null)
        {
            return Task.FromResult(new HelpdeskActionResult(HelpdeskActionStatus.Rejected, invalid));
        }

        JsonObject body = new() { ["goodwill"] = request.Goodwill };
        Put(body, "serial_number", request.SerialNumber);
        Put(body, "equipment_passport_id", request.EquipmentPassportId?.ToString("D", CultureInfo.InvariantCulture));
        Put(body, "reported_symptom", request.ReportedSymptom);

        return QueueAsync(NewEntry(HelpdeskAction.EscalateRma, request.CommandId, request.TicketId, request.TicketNumber, body, request.OccurredAtUtc), fallback: null, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<HelpdeskFlushResult> FlushAsync(CancellationToken cancellationToken)
    {
        var entries = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        if (entries.Count == 0)
        {
            return new HelpdeskFlushResult(0, 0, 0, Offline: false);
        }

        if (!api.IsAuthenticated)
        {
            return new HelpdeskFlushResult(0, 0, entries.Count, Offline: true);
        }

        var sent = 0;
        var conflicts = 0;
        var offline = false;

        foreach (var entry in entries)
        {
            if (entry.NextAttemptAt > Now())
            {
                offline = true;
                break;
            }

            var outcome = await SendAsync(entry, cancellationToken).ConfigureAwait(false);
            await store.SaveAsync(cancellationToken).ConfigureAwait(false);

            if (outcome == HelpdeskSendOutcome.Sent)
            {
                sent++;
            }
            else if (outcome == HelpdeskSendOutcome.Refused)
            {
                conflicts++;
            }
            else
            {
                // The network is most likely down: keep the order of the remaining entries.
                offline = true;
                break;
            }
        }

        var remaining = (await store.PendingAsync(cancellationToken).ConfigureAwait(false)).Count;

        return new HelpdeskFlushResult(sent, conflicts, remaining, offline);
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

        entry.Status = HelpdeskOutboxStatus.Dismissed;
        entry.UpdatedAt = Now();
        await store.SaveAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string ContextKey(Guid ticketId) => "context:" + ticketId.ToString("D", CultureInfo.InvariantCulture);

    private static IReadOnlyList<HelpdeskTicketRisk> Sort(IReadOnlyList<HelpdeskTicketRisk> tickets) =>
    [
        .. tickets
            .OrderByDescending(ticket => ticket.IsBreached)
            .ThenByDescending(ticket => ticket.RiskPercent)
            .ThenBy(ticket => ticket.EarliestDueAt ?? DateTime.MaxValue),
    ];

    private static HelpdeskPendingAction ToPending(HelpdeskOutboxEntry entry) =>
        new(entry.Id, entry.Action, entry.TicketId, entry.TicketNumber, DateTime.SpecifyKind(entry.OccurredAtUtc, DateTimeKind.Utc), entry.LastError);

    private static string? ValidateTicket(Guid ticketId) => ticketId == Guid.Empty ? TicketKey : null;

    private static bool TooLong(string? text) => text is { Length: > MaxText };

    private static string? ValidateTime(HelpdeskTimeLogRequest request)
    {
        return (request.Minutes is < 1 or > MaxMinutes, TooLong(request.Description)) switch
        {
            (true, _) => MinutesKey,
            (false, true) => TextKey,
            (false, false) => null,
        };
    }

    private static string? ValidateVisit(HelpdeskFieldServiceRequest request)
    {
        var endsBeforeStart = request.ScheduledStart is { } start && request.ScheduledEnd is { } end && end <= start;

        return (endsBeforeStart, TooLong(request.LocationAddress)) switch
        {
            (true, _) => ScheduleKey,
            (false, true) => TextKey,
            (false, false) => null,
        };
    }

    private static string? Iso(DateTime? value) =>
        value is { } instant ? DateTime.SpecifyKind(instant, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture) : null;

    private static void Put(JsonObject body, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            body[name] = value.Trim();
        }
    }

    private static string Route(HelpdeskOutboxEntry entry)
    {
        var ticket = TicketBase + entry.TicketId.ToString("D", CultureInfo.InvariantCulture);

        return entry.Action switch
        {
            HelpdeskAction.LogTime => ticket + "/time-entries",
            HelpdeskAction.EscalateFieldService => ticket + "/escalate/field-service",
            HelpdeskAction.EscalateRma => ticket + "/escalate/rma",
            _ => throw new ArgumentOutOfRangeException(nameof(entry)),
        };
    }

    private static string Body(HelpdeskOutboxEntry entry)
    {
        var body = JsonNode.Parse(entry.PayloadJson)?.AsObject() ?? [];
        body["idempotency_key"] = entry.IdempotencyKey.ToString("D", CultureInfo.InvariantCulture);

        return body.ToJsonString();
    }

    private static string ErrorText(string body, HttpStatusCode status)
    {
        try
        {
            if (JsonNode.Parse(body) is JsonObject root)
            {
                if (root["errors"] is JsonObject errors)
                {
                    var first = errors.Select(field => field.Value).OfType<JsonArray>().SelectMany(list => list).FirstOrDefault();
                    if (first is not null)
                    {
                        return first.ToString();
                    }
                }

                if (root["message"] is JsonValue message)
                {
                    return message.ToString();
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON: the status below describes it.
        }

        return "HTTP " + ((int)status).ToString(CultureInfo.InvariantCulture);
    }

    private static bool IsTransient(HttpStatusCode status) =>
        status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)status is 425 or >= 500;

    private DateTime Now() => time.GetUtcNow().UtcDateTime;

    private async Task<HelpdeskLoad<T>?> ReadAsync<T>(string key, Func<CancellationToken, Task<string>> fetch, Func<string, T> parse, CancellationToken cancellationToken)
    {
        if (api.IsAuthenticated)
        {
            try
            {
                var json = await fetch(cancellationToken).ConfigureAwait(false);
                var data = parse(json);
                var at = Now();
                await store.SaveCacheAsync(key, json, at, cancellationToken).ConfigureAwait(false);

                return new HelpdeskLoad<T>(data, FromCache: false, at);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                // Offline or a bad answer: the cache below. A bad answer never replaces a good cache.
            }
        }

        var cached = await store.FindCacheAsync(key, cancellationToken).ConfigureAwait(false);

        return cached is null
            ? null
            : new HelpdeskLoad<T>(parse(cached.Json), FromCache: true, DateTime.SpecifyKind(cached.SyncedAtUtc, DateTimeKind.Utc));
    }

    private HelpdeskOutboxEntry NewEntry(HelpdeskAction action, Guid commandId, Guid ticketId, string ticketNumber, JsonObject body, DateTime occurredAtUtc) => new()
    {
        Id = commandId,
        TenantId = tenant.TenantId,
        Action = action,
        TicketId = ticketId,
        TicketNumber = ticketNumber ?? string.Empty,
        PayloadJson = body.ToJsonString(),
        IdempotencyKey = commandId,
        OccurredAtUtc = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc),
        NextAttemptAt = Now(),
    };

    private async Task<HelpdeskActionResult> QueueAsync(HelpdeskOutboxEntry entry, HelpdeskTimeOutcome? fallback, CancellationToken cancellationToken)
    {
        var action = entry.Action;
        await store.EnqueueAsync(entry, cancellationToken).ConfigureAwait(false);

        _ = await FlushAsync(cancellationToken).ConfigureAwait(false);

        return entry.Status switch
        {
            HelpdeskOutboxStatus.Sent => new HelpdeskActionResult(
                HelpdeskActionStatus.Sent,
                TimeOutcome: action == HelpdeskAction.LogTime ? HelpdeskParser.ParseTimeEntry(entry.ResultJson ?? string.Empty) ?? fallback : null,
                TrackingToken: action == HelpdeskAction.EscalateRma ? HelpdeskParser.ParseTrackingToken(entry.ResultJson ?? string.Empty) : null),
            HelpdeskOutboxStatus.Conflict => new HelpdeskActionResult(HelpdeskActionStatus.Conflict, Detail: entry.LastError),
            HelpdeskOutboxStatus.Pending or HelpdeskOutboxStatus.Dismissed => new HelpdeskActionResult(HelpdeskActionStatus.Queued, TimeOutcome: fallback),
            _ => new HelpdeskActionResult(HelpdeskActionStatus.Queued, TimeOutcome: fallback),
        };
    }

    private async Task<HelpdeskSendOutcome> SendAsync(HelpdeskOutboxEntry entry, CancellationToken cancellationToken)
    {
        HelpdeskApiReply reply;
        try
        {
            reply = await api.SendAsync(Route(entry), Body(entry), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return Retry(entry, ex.Message);
        }

        if (reply.IsSuccess)
        {
            entry.Status = HelpdeskOutboxStatus.Sent;
            entry.ResultJson = reply.Body;
            entry.LastError = null;
            entry.UpdatedAt = Now();

            return HelpdeskSendOutcome.Sent;
        }

        if (reply.Status == HttpStatusCode.Unauthorized || IsTransient(reply.Status))
        {
            return Retry(entry, ErrorText(reply.Body, reply.Status));
        }

        entry.Status = HelpdeskOutboxStatus.Conflict;
        entry.LastError = ErrorText(reply.Body, reply.Status);
        entry.UpdatedAt = Now();

        return HelpdeskSendOutcome.Refused;
    }

    private HelpdeskSendOutcome Retry(HelpdeskOutboxEntry entry, string error)
    {
        entry.Attempts++;
        entry.LastError = error;
        entry.UpdatedAt = Now();

        if (!SyncRetryPolicy.CanRetry(entry.Attempts))
        {
            entry.Status = HelpdeskOutboxStatus.Conflict;

            return HelpdeskSendOutcome.Refused;
        }

        entry.NextAttemptAt = Now() + SyncRetryPolicy.NextDelay(entry.Attempts);

        return HelpdeskSendOutcome.Retry;
    }
}
