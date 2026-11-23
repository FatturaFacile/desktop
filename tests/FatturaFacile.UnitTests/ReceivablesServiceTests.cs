// <copyright file="ReceivablesServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the offline-first receivables service: validation, the outbox, idempotent replay, ordered flush, retries, refusals, the cache fallback of the reads and the local credit check.
/// </summary>
public sealed class ReceivablesServiceTests
{
    private const string AgingJson = """{"data":{"as_of":"2027-06-30","currency":"EUR","totals":{},"customers":[]}}""";

    private const string CreditJson = """
        {"data":{"customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","as_of":"2027-06-30","limit":{"id":"33333333-3333-3333-3333-333333333333","customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","currency":"EUR","granted_limit":"1000.00","insured_limit":"0.00","insured_cover":"0.00","coverage_percentage":"0.00","max_overdue_days":30,"overdue_tolerance_amount":"50.00","reaction_policy":"block_order","valid_from":"2026-01-01","valid_to":null,"is_active":true},"exposure":{"open_invoices":"900.00","unbilled_delivery_notes":"100.00","confirmed_orders":"200.00","unreconciled_payments":"50.00","total_exposure":"1150.00","overdue_amount":"300.00","overdue_items":2,"max_days_past_due":45},"utilization_percent":"115.00","available_credit":"-150.00","insured_cover":"0.00","uninsured_exposure":"1150.00","is_credit_hold":false,"is_blocked":true,"decision":null,"overrides":[{"id":"44444444-4444-4444-4444-444444444444","customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","scope":"order","reason":"Approved by the CFO","authorized_by":"cfo","expires_at":"2027-07-31T00:00:00Z","revoked_at":null}]}}
        """;

    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid CustomerId = new(0xC0C0C0C0, 0xC0C0, 0xC0C0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0);
    private static readonly Guid ItemId = new(0x11111111, 0x1111, 0x1111, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11);
    private static readonly DateOnly Day = new(2027, 6, 30);

    private readonly Mock<IReceivablesApiClient> api = new();
    private readonly Mock<ICurrentTenantService> tenant = new();
    private readonly InMemoryReceivablesStore store = new();
    private readonly ManualTimeProvider time = new();
    private readonly List<(string Method, string Route, string? Body)> sent = [];
    private ReceivablesApiReply reply = new(HttpStatusCode.Created, """{"data":{}}""");

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesServiceTests"/> class.
    /// </summary>
    public ReceivablesServiceTests()
    {
        _ = tenant.SetupGet(t => t.TenantId).Returns(Guid.NewGuid());
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), Any))
            .Callback<string, string, string?, CancellationToken>((method, route, body, _) => sent.Add((method, route, body)))
            .ReturnsAsync(() => reply);
    }

    /// <summary>
    /// Requests that cannot be right are refused locally with a localization key and never reach the outbox or the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Actions_InvalidRequests_AreRejectedBeforeTheOutbox()
    {
        var service = Create();
        var noLines = Allocation() with { Lines = [] };
        var zero = Allocation(0m);
        var creditNote = Allocation() with { Lines = [new AllocationLine(ItemId, AllocationType.CreditNote, 10m, Day, null, null, null)] };
        var noCustomer = Allocation() with { CustomerId = Guid.Empty };
        var now = time.GetUtcNow().UtcDateTime;

        Assert.Equal(ReceivablesService.LinesKey, (await service.ApplyAllocationsAsync(noLines, CancellationToken.None)).ErrorKey);
        Assert.Equal(ReconciliationRules.AmountKey, (await service.ApplyAllocationsAsync(zero, CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.SourceKey, (await service.ApplyAllocationsAsync(creditNote, CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.CustomerKey, (await service.ApplyAllocationsAsync(noCustomer, CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.ReasonKey, (await service.SetDisputeAsync(new DisputeRequest(Guid.NewGuid(), CustomerId, "INV-1", ItemId, true, "  ", now), CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.ReasonKey, (await service.GrantOverrideAsync(new GrantOverrideRequest(Guid.NewGuid(), CustomerId, "Rossi", CreditScope.Order, "ok", null, now), CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.DatesKey, (await service.GrantOverrideAsync(new GrantOverrideRequest(Guid.NewGuid(), CustomerId, "Rossi", CreditScope.Order, "Agreed with the CFO", now.AddDays(-1), now), CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.LimitKey, (await service.SaveCreditLimitAsync(LimitRequest(null) with { GrantedLimit = -1m }, CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.LimitKey, (await service.SaveCreditLimitAsync(LimitRequest(null) with { CoveragePercentage = 120m }, CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.InsurerKey, (await service.SaveCreditLimitAsync(LimitRequest(null) with { InsurerName = null, PolicyNumber = " " }, CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.DatesKey, (await service.SaveCreditLimitAsync(LimitRequest(null) with { ValidTo = Day.AddDays(-1) }, CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.RateKey, (await service.UpdateProvisionRatesAsync(new ProvisionRatesRequest(Guid.NewGuid(), new Dictionary<string, decimal>(StringComparer.Ordinal) { ["1_30"] = 101m }, now), CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.RateKey, (await service.UpdateProvisionRatesAsync(new ProvisionRatesRequest(Guid.NewGuid(), new Dictionary<string, decimal>(StringComparer.Ordinal), now), CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.DatesKey, (await service.SendStatementAsync(new SendStatementRequest(Guid.NewGuid(), CustomerId, "Rossi", Day, Day.AddDays(-1), null, "it", now), CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.TextKey, (await service.SendStatementAsync(new SendStatementRequest(Guid.NewGuid(), CustomerId, "Rossi", Day.AddDays(-30), Day, null, "es", now), CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesService.RecipientKey, (await service.SendStatementAsync(new SendStatementRequest(Guid.NewGuid(), CustomerId, "Rossi", Day.AddDays(-30), Day, "not-an-email", "it", now), CancellationToken.None)).ErrorKey);
        Assert.Equal(ReceivablesActionStatus.Rejected, (await service.ReverseAllocationAsync(new ReverseAllocationRequest(Guid.NewGuid(), CustomerId, "INV-1", Guid.Empty, now), CancellationToken.None)).Status);
        Assert.Empty(store.Entries);
        Assert.Empty(sent);
    }

    /// <summary>
    /// Online, the allocations are posted with amounts as strings and the command id as idempotency key, and the answer is returned.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApplyAllocations_Online_SendsTheAllocationsWithTheIdempotencyKey()
    {
        var request = Allocation();

        var result = await Create().ApplyAllocationsAsync(request, CancellationToken.None);

        Assert.Equal(ReceivablesActionStatus.Sent, result.Status);
        Assert.True(result.IsAccepted);
        Assert.Equal("""{"data":{}}""", result.ResultJson);
        var call = Assert.Single(sent);
        Assert.Equal("POST", call.Method);
        Assert.Equal("invoicing/receivables/allocations", call.Route);
        using var json = Body(call);
        var line = json.RootElement.GetProperty("allocations")[0];
        Assert.Equal("payment", line.GetProperty("type").GetString());
        Assert.Equal("40.00", line.GetProperty("amount").GetString());
        Assert.Equal("2027-06-30", line.GetProperty("value_date").GetString());
        Assert.Equal("ledger-1", line.GetProperty("source_id").GetString());
        Assert.Equal(ItemId.ToString("D"), line.GetProperty("receivable_item_id").GetString());
        Assert.Equal(request.CommandId.ToString("D"), json.RootElement.GetProperty("idempotency_key").GetString());
        Assert.Equal(ReceivablesOutboxStatus.Sent, Assert.Single(store.Entries).Status);
    }

    /// <summary>
    /// Offline, the allocation is saved with its key and the instant of the user, and nothing is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApplyAllocations_Offline_QueuesTheEntryWithItsKey()
    {
        GoOffline();
        var request = Allocation(occurredAt: time.GetUtcNow().UtcDateTime.AddMinutes(-7));

        var result = await Create().ApplyAllocationsAsync(request, CancellationToken.None);

        Assert.Equal(ReceivablesActionStatus.Queued, result.Status);
        var entry = Assert.Single(store.Entries);
        Assert.Equal(ReceivablesOutboxStatus.Pending, entry.Status);
        Assert.Equal(request.CommandId, entry.IdempotencyKey);
        Assert.Equal(request.OccurredAtUtc, entry.OccurredAtUtc);
        Assert.Equal(ReceivablesAction.ApplyAllocations, entry.Action);
        Assert.Equal("INV-1", entry.Reference);
        Assert.Equal(CustomerId, entry.CustomerId);
        Assert.Empty(sent);
    }

    /// <summary>
    /// Replaying the same command never applies the allocation twice: the second call returns the first outcome without sending.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApplyAllocations_SameCommandTwice_IsSentOnlyOnce()
    {
        var service = Create();
        var request = Allocation();

        var first = await service.ApplyAllocationsAsync(request, CancellationToken.None);
        var second = await service.ApplyAllocationsAsync(request, CancellationToken.None);

        Assert.Equal(ReceivablesActionStatus.Sent, first.Status);
        Assert.Equal(ReceivablesActionStatus.Sent, second.Status);
        _ = Assert.Single(sent);
        _ = Assert.Single(store.Entries);
    }

    /// <summary>
    /// Queued entries are sent oldest first once the server is reachable, and a second pass sends nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_AfterConnectivityReturns_SendsOldestFirstAndOnlyOnce()
    {
        GoOffline();
        var service = Create();
        var start = time.GetUtcNow().UtcDateTime;
        _ = await service.ApplyAllocationsAsync(Allocation(20m, start.AddMinutes(5)), CancellationToken.None);
        _ = await service.ApplyAllocationsAsync(Allocation(10m, start), CancellationToken.None);
        var offlineFlush = await service.FlushAsync(CancellationToken.None);
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);

        var online = await service.FlushAsync(CancellationToken.None);
        var again = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(new ReceivablesFlushResult(0, 0, 2, Offline: true), offlineFlush);
        Assert.Equal(new ReceivablesFlushResult(2, 0, 0, Offline: false), online);
        Assert.Equal(new ReceivablesFlushResult(0, 0, 0, Offline: false), again);
        Assert.Equal(2, sent.Count);
        using var firstBody = Body(sent[0]);
        using var secondBody = Body(sent[1]);
        Assert.Equal("10.00", firstBody.RootElement.GetProperty("allocations")[0].GetProperty("amount").GetString());
        Assert.Equal("20.00", secondBody.RootElement.GetProperty("allocations")[0].GetProperty("amount").GetString());
    }

    /// <summary>
    /// Disputing sends the trimmed reason; clearing the dispute sends the state alone.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SetDispute_MarkAndClear_SendTheStateWithTheReasonOnlyWhenDisputing()
    {
        var service = Create();
        var now = time.GetUtcNow().UtcDateTime;

        _ = await service.SetDisputeAsync(new DisputeRequest(Guid.NewGuid(), CustomerId, "INV-1", ItemId, true, " Wrong VAT ", now), CancellationToken.None);
        _ = await service.SetDisputeAsync(new DisputeRequest(Guid.NewGuid(), CustomerId, "INV-1", ItemId, false, "ignored", now.AddMinutes(1)), CancellationToken.None);

        Assert.Equal(2, sent.Count);
        Assert.All(sent, call => Assert.Equal("invoicing/receivables/items/11111111-1111-1111-1111-111111111111/dispute", call.Route));
        using var mark = Body(sent[0]);
        using var clear = Body(sent[1]);
        Assert.True(mark.RootElement.GetProperty("disputed").GetBoolean());
        Assert.Equal("Wrong VAT", mark.RootElement.GetProperty("reason").GetString());
        Assert.False(clear.RootElement.GetProperty("disputed").GetBoolean());
        Assert.False(clear.RootElement.TryGetProperty("reason", out _));
    }

    /// <summary>
    /// A 422 answer is a conflict with the first message of the server; it is listed and the user can dismiss it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_ServerRefusal_BecomesAConflictThatCanBeDismissed()
    {
        reply = new ReceivablesApiReply(HttpStatusCode.UnprocessableEntity, """{"message":"The given data was invalid.","errors":{"allocations.0.amount":["The amount exceeds the open amount."]}}""");
        var service = Create();

        var result = await service.ApplyAllocationsAsync(Allocation(), CancellationToken.None);
        var queue = await service.LoadQueueAsync(CancellationToken.None);

        Assert.Equal(ReceivablesActionStatus.Conflict, result.Status);
        Assert.Equal("The amount exceeds the open amount.", result.Detail);
        Assert.Empty(queue.Pending);
        var conflict = Assert.Single(queue.Conflicts);
        Assert.Equal(ReceivablesAction.ApplyAllocations, conflict.Action);
        Assert.Equal("INV-1", conflict.Reference);

        await service.DismissConflictAsync(conflict.Id, CancellationToken.None);

        Assert.Empty((await service.LoadQueueAsync(CancellationToken.None)).Conflicts);
        Assert.Equal(ReceivablesOutboxStatus.Dismissed, Assert.Single(store.Entries).Status);
    }

    /// <summary>
    /// A 503 keeps the entry pending with a back-off; it is not retried before its time and goes through when it is.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_TransientError_RetriesLaterWithBackOff()
    {
        reply = new ReceivablesApiReply(HttpStatusCode.ServiceUnavailable, string.Empty);
        var service = Create();

        var first = await service.ApplyAllocationsAsync(Allocation(), CancellationToken.None);
        var tooSoon = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(ReceivablesActionStatus.Queued, first.Status);
        var entry = Assert.Single(store.Entries);
        Assert.Equal(ReceivablesOutboxStatus.Pending, entry.Status);
        Assert.Equal(1, entry.Attempts);
        Assert.Equal("HTTP 503", entry.LastError);
        Assert.Equal(new ReceivablesFlushResult(0, 0, 1, Offline: true), tooSoon);
        _ = Assert.Single(sent);

        reply = new ReceivablesApiReply(HttpStatusCode.Created, """{"data":{}}""");
        time.Advance(TimeSpan.FromMinutes(5));
        var later = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(1, later.Sent);
        Assert.Equal(ReceivablesOutboxStatus.Sent, entry.Status);
        Assert.Equal(2, sent.Count);
    }

    /// <summary>
    /// An expired session (401) is retried after login, not treated as a refusal.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_Unauthorized_IsRetriedNotRefused()
    {
        reply = new ReceivablesApiReply(HttpStatusCode.Unauthorized, string.Empty);

        var result = await Create().ApplyAllocationsAsync(Allocation(), CancellationToken.None);

        Assert.Equal(ReceivablesActionStatus.Queued, result.Status);
        Assert.Equal(ReceivablesOutboxStatus.Pending, Assert.Single(store.Entries).Status);
    }

    /// <summary>
    /// A network failure keeps the entry pending with the error and stops the replay.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_NetworkException_QueuesTheEntryAndStopsTheReplay()
    {
        _ = api.Setup(a => a.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), Any)).ThrowsAsync(new HttpRequestException("down"));

        var result = await Create().ApplyAllocationsAsync(Allocation(), CancellationToken.None);

        Assert.Equal(ReceivablesActionStatus.Queued, result.Status);
        var entry = Assert.Single(store.Entries);
        Assert.Equal(1, entry.Attempts);
        Assert.Equal("down", entry.LastError);
    }

    /// <summary>
    /// After eight failed attempts the entry stops retrying and waits for the user as a conflict.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_AfterTheLastAttempt_TheEntryBecomesAConflict()
    {
        reply = new ReceivablesApiReply(HttpStatusCode.ServiceUnavailable, string.Empty);
        var service = Create();
        _ = await service.ApplyAllocationsAsync(Allocation(), CancellationToken.None);

        for (var attempt = 2; attempt <= 8; attempt++)
        {
            time.Advance(TimeSpan.FromHours(2));
            _ = await service.FlushAsync(CancellationToken.None);
        }

        var entry = Assert.Single(store.Entries);
        Assert.Equal(8, entry.Attempts);
        Assert.Equal(ReceivablesOutboxStatus.Conflict, entry.Status);
        Assert.Equal(8, sent.Count);
    }

    /// <summary>
    /// Without a session the queued entries stay where they are and the flush says it is offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_WithoutSession_ReportsOffline()
    {
        GoOffline();
        var service = Create();
        _ = await service.ApplyAllocationsAsync(Allocation(), CancellationToken.None);

        var result = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(new ReceivablesFlushResult(0, 0, 1, Offline: true), result);
    }

    /// <summary>
    /// The deletion is a DELETE with no body; a limit that is already gone counts as deleted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DeleteCreditLimit_SendsDeleteWithoutBody_AndTreatsNotFoundAsDone()
    {
        reply = new ReceivablesApiReply(HttpStatusCode.NotFound, string.Empty);
        var limitId = Guid.NewGuid();

        var result = await Create().DeleteCreditLimitAsync(new DeleteCreditLimitRequest(Guid.NewGuid(), CustomerId, "Rossi", limitId, time.GetUtcNow().UtcDateTime), CancellationToken.None);

        Assert.Equal(ReceivablesActionStatus.Sent, result.Status);
        var (method, route, body) = Assert.Single(sent);
        Assert.Equal("DELETE", method);
        Assert.Equal("invoicing/receivables/credit-limits/" + limitId.ToString("D"), route);
        Assert.Null(body);
    }

    /// <summary>
    /// A new limit is posted with its customer; an existing one is updated with a PUT that does not carry the customer.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveCreditLimit_CreateAndUpdate_UsePostAndPut()
    {
        var service = Create();
        var limitId = Guid.NewGuid();

        _ = await service.SaveCreditLimitAsync(LimitRequest(null), CancellationToken.None);
        _ = await service.SaveCreditLimitAsync(LimitRequest(limitId), CancellationToken.None);

        Assert.Equal(("POST", "invoicing/receivables/credit-limits"), (sent[0].Method, sent[0].Route));
        Assert.Equal(("PUT", "invoicing/receivables/credit-limits/" + limitId.ToString("D")), (sent[1].Method, sent[1].Route));
        using var create = Body(sent[0]);
        using var update = Body(sent[1]);
        Assert.Equal(CustomerId.ToString("D"), create.RootElement.GetProperty("customer_id").GetString());
        Assert.False(update.RootElement.TryGetProperty("customer_id", out _));
        Assert.Equal("EUR", create.RootElement.GetProperty("currency").GetString());
        Assert.Equal("1000.00", create.RootElement.GetProperty("granted_limit").GetString());
        Assert.Equal("500.00", create.RootElement.GetProperty("insured_limit").GetString());
        Assert.Equal("80.00", create.RootElement.GetProperty("coverage_percentage").GetString());
        Assert.Equal(30, create.RootElement.GetProperty("max_overdue_days").GetInt32());
        Assert.Equal("block_order", create.RootElement.GetProperty("reaction_policy").GetString());
        Assert.Equal("2027-06-30", create.RootElement.GetProperty("valid_from").GetString());
        Assert.True(create.RootElement.GetProperty("is_active").GetBoolean());
        Assert.False(create.RootElement.TryGetProperty("valid_to", out _));
    }

    /// <summary>
    /// An override is granted on the customer route with a trimmed reason and revoked on its own route.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Overrides_GrantAndRevoke_UseTheContractRoutes()
    {
        var service = Create();
        var overrideId = Guid.NewGuid();
        var now = time.GetUtcNow().UtcDateTime;

        _ = await service.GrantOverrideAsync(new GrantOverrideRequest(Guid.NewGuid(), CustomerId, "Rossi", CreditScope.DeliveryNote, "  Agreed with the CFO  ", now.AddDays(7), now), CancellationToken.None);
        _ = await service.RevokeOverrideAsync(new RevokeOverrideRequest(Guid.NewGuid(), CustomerId, "Rossi", overrideId, "No longer needed", now), CancellationToken.None);

        Assert.Equal("invoicing/receivables/customers/" + CustomerId.ToString("D") + "/overrides", sent[0].Route);
        Assert.Equal("invoicing/receivables/overrides/" + overrideId.ToString("D") + "/revoke", sent[1].Route);
        using var grant = Body(sent[0]);
        using var revoke = Body(sent[1]);
        Assert.Equal("delivery_note", grant.RootElement.GetProperty("scope").GetString());
        Assert.Equal("Agreed with the CFO", grant.RootElement.GetProperty("reason").GetString());
        Assert.StartsWith("2026-11-28", grant.RootElement.GetProperty("expires_at").GetString(), StringComparison.Ordinal);
        Assert.Equal("No longer needed", revoke.RootElement.GetProperty("reason").GetString());
    }

    /// <summary>
    /// The provision rates are put as percentages per bucket key.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task UpdateProvisionRates_SendsThePercentPerBucket()
    {
        var rates = new Dictionary<string, decimal>(StringComparer.Ordinal) { ["1_30"] = 2.5m, ["121_plus"] = 60m };

        _ = await Create().UpdateProvisionRatesAsync(new ProvisionRatesRequest(Guid.NewGuid(), rates, time.GetUtcNow().UtcDateTime), CancellationToken.None);

        var call = Assert.Single(sent);
        Assert.Equal(("PUT", "invoicing/receivables/provision/rates"), (call.Method, call.Route));
        using var json = Body(call);
        Assert.Equal(2.5m, json.RootElement.GetProperty("rates").GetProperty("1_30").GetDecimal());
        Assert.Equal(60m, json.RootElement.GetProperty("rates").GetProperty("121_plus").GetDecimal());
    }

    /// <summary>
    /// The statement email carries the period, the optional recipient and the language.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SendStatement_PostsThePeriodTheRecipientAndTheLanguage()
    {
        _ = await Create().SendStatementAsync(new SendStatementRequest(Guid.NewGuid(), CustomerId, "Rossi", Day.AddDays(-89), Day, " accounts@rossi.example ", "de", time.GetUtcNow().UtcDateTime), CancellationToken.None);

        var call = Assert.Single(sent);
        Assert.Equal("invoicing/receivables/customers/" + CustomerId.ToString("D") + "/statement/send", call.Route);
        using var json = Body(call);
        Assert.Equal("2027-04-02", json.RootElement.GetProperty("from").GetString());
        Assert.Equal("2027-06-30", json.RootElement.GetProperty("to").GetString());
        Assert.Equal("de", json.RootElement.GetProperty("locale").GetString());
        Assert.Equal("accounts@rossi.example", json.RootElement.GetProperty("recipient").GetString());
    }

    /// <summary>
    /// A read caches the answer under its route; offline the same read returns it from the cache with the instant it was pulled.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reads_Online_CacheTheAnswer_AndOfflineServeTheCache()
    {
        var route = ReceivablesRoutes.Aging(null, null);
        _ = api.Setup(a => a.GetAsync(route, Any)).ReturnsAsync(AgingJson);
        var service = Create();

        var online = await service.LoadAgingAsync(null, null, CancellationToken.None);
        GoOffline();
        time.Advance(TimeSpan.FromHours(3));
        var offline = await service.LoadAgingAsync(null, null, CancellationToken.None);

        Assert.NotNull(online);
        Assert.False(online.FromCache);
        Assert.NotNull(offline);
        Assert.True(offline.FromCache);
        Assert.Equal(online.SyncedAtUtc, offline.SyncedAtUtc);
        Assert.Equal("EUR", offline.Data.Currency);
    }

    /// <summary>
    /// Offline with nothing cached the read has no answer.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reads_OfflineWithoutCache_ReturnNothing()
    {
        GoOffline();

        Assert.Null(await Create().LoadAgingAsync(null, null, CancellationToken.None));
        Assert.Null(await Create().LoadCreditStatusAsync(CustomerId, CancellationToken.None));
    }

    /// <summary>
    /// A server error or a broken answer shows the cache and never overwrites a good cached answer.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reads_BadOrFailedAnswer_FallBackToTheCacheWithoutReplacingIt()
    {
        var route = ReceivablesRoutes.Aging(null, null);
        await store.SaveCacheAsync(route, AgingJson, time.GetUtcNow().UtcDateTime.AddDays(-1), CancellationToken.None);
        var service = Create();

        _ = api.Setup(a => a.GetAsync(route, Any)).ReturnsAsync("{not json");
        var broken = await service.LoadAgingAsync(null, null, CancellationToken.None);
        _ = api.Setup(a => a.GetAsync(route, Any)).ThrowsAsync(new HttpRequestException("500"));
        var failed = await service.LoadAgingAsync(null, null, CancellationToken.None);

        Assert.NotNull(broken);
        Assert.True(broken.FromCache);
        Assert.NotNull(failed);
        Assert.True(failed.FromCache);
        Assert.Equal(AgingJson, (await store.FindCacheAsync(route, CancellationToken.None))!.Json);
    }

    /// <summary>
    /// Loading items online trims the cache of the item pages; the cached fallback does not.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadItems_Online_TrimsTheItemCache()
    {
        const string page = """{"data":[],"meta":{"current_page":1,"last_page":1,"total":0}}""";
        _ = api.Setup(a => a.GetAsync(ReceivablesRoutes.Items(ReceivableItemFilter.Default), Any)).ReturnsAsync(page);
        var service = Create();

        var online = await service.LoadItemsAsync(ReceivableItemFilter.Default, CancellationToken.None);
        GoOffline();
        _ = await service.LoadItemsAsync(ReceivableItemFilter.Default, CancellationToken.None);

        Assert.NotNull(online);
        Assert.Empty(online.Data.Items);
        Assert.Equal([ReceivablesRoutes.ItemsPrefix], store.TrimmedPrefixes);
    }

    /// <summary>
    /// Offline, the credit check is decided locally with the policy rules from the cached credit position, overrides included.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CheckCredit_Offline_IsDecidedLocallyFromTheCachedCreditPosition()
    {
        await store.SaveCacheAsync(ReceivablesRoutes.Credit(CustomerId), CreditJson, time.GetUtcNow().UtcDateTime, CancellationToken.None);
        GoOffline();
        var service = Create();

        var order = await service.CheckCreditAsync(CustomerId, CreditScope.Order, 0m, CancellationToken.None);
        var note = await service.CheckCreditAsync(CustomerId, CreditScope.DeliveryNote, 0m, CancellationToken.None);

        Assert.NotNull(order);
        Assert.True(order.FromCache);
        Assert.Equal(CreditOutcome.Ok, order.Data.Outcome);
        Assert.True(order.Data.Overridden);
        Assert.Equal(["over_limit", "overdue_days"], order.Data.Breaches);
        Assert.NotNull(note);
        Assert.Equal(CreditOutcome.Warning, note.Data.Outcome);
        Assert.False(note.Data.Overridden);
        Assert.Equal(1150m, note.Data.Exposure);
    }

    /// <summary>
    /// Online, the credit check is the answer of the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CheckCredit_Online_UsesTheAnswerOfTheServer()
    {
        var route = ReceivablesRoutes.CreditCheck(CustomerId, CreditScope.Order, 250m);
        _ = api.Setup(a => a.GetAsync(route, Any)).ReturnsAsync("""{"data":{"outcome":"block","breaches":["credit_hold"],"reason":"On hold","exposure":"250.00","limit":"0.00","overridden":false,"reaction_policy":"credit_hold"}}""");

        var result = await Create().CheckCreditAsync(CustomerId, CreditScope.Order, 250m, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.FromCache);
        Assert.Equal(CreditOutcome.Block, result.Data.Outcome);
        Assert.Equal("On hold", result.Data.Reason);
    }

    /// <summary>
    /// The statement file needs the server: offline there is none, online it is downloaded, and only pdf and xlsx are accepted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ExportStatement_OnlineReturnsTheFile_OfflineReturnsNothing()
    {
        var file = new ReceivablesFile("statement.pdf", new byte[] { 1, 2, 3 });
        _ = api.Setup(a => a.DownloadAsync(It.IsAny<string>(), It.IsAny<string>(), Any)).ReturnsAsync(file);
        var service = Create();

        var online = await service.ExportStatementAsync(CustomerId, "pdf", Day.AddDays(-30), Day, null, CancellationToken.None);
        GoOffline();
        var offline = await service.ExportStatementAsync(CustomerId, "pdf", Day.AddDays(-30), Day, null, CancellationToken.None);

        Assert.Same(file, online);
        Assert.Null(offline);
        _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.ExportStatementAsync(CustomerId, "docx", Day, Day, null, CancellationToken.None));
        api.Verify(a => a.DownloadAsync(It.Is<string>(route => route.EndsWith("/statement/pdf?from=2027-05-31&to=2027-06-30", StringComparison.Ordinal)), It.IsAny<string>(), Any), Times.Once);
    }

    /// <summary>
    /// The queue lists the waiting actions with their label and the instant the user acted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadQueue_ListsPendingEntriesWithTheirLabel()
    {
        GoOffline();
        var service = Create();
        var request = Allocation(occurredAt: time.GetUtcNow().UtcDateTime.AddMinutes(-3));
        _ = await service.ApplyAllocationsAsync(request, CancellationToken.None);

        var queue = await service.LoadQueueAsync(CancellationToken.None);

        var pending = Assert.Single(queue.Pending);
        Assert.Equal("INV-1", pending.Reference);
        Assert.Equal(request.OccurredAtUtc, pending.OccurredAtUtc);
        Assert.Equal(DateTimeKind.Utc, pending.OccurredAtUtc.Kind);
        Assert.Empty(queue.Conflicts);
    }

    private static JsonDocument Body((string Method, string Route, string? Body) call) => JsonDocument.Parse(call.Body!);

    private ReceivablesService Create() => new(api.Object, store, tenant.Object, time);

    private void GoOffline() => _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

    private ApplyAllocationsRequest Allocation(decimal amount = 40m, DateTime? occurredAt = null) =>
        new(Guid.NewGuid(), CustomerId, "INV-1", [new AllocationLine(ItemId, AllocationType.Payment, amount, Day, "ledger-1", null, null)], occurredAt ?? time.GetUtcNow().UtcDateTime);

    private CreditLimitRequest LimitRequest(Guid? limitId) =>
        new(Guid.NewGuid(), CustomerId, "Rossi", limitId, "EUR", 1000m, 500m, "Euler Hermes", "POL-1", 80m, 30, 50m, CreditReactionPolicy.BlockOrder, Day, null, true, null, time.GetUtcNow().UtcDateTime);
}
