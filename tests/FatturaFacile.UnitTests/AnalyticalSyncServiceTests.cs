// <copyright file="AnalyticalSyncServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using FatturaFacile.Application.AnalyticalAccounting;
using FatturaFacile.Application.Features.AnalyticalAccounting;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the analytical sync: the offline queue flush, the atomic cache replacement, the offline fallback and the handlers.
/// </summary>
public sealed class AnalyticalSyncServiceTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTimeOffset Now = new(2027, 3, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IAnalyticalApiClient> api = new();
    private readonly Mock<IAnalyticalSnapshotStore> store = new();
    private readonly Mock<IAnalyticalOperationQueue> queue = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticalSyncServiceTests"/> class.
    /// </summary>
    public AnalyticalSyncServiceTests()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.GetCentersAsync(Any)).ReturnsAsync(AnalyticalTestData.Centers);
        _ = api.Setup(a => a.GetRunsAsync(Any)).ReturnsAsync(AnalyticalTestData.Runs);
        _ = api.Setup(a => a.GetMarginAsync(It.IsAny<string?>(), It.IsAny<string?>(), Any)).ReturnsAsync(AnalyticalTestData.Margin);
        _ = api.Setup(a => a.GetComparisonAsync(It.IsAny<string?>(), It.IsAny<string?>(), Any)).ReturnsAsync(AnalyticalTestData.Comparison);
        _ = queue.Setup(q => q.PendingAsync(Any)).ReturnsAsync([]);
    }

    /// <summary>
    /// A successful sync stores the four read models stamped with the clock.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Online_StoresTheFourReadModels()
    {
        var result = await Create().SyncAsync("2027-03-01", "2027-03-31", CancellationToken.None);

        Assert.Equal(AnalyticalSyncStatus.Synced, result.Status);
        Assert.Equal(Now.UtcDateTime, result.SyncedAtUtc);
        foreach (var key in new[] { AnalyticalContract.CentersKey, AnalyticalContract.RunsKey, AnalyticalContract.MarginKey, AnalyticalContract.ComparisonKey })
        {
            store.Verify(s => s.SaveAsync(key, It.IsAny<string>(), Now.UtcDateTime, Any), Times.Once);
        }

        api.Verify(a => a.GetMarginAsync("2027-03-01", "2027-03-31", Any), Times.Once);
    }

    /// <summary>
    /// Without a session nothing is requested.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WithoutSession_IsUnauthorizedAndAsksNothing()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        var result = await Create().SyncAsync(from: null, to: null, CancellationToken.None);

        Assert.Equal(AnalyticalSyncStatus.Unauthorized, result.Status);
        api.Verify(a => a.GetCentersAsync(Any), Times.Never);
        queue.Verify(q => q.PendingAsync(Any), Times.Never);
    }

    /// <summary>
    /// Queued entries are sent first: an accepted one is marked sent, a refused one is parked, an unreachable server leaves it pending.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_FlushesTheQueueBeforePulling()
    {
        PendingAnalyticalOperation accepted = new(Guid.NewGuid(), "{\"ok\":1}");
        PendingAnalyticalOperation refused = new(Guid.NewGuid(), "{\"bad\":1}");
        PendingAnalyticalOperation unreachable = new(Guid.NewGuid(), "{\"later\":1}");
        _ = queue.Setup(q => q.PendingAsync(Any)).ReturnsAsync([accepted, refused, unreachable]);
        _ = api.Setup(a => a.PostEntryAsync(accepted.PayloadJson, Any)).ReturnsAsync(new AnalyticalPostReply(Accepted: true, Refused: false));
        _ = api.Setup(a => a.PostEntryAsync(refused.PayloadJson, Any)).ReturnsAsync(new AnalyticalPostReply(Accepted: false, Refused: true, "Period consolidated"));
        _ = api.Setup(a => a.PostEntryAsync(unreachable.PayloadJson, Any)).ReturnsAsync(new AnalyticalPostReply(Accepted: false, Refused: false));

        var result = await Create().SyncAsync(from: null, to: null, CancellationToken.None);

        Assert.Equal(1, result.SentOperations);
        Assert.Equal(1, result.FailedOperations);
        queue.Verify(q => q.MarkSentAsync(accepted.Id, Any), Times.Once);
        queue.Verify(q => q.MarkFailedAsync(refused.Id, "Period consolidated", Any), Times.Once);
        queue.Verify(q => q.MarkSentAsync(unreachable.Id, Any), Times.Never);
        queue.Verify(q => q.MarkFailedAsync(unreachable.Id, It.IsAny<string?>(), Any), Times.Never);
    }

    /// <summary>
    /// A server that cannot be reached, or a timeout, is offline and leaves the cache untouched.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_ServerUnreachable_IsOfflineAndSavesNothing()
    {
        _ = api.Setup(a => a.GetRunsAsync(Any)).ThrowsAsync(new HttpRequestException("reset"));

        Assert.Equal(AnalyticalSyncStatus.Offline, (await Create().SyncAsync(from: null, to: null, CancellationToken.None)).Status);

        _ = api.Setup(a => a.GetCentersAsync(Any)).ThrowsAsync(new TaskCanceledException("timeout"));

        Assert.Equal(AnalyticalSyncStatus.Offline, (await Create().SyncAsync(from: null, to: null, CancellationToken.None)).Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A refusal is classified, and any other error is a failure.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <param name="expected">The expected sync status.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, AnalyticalSyncStatus.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, AnalyticalSyncStatus.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError, AnalyticalSyncStatus.Failed)]
    public async Task Sync_HttpError_IsClassified(HttpStatusCode status, AnalyticalSyncStatus expected)
    {
        _ = api.Setup(a => a.GetCentersAsync(Any)).ThrowsAsync(new HttpRequestException("refused", inner: null, status));

        var result = await Create().SyncAsync(from: null, to: null, CancellationToken.None);

        Assert.Equal(expected, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A payload that does not parse never replaces a good cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_UnparsablePayload_FailsAndSavesNothing()
    {
        _ = api.Setup(a => a.GetMarginAsync(It.IsAny<string?>(), It.IsAny<string?>(), Any)).ReturnsAsync("garbage");

        var result = await Create().SyncAsync(from: null, to: null, CancellationToken.None);

        Assert.Equal(AnalyticalSyncStatus.Failed, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// The overview reads the cache and the queue only, and is empty before the first sync.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Overview_ReadsTheCacheOnly()
    {
        var syncedAt = new DateTime(2027, 3, 1, 8, 0, 0, DateTimeKind.Utc);
        _ = queue.Setup(q => q.CountPendingAsync(Any)).ReturnsAsync(2);
        GetAnalyticalOverviewQueryHandler handler = new(store.Object, queue.Object);

        var empty = await handler.Handle(new GetAnalyticalOverviewQuery(), CancellationToken.None);

        Assert.Empty(empty.Centers);
        Assert.Null(empty.Margin);
        Assert.Null(empty.SyncedAtUtc);
        Assert.Equal(2, empty.PendingOperations);

        _ = store.Setup(s => s.FindAsync(AnalyticalContract.CentersKey, Any)).ReturnsAsync(new StoredAnalyticalPayload(AnalyticalTestData.Centers, syncedAt));
        _ = store.Setup(s => s.FindAsync(AnalyticalContract.RunsKey, Any)).ReturnsAsync(new StoredAnalyticalPayload(AnalyticalTestData.Runs, syncedAt));
        _ = store.Setup(s => s.FindAsync(AnalyticalContract.MarginKey, Any)).ReturnsAsync(new StoredAnalyticalPayload(AnalyticalTestData.Margin, syncedAt));
        _ = store.Setup(s => s.FindAsync(AnalyticalContract.ComparisonKey, Any)).ReturnsAsync(new StoredAnalyticalPayload(AnalyticalTestData.Comparison, syncedAt));

        var overview = await handler.Handle(new GetAnalyticalOverviewQuery(), CancellationToken.None);

        _ = Assert.Single(overview.Centers);
        _ = Assert.Single(overview.Runs);
        Assert.Equal(3350m, overview.Margin?.OperatingResult);
        _ = Assert.Single(overview.Comparison);
        Assert.Equal(syncedAt, overview.SyncedAtUtc);
        api.VerifyNoOtherCalls();
    }

    /// <summary>
    /// A valid draft is queued as the API payload; an invalid one is refused with its error codes and never queued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task QueueCommand_QueuesAValidDraftAndRefusesAnInvalidOne()
    {
        var id = Guid.NewGuid();
        _ = queue.Setup(q => q.EnqueueAsync(It.IsAny<string>(), Any)).ReturnsAsync(id);
        QueueAnalyticalEntryCommandHandler handler = new(queue.Object);
        AnalyticalEntryDraft valid = new(new DateOnly(2027, 3, 15), Description: null, [new AnalyticalEntryLineDraft(50m, "FIXED_DIRECT", CostCenterId: "it")]);
        AnalyticalEntryDraft invalid = new(new DateOnly(2027, 3, 15), Description: null, []);

        var queued = await handler.Handle(new QueueAnalyticalEntryCommand(valid), CancellationToken.None);
        var refused = await handler.Handle(new QueueAnalyticalEntryCommand(invalid), CancellationToken.None);

        Assert.True(queued.IsQueued);
        Assert.Equal(id, queued.OperationId);
        Assert.False(refused.IsQueued);
        Assert.Contains("Analytical_EntryNeedsLines", refused.Errors, StringComparer.Ordinal);
        queue.Verify(q => q.EnqueueAsync(valid.ToApiJson(), Any), Times.Once);
        queue.Verify(q => q.EnqueueAsync(It.IsAny<string>(), Any), Times.Once);
    }

    /// <summary>
    /// The sync command delegates to the sync service.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SyncCommand_DelegatesToTheService()
    {
        var sync = new Mock<IAnalyticalSyncService>();
        _ = sync.Setup(s => s.SyncAsync("2027-03-01", to: null, Any)).ReturnsAsync(new AnalyticalSyncResult(AnalyticalSyncStatus.Offline));

        var result = await new SyncAnalyticalAccountingCommandHandler(sync.Object).Handle(new SyncAnalyticalAccountingCommand("2027-03-01"), CancellationToken.None);

        Assert.Equal(AnalyticalSyncStatus.Offline, result.Status);
    }

    private AnalyticalSyncService Create() => new(api.Object, store.Object, queue.Object, new FixedTime(Now));

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
