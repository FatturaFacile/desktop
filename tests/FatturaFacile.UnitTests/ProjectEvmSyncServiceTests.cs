// <copyright file="ProjectEvmSyncServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using FatturaFacile.Application.Features.ProjectEvm;
using FatturaFacile.Application.ProjectEvm;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the profitability sync: the atomic save, the offline fallback, the classification of failures and the handlers.
/// </summary>
public sealed class ProjectEvmSyncServiceTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTimeOffset Now = new(2027, 3, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IProjectEvmApiClient> api = new();
    private readonly Mock<IProjectEvmSnapshotStore> store = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectEvmSyncServiceTests"/> class.
    /// </summary>
    public ProjectEvmSyncServiceTests()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.GetBreakdownAsync(ProjectEvmTestData.ProjectId, Any)).ReturnsAsync(ProjectEvmTestData.Breakdown);
        _ = api.Setup(a => a.GetWaterfallAsync(ProjectEvmTestData.ProjectId, Any)).ReturnsAsync(ProjectEvmTestData.Waterfall);
        _ = api.Setup(a => a.GetScurveAsync(ProjectEvmTestData.ProjectId, Any)).ReturnsAsync(ProjectEvmTestData.Scurve);
        _ = api.Setup(a => a.GetProjectsAsync(Any)).ReturnsAsync(ProjectEvmTestData.Projects);
    }

    /// <summary>
    /// A successful pull stores the three views in one save, stamped with the clock.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Online_StoresTheThreeViewsInOneSave()
    {
        var result = await Create().SyncAsync(ProjectEvmTestData.ProjectId, CancellationToken.None);

        Assert.Equal(ProjectEvmSyncStatus.Synced, result.Status);
        Assert.Equal(Now.UtcDateTime, result.SyncedAtUtc);
        store.Verify(s => s.SaveAsync(ProjectEvmTestData.ProjectId, It.Is<ProjectEvmPayload>(p => string.Equals(p.BreakdownJson, ProjectEvmTestData.Breakdown, StringComparison.Ordinal) && string.Equals(p.ScurveJson, ProjectEvmTestData.Scurve, StringComparison.Ordinal)), Now.UtcDateTime, Any), Times.Once);
    }

    /// <summary>
    /// Without a session nothing is requested and the user is asked to sign in.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WithoutSession_IsUnauthorizedAndAsksNothing()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        var result = await Create().SyncAsync(ProjectEvmTestData.ProjectId, CancellationToken.None);

        Assert.Equal(ProjectEvmSyncStatus.Unauthorized, result.Status);
        api.Verify(a => a.GetBreakdownAsync(It.IsAny<string>(), Any), Times.Never);
        Assert.Equal(ProjectEvmSyncStatus.Unauthorized, (await Create().SyncProjectsAsync(CancellationToken.None)).Status);
    }

    /// <summary>
    /// A server that cannot be reached, or a connection lost half way, leaves the cache untouched and reports offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_ServerUnreachable_IsOfflineAndSavesNothing()
    {
        _ = api.Setup(a => a.GetScurveAsync(ProjectEvmTestData.ProjectId, Any)).ThrowsAsync(new HttpRequestException("reset"));

        var result = await Create().SyncAsync(ProjectEvmTestData.ProjectId, CancellationToken.None);

        Assert.Equal(ProjectEvmSyncStatus.Offline, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<ProjectEvmPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A timeout is offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Timeout_IsOffline()
    {
        _ = api.Setup(a => a.GetBreakdownAsync(ProjectEvmTestData.ProjectId, Any)).ThrowsAsync(new TaskCanceledException("timeout"));

        Assert.Equal(ProjectEvmSyncStatus.Offline, (await Create().SyncAsync(ProjectEvmTestData.ProjectId, CancellationToken.None)).Status);
    }

    /// <summary>
    /// A refusal is classified, and any other error is a failure.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <param name="expected">The expected sync status.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, ProjectEvmSyncStatus.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, ProjectEvmSyncStatus.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError, ProjectEvmSyncStatus.Failed)]
    public async Task Sync_HttpError_IsClassified(HttpStatusCode status, ProjectEvmSyncStatus expected)
    {
        _ = api.Setup(a => a.GetBreakdownAsync(ProjectEvmTestData.ProjectId, Any)).ThrowsAsync(new HttpRequestException("refused", inner: null, status));

        var result = await Create().SyncAsync(ProjectEvmTestData.ProjectId, CancellationToken.None);

        Assert.Equal(expected, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<ProjectEvmPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A payload that does not parse never replaces a good cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_UnparsablePayload_FailsAndSavesNothing()
    {
        _ = api.Setup(a => a.GetWaterfallAsync(ProjectEvmTestData.ProjectId, Any)).ReturnsAsync("garbage");

        var result = await Create().SyncAsync(ProjectEvmTestData.ProjectId, CancellationToken.None);

        Assert.Equal(ProjectEvmSyncStatus.Failed, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<ProjectEvmPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// The project picker is pulled and stored.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SyncProjects_StoresThePicker()
    {
        var result = await Create().SyncProjectsAsync(CancellationToken.None);

        Assert.Equal(ProjectEvmSyncStatus.Synced, result.Status);
        store.Verify(s => s.SaveProjectsAsync(ProjectEvmTestData.Projects, Now.UtcDateTime, Any), Times.Once);
    }

    /// <summary>
    /// The sync command pulls the project picker without a project, and the snapshot with one.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SyncCommand_RoutesToThePickerOrTheSnapshot()
    {
        var sync = new Mock<IProjectEvmSyncService>();
        _ = sync.Setup(s => s.SyncAsync(ProjectEvmTestData.ProjectId, Any)).ReturnsAsync(new ProjectEvmSyncResult(ProjectEvmSyncStatus.Synced));
        _ = sync.Setup(s => s.SyncProjectsAsync(Any)).ReturnsAsync(new ProjectEvmSyncResult(ProjectEvmSyncStatus.Offline));
        SyncProjectEvmSnapshotCommandHandler handler = new(sync.Object);

        Assert.Equal(ProjectEvmSyncStatus.Synced, (await handler.Handle(new SyncProjectEvmSnapshotCommand(ProjectEvmTestData.ProjectId), CancellationToken.None)).Status);
        Assert.Equal(ProjectEvmSyncStatus.Offline, (await handler.Handle(new SyncProjectEvmSnapshotCommand(ProjectId: null), CancellationToken.None)).Status);
    }

    /// <summary>
    /// The queries read the cache only: a snapshot and the picker, or nothing before the first sync.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queries_ReadTheCacheOnly()
    {
        var syncedAt = new DateTime(2027, 3, 1, 8, 0, 0, DateTimeKind.Utc);
        _ = store.Setup(s => s.FindAsync(ProjectEvmTestData.ProjectId, Any)).ReturnsAsync(new StoredProjectEvmPayload(ProjectEvmTestData.Payload(), syncedAt));
        _ = store.Setup(s => s.FindProjectsAsync(Any)).ReturnsAsync(ProjectEvmTestData.Projects);

        var snapshot = await new GetProjectEvmSnapshotQueryHandler(store.Object).Handle(new GetProjectEvmSnapshotQuery(ProjectEvmTestData.ProjectId), CancellationToken.None);
        var projects = await new GetProjectEvmProjectsQueryHandler(store.Object).Handle(new GetProjectEvmProjectsQuery(), CancellationToken.None);

        Assert.NotNull(snapshot);
        Assert.Equal(syncedAt, snapshot.SyncedAtUtc);
        Assert.Equal(2, projects.Count);
        Assert.Null(await new GetProjectEvmSnapshotQueryHandler(store.Object).Handle(new GetProjectEvmSnapshotQuery("other"), CancellationToken.None));
        api.VerifyNoOtherCalls();
    }

    private ProjectEvmSyncService Create() => new(api.Object, store.Object, new FixedTime(Now));

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
