// <copyright file="AiCopilotServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.AiCopilot;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.AiCopilot;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the AI copilot cache refresh, the offline decision queue and its flush, on in-memory SQLite.
/// </summary>
public sealed class AiCopilotServiceTests : IDisposable
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly FakeAiApiClient api = new();
    private readonly AiCopilotService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiCopilotServiceTests"/> class.
    /// </summary>
    public AiCopilotServiceTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        service = new AiCopilotService(api, new AiCopilotStore(db, tenant), TimeProvider.System);
    }

    /// <summary>
    /// The confidence bands start at 70% (medium) and 90% (high).
    /// </summary>
    [Fact]
    public void Confidence_BandsFollowThresholds()
    {
        Assert.Equal(AiConfidenceBand.High, AiConfidence.BandOf(0.90));
        Assert.Equal(AiConfidenceBand.High, AiConfidence.BandOf(1.0));
        Assert.Equal(AiConfidenceBand.Medium, AiConfidence.BandOf(0.89));
        Assert.Equal(AiConfidenceBand.Medium, AiConfidence.BandOf(0.70));
        Assert.Equal(AiConfidenceBand.Low, AiConfidence.BandOf(0.69));
        Assert.Equal(AiConfidenceBand.Low, AiConfidence.BandOf(0));
    }

    /// <summary>
    /// A refresh caches the open queue, orders it by urgency and stores the insights.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Refresh_CachesSuggestionsAndInsights()
    {
        var low = Dto(priority: "low", confidence: 0.95);
        var high = Dto(priority: "high", confidence: 0.60);
        api.Queue = [low, high];
        api.Insights = new AiInsightsDto(
            new AiCashFlowDto("CHF", 30, ["2026-11-21"], [10m], [20m], [30m]),
            PaymentRiskCount: 2,
            Recommendations: [],
            Anomalies: [Dto(kind: "anomaly")]);

        var result = await service.RefreshAsync(CancellationToken.None);

        Assert.Equal(AiRefreshStatus.Ok, result.Status);
        Assert.Equal([high.Id, low.Id], result.Snapshot.Suggestions.Select(s => s.Id));
        Assert.NotNull(result.Snapshot.Insights);
        Assert.Equal("CHF", result.Snapshot.Insights.CashFlow?.Currency);
        _ = Assert.Single(result.Snapshot.Insights.Anomalies);
        _ = Assert.NotNull(result.Snapshot.InsightsFetchedAt);

        // The cache survives without the server.
        api.ReadFailure = 0;
        var offline = await service.LoadAsync(CancellationToken.None);
        Assert.Equal(2, offline.Suggestions.Count);
    }

    /// <summary>
    /// A refresh drops suggestions that were decided elsewhere, and pruning never touches a row with a queued decision.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Refresh_RemovesStaleRows_ButPruningKeepsQueued()
    {
        var kept = Dto();
        var stale = Dto();
        var queued = Dto();
        api.Queue = [kept, stale, queued];
        _ = await service.RefreshAsync(CancellationToken.None);
        api.IsAuthenticated = false;
        _ = await service.DecideAsync(queued.Id, new AiDecisionRequest(Approve: true, Text: "ok"), CancellationToken.None);
        api.IsAuthenticated = true;

        // Pruning in isolation (a decision queued between the flush and the pull): the queued row survives.
        await new AiCopilotStore(db, tenant).RemoveMissingAsync(new HashSet<Guid> { kept.Id }, CancellationToken.None);
        db.ChangeTracker.Clear();
        var afterPrune = await service.LoadAsync(CancellationToken.None);
        Assert.Equal(2, afterPrune.Suggestions.Count);
        Assert.Contains(afterPrune.Suggestions, s => s.Id == queued.Id && s.QueuedDecision == AiQueuedDecision.Approve);

        // A full refresh flushes the decision first and then mirrors the server queue.
        api.Queue = [kept];
        var synced = await service.RefreshAsync(CancellationToken.None);

        Assert.Equal(AiRefreshStatus.Ok, synced.Status);
        Assert.Equal(1, synced.Flush.Sent);
        Assert.Equal([kept.Id], synced.Snapshot.Suggestions.Select(s => s.Id));
        Assert.Equal(0, synced.Snapshot.QueuedCount);
    }

    /// <summary>
    /// A 403 reports the forbidden state and leaves the cache untouched.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Refresh_Forbidden_KeepsCache()
    {
        api.Queue = [Dto()];
        _ = await service.RefreshAsync(CancellationToken.None);

        api.ReadFailure = 403;
        var result = await service.RefreshAsync(CancellationToken.None);

        Assert.Equal(AiRefreshStatus.Forbidden, result.Status);
        _ = Assert.Single(result.Snapshot.Suggestions);
    }

    /// <summary>
    /// A network failure or a missing session is reported without losing the cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Refresh_NetworkDownOrSignedOut_ReturnsCachedData()
    {
        api.Queue = [Dto()];
        _ = await service.RefreshAsync(CancellationToken.None);

        api.ReadFailure = 0;
        var offline = await service.RefreshAsync(CancellationToken.None);
        api.ReadFailure = null;
        api.IsAuthenticated = false;
        var signedOut = await service.RefreshAsync(CancellationToken.None);

        Assert.Equal(AiRefreshStatus.Offline, offline.Status);
        Assert.Equal(AiRefreshStatus.AuthRequired, signedOut.Status);
        _ = Assert.Single(offline.Snapshot.Suggestions);
        _ = Assert.Single(signedOut.Snapshot.Suggestions);
    }

    /// <summary>
    /// An online approval is sent with the edited payload and the suggestion leaves the queue.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Decide_Online_SendsAndRemoves()
    {
        var suggestion = Dto();
        api.Queue = [suggestion];
        _ = await service.RefreshAsync(CancellationToken.None);

        var result = await service.DecideAsync(suggestion.Id, new AiDecisionRequest(Approve: true, Text: "  fine  ", """{"amount":5}"""), CancellationToken.None);

        Assert.Equal(AiDecisionStatus.Sent, result.Status);
        Assert.Empty(result.Snapshot.Suggestions);
        var sent = Assert.Single(api.Sent);
        Assert.Equal(("approve", suggestion.Id, "fine", """{"amount":5}"""), sent);
    }

    /// <summary>
    /// Without a connection the decision is queued locally, keeps the suggestion visible and is counted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Decide_Offline_QueuesDecision()
    {
        var suggestion = Dto();
        api.Queue = [suggestion];
        _ = await service.RefreshAsync(CancellationToken.None);
        api.IsAuthenticated = false;

        var result = await service.DecideAsync(suggestion.Id, new AiDecisionRequest(Approve: false, Text: "not needed"), CancellationToken.None);

        Assert.Equal(AiDecisionStatus.Queued, result.Status);
        Assert.Equal(1, result.Snapshot.QueuedCount);
        Assert.Empty(api.Sent);
        var row = Assert.Single(result.Snapshot.Suggestions);
        Assert.Equal(AiQueuedDecision.Reject, row.QueuedDecision);
        Assert.Equal("not needed", row.QueuedFeedback);
        _ = Assert.NotNull(row.QueuedAt);
    }

    /// <summary>
    /// A network failure while sending queues the decision instead of losing it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Decide_TransientFailure_QueuesDecision()
    {
        var suggestion = Dto();
        api.Queue = [suggestion];
        _ = await service.RefreshAsync(CancellationToken.None);
        api.Outcomes[suggestion.Id] = new AiDecisionOutcome(AiApiStatus.Transient, "timeout");

        var result = await service.DecideAsync(suggestion.Id, new AiDecisionRequest(Approve: true, Text: null), CancellationToken.None);

        Assert.Equal(AiDecisionStatus.Queued, result.Status);
        Assert.Equal(1, result.Snapshot.QueuedCount);
    }

    /// <summary>
    /// A 409 removes the suggestion from the queue, a 404 as well.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Decide_AlreadyDecidedOrUnknown_RemovesSuggestion()
    {
        var decided = Dto();
        var unknown = Dto();
        api.Queue = [decided, unknown];
        _ = await service.RefreshAsync(CancellationToken.None);
        api.Outcomes[decided.Id] = new AiDecisionOutcome(AiApiStatus.AlreadyDecided, "already");
        api.Outcomes[unknown.Id] = new AiDecisionOutcome(AiApiStatus.NotFound, "missing");

        var first = await service.DecideAsync(decided.Id, new AiDecisionRequest(Approve: true, Text: null), CancellationToken.None);
        var second = await service.DecideAsync(unknown.Id, new AiDecisionRequest(Approve: true, Text: null), CancellationToken.None);

        Assert.Equal(AiDecisionStatus.AlreadyDecided, first.Status);
        Assert.Equal(AiDecisionStatus.NotFound, second.Status);
        Assert.Empty(second.Snapshot.Suggestions);
        Assert.Equal(0, second.Snapshot.QueuedCount);
    }

    /// <summary>
    /// A 422 keeps the suggestion pending, does not queue it and exposes the server explanation.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Decide_Incomplete_KeepsPendingAndShowsDetail()
    {
        var suggestion = Dto();
        api.Queue = [suggestion];
        _ = await service.RefreshAsync(CancellationToken.None);
        api.Outcomes[suggestion.Id] = new AiDecisionOutcome(AiApiStatus.Incomplete, "Customer is missing.");

        var result = await service.DecideAsync(suggestion.Id, new AiDecisionRequest(Approve: true, Text: null), CancellationToken.None);

        Assert.Equal(AiDecisionStatus.Incomplete, result.Status);
        Assert.Equal("Customer is missing.", result.Detail);
        Assert.Equal(0, result.Snapshot.QueuedCount);
        var row = Assert.Single(result.Snapshot.Suggestions);
        Assert.Equal("Customer is missing.", row.LastError);
        Assert.Equal(AiQueuedDecision.None, row.QueuedDecision);
    }

    /// <summary>
    /// Deciding an unknown suggestion reports that it was not found.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Decide_UnknownLocally_ReportsNotFound()
    {
        var result = await service.DecideAsync(Guid.NewGuid(), new AiDecisionRequest(Approve: true, Text: null), CancellationToken.None);

        Assert.Equal(AiDecisionStatus.NotFound, result.Status);
        Assert.Empty(api.Sent);
    }

    /// <summary>
    /// The flush pushes the queue oldest first with the stored feedback and empties it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_PushesQueuedDecisionsInOrder()
    {
        var first = Dto();
        var second = Dto();
        api.Queue = [first, second];
        _ = await service.RefreshAsync(CancellationToken.None);
        api.IsAuthenticated = false;
        _ = await service.DecideAsync(first.Id, new AiDecisionRequest(Approve: true, Text: "yes", """{"a":1}"""), CancellationToken.None);
        _ = await service.DecideAsync(second.Id, new AiDecisionRequest(Approve: false, Text: "too risky"), CancellationToken.None);
        api.IsAuthenticated = true;

        var flush = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(2, flush.Sent);
        Assert.Equal(0, flush.Remaining);
        Assert.Equal([("approve", first.Id, "yes", """{"a":1}"""), ("reject", second.Id, "too risky", null)], api.Sent);
        Assert.Empty((await service.LoadAsync(CancellationToken.None)).Suggestions);
    }

    /// <summary>
    /// A failed flush keeps the decision queued, counts the attempt and stops before the next decisions.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_NetworkFailure_KeepsDecisionQueued()
    {
        var first = Dto();
        var second = Dto();
        api.Queue = [first, second];
        _ = await service.RefreshAsync(CancellationToken.None);
        api.IsAuthenticated = false;
        _ = await service.DecideAsync(first.Id, new AiDecisionRequest(Approve: true, Text: null), CancellationToken.None);
        _ = await service.DecideAsync(second.Id, new AiDecisionRequest(Approve: true, Text: null), CancellationToken.None);
        api.IsAuthenticated = true;
        api.Outcomes[first.Id] = new AiDecisionOutcome(AiApiStatus.Transient, "no route");

        var flush = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(0, flush.Sent);
        Assert.Equal(2, flush.Remaining);
        _ = Assert.Single(api.Sent);
        var queued = await db.Set<AiSuggestion>().Where(s => s.Id == first.Id).SingleAsync();
        Assert.Equal(AiQueuedDecision.Approve, queued.QueuedDecision);
        Assert.Equal(1, queued.QueuedAttempts);
        Assert.Equal("no route", queued.LastError);
    }

    /// <summary>
    /// A 409 drops the queued decision, a 422 returns the suggestion to pending with the explanation, a 404 drops it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_ConflictAndIncomplete_AreResolved()
    {
        var decided = Dto();
        var incomplete = Dto();
        var gone = Dto();
        api.Queue = [decided, incomplete, gone];
        _ = await service.RefreshAsync(CancellationToken.None);
        api.IsAuthenticated = false;
        foreach (var id in new[] { decided.Id, incomplete.Id, gone.Id })
        {
            _ = await service.DecideAsync(id, new AiDecisionRequest(Approve: true, Text: null), CancellationToken.None);
        }

        api.IsAuthenticated = true;
        api.Outcomes[decided.Id] = new AiDecisionOutcome(AiApiStatus.AlreadyDecided, "already");
        api.Outcomes[incomplete.Id] = new AiDecisionOutcome(AiApiStatus.Incomplete, "Missing customer.");
        api.Outcomes[gone.Id] = new AiDecisionOutcome(AiApiStatus.NotFound, "gone");

        var flush = await service.FlushAsync(CancellationToken.None);

        Assert.Equal(0, flush.Sent);
        Assert.Equal(2, flush.Dropped);
        Assert.Equal(1, flush.Refused);
        Assert.Equal(0, flush.Remaining);
        var left = Assert.Single((await service.LoadAsync(CancellationToken.None)).Suggestions);
        Assert.Equal(incomplete.Id, left.Id);
        Assert.Equal("Missing customer.", left.LastError);
        Assert.Equal(AiQueuedDecision.None, left.QueuedDecision);
    }

    /// <summary>
    /// An expired session during the flush asks for a new sign-in and keeps the queue.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_Unauthorized_RequiresSignIn()
    {
        var suggestion = Dto();
        api.Queue = [suggestion];
        _ = await service.RefreshAsync(CancellationToken.None);
        api.IsAuthenticated = false;
        _ = await service.DecideAsync(suggestion.Id, new AiDecisionRequest(Approve: true, Text: null), CancellationToken.None);
        api.IsAuthenticated = true;
        api.Outcomes[suggestion.Id] = new AiDecisionOutcome(AiApiStatus.Unauthorized, "expired");

        var refresh = await service.RefreshAsync(CancellationToken.None);

        Assert.Equal(AiRefreshStatus.AuthRequired, refresh.Status);
        Assert.Equal(1, refresh.Snapshot.QueuedCount);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    private static AiSuggestionDto Dto(string kind = "sales_quote", string priority = "normal", double confidence = 0.8) => new(
        Guid.NewGuid(),
        kind,
        "workflow",
        "email",
        "ref-" + Guid.NewGuid().ToString("N")[..6],
        SourceText: null,
        ProposedPayloadJson: JsonSerializer.Serialize(new { customer = "ACME" }),
        FieldConfidencesJson: "{}",
        confidence,
        Amount: 120.5m,
        Reasoning: "Looks right",
        Status: "pending",
        priority,
        ProviderUsed: null,
        ModelUsed: null,
        ExpiresAt: null,
        CreatedAt: DateTime.UtcNow);
}
