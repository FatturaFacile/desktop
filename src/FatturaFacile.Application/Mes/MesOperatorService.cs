// <copyright file="MesOperatorService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// Offline-first orchestrator of the MES operator board. Every operator command is validated against the phase state machine and
/// saved in the local outbox before anything is sent, so a disconnected tablet or desktop loses nothing; the outbox is sent in
/// the order the operator acted, each command with its own idempotency key and the instant the operator acted, so a retry after
/// a lost answer never acts twice and the server measures the real time. A command the server refuses is shown as a conflict,
/// and the commands that depend on it are refused with it.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="store">The local cache and outbox.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="time">The clock.</param>
public sealed partial class MesOperatorService(IMesApiClient api, IMesStore store, ICurrentTenantService tenant, TimeProvider time) : IMesOperatorService
{
    /// <summary>
    /// The localization key of a command the state machine does not allow.
    /// </summary>
    public const string NotAllowedKey = "Mes_Err_NotAllowed";

    /// <summary>
    /// The localization key of scrapped pieces without a defect reason.
    /// </summary>
    public const string DefectReasonKey = "Mes_Err_DefectReason";

    /// <summary>
    /// The localization key of an empty or negative quantity.
    /// </summary>
    public const string QuantityKey = "Mes_Err_Quantity";

    /// <summary>
    /// The localization key of a malformed lot number.
    /// </summary>
    public const string LotCodeKey = "Mes_Err_LotCode";

    /// <summary>
    /// The localization key of a command refused because an earlier command of the same phase was refused.
    /// </summary>
    public const string BlockedKey = "Mes_Err_Blocked";

    private const string ApiBase = "manufacturing/mes/phases/";

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._\\-/]{0,63}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 500)]
    private static partial Regex LotPattern { get; }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MesWorkCenter>> GetWorkCentersAsync(CancellationToken cancellationToken)
    {
        if (api.IsAuthenticated)
        {
            try
            {
                var json = await api.GetWorkCentersAsync(cancellationToken).ConfigureAwait(false);
                var centers = MesBoardParser.ParseWorkCenters(json);
                await store.SaveBoardAsync(LocalMesBoard.WorkCentreList, json, Now(), cancellationToken).ConfigureAwait(false);
                return centers;
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                // Offline or a bad answer: the cached list below.
            }
        }

        var cached = await store.FindBoardAsync(LocalMesBoard.WorkCentreList, cancellationToken).ConfigureAwait(false);

        return cached is null ? [] : MesBoardParser.ParseWorkCenters(cached.BoardJson);
    }

    /// <inheritdoc/>
    public async Task<MesBoardView?> LoadBoardAsync(Guid workCenterId, CancellationToken cancellationToken)
    {
        _ = await FlushAsync(cancellationToken).ConfigureAwait(false);

        MesBoardSnapshot? snapshot = null;
        DateTime? syncedAt = null;
        var fromCache = false;

        if (api.IsAuthenticated)
        {
            try
            {
                var json = await api.GetBoardAsync(workCenterId, cancellationToken).ConfigureAwait(false);
                snapshot = MesBoardParser.ParseBoard(json);
                syncedAt = Now();
                await store.SaveBoardAsync(workCenterId, json, syncedAt.Value, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                // Offline or a bad answer: a bad answer never replaces a good cache.
            }
        }

        if (snapshot is null)
        {
            var cached = await store.FindBoardAsync(workCenterId, cancellationToken).ConfigureAwait(false);
            if (cached is null)
            {
                return null;
            }

            snapshot = MesBoardParser.ParseBoard(cached.BoardJson);
            syncedAt = cached.SyncedAtUtc;
            fromCache = true;
        }

        var pending = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        var rows = MesLocalProjection.Project(snapshot.Rows, snapshot.ServerTimeUtc, pending.Select(ToCommand), Now(), advanceUntouched: fromCache);
        var conflicts = await store.ConflictsAsync(cancellationToken).ConfigureAwait(false);

        return new MesBoardView(
            snapshot.WorkCenterId,
            snapshot.WorkCenterName,
            rows,
            fromCache,
            syncedAt,
            pending.Count,
            [.. conflicts.Select(entry => new MesConflict(entry.Id, entry.Action, entry.OccurredAtUtc, entry.LastError ?? string.Empty))]);
    }

    /// <inheritdoc/>
    public Task<MesExecuteResult> ExecuteAsync(MesCommand command, MesPhaseRow row, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(row);

        return ExecuteCoreAsync(command, row, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<MesFlushResult> FlushAsync(CancellationToken cancellationToken) => FlushCoreAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task DismissConflictAsync(Guid commandId, CancellationToken cancellationToken)
    {
        var conflicts = await store.ConflictsAsync(cancellationToken).ConfigureAwait(false);
        var entry = conflicts.FirstOrDefault(candidate => candidate.Id == commandId);
        if (entry is null)
        {
            return;
        }

        entry.Status = MesOutboxStatus.Dismissed;
        entry.UpdatedAt = Now();
        await store.SaveAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string? Validate(MesCommand command, MesPhaseRow row) =>
        MesStateMachine.IsAllowed(command.Action, row.Status) ? ValidateFields(command, row) : NotAllowedKey;

    private static string? ValidateFields(MesCommand command, MesPhaseRow row) => command.Action switch
    {
        MesAction.DeclareProduction => ValidateDeclaration(command),
        MesAction.ConsumeMaterial => ValidateConsumption(command, row),
        MesAction.TakeOver or MesAction.StartSetup or MesAction.StartProduction => null,
        MesAction.Suspend or MesAction.Resume or MesAction.Close => null,
        _ => null,
    };

    private static string? ValidateDeclaration(MesCommand command)
    {
        var good = command.Amount(MesCommand.GoodField);
        var scrap = command.Amount(MesCommand.ScrapField);
        var emptyOrNegative = Math.Min(good, scrap) < 0 || good + scrap <= 0;
        var scrapWithoutReason = scrap > 0 && string.IsNullOrEmpty(command.Text(MesCommand.DefectField));

        return (emptyOrNegative, scrapWithoutReason) switch
        {
            (true, _) => QuantityKey,
            (false, true) => DefectReasonKey,
            (false, false) => null,
        };
    }

    private static string? ValidateConsumption(MesCommand command, MesPhaseRow row)
    {
        var lot = command.Text(MesCommand.LotField);
        var item = command.Text(MesCommand.ItemField);
        var tracked = row.Components.Any(component => component.LotTracked && string.Equals(component.ItemId.ToString("D", CultureInfo.InvariantCulture), item, StringComparison.Ordinal));
        var missingLot = tracked && string.IsNullOrWhiteSpace(lot);
        var malformedLot = lot is not null && !LotPattern.IsMatch(lot);

        return (command.Amount(MesCommand.QuantityField) <= 0, missingLot || malformedLot) switch
        {
            (true, _) => QuantityKey,
            (false, true) => LotCodeKey,
            (false, false) => null,
        };
    }

    private static MesCommand ToCommand(MesOutboxEntry entry)
    {
        var payload = Fields(entry).ToDictionary(field => field.Key, field => Scalar(field.Value), StringComparer.Ordinal);

        return new MesCommand(entry.Id, entry.Action, entry.PhaseId, entry.OperationId, DateTime.SpecifyKind(entry.OccurredAtUtc, DateTimeKind.Utc), payload);
    }

    private static Dictionary<string, JsonElement> Fields(MesOutboxEntry entry) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(entry.PayloadJson) ?? throw new JsonException("The outbox payload is empty.");

    private static object? Scalar(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Undefined or JsonValueKind.Null => null,
        JsonValueKind.Object or JsonValueKind.Array => null,
        JsonValueKind.Number => null,
        _ => null,
    };

    private static string Route(MesOutboxEntry entry) => entry.Action switch
    {
        MesAction.TakeOver => ApiBase + "take-over",
        MesAction.StartSetup => ApiBase + entry.PhaseId + "/start-setup",
        MesAction.StartProduction => ApiBase + entry.PhaseId + "/start-production",
        MesAction.Suspend => ApiBase + entry.PhaseId + "/suspend",
        MesAction.Resume => ApiBase + entry.PhaseId + "/resume",
        MesAction.DeclareProduction => ApiBase + entry.PhaseId + "/declarations",
        MesAction.ConsumeMaterial => ApiBase + entry.PhaseId + "/material-consumptions",
        MesAction.Close => ApiBase + entry.PhaseId + "/close",
        _ => throw new ArgumentOutOfRangeException(nameof(entry)),
    };

    private static string Body(MesOutboxEntry entry)
    {
        var body = Fields(entry)
            .Where(field => field.Value.ValueKind != JsonValueKind.Null)
            .ToDictionary(field => field.Key, field => (object?)field.Value, StringComparer.Ordinal);

        if (entry.Action == MesAction.TakeOver)
        {
            body["production_order_operation_id"] = entry.OperationId.ToString("D", CultureInfo.InvariantCulture);
        }

        body["idempotency_key"] = entry.IdempotencyKey.ToString("D", CultureInfo.InvariantCulture);
        body["occurred_at"] = DateTime.SpecifyKind(entry.OccurredAtUtc, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture);

        return JsonSerializer.Serialize(body);
    }

    private static string ErrorText(string body, HttpStatusCode status)
    {
        try
        {
            var found = FirstError(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body));
            if (found is not null)
            {
                return found;
            }
        }
        catch (JsonException)
        {
            // Not JSON: the status below describes it.
        }

        return "HTTP " + ((int)status).ToString(CultureInfo.InvariantCulture);
    }

    private static string? FirstError(Dictionary<string, JsonElement>? root)
    {
        if (root is null)
        {
            return null;
        }

        if (root.TryGetValue("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
        {
            var fields = errors.Deserialize<Dictionary<string, string[]>>() ?? throw new JsonException("The error list is empty.");
            var first = fields.FirstOrDefault(field => field.Value.Length > 0);
            if (first.Value is { Length: > 0 })
            {
                return first.Value[0];
            }
        }

        return root.TryGetValue("message", out var message) && message.ValueKind == JsonValueKind.String ? message.GetString() : null;
    }

    private static bool IsTransient(HttpStatusCode status) =>
        status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)status is 425 or >= 500;

    private DateTime Now() => time.GetUtcNow().UtcDateTime;

    private async Task<MesExecuteResult> ExecuteCoreAsync(MesCommand command, MesPhaseRow row, CancellationToken cancellationToken)
    {
        var invalid = Validate(command, row);
        if (invalid is not null)
        {
            return new MesExecuteResult(MesExecuteStatus.Rejected, invalid);
        }

        MesOutboxEntry entry = new()
        {
            Id = command.Id,
            TenantId = tenant.TenantId,
            Action = command.Action,
            OperationId = command.OperationId,
            PhaseId = command.PhaseId,
            PayloadJson = JsonSerializer.Serialize(command.Payload),
            IdempotencyKey = command.Id,
            OccurredAtUtc = DateTime.SpecifyKind(command.OccurredAtUtc, DateTimeKind.Utc),
            NextAttemptAt = Now(),
        };
        await store.EnqueueAsync(entry, cancellationToken).ConfigureAwait(false);

        _ = await FlushCoreAsync(cancellationToken).ConfigureAwait(false);

        return entry.Status switch
        {
            MesOutboxStatus.Sent => new MesExecuteResult(MesExecuteStatus.Sent),
            MesOutboxStatus.Conflict => new MesExecuteResult(MesExecuteStatus.Conflict, Detail: entry.LastError),
            MesOutboxStatus.Pending or MesOutboxStatus.Dismissed => new MesExecuteResult(MesExecuteStatus.Queued),
            _ => new MesExecuteResult(MesExecuteStatus.Queued),
        };
    }

    private async Task<MesFlushResult> FlushCoreAsync(CancellationToken cancellationToken)
    {
        var entries = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        if (entries.Count == 0)
        {
            return new MesFlushResult(0, 0, 0, Offline: false);
        }

        if (!api.IsAuthenticated)
        {
            return new MesFlushResult(0, 0, entries.Count, Offline: true);
        }

        var sent = 0;
        var conflicts = 0;
        var offline = false;
        HashSet<Guid> refused = [];

        foreach (var entry in entries)
        {
            if (entry.NextAttemptAt > Now())
            {
                offline = true;
                break;
            }

            if (refused.Contains(entry.OperationId))
            {
                Refuse(entry, BlockedKey);
                conflicts++;
                await store.SaveAsync(cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (entry.Action != MesAction.TakeOver && entry.PhaseId is null)
            {
                // Waits for the take-over of the same operation, which gives it its phase.
                continue;
            }

            var outcome = await SendAsync(entry, cancellationToken).ConfigureAwait(false);
            await store.SaveAsync(cancellationToken).ConfigureAwait(false);

            if (outcome == MesSendOutcome.Sent)
            {
                sent++;
            }
            else if (outcome == MesSendOutcome.Refused)
            {
                conflicts++;
                _ = refused.Add(entry.OperationId);
            }
            else
            {
                offline = true;
                break;
            }
        }

        var remaining = (await store.PendingAsync(cancellationToken).ConfigureAwait(false)).Count;

        return new MesFlushResult(sent, conflicts, remaining, offline);
    }

    private async Task<MesSendOutcome> SendAsync(MesOutboxEntry entry, CancellationToken cancellationToken)
    {
        MesApiReply reply;
        try
        {
            reply = await api.SendCommandAsync(Route(entry), Body(entry), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return Retry(entry, ex.Message);
        }

        if (reply.IsSuccess)
        {
            entry.Status = MesOutboxStatus.Sent;
            entry.UpdatedAt = Now();
            entry.LastError = null;
            if (entry.Action == MesAction.TakeOver)
            {
                await AssignPhaseAsync(entry, reply.Body, cancellationToken).ConfigureAwait(false);
            }

            return MesSendOutcome.Sent;
        }

        if (reply.Status == HttpStatusCode.Unauthorized || IsTransient(reply.Status))
        {
            return Retry(entry, ErrorText(reply.Body, reply.Status));
        }

        Refuse(entry, ErrorText(reply.Body, reply.Status));

        return MesSendOutcome.Refused;
    }

    private async Task AssignPhaseAsync(MesOutboxEntry entry, string body, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var id = document.RootElement.GetProperty("data").GetProperty("id").GetString();
            if (Guid.TryParse(id, out var phaseId))
            {
                await store.AssignPhaseAsync(entry.OperationId, phaseId, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            // The answer has no phase: later commands of the operation stay pending and are refused by the server.
        }
    }

    private MesSendOutcome Retry(MesOutboxEntry entry, string error)
    {
        entry.Attempts++;
        entry.LastError = error;
        entry.UpdatedAt = Now();

        if (!SyncRetryPolicy.CanRetry(entry.Attempts))
        {
            entry.Status = MesOutboxStatus.Conflict;
            return MesSendOutcome.Refused;
        }

        entry.NextAttemptAt = Now() + SyncRetryPolicy.NextDelay(entry.Attempts);

        return MesSendOutcome.Retry;
    }

    private void Refuse(MesOutboxEntry entry, string error)
    {
        entry.Status = MesOutboxStatus.Conflict;
        entry.LastError = error;
        entry.UpdatedAt = Now();
    }
}
