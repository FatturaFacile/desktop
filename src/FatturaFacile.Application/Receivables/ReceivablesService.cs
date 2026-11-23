// <copyright file="ReceivablesService.cs" company="Fattura Facile">
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
using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Offline-first receivables (Mod 013): reads go to the server and fall back to the last cached answer; writes are saved in the
/// outbox first and replayed in order when the server is reachable. Every write carries its command id as idempotency key, the
/// outbox accepts a command id only once, so a command can never be queued twice, and a replayed entry whose first answer got lost
/// is refused by the server as a conflict instead of being applied again.
/// </summary>
/// <param name="api">The receivables endpoints.</param>
/// <param name="store">The local cache and outbox.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="time">The clock.</param>
public sealed class ReceivablesService(IReceivablesApiClient api, IReceivablesStore store, ICurrentTenantService tenant, TimeProvider time) : IReceivablesService
{
    /// <summary>
    /// The localization key of a missing customer.
    /// </summary>
    public const string CustomerKey = "Receivables_Err_Customer";

    /// <summary>
    /// The localization key of a request without items or allocations to act on.
    /// </summary>
    public const string LinesKey = "Receivables_Err_Lines";

    /// <summary>
    /// The localization key of a missing or too short reason.
    /// </summary>
    public const string ReasonKey = "Receivables_Err_Reason";

    /// <summary>
    /// The localization key of an invalid credit limit.
    /// </summary>
    public const string LimitKey = "Receivables_Err_Limit";

    /// <summary>
    /// The localization key of an insured limit without insurer or policy.
    /// </summary>
    public const string InsurerKey = "Receivables_Err_Insurer";

    /// <summary>
    /// The localization key of a period that ends before it starts, or an expiry in the past.
    /// </summary>
    public const string DatesKey = "Receivables_Err_Dates";

    /// <summary>
    /// The localization key of a provision rate outside 0 to 100.
    /// </summary>
    public const string RateKey = "Receivables_Err_Rate";

    /// <summary>
    /// The localization key of a credit note without its source document.
    /// </summary>
    public const string SourceKey = "Receivables_Err_Source";

    /// <summary>
    /// The localization key of a text that is too long or a language that is not supported.
    /// </summary>
    public const string TextKey = "Receivables_Err_Text";

    /// <summary>
    /// The localization key of an invalid recipient.
    /// </summary>
    public const string RecipientKey = "Receivables_Err_Recipient";

    /// <summary>
    /// The shortest reason of an override in characters.
    /// </summary>
    public const int MinReason = 5;

    /// <summary>
    /// The longest free text in characters.
    /// </summary>
    public const int MaxText = 2000;

    private const int MaxReference = 200;
    private const int ItemsKeep = 40;
    private const string Post = "POST";
    private const string Put = "PUT";
    private const string Delete = "DELETE";
    private static readonly string[] s_locales = ["it", "de", "fr", "en"];

    private enum SendOutcome
    {
        Sent = 0,
        Retry = 1,
        Refused = 2,
    }

    /// <inheritdoc/>
    public Task<ReceivablesLoad<ReceivablesDashboard>?> LoadDashboardAsync(string? currency, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.Dashboard(currency), ReceivablesParser.ParseDashboard, cancellationToken);

    /// <inheritdoc/>
    public Task<ReceivablesLoad<AgingReport>?> LoadAgingAsync(Guid? customerId, string? currency, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.Aging(customerId, currency), ReceivablesParser.ParseAging, cancellationToken);

    /// <inheritdoc/>
    public async Task<ReceivablesLoad<ReceivableItemPage>?> LoadItemsAsync(ReceivableItemFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var load = await ReadAsync(ReceivablesRoutes.Items(filter), ReceivablesParser.ParseItemPage, cancellationToken).ConfigureAwait(false);
        if (load is { FromCache: false })
        {
            await store.TrimCacheAsync(ReceivablesRoutes.ItemsPrefix, ItemsKeep, cancellationToken).ConfigureAwait(false);
        }

        return load;
    }

    /// <inheritdoc/>
    public Task<ReceivablesLoad<ReceivableItem>?> LoadItemAsync(Guid id, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.Item(id), ReceivablesParser.ParseItem, cancellationToken);

    /// <inheritdoc/>
    public Task<ReceivablesLoad<CustomerLedger>?> LoadLedgerAsync(Guid customerId, DateOnly? from, DateOnly? to, string? currency, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.Ledger(customerId, from, to, currency), ReceivablesParser.ParseLedger, cancellationToken);

    /// <inheritdoc/>
    public Task<ReceivablesLoad<IReadOnlyList<UnreconciledPayment>>?> LoadUnreconciledPaymentsAsync(Guid customerId, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.UnreconciledPayments(customerId), ReceivablesParser.ParseUnreconciledPayments, cancellationToken);

    /// <inheritdoc/>
    public Task<ReceivablesLoad<CustomerCreditStatus>?> LoadCreditStatusAsync(Guid customerId, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.Credit(customerId), ReceivablesParser.ParseCredit, cancellationToken);

    /// <inheritdoc/>
    public Task<ReceivablesLoad<CreditRiskScore>?> LoadRiskAsync(Guid customerId, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.Risk(customerId), ReceivablesParser.ParseRisk, cancellationToken);

    /// <inheritdoc/>
    public Task<ReceivablesLoad<IReadOnlyList<CustomerCreditLimit>>?> LoadCreditLimitsAsync(Guid customerId, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.CreditLimits(customerId), ReceivablesParser.ParseLimits, cancellationToken);

    /// <inheritdoc/>
    public async Task<ReceivablesLoad<CreditDecision>?> CheckCreditAsync(Guid customerId, CreditScope scope, decimal amount, CancellationToken cancellationToken)
    {
        if (api.IsAuthenticated)
        {
            try
            {
                var json = await api.GetAsync(ReceivablesRoutes.CreditCheck(customerId, scope, amount), cancellationToken).ConfigureAwait(false);

                return new ReceivablesLoad<CreditDecision>(ReceivablesParser.ParseDecision(json), FromCache: false, Now());
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                // Offline or a bad answer: decided locally below.
            }
        }

        var status = await LoadCreditStatusAsync(customerId, cancellationToken).ConfigureAwait(false);
        if (status is null)
        {
            return null;
        }

        var position = status.Data;
        var decision = CreditDecisionPolicy.Evaluate(position.Exposure, position.Limit, position.IsCreditHold, scope, amount, position.Overrides, Now());

        return new ReceivablesLoad<CreditDecision>(decision, FromCache: true, status.SyncedAtUtc);
    }

    /// <inheritdoc/>
    public Task<ReceivablesLoad<DsoResult>?> LoadDsoAsync(Guid? customerId, int periodDays, string? currency, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.Dso(customerId, Math.Clamp(periodDays, 1, DsoCalculator.MaxPeriodDays), currency), ReceivablesParser.ParseDso, cancellationToken);

    /// <inheritdoc/>
    public Task<ReceivablesLoad<ProvisionSummary>?> LoadProvisionAsync(Guid? customerId, string? currency, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.Provision(customerId, currency), ReceivablesParser.ParseProvision, cancellationToken);

    /// <inheritdoc/>
    public Task<ReceivablesLoad<CustomerStatement>?> LoadStatementAsync(Guid customerId, DateOnly from, DateOnly to, string? currency, CancellationToken cancellationToken) =>
        ReadAsync(ReceivablesRoutes.Statement(customerId, from, to, currency), ReceivablesParser.ParseStatement, cancellationToken);

    /// <inheritdoc/>
    public async Task<ReceivablesFile?> ExportStatementAsync(Guid customerId, string format, DateOnly from, DateOnly to, string? currency, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        if (!string.Equals(format, "pdf", StringComparison.Ordinal) && !string.Equals(format, "xlsx", StringComparison.Ordinal))
        {
            throw new ArgumentOutOfRangeException(nameof(format));
        }

        if (!api.IsAuthenticated)
        {
            return null;
        }

        var name = "statement-" + customerId.ToString("N", CultureInfo.InvariantCulture)[..8] + "-" + from.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "-" + to.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "." + format;

        try
        {
            return await api.DownloadAsync(ReceivablesRoutes.StatementFile(customerId, format, from, to, currency), name, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public Task<ReceivablesActionResult> ApplyAllocationsAsync(ApplyAllocationsRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = ValidateAllocations(request);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        JsonArray lines = [];
        foreach (var line in request.Lines)
        {
            JsonObject row = new()
            {
                ["receivable_item_id"] = Id(line.ReceivableItemId),
                ["type"] = line.Type.ToWire(),
                ["amount"] = Amount(line.Amount),
                ["value_date"] = Day(line.ValueDate),
            };
            PutText(row, "source_id", line.SourceId);
            PutText(row, "reference", line.Reference);
            PutText(row, "notes", line.Notes);
            lines.Add(row);
        }

        JsonObject body = new() { ["allocations"] = lines };

        return QueueAsync(NewEntry(ReceivablesAction.ApplyAllocations, Post, ReceivablesRoutes.Allocations, request.CommandId, request.CustomerId, request.Reference, body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ReceivablesActionResult> ReverseAllocationAsync(ReverseAllocationRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = ValidateCustomer(request.CustomerId) ?? (request.AllocationId == Guid.Empty ? LinesKey : null);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        return QueueAsync(NewEntry(ReceivablesAction.ReverseAllocation, Post, ReceivablesRoutes.ReverseAllocation(request.AllocationId), request.CommandId, request.CustomerId, request.Reference, [], request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ReceivablesActionResult> SetDisputeAsync(DisputeRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = ValidateCustomer(request.CustomerId) ?? ValidateDispute(request);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        JsonObject body = new() { ["disputed"] = request.Disputed };
        if (request.Disputed)
        {
            PutText(body, "reason", request.DisputeReason);
        }

        return QueueAsync(NewEntry(ReceivablesAction.SetDispute, Post, ReceivablesRoutes.ItemDispute(request.ItemId), request.CommandId, request.CustomerId, request.Reference, body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ReceivablesActionResult> SaveCreditLimitAsync(CreditLimitRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = ValidateCustomer(request.CustomerId) ?? ValidateLimit(request);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        var update = request.LimitId is { } limitId && limitId != Guid.Empty;
        JsonObject body = new()
        {
            ["currency"] = request.Currency.Trim().ToUpperInvariant(),
            ["granted_limit"] = Amount(request.GrantedLimit),
            ["reaction_policy"] = request.ReactionPolicy.ToWire(),
            ["valid_from"] = Day(request.ValidFrom),
            ["is_active"] = request.IsActive,
        };
        if (!update)
        {
            body["customer_id"] = Id(request.CustomerId);
        }

        if (request.InsuredLimit is { } insured)
        {
            body["insured_limit"] = Amount(insured);
        }

        if (request.CoveragePercentage is { } coverage)
        {
            body["coverage_percentage"] = Amount(coverage);
        }

        if (request.MaxOverdueDays is { } days)
        {
            body["max_overdue_days"] = days;
        }

        if (request.OverdueToleranceAmount is { } tolerance)
        {
            body["overdue_tolerance_amount"] = Amount(tolerance);
        }

        if (request.ValidTo is { } validTo)
        {
            body["valid_to"] = Day(validTo);
        }

        PutText(body, "insurer_name", request.InsurerName);
        PutText(body, "policy_number", request.PolicyNumber);
        PutText(body, "notes", request.Notes);

        var entry = update
            ? NewEntry(ReceivablesAction.SaveCreditLimit, Put, ReceivablesRoutes.CreditLimit(request.LimitId!.Value), request.CommandId, request.CustomerId, request.Reference, body, request.OccurredAtUtc)
            : NewEntry(ReceivablesAction.SaveCreditLimit, Post, ReceivablesRoutes.CreditLimitsRoot, request.CommandId, request.CustomerId, request.Reference, body, request.OccurredAtUtc);

        return QueueAsync(entry, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ReceivablesActionResult> DeleteCreditLimitAsync(DeleteCreditLimitRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = ValidateCustomer(request.CustomerId) ?? (request.LimitId == Guid.Empty ? LinesKey : null);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        return QueueAsync(NewEntry(ReceivablesAction.DeleteCreditLimit, Delete, ReceivablesRoutes.CreditLimit(request.LimitId), request.CommandId, request.CustomerId, request.Reference, [], request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ReceivablesActionResult> GrantOverrideAsync(GrantOverrideRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reason = (request.Reason ?? string.Empty).Trim();
        var invalid = ValidateCustomer(request.CustomerId)
            ?? (reason.Length < MinReason || reason.Length > MaxText ? ReasonKey : null)
            ?? (request.ExpiresAt is { } expires && expires <= Now() ? DatesKey : null);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        JsonObject body = new() { ["scope"] = request.Scope.ToWire(), ["reason"] = reason };
        if (request.ExpiresAt is { } expiresAt)
        {
            body["expires_at"] = DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture);
        }

        return QueueAsync(NewEntry(ReceivablesAction.GrantOverride, Post, ReceivablesRoutes.Overrides(request.CustomerId), request.CommandId, request.CustomerId, request.Reference, body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ReceivablesActionResult> RevokeOverrideAsync(RevokeOverrideRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invalid = ValidateCustomer(request.CustomerId) ?? (request.OverrideId == Guid.Empty ? LinesKey : null) ?? (TooLong(request.RevokeReason) ? TextKey : null);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        JsonObject body = [];
        PutText(body, "reason", request.RevokeReason);

        return QueueAsync(NewEntry(ReceivablesAction.RevokeOverride, Post, ReceivablesRoutes.RevokeOverride(request.OverrideId), request.CommandId, request.CustomerId, request.Reference, body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ReceivablesActionResult> UpdateProvisionRatesAsync(ProvisionRatesRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Rates.Count == 0 || request.Rates.Values.Any(rate => !ProvisionRates.IsValid(rate)))
        {
            return Rejected(RateKey);
        }

        JsonObject rates = [];
        foreach (var (key, rate) in request.Rates)
        {
            rates[key] = rate;
        }

        JsonObject body = new() { ["rates"] = rates };

        return QueueAsync(NewEntry(ReceivablesAction.UpdateProvisionRates, Put, ReceivablesRoutes.ProvisionRatesRoute, request.CommandId, Guid.Empty, "provision", body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ReceivablesActionResult> SendStatementAsync(SendStatementRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var recipient = request.Recipient?.Trim();
        var invalid = ValidateCustomer(request.CustomerId)
            ?? (request.From > request.To ? DatesKey : null)
            ?? (s_locales.Contains(request.Locale, StringComparer.Ordinal) ? null : TextKey)
            ?? (string.IsNullOrEmpty(recipient) || recipient.Contains('@', StringComparison.Ordinal) ? null : RecipientKey);
        if (invalid is not null)
        {
            return Rejected(invalid);
        }

        JsonObject body = new() { ["from"] = Day(request.From), ["to"] = Day(request.To), ["locale"] = request.Locale };
        PutText(body, "recipient", recipient);

        return QueueAsync(NewEntry(ReceivablesAction.SendStatement, Post, ReceivablesRoutes.StatementSend(request.CustomerId), request.CommandId, request.CustomerId, request.Reference, body, request.OccurredAtUtc), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ReceivablesFlushResult> FlushAsync(CancellationToken cancellationToken)
    {
        var entries = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        if (entries.Count == 0)
        {
            return new ReceivablesFlushResult(0, 0, 0, Offline: false);
        }

        if (!api.IsAuthenticated)
        {
            return new ReceivablesFlushResult(0, 0, entries.Count, Offline: true);
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

            if (outcome == SendOutcome.Sent)
            {
                sent++;
            }
            else if (outcome == SendOutcome.Refused)
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

        return new ReceivablesFlushResult(sent, conflicts, remaining, offline);
    }

    /// <inheritdoc/>
    public async Task<ReceivablesQueue> LoadQueueAsync(CancellationToken cancellationToken)
    {
        var pending = await store.PendingAsync(cancellationToken).ConfigureAwait(false);
        var conflicts = await store.ConflictsAsync(cancellationToken).ConfigureAwait(false);

        return new ReceivablesQueue([.. pending.Select(ToPending)], [.. conflicts.Select(ToPending)]);
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

        entry.Status = ReceivablesOutboxStatus.Dismissed;
        entry.UpdatedAt = Now();
        await store.SaveAsync(cancellationToken).ConfigureAwait(false);
    }

    private static Task<ReceivablesActionResult> Rejected(string key) =>
        Task.FromResult(new ReceivablesActionResult(ReceivablesActionStatus.Rejected, key));

    private static string? ValidateCustomer(Guid customerId) => customerId == Guid.Empty ? CustomerKey : null;

    private static bool TooLong(string? text) => text is { Length: > MaxText };

    private static string? ValidateAllocations(ApplyAllocationsRequest request)
    {
        if (ValidateCustomer(request.CustomerId) is { } missing)
        {
            return missing;
        }

        if (request.Lines.Count == 0 || request.Lines.Any(line => line.ReceivableItemId == Guid.Empty))
        {
            return LinesKey;
        }

        if (request.Lines.Any(line => line.Amount <= 0m))
        {
            return ReconciliationRules.AmountKey;
        }

        if (request.Lines.Any(line => line.Type == AllocationType.CreditNote && string.IsNullOrWhiteSpace(line.SourceId)))
        {
            return SourceKey;
        }

        return request.Lines.Any(line => TooLong(line.Notes) || TooLong(line.Reference)) ? TextKey : null;
    }

    private static string? ValidateDispute(DisputeRequest request)
    {
        if (request.ItemId == Guid.Empty)
        {
            return LinesKey;
        }

        if (request.Disputed && string.IsNullOrWhiteSpace(request.DisputeReason))
        {
            return ReasonKey;
        }

        return TooLong(request.DisputeReason) ? TextKey : null;
    }

    private static string? ValidateLimit(CreditLimitRequest request)
    {
        if (request.Currency is not { Length: 3 } || request.GrantedLimit < 0m)
        {
            return LimitKey;
        }

        if (request.InsuredLimit is < 0m || request.CoveragePercentage is < 0m or > 100m || request.MaxOverdueDays is < 0 || request.OverdueToleranceAmount is < 0m)
        {
            return LimitKey;
        }

        if (request.InsuredLimit is > 0m && string.IsNullOrWhiteSpace(request.InsurerName) && string.IsNullOrWhiteSpace(request.PolicyNumber))
        {
            return InsurerKey;
        }

        if (request.ValidTo is { } to && to < request.ValidFrom)
        {
            return DatesKey;
        }

        return TooLong(request.Notes) ? TextKey : null;
    }

    private static string Id(Guid id) => id.ToString("D", CultureInfo.InvariantCulture);

    private static string Day(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Amount(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);

    private static void PutText(JsonObject body, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            body[name] = value.Trim();
        }
    }

    private static ReceivablesPendingAction ToPending(ReceivablesOutboxEntry entry) =>
        new(entry.Id, entry.Action, entry.Reference, DateTime.SpecifyKind(entry.OccurredAtUtc, DateTimeKind.Utc), entry.LastError);

    private static string? Body(ReceivablesOutboxEntry entry)
    {
        if (string.Equals(entry.HttpMethod, Delete, StringComparison.Ordinal))
        {
            return null;
        }

        var body = JsonNode.Parse(entry.PayloadJson)?.AsObject() ?? [];
        body["idempotency_key"] = Id(entry.IdempotencyKey);

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

    private ReceivablesOutboxEntry NewEntry(ReceivablesAction action, string method, string route, Guid commandId, Guid customerId, string? reference, JsonObject body, DateTime occurredAtUtc)
    {
        var label = (reference ?? string.Empty).Trim();

        return new ReceivablesOutboxEntry
        {
            Id = commandId,
            TenantId = tenant.TenantId,
            Action = action,
            HttpMethod = method,
            Route = route,
            CustomerId = customerId,
            Reference = label.Length > MaxReference ? label[..MaxReference] : label,
            PayloadJson = body.ToJsonString(),
            IdempotencyKey = commandId,
            OccurredAtUtc = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc),
            NextAttemptAt = Now(),
        };
    }

    private async Task<ReceivablesLoad<T>?> ReadAsync<T>(string route, Func<string, T> parse, CancellationToken cancellationToken)
    {
        if (api.IsAuthenticated)
        {
            try
            {
                var json = await api.GetAsync(route, cancellationToken).ConfigureAwait(false);
                var data = parse(json);
                var at = Now();
                await store.SaveCacheAsync(route, json, at, cancellationToken).ConfigureAwait(false);

                return new ReceivablesLoad<T>(data, FromCache: false, at);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                // Offline or a bad answer: the cache below. A bad answer never replaces a good cache.
            }
        }

        var cached = await store.FindCacheAsync(route, cancellationToken).ConfigureAwait(false);

        return cached is null
            ? null
            : new ReceivablesLoad<T>(parse(cached.Json), FromCache: true, DateTime.SpecifyKind(cached.SyncedAtUtc, DateTimeKind.Utc));
    }

    private async Task<ReceivablesActionResult> QueueAsync(ReceivablesOutboxEntry entry, CancellationToken cancellationToken)
    {
        var queued = await store.EnqueueAsync(entry, cancellationToken).ConfigureAwait(false);

        if (queued.Status == ReceivablesOutboxStatus.Pending)
        {
            _ = await FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        return queued.Status switch
        {
            ReceivablesOutboxStatus.Sent => new ReceivablesActionResult(ReceivablesActionStatus.Sent, ResultJson: queued.ResultJson),
            ReceivablesOutboxStatus.Conflict => new ReceivablesActionResult(ReceivablesActionStatus.Conflict, Detail: queued.LastError),
            _ => new ReceivablesActionResult(ReceivablesActionStatus.Queued),
        };
    }

    private async Task<SendOutcome> SendAsync(ReceivablesOutboxEntry entry, CancellationToken cancellationToken)
    {
        ReceivablesApiReply reply;
        try
        {
            reply = await api.SendAsync(entry.HttpMethod, entry.Route, Body(entry), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return Retry(entry, ex.Message);
        }

        // Deleting what is already gone is a success: the delete is idempotent.
        var gone = reply.Status == HttpStatusCode.NotFound && string.Equals(entry.HttpMethod, Delete, StringComparison.Ordinal);

        if (reply.IsSuccess || gone)
        {
            entry.Status = ReceivablesOutboxStatus.Sent;
            entry.ResultJson = reply.Body;
            entry.LastError = null;
            entry.UpdatedAt = Now();

            return SendOutcome.Sent;
        }

        if (reply.Status == HttpStatusCode.Unauthorized || IsTransient(reply.Status))
        {
            return Retry(entry, ErrorText(reply.Body, reply.Status));
        }

        entry.Status = ReceivablesOutboxStatus.Conflict;
        entry.LastError = ErrorText(reply.Body, reply.Status);
        entry.UpdatedAt = Now();

        return SendOutcome.Refused;
    }

    private SendOutcome Retry(ReceivablesOutboxEntry entry, string error)
    {
        entry.Attempts++;
        entry.LastError = error;
        entry.UpdatedAt = Now();

        if (!SyncRetryPolicy.CanRetry(entry.Attempts))
        {
            entry.Status = ReceivablesOutboxStatus.Conflict;

            return SendOutcome.Refused;
        }

        entry.NextAttemptAt = Now() + SyncRetryPolicy.NextDelay(entry.Attempts);

        return SendOutcome.Retry;
    }
}
