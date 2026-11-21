// <copyright file="DssSyncServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using FatturaFacile.Application.Dss;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Executive DSS sync orchestrator: the atomic save, the offline fallback and the classification of failures.
/// </summary>
public sealed class DssSyncServiceTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTimeOffset Now = new(2026, 6, 30, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IDssApiClient> api = new();
    private readonly Mock<IDssSnapshotStore> store = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DssSyncServiceTests"/> class.
    /// </summary>
    public DssSyncServiceTests()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.GetOverviewAsync(It.IsAny<DssFilter>(), Any)).ReturnsAsync(DssTestData.Overview);
        _ = api.Setup(a => a.GetProfitabilityAsync(It.IsAny<DssFilter>(), Any)).ReturnsAsync(DssTestData.Profitability);
        _ = api.Setup(a => a.GetVarianceBridgeAsync(It.IsAny<DssFilter>(), Any)).ReturnsAsync(DssTestData.VarianceBridge);
        _ = api.Setup(a => a.GetSimulationInputsAsync(It.IsAny<DssFilter>(), Any)).ReturnsAsync(DssTestData.SimulationInputs);
        _ = api.Setup(a => a.GetLegalEntitiesAsync(Any)).ReturnsAsync(DssTestData.LegalEntities);
    }

    /// <summary>
    /// A successful pull stores the three views, the what-if inputs and the legal entities in one save, stamped with the clock.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Online_StoresEverythingInOneSave()
    {
        var result = await Create().SyncAsync(DssFilter.Default, CancellationToken.None);

        Assert.Equal(DssSyncStatus.Synced, result.Status);
        Assert.Equal(Now.UtcDateTime, result.SyncedAtUtc);
        store.Verify(s => s.SaveAsync(DssFilter.Default, It.Is<DssPayload>(p => string.Equals(p.OverviewJson, DssTestData.Overview, StringComparison.Ordinal) && string.Equals(p.SimulationInputsJson, DssTestData.SimulationInputs, StringComparison.Ordinal) && string.Equals(p.LegalEntitiesJson, DssTestData.LegalEntities, StringComparison.Ordinal)), Now.UtcDateTime, Any), Times.Once);
    }

    /// <summary>
    /// Without a session nothing is requested and the user is asked to sign in.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WithoutSession_IsUnauthorizedAndAsksNothing()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        var result = await Create().SyncAsync(DssFilter.Default, CancellationToken.None);

        Assert.Equal(DssSyncStatus.Unauthorized, result.Status);
        api.Verify(a => a.GetOverviewAsync(It.IsAny<DssFilter>(), Any), Times.Never);
        store.Verify(s => s.SaveAsync(It.IsAny<DssFilter>(), It.IsAny<DssPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A server that cannot be reached leaves the cache untouched and reports offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_ServerUnreachable_IsOfflineAndKeepsTheCache()
    {
        _ = api.Setup(a => a.GetOverviewAsync(It.IsAny<DssFilter>(), Any)).ThrowsAsync(new HttpRequestException("no route"));

        var result = await Create().SyncAsync(DssFilter.Default, CancellationToken.None);

        Assert.Equal(DssSyncStatus.Offline, result.Status);
        Assert.Contains("no route", result.Detail, StringComparison.Ordinal);
        store.Verify(s => s.SaveAsync(It.IsAny<DssFilter>(), It.IsAny<DssPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A connection lost half way through is offline too, and nothing partial is saved.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_ConnectionLostHalfWay_SavesNothing()
    {
        _ = api.Setup(a => a.GetVarianceBridgeAsync(It.IsAny<DssFilter>(), Any)).ThrowsAsync(new HttpRequestException("reset"));

        var result = await Create().SyncAsync(DssFilter.Default, CancellationToken.None);

        Assert.Equal(DssSyncStatus.Offline, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<DssFilter>(), It.IsAny<DssPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A timeout is offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Timeout_IsOffline()
    {
        _ = api.Setup(a => a.GetProfitabilityAsync(It.IsAny<DssFilter>(), Any)).ThrowsAsync(new TaskCanceledException("timeout"));

        var result = await Create().SyncAsync(DssFilter.Default, CancellationToken.None);

        Assert.Equal(DssSyncStatus.Offline, result.Status);
    }

    /// <summary>
    /// A cancellation asked by the caller is not swallowed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_CancelledByCaller_Propagates()
    {
        CancellationToken cancelled = new(canceled: true);
        // The shared matcher is a constant (the default token): a cancelled token needs the real matcher inline.
        _ = api.Setup(a => a.GetOverviewAsync(It.IsAny<DssFilter>(), It.IsAny<CancellationToken>())).Returns(Task.FromCanceled<string>(cancelled));

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Create().SyncAsync(DssFilter.Default, cancelled));
    }

    /// <summary>
    /// The HTTP status decides the outcome: 401 sign in, 403 forbidden, anything else a failure.
    /// </summary>
    /// <param name="status">The status the server answered.</param>
    /// <param name="expected">The expected outcome.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, DssSyncStatus.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, DssSyncStatus.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError, DssSyncStatus.Failed)]
    [InlineData(HttpStatusCode.UnprocessableEntity, DssSyncStatus.Failed)]
    public async Task Sync_HttpError_IsClassifiedByStatus(HttpStatusCode status, DssSyncStatus expected)
    {
        _ = api.Setup(a => a.GetOverviewAsync(It.IsAny<DssFilter>(), Any)).ThrowsAsync(new HttpRequestException("boom", inner: null, status));

        var result = await Create().SyncAsync(DssFilter.Default, CancellationToken.None);

        Assert.Equal(expected, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<DssFilter>(), It.IsAny<DssPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A user who may read the cockpit but not simulate still gets the cockpit: the refused extras are skipped.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_ExtrasRefused_StillSyncs()
    {
        _ = api.Setup(a => a.GetSimulationInputsAsync(It.IsAny<DssFilter>(), Any)).ThrowsAsync(new HttpRequestException("forbidden", inner: null, HttpStatusCode.Forbidden));
        _ = api.Setup(a => a.GetLegalEntitiesAsync(Any)).ThrowsAsync(new HttpRequestException("missing", inner: null, HttpStatusCode.NotFound));

        var result = await Create().SyncAsync(DssFilter.Default, CancellationToken.None);

        Assert.Equal(DssSyncStatus.Synced, result.Status);
        store.Verify(s => s.SaveAsync(DssFilter.Default, It.Is<DssPayload>(p => p.SimulationInputsJson == null && p.LegalEntitiesJson == null), It.IsAny<DateTime>(), Any), Times.Once);
    }

    /// <summary>
    /// A payload that does not parse never replaces a good cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_InvalidPayload_FailsWithoutSaving()
    {
        _ = api.Setup(a => a.GetOverviewAsync(It.IsAny<DssFilter>(), Any)).ReturnsAsync("<html>gateway</html>");

        var result = await Create().SyncAsync(DssFilter.Default, CancellationToken.None);

        Assert.Equal(DssSyncStatus.Failed, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<DssFilter>(), It.IsAny<DssPayload>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// The filter reaches the server and the cache as it is.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_UsesTheGivenFilter()
    {
        var filter = new DssFilter("ent-1", "EUR", "QTD", "BUDGET");

        _ = await Create().SyncAsync(filter, CancellationToken.None);

        api.Verify(a => a.GetOverviewAsync(filter, Any), Times.Once);
        store.Verify(s => s.SaveAsync(filter, It.IsAny<DssPayload>(), It.IsAny<DateTime>(), Any), Times.Once);
    }

    private DssSyncService Create() => new(api.Object, store.Object, new FixedTime(Now));

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
