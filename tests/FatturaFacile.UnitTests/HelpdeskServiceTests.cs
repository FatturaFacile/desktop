// <copyright file="HelpdeskServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the offline-first helpdesk service: validation, the outbox, idempotency keys, ordered replay, retries, refusals,
/// the idempotent escalations and the cache fallback of the reads.
/// </summary>
public sealed class HelpdeskServiceTests
{
    private const string BreachRisk = """        [{"ticket_id":"11111111-1111-1111-1111-111111111111","ticket_number":"TCK-1","subject":"Low","priority":"low","status":"open","risk_percent":80,"targets":[{"target":"response","due_at":"2027-01-10T10:00:00Z","elapsed_percent":80,"state":"at_risk"}]},{"ticket_id":"22222222-2222-2222-2222-222222222222","ticket_number":"TCK-2","subject":"Breached","priority":"high","status":"open","risk_percent":120,"targets":[{"target":"resolution","due_at":"2027-01-10T08:00:00Z","elapsed_percent":120,"state":"breached"}]},{"ticket_id":"33333333-3333-3333-3333-333333333333","ticket_number":"TCK-3","subject":"Risky","priority":"high","status":"open","risk_percent":95,"targets":[{"target":"response","due_at":"2027-01-10T09:00:00Z","elapsed_percent":95,"state":"at_risk"}]}]""";

    private const string PartialEntry = """{"data":{"id":"x","billing_status":"partially_deducted","contract_hours":"0.25","billable_hours":"0.25","total_charge":"20.00"}}""";

    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid TicketId = new(0x11111111, 0x1111, 0x1111, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11);

    private readonly Mock<IHelpdeskApiClient> api = new();
    private readonly Mock<ICurrentTenantService> tenant = new();
    private readonly InMemoryHelpdeskStore store = new();
    private readonly ManualTimeProvider time = new();
    private readonly List<(string Route, string Body)> sent = [];
    private HelpdeskApiReply reply = new(HttpStatusCode.Created, PartialEntry);

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpdeskServiceTests"/> class.
    /// </summary>
    public HelpdeskServiceTests()
    {
        _ = tenant.SetupGet(t => t.TenantId).Returns(Guid.NewGuid());
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.SendAsync(It.IsAny<string>(), It.IsAny<string>(), Any))
            .Callback<string, string, CancellationToken>((route, body, _) => sent.Add((route, body)))
            .ReturnsAsync(() => reply);
    }

    /// <summary>
    /// A time entry outside one day, or with a text that is too long, is refused locally and never reaches the outbox.
    /// </summary>
    /// <param name="minutes">The minutes.</param>
    /// <param name="descriptionLength">The length of the description.</param>
    /// <param name="expectedKey">The expected localization key.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(0, 0, HelpdeskService.MinutesKey)]
    [InlineData(-5, 0, HelpdeskService.MinutesKey)]
    [InlineData(1441, 0, HelpdeskService.MinutesKey)]
    [InlineData(30, 2001, HelpdeskService.TextKey)]
    public async Task LogTime_InvalidEntry_IsRejectedBeforeTheOutbox(int minutes, int descriptionLength, string expectedKey)
    {
        var request = TimeRequest(minutes) with { Description = new string('d', descriptionLength) };

        var result = await Create().LogTimeAsync(request, CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Rejected, result.Status);
        Assert.Equal(expectedKey, result.ErrorKey);
        Assert.Empty(store.Entries);
        Assert.Empty(sent);
    }

    /// <summary>
    /// A request without a ticket is refused locally.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LogTime_WithoutTicket_IsRejected()
    {
        var result = await Create().LogTimeAsync(TimeRequest(30) with { TicketId = Guid.Empty }, CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Rejected, result.Status);
        Assert.Equal(HelpdeskService.TicketKey, result.ErrorKey);
    }

    /// <summary>
    /// Online, the entry is sent to the ticket route with the idempotency key and the answer says how it was settled.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LogTime_Online_SendsToTheRouteAndReturnsTheServerSettlement()
    {
        var request = TimeRequest(30);

        var result = await Create().LogTimeAsync(request, CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Sent, result.Status);
        Assert.NotNull(result.TimeOutcome);
        Assert.Equal(HelpdeskTimeOutcome.PartiallyDeducted, result.TimeOutcome.BillingStatus);
        Assert.False(result.TimeOutcome.IsEstimate);
        Assert.Equal(0.25m, result.TimeOutcome.ContractHours);
        Assert.Equal(20.00m, result.TimeOutcome.TotalCharge);

        var (route, requestBody) = Assert.Single(sent);
        Assert.Equal("esm/tickets/11111111-1111-1111-1111-111111111111/time-entries", route);
        using var json = JsonDocument.Parse(requestBody);
        Assert.Equal(30, json.RootElement.GetProperty("minutes").GetInt32());
        Assert.True(json.RootElement.GetProperty("is_billable").GetBoolean());
        Assert.Equal("Replaced the toner", json.RootElement.GetProperty("description").GetString());
        Assert.Equal(request.CommandId.ToString("D"), json.RootElement.GetProperty("idempotency_key").GetString());
        Assert.Equal(HelpdeskOutboxStatus.Sent, Assert.Single(store.Entries).Status);
    }

    /// <summary>
    /// Offline, the entry is saved with its key and the instant of the agent, nothing is sent and a local estimate is returned.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LogTime_Offline_QueuesTheEntryWithAnEstimate()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var request = TimeRequest(30) with { RemainingContractHours = 2m, OccurredAtUtc = time.GetUtcNow().UtcDateTime.AddMinutes(-7) };

        var result = await Create().LogTimeAsync(request, CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Queued, result.Status);
        Assert.NotNull(result.TimeOutcome);
        Assert.True(result.TimeOutcome.IsEstimate);
        Assert.Equal(HelpdeskTimeOutcome.ContractDeducted, result.TimeOutcome.BillingStatus);
        var entry = Assert.Single(store.Entries);
        Assert.Equal(HelpdeskOutboxStatus.Pending, entry.Status);
        Assert.Equal(request.CommandId, entry.IdempotencyKey);
        Assert.Equal(request.OccurredAtUtc, entry.OccurredAtUtc);
        Assert.Equal(HelpdeskAction.LogTime, entry.Action);
        Assert.Empty(sent);
    }

    /// <summary>
    /// When connectivity returns the queued entries are sent once, oldest first, and a second pass sends nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_AfterReconnect_ReplaysInOrderAndOnlyOnce()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var service = Create();
        var now = time.GetUtcNow().UtcDateTime;
        var later = TimeRequest(20) with { OccurredAtUtc = now.AddMinutes(-1) };
        var earlier = TimeRequest(10) with { OccurredAtUtc = now.AddMinutes(-30) };
        _ = await service.LogTimeAsync(later, CancellationToken.None);
        _ = await service.LogTimeAsync(earlier, CancellationToken.None);
        var offline = await service.FlushAsync(CancellationToken.None);

        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        var online = await service.FlushAsync(CancellationToken.None);
        var again = await service.FlushAsync(CancellationToken.None);

        Assert.True(offline.Offline);
        Assert.Equal(2, offline.Remaining);
        Assert.Equal(2, online.Sent);
        Assert.Equal(0, online.Remaining);
        Assert.Equal(new HelpdeskFlushResult(0, 0, 0, Offline: false), again);
        Assert.Equal(2, sent.Count);
        Assert.Contains("\"minutes\":10", sent[0].Body, StringComparison.Ordinal);
        Assert.Contains("\"minutes\":20", sent[1].Body, StringComparison.Ordinal);
    }

    /// <summary>
    /// A transient server failure keeps the entry queued with a back-off, and it is sent once the delay has passed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_TransientFailure_KeepsTheEntryAndBacksOff()
    {
        reply = new HelpdeskApiReply(HttpStatusCode.ServiceUnavailable, "{}");
        var service = Create();

        var first = await service.LogTimeAsync(TimeRequest(30), CancellationToken.None);
        var tooSoon = await service.FlushAsync(CancellationToken.None);
        reply = new HelpdeskApiReply(HttpStatusCode.Created, PartialEntry);
        time.Advance(TimeSpan.FromMinutes(5));
        var afterDelay = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Queued, first.Status);
        Assert.True(tooSoon.Offline);
        Assert.Equal(1, tooSoon.Remaining);
        Assert.Equal(1, afterDelay.Sent);
        Assert.Equal(2, sent.Count);
        var entry = Assert.Single(store.Entries);
        Assert.Equal(HelpdeskOutboxStatus.Sent, entry.Status);
        Assert.Equal(1, entry.Attempts);
    }

    /// <summary>
    /// A network failure and an expired session both leave the entry queued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_NetworkErrorOrUnauthorized_LeavesTheEntryQueued()
    {
        _ = api.Setup(a => a.SendAsync(It.IsAny<string>(), It.IsAny<string>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var service = Create();
        var network = await service.LogTimeAsync(TimeRequest(30), CancellationToken.None);

        reply = new HelpdeskApiReply(HttpStatusCode.Unauthorized, "{}");
        _ = api.Setup(a => a.SendAsync(It.IsAny<string>(), It.IsAny<string>(), Any)).ReturnsAsync(() => reply);
        time.Advance(TimeSpan.FromHours(1));
        _ = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Queued, network.Status);
        var entry = Assert.Single(store.Entries);
        Assert.Equal(HelpdeskOutboxStatus.Pending, entry.Status);
        Assert.Equal(2, entry.Attempts);
    }

    /// <summary>
    /// A refusal of the server (validation, closed ticket) turns the entry into a conflict with the message, until dismissed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_Refused_BecomesAConflictUntilDismissed()
    {
        reply = new HelpdeskApiReply(HttpStatusCode.UnprocessableEntity, """{"message":"Invalid","errors":{"minutes":["The ticket is closed."]}}""");
        var service = Create();

        var result = await service.LogTimeAsync(TimeRequest(30), CancellationToken.None);
        var entry = Assert.Single(store.Entries);
        await service.DismissConflictAsync(entry.Id, CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Conflict, result.Status);
        Assert.Equal("The ticket is closed.", result.Detail);
        Assert.Equal(HelpdeskOutboxStatus.Dismissed, entry.Status);
        Assert.Empty(await store.ConflictsAsync(CancellationToken.None));
        Assert.Empty(await store.PendingAsync(CancellationToken.None));
    }

    /// <summary>
    /// A refusal without a readable body is described by its status.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_RefusedWithoutBody_UsesTheStatus()
    {
        reply = new HelpdeskApiReply(HttpStatusCode.NotFound, "<html>");

        var result = await Create().LogTimeAsync(TimeRequest(30), CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Conflict, result.Status);
        Assert.Equal("HTTP 404", result.Detail);
    }

    /// <summary>
    /// The Field Service escalation sends the optional fields it was given and nothing else.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task EscalateToFieldService_SendsTheGivenFieldsOnly()
    {
        var start = new DateTime(2027, 2, 1, 8, 0, 0, DateTimeKind.Utc);
        HelpdeskFieldServiceRequest request = new(Guid.NewGuid(), TicketId, "TCK-1", " repair ", start, start.AddHours(2), null, time.GetUtcNow().UtcDateTime);

        var result = await Create().EscalateToFieldServiceAsync(request, CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Sent, result.Status);
        var (route, requestBody) = Assert.Single(sent);
        Assert.Equal("esm/tickets/11111111-1111-1111-1111-111111111111/escalate/field-service", route);
        using var json = JsonDocument.Parse(requestBody);
        Assert.Equal("repair", json.RootElement.GetProperty("work_type").GetString());
        Assert.Equal(start.ToString("O", System.Globalization.CultureInfo.InvariantCulture), json.RootElement.GetProperty("scheduled_start").GetString());
        Assert.False(json.RootElement.TryGetProperty("location_address", out _));
        Assert.Equal(request.CommandId.ToString("D"), json.RootElement.GetProperty("idempotency_key").GetString());
    }

    /// <summary>
    /// A repeated escalation is answered with the existing record (200) and counts as sent, so a replay is harmless.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task EscalateToFieldService_RepeatedAndAnsweredWithTheExistingRecord_IsSent()
    {
        var service = Create();
        HelpdeskFieldServiceRequest first = new(Guid.NewGuid(), TicketId, "TCK-1", null, null, null, null, time.GetUtcNow().UtcDateTime);
        reply = new HelpdeskApiReply(HttpStatusCode.Created, """{"data":{"id":"wo-1"}}""");
        var created = await service.EscalateToFieldServiceAsync(first, CancellationToken.None);

        reply = new HelpdeskApiReply(HttpStatusCode.OK, """{"data":{"id":"wo-1"}}""");
        var repeated = await service.EscalateToFieldServiceAsync(first with { CommandId = Guid.NewGuid() }, CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Sent, created.Status);
        Assert.Equal(HelpdeskActionStatus.Sent, repeated.Status);
        Assert.Equal(2, sent.Count);
    }

    /// <summary>
    /// A visit that ends before it starts is refused locally.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task EscalateToFieldService_EndBeforeStart_IsRejected()
    {
        var start = new DateTime(2027, 2, 1, 8, 0, 0, DateTimeKind.Utc);
        HelpdeskFieldServiceRequest request = new(Guid.NewGuid(), TicketId, "TCK-1", null, start, start.AddHours(-1), null, time.GetUtcNow().UtcDateTime);

        var result = await Create().EscalateToFieldServiceAsync(request, CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Rejected, result.Status);
        Assert.Equal(HelpdeskService.ScheduleKey, result.ErrorKey);
        Assert.Empty(sent);
    }

    /// <summary>
    /// The RMA escalation sends the unit and the goodwill flag, and returns the tracking token of the customer.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task EscalateToRma_SendsTheUnitAndReturnsTheTrackingToken()
    {
        reply = new HelpdeskApiReply(HttpStatusCode.Created, """{"data":{"id":"rma-1"},"meta":{"tracking_token":"tok-9"}}""");
        HelpdeskRmaRequest request = new(Guid.NewGuid(), TicketId, "TCK-1", "SN-100", null, Goodwill: true, "Does not boot", time.GetUtcNow().UtcDateTime);

        var result = await Create().EscalateToRmaAsync(request, CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Sent, result.Status);
        Assert.Equal("tok-9", result.TrackingToken);
        var (route, requestBody) = Assert.Single(sent);
        Assert.Equal("esm/tickets/11111111-1111-1111-1111-111111111111/escalate/rma", route);
        using var json = JsonDocument.Parse(requestBody);
        Assert.Equal("SN-100", json.RootElement.GetProperty("serial_number").GetString());
        Assert.True(json.RootElement.GetProperty("goodwill").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("equipment_passport_id", out _));
    }

    /// <summary>
    /// An RMA the server cannot open (no unit, closed ticket) is a conflict with the reason of the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task EscalateToRma_RefusedByTheServer_IsAConflict()
    {
        reply = new HelpdeskApiReply(HttpStatusCode.UnprocessableEntity, """{"message":"The RMA needs a unit."}""");
        HelpdeskRmaRequest request = new(Guid.NewGuid(), TicketId, "TCK-1", null, null, Goodwill: false, null, time.GetUtcNow().UtcDateTime);

        var result = await Create().EscalateToRmaAsync(request, CancellationToken.None);

        Assert.Equal(HelpdeskActionStatus.Conflict, result.Status);
        Assert.Equal("The RMA needs a unit.", result.Detail);
        _ = Assert.Single(await store.ConflictsAsync(CancellationToken.None));
    }

    /// <summary>
    /// The workbench lists the breached tickets first, then by risk, and caches the answer.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadWorkbench_Online_SortsByUrgencyAndCaches()
    {
        _ = api.Setup(a => a.GetBreachRiskAsync(75, Any)).ReturnsAsync(BreachRisk);

        var workbench = await Create().LoadWorkbenchAsync(75, CancellationToken.None);

        Assert.False(workbench.FromCache);
        DssTestData.SameStrings(["TCK-2", "TCK-3", "TCK-1"], workbench.Tickets.Select(ticket => ticket.TicketNumber));
        Assert.Equal(time.GetUtcNow().UtcDateTime, workbench.SyncedAtUtc);
        Assert.NotNull(await store.FindCacheAsync("breach-risk", CancellationToken.None));
    }

    /// <summary>
    /// Offline, the workbench shows the last cached list; with nothing cached it is empty and never synced.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadWorkbench_Offline_UsesTheCacheOrIsEmpty()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var service = Create();
        var empty = await service.LoadWorkbenchAsync(75, CancellationToken.None);
        await store.SaveCacheAsync("breach-risk", BreachRisk, time.GetUtcNow().UtcDateTime.AddHours(-1), CancellationToken.None);

        var cached = await service.LoadWorkbenchAsync(75, CancellationToken.None);

        Assert.Empty(empty.Tickets);
        Assert.Null(empty.SyncedAtUtc);
        Assert.True(cached.FromCache);
        Assert.Equal(3, cached.Tickets.Count);
        Assert.Equal(time.GetUtcNow().UtcDateTime.AddHours(-1), cached.SyncedAtUtc);
    }

    /// <summary>
    /// A server failure or a malformed answer never replaces a good cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadWorkbench_BadAnswer_KeepsTheGoodCache()
    {
        await store.SaveCacheAsync("breach-risk", BreachRisk, time.GetUtcNow().UtcDateTime.AddHours(-1), CancellationToken.None);
        _ = api.Setup(a => a.GetBreachRiskAsync(75, Any)).ReturnsAsync("{not json");

        var workbench = await Create().LoadWorkbenchAsync(75, CancellationToken.None);

        Assert.True(workbench.FromCache);
        Assert.Equal(3, workbench.Tickets.Count);
        Assert.Equal(BreachRisk, (await store.FindCacheAsync("breach-risk", CancellationToken.None))?.Json);
    }

    /// <summary>
    /// Loading the workbench first sends what was queued offline, and lists what is still waiting or refused.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadWorkbench_SendsTheQueueFirstAndListsPendingAndConflicts()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var service = Create();
        _ = await service.LogTimeAsync(TimeRequest(15), CancellationToken.None);
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.GetBreachRiskAsync(75, Any)).ReturnsAsync(BreachRisk);
        reply = new HelpdeskApiReply(HttpStatusCode.UnprocessableEntity, """{"message":"Closed"}""");

        var workbench = await service.LoadWorkbenchAsync(75, CancellationToken.None);

        Assert.Empty(workbench.Pending);
        var conflict = Assert.Single(workbench.Conflicts);
        Assert.Equal("Closed", conflict.Error);
        Assert.Equal(HelpdeskAction.LogTime, conflict.Action);
        Assert.Equal("TCK-1", conflict.TicketNumber);
    }

    /// <summary>
    /// The metrics are cached per period and grouping.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadMetrics_CachesPerGroupingAndFallsBackOffline()
    {
        var from = new DateOnly(2027, 1, 1);
        var to = new DateOnly(2027, 1, 31);
        _ = api.Setup(a => a.GetMetricsAsync(from, to, "queue", Any)).ReturnsAsync("""{"group_by":"queue","groups":[{"key":"L1","tickets":2}]}""");
        var service = Create();
        var online = await service.LoadMetricsAsync(from, to, "queue", CancellationToken.None);

        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var offline = await service.LoadMetricsAsync(from, to, "queue", CancellationToken.None);
        var otherGrouping = await service.LoadMetricsAsync(from, to, "priority", CancellationToken.None);

        Assert.NotNull(online);
        Assert.False(online.FromCache);
        Assert.NotNull(offline);
        Assert.True(offline.FromCache);
        Assert.Equal(2, offline.Data.TotalTickets);
        Assert.Null(otherGrouping);
    }

    /// <summary>
    /// The customer context and the escalation history fall back to the cache of the ticket offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadCustomerContextAndEscalations_FallBackToTheCacheOffline()
    {
        _ = api.Setup(a => a.GetCustomerContextAsync(TicketId, Any)).ReturnsAsync("""{"contracts":[{"name":"Gold","remaining_hours":"3"}]}""");
        _ = api.Setup(a => a.GetEscalationsAsync(TicketId, Any)).ReturnsAsync("""[{"target":"response","action":"notify","outcome":"sent","fired_at":"2027-01-10T09:00:00Z"}]""");
        var service = Create();
        _ = await service.LoadCustomerContextAsync(TicketId, CancellationToken.None);
        _ = await service.LoadEscalationsAsync(TicketId, CancellationToken.None);

        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var context = await service.LoadCustomerContextAsync(TicketId, CancellationToken.None);
        var escalations = await service.LoadEscalationsAsync(TicketId, CancellationToken.None);
        var otherTicket = await service.LoadCustomerContextAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.NotNull(context);
        Assert.True(context.FromCache);
        Assert.Equal(3m, context.Data.RemainingContractHours);
        Assert.NotNull(escalations);
        Assert.Equal("notify", Assert.Single(escalations.Data).Action);
        Assert.Null(otherTicket);
    }

    /// <summary>
    /// The knowledge base search passes the locale and the text, caches per search and trims the old searches.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SearchKnowledge_PassesTheLocaleAndCachesPerSearch()
    {
        _ = api.Setup(a => a.SearchKnowledgeAsync("router", "de-CH", Any)).ReturnsAsync("""{"data":[{"id":"a1","title":"Router zuruecksetzen"}]}""");
        var service = Create();

        var online = await service.SearchKnowledgeAsync("  router ", "de-CH", CancellationToken.None);
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        var offline = await service.SearchKnowledgeAsync("router", "de-CH", CancellationToken.None);
        var neverMade = await service.SearchKnowledgeAsync("vpn", "de-CH", CancellationToken.None);

        Assert.NotNull(online);
        Assert.False(online.FromCache);
        Assert.Equal("Router zuruecksetzen", Assert.Single(online.Data).Title);
        Assert.NotNull(offline);
        Assert.True(offline.FromCache);
        Assert.Null(neverMade);
        api.Verify(a => a.SearchKnowledgeAsync("router", "de-CH", Any), Times.Once);
        Assert.Equal(1, store.TrimCalls);
    }

    private static HelpdeskTimeLogRequest TimeRequest(int minutes) =>
        new(Guid.NewGuid(), TicketId, "TCK-1", minutes, null, "Replaced the toner", IsBillable: true, new DateTime(2027, 1, 10, 9, 0, 0, DateTimeKind.Utc));

    private HelpdeskService Create() => new(api.Object, store, tenant.Object, time);

    private sealed class InMemoryHelpdeskStore : IHelpdeskStore
    {
        private readonly List<LocalHelpdeskCache> cache = [];

        public List<HelpdeskOutboxEntry> Entries { get; } = [];

        public int TrimCalls { get; private set; }

        public Task<LocalHelpdeskCache?> FindCacheAsync(string key, CancellationToken cancellationToken) =>
            Task.FromResult(cache.FirstOrDefault(row => string.Equals(row.CacheKey, key, StringComparison.Ordinal)));

        public Task SaveCacheAsync(string key, string json, DateTime syncedAtUtc, CancellationToken cancellationToken)
        {
            _ = cache.RemoveAll(row => string.Equals(row.CacheKey, key, StringComparison.Ordinal));
            cache.Add(new LocalHelpdeskCache { CacheKey = key, Json = json, SyncedAtUtc = syncedAtUtc });

            return Task.CompletedTask;
        }

        public Task TrimCacheAsync(string prefix, int keep, CancellationToken cancellationToken)
        {
            TrimCalls++;

            return Task.CompletedTask;
        }

        public Task EnqueueAsync(HelpdeskOutboxEntry entry, CancellationToken cancellationToken)
        {
            Entries.Add(entry);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<HelpdeskOutboxEntry>> PendingAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<HelpdeskOutboxEntry>>([.. Entries.Where(e => e.Status == HelpdeskOutboxStatus.Pending).OrderBy(e => e.OccurredAtUtc)]);

        public Task<IReadOnlyList<HelpdeskOutboxEntry>> ConflictsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<HelpdeskOutboxEntry>>([.. Entries.Where(e => e.Status == HelpdeskOutboxStatus.Conflict)]);

        public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
