// <copyright file="SyncEngineTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the push/pull orchestration and retry policy of the sync engine.
/// </summary>
public sealed class SyncEngineTests
{
    private static readonly DateTime Now = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISyncApiClient> client = new();
    private readonly Mock<ISyncStore> store = new();
    private readonly Mock<ISyncEntityHandler> handler = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncEngineTests"/> class.
    /// </summary>
    public SyncEngineTests()
    {
        _ = client.SetupGet(c => c.IsAuthenticated).Returns(value: true);
        _ = handler.SetupGet(h => h.EntityName).Returns("contacts");
        _ = handler.SetupGet(h => h.WriteRoute).Returns("contacts");
        _ = handler.Setup(h => h.BuildPayloadAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync("{}");
        _ = handler
            .Setup(h => h.ApplyAsync(It.IsAny<IReadOnlyList<JsonElement>>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<IReadOnlySet<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncApplyResult(Applied: 2, Skipped: 1));

        _ = store.Setup(s => s.GetDueEntriesAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _ = store.Setup(s => s.GetCheckpointAsync("contacts", It.IsAny<CancellationToken>())).ReturnsAsync((DateTime?)null);
        _ = store.Setup(s => s.GetProtectedEntityIdsAsync("contacts", It.IsAny<CancellationToken>())).ReturnsAsync(new HashSet<Guid>());

        _ = client
            .Setup(c => c.GetDeltaAsync("contacts", It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncDeltaPage(Data: [], Deleted: [], NextCursor: null, Now, HasMore: false));
    }

    /// <summary>
    /// The back-off doubles per attempt and is capped at one hour.
    /// </summary>
    [Fact]
    public void RetryPolicy_DoublesAndCaps()
    {
        Assert.Equal(TimeSpan.FromSeconds(15), SyncRetryPolicy.NextDelay(1));
        Assert.Equal(TimeSpan.FromSeconds(30), SyncRetryPolicy.NextDelay(2));
        Assert.Equal(TimeSpan.FromSeconds(60), SyncRetryPolicy.NextDelay(3));
        Assert.Equal(TimeSpan.FromHours(1), SyncRetryPolicy.NextDelay(30));
        Assert.True(SyncRetryPolicy.CanRetry(SyncRetryPolicy.MaxAttempts - 1));
        Assert.False(SyncRetryPolicy.CanRetry(SyncRetryPolicy.MaxAttempts));
    }

    /// <summary>
    /// Nothing runs while the user is not signed in.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_WithoutSession_DoesNothing()
    {
        _ = client.SetupGet(c => c.IsAuthenticated).Returns(value: false);

        var result = await CreateEngine().RunAsync(CancellationToken.None);

        Assert.True(result.AuthRequired);
        store.Verify(s => s.GetDueEntriesAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// An accepted write is marked as synced and sent with its idempotency key.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_PushSuccess_MarksEntrySynced()
    {
        var entry = Queue(SyncOperation.Create);
        _ = client.Setup(c => c.SendAsync(It.IsAny<SyncRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SyncSendOutcome(SyncSendResult.Success));

        var result = await CreateEngine().RunAsync(CancellationToken.None);

        Assert.Equal(SyncOutboxStatus.Synced, entry.Status);
        Assert.Equal(1, result.Pushed);
        client.Verify(
            c => c.SendAsync(
                It.Is<SyncRequest>(r => r.IdempotencyKey == entry.IdempotencyKey && r.Route == "contacts" && r.Operation == SyncOperation.Create),
                It.IsAny<CancellationToken>()),
            Times.Once);
        store.Verify(s => s.SaveEntryAsync(entry, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// A transient failure schedules a retry with back-off and stops the push loop.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_TransientFailure_SchedulesRetryAndStops()
    {
        var first = Queue(SyncOperation.Update);
        var second = Queue(SyncOperation.Update);
        _ = store.Setup(s => s.GetDueEntriesAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([first, second]);
        _ = client.Setup(c => c.SendAsync(It.IsAny<SyncRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SyncSendOutcome(SyncSendResult.Transient, "offline"));

        var result = await CreateEngine().RunAsync(CancellationToken.None);

        Assert.Equal(SyncOutboxStatus.Pending, first.Status);
        Assert.Equal(1, first.Attempts);
        Assert.Equal(Now + SyncRetryPolicy.NextDelay(1), first.NextAttemptAt);
        Assert.Equal(0, second.Attempts);
        _ = Assert.Single(result.Errors, e => e.Contains("offline", StringComparison.Ordinal));
        client.Verify(c => c.SendAsync(It.IsAny<SyncRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// When the attempts are exhausted the entry fails permanently.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_TransientFailure_AfterMaxAttempts_Fails()
    {
        var entry = Queue(SyncOperation.Update);
        entry.Attempts = SyncRetryPolicy.MaxAttempts - 1;
        _ = client.Setup(c => c.SendAsync(It.IsAny<SyncRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SyncSendOutcome(SyncSendResult.Transient, "offline"));

        var result = await CreateEngine().RunAsync(CancellationToken.None);

        Assert.Equal(SyncOutboxStatus.Failed, entry.Status);
        Assert.Equal(1, result.Failed);
    }

    /// <summary>
    /// A server conflict and a validation rejection are classified differently.
    /// </summary>
    /// <param name="outcome">The server outcome.</param>
    /// <param name="expected">The expected entry status.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(SyncSendResult.Conflict, SyncOutboxStatus.Conflict)]
    [InlineData(SyncSendResult.Rejected, SyncOutboxStatus.Failed)]
    public async Task Run_PermanentOutcome_SetsStatus(SyncSendResult outcome, SyncOutboxStatus expected)
    {
        var entry = Queue(SyncOperation.Update);
        _ = client.Setup(c => c.SendAsync(It.IsAny<SyncRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SyncSendOutcome(outcome, "422"));

        _ = await CreateEngine().RunAsync(CancellationToken.None);

        Assert.Equal(expected, entry.Status);
        Assert.Equal("422", entry.LastError);
    }

    /// <summary>
    /// An expired session stops the pass without consuming a retry attempt.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_Unauthorized_RequiresSignInAndSkipsPull()
    {
        var entry = Queue(SyncOperation.Create);
        _ = client.Setup(c => c.SendAsync(It.IsAny<SyncRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SyncSendOutcome(SyncSendResult.Unauthorized));

        var result = await CreateEngine().RunAsync(CancellationToken.None);

        Assert.True(result.AuthRequired);
        Assert.Equal(0, entry.Attempts);
        client.Verify(c => c.GetDeltaAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// A record deleted locally after it was queued is not sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_RecordGoneLocally_IsNotSent()
    {
        var entry = Queue(SyncOperation.Update);
        _ = handler.Setup(h => h.BuildPayloadAsync(entry.EntityId, It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

        _ = await CreateEngine().RunAsync(CancellationToken.None);

        Assert.Equal(SyncOutboxStatus.Synced, entry.Status);
        client.Verify(c => c.SendAsync(It.IsAny<SyncRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// The pull pages through the cursor, protects pending records and stores the first server time as checkpoint.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_Pull_PagesAndStoresFirstServerTimeAsCheckpoint()
    {
        var since = Now.AddHours(-1);
        var protectedId = Guid.NewGuid();
        var firstTime = Now.AddSeconds(1);
        _ = store.Setup(s => s.GetCheckpointAsync("contacts", It.IsAny<CancellationToken>())).ReturnsAsync(since);
        _ = store.Setup(s => s.GetProtectedEntityIdsAsync("contacts", It.IsAny<CancellationToken>())).ReturnsAsync(new HashSet<Guid> { protectedId });
        _ = client
            .Setup(c => c.GetDeltaAsync("contacts", since, It.Is<string?>(c => c == null), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncDeltaPage(Data: [], Deleted: [], "cursor-1", firstTime, HasMore: true));
        _ = client
            .Setup(c => c.GetDeltaAsync("contacts", since, "cursor-1", It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncDeltaPage(Data: [], Deleted: [], NextCursor: null, Now.AddSeconds(9), HasMore: false));

        var result = await CreateEngine().RunAsync(CancellationToken.None);

        Assert.Equal(4, result.Pulled);
        Assert.Equal(2, result.SkippedPulls);
        handler.Verify(
            h => h.ApplyAsync(It.IsAny<IReadOnlyList<JsonElement>>(), It.IsAny<IReadOnlyList<Guid>>(), It.Is<IReadOnlySet<Guid>>(p => p.Contains(protectedId)), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        store.Verify(s => s.SaveCheckpointAsync("contacts", firstTime, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// A network failure while pulling is reported and does not move the checkpoint.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_PullNetworkFailure_IsReportedWithoutCheckpoint()
    {
        _ = client
            .Setup(c => c.GetDeltaAsync("contacts", It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("offline"));

        var result = await CreateEngine().RunAsync(CancellationToken.None);

        _ = Assert.Single(result.Errors);
        store.Verify(s => s.SaveCheckpointAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static SyncOutboxEntry NewEntry(SyncOperation operation)
        => new() { EntityName = "contacts", EntityId = Guid.NewGuid(), Operation = operation, NextAttemptAt = Now };

    private SyncEngine CreateEngine() => new(client.Object, store.Object, [handler.Object], new FixedTimeProvider(Now));

    private SyncOutboxEntry Queue(SyncOperation operation)
    {
        var entry = NewEntry(operation);
        _ = store.Setup(s => s.GetDueEntriesAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([entry]);
        return entry;
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
