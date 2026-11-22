// <copyright file="ProjectBillingSyncServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using FatturaFacile.Application.Features.ProjectBilling;
using FatturaFacile.Application.ProjectBilling;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the project billing sync: the atomic save, the offline fallback, the classification of failures and the handlers.
/// </summary>
public sealed class ProjectBillingSyncServiceTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTimeOffset Now = new(2027, 3, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IProjectBillingApiClient> api = new();
    private readonly Mock<IProjectBillingSnapshotStore> store = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectBillingSyncServiceTests"/> class.
    /// </summary>
    public ProjectBillingSyncServiceTests()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.GetSummaryAsync(ProjectBillingTestData.ProjectId, Any)).ReturnsAsync(ProjectBillingTestData.Summary);
        _ = api.Setup(a => a.GetContractAsync(ProjectBillingTestData.ProjectId, Any)).ReturnsAsync(ProjectBillingTestData.Contract);
        _ = api.Setup(a => a.GetCertificatesAsync(ProjectBillingTestData.ProjectId, Any)).ReturnsAsync(ProjectBillingTestData.Certificates);
        _ = api.Setup(a => a.GetRetentionAsync(ProjectBillingTestData.ProjectId, Any)).ReturnsAsync(ProjectBillingTestData.Retention);
        _ = api.Setup(a => a.GetRetainerAsync(ProjectBillingTestData.ProjectId, Any)).ReturnsAsync(ProjectBillingTestData.Retainer);
    }

    /// <summary>
    /// A successful pull stores every view in one save, stamped with the clock.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Online_StoresEveryViewInOneSave()
    {
        var result = await Create().SyncAsync(ProjectBillingTestData.ProjectId, CancellationToken.None);

        Assert.Equal(ProjectBillingSyncStatus.Synced, result.Status);
        Assert.Equal(Now.UtcDateTime, result.SyncedAtUtc);
        store.Verify(s => s.SaveAsync(ProjectBillingTestData.ProjectId, It.Is<ProjectBillingPayload>(p => string.Equals(p.SummaryJson, ProjectBillingTestData.Summary, StringComparison.Ordinal) && string.Equals(p.RetainerJson, ProjectBillingTestData.Retainer, StringComparison.Ordinal)), Now.UtcDateTime, Any), Times.Once);
    }

    /// <summary>
    /// A project without a contract asks neither the retention nor the retainer.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WithoutContract_SkipsRetentionAndRetainer()
    {
        _ = api.Setup(a => a.GetContractAsync(ProjectBillingTestData.ProjectId, Any)).ReturnsAsync(ProjectBillingTestData.Absent);
        _ = api.Setup(a => a.GetCertificatesAsync(ProjectBillingTestData.ProjectId, Any)).ReturnsAsync("[]");

        var result = await Create().SyncAsync(ProjectBillingTestData.ProjectId, CancellationToken.None);

        Assert.Equal(ProjectBillingSyncStatus.Synced, result.Status);
        api.Verify(a => a.GetRetentionAsync(It.IsAny<string>(), Any), Times.Never);
        api.Verify(a => a.GetRetainerAsync(It.IsAny<string>(), Any), Times.Never);
        store.Verify(s => s.SaveAsync(ProjectBillingTestData.ProjectId, It.Is<ProjectBillingPayload>(p => string.Equals(p.RetentionJson, ProjectBillingTestData.Absent, StringComparison.Ordinal)), Now.UtcDateTime, Any), Times.Once);
    }

    /// <summary>
    /// Without a session nothing is requested and the user is asked to sign in.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WithoutSession_IsUnauthorizedAndAsksNothing()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        var result = await Create().SyncAsync(ProjectBillingTestData.ProjectId, CancellationToken.None);

        Assert.Equal(ProjectBillingSyncStatus.Unauthorized, result.Status);
        api.Verify(a => a.GetSummaryAsync(It.IsAny<string>(), Any), Times.Never);
    }

    /// <summary>
    /// A server that cannot be reached, or a connection lost half way, leaves the cache untouched and reports offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_ServerUnreachable_IsOfflineAndSavesNothing()
    {
        _ = api.Setup(a => a.GetCertificatesAsync(ProjectBillingTestData.ProjectId, Any)).ThrowsAsync(new HttpRequestException("reset"));

        var result = await Create().SyncAsync(ProjectBillingTestData.ProjectId, CancellationToken.None);

        Assert.Equal(ProjectBillingSyncStatus.Offline, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<ProjectBillingPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A timeout is offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Timeout_IsOffline()
    {
        _ = api.Setup(a => a.GetSummaryAsync(ProjectBillingTestData.ProjectId, Any)).ThrowsAsync(new TaskCanceledException("timeout"));

        Assert.Equal(ProjectBillingSyncStatus.Offline, (await Create().SyncAsync(ProjectBillingTestData.ProjectId, CancellationToken.None)).Status);
    }

    /// <summary>
    /// A refusal is classified, and any other error is a failure.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <param name="expected">The expected sync status.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, ProjectBillingSyncStatus.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, ProjectBillingSyncStatus.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError, ProjectBillingSyncStatus.Failed)]
    public async Task Sync_HttpError_IsClassified(HttpStatusCode status, ProjectBillingSyncStatus expected)
    {
        _ = api.Setup(a => a.GetSummaryAsync(ProjectBillingTestData.ProjectId, Any)).ThrowsAsync(new HttpRequestException("refused", inner: null, status));

        var result = await Create().SyncAsync(ProjectBillingTestData.ProjectId, CancellationToken.None);

        Assert.Equal(expected, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<ProjectBillingPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A payload that does not parse never replaces a good cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_UnparsablePayload_FailsAndSavesNothing()
    {
        _ = api.Setup(a => a.GetSummaryAsync(ProjectBillingTestData.ProjectId, Any)).ReturnsAsync("[]");

        var result = await Create().SyncAsync(ProjectBillingTestData.ProjectId, CancellationToken.None);

        Assert.Equal(ProjectBillingSyncStatus.Failed, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<ProjectBillingPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// The query handler reads the cache only, and the command handler delegates to the sync service.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Handlers_ReadTheCacheAndDelegateTheSync()
    {
        _ = store.Setup(s => s.FindAsync(ProjectBillingTestData.ProjectId, Any)).ReturnsAsync(new StoredProjectBillingPayload(ProjectBillingTestData.Payload(), Now.UtcDateTime));
        var sync = new Mock<IProjectBillingSyncService>();
        _ = sync.Setup(s => s.SyncAsync(ProjectBillingTestData.ProjectId, Any)).ReturnsAsync(new ProjectBillingSyncResult(ProjectBillingSyncStatus.Synced, Now.UtcDateTime));

        var snapshot = await new GetProjectBillingSnapshotQueryHandler(store.Object).Handle(new GetProjectBillingSnapshotQuery(ProjectBillingTestData.ProjectId), CancellationToken.None);
        var missing = await new GetProjectBillingSnapshotQueryHandler(Mock.Of<IProjectBillingSnapshotStore>()).Handle(new GetProjectBillingSnapshotQuery("other"), CancellationToken.None);
        var synced = await new SyncProjectBillingCommandHandler(sync.Object).Handle(new SyncProjectBillingCommand(ProjectBillingTestData.ProjectId), CancellationToken.None);

        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.Certificates.Count);
        Assert.Null(missing);
        Assert.Equal(ProjectBillingSyncStatus.Synced, synced.Status);
        api.Verify(a => a.GetSummaryAsync(It.IsAny<string>(), Any), Times.Never);
    }

    private ProjectBillingSyncService Create() => new(api.Object, store.Object, new FixedTime(Now));

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
