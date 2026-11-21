// <copyright file="ManufacturingSyncServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using System.Net;
using System.Net.Http;
using FatturaFacile.Application.Manufacturing;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Mod 061 sync orchestrator: the atomic replace, the optional OEE pieces, the offline fallback and the failure classes.
/// </summary>
public sealed class ManufacturingSyncServiceTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTimeOffset Now = new(2026, 11, 21, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IManufacturingApiClient> api = new();
    private readonly Mock<IManufacturingStore> store = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ManufacturingSyncServiceTests"/> class.
    /// </summary>
    public ManufacturingSyncServiceTests()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.GetListAsync("work-centers", Any)).ReturnsAsync(ManufacturingTestData.WorkCenters);
        _ = api.Setup(a => a.GetListAsync("machines", Any)).ReturnsAsync(ManufacturingTestData.Machines);
        _ = api.Setup(a => a.GetListAsync("working-calendars", Any)).ReturnsAsync(ManufacturingTestData.Calendars);
        _ = api.Setup(a => a.GetListAsync("routings", Any)).ReturnsAsync(ManufacturingTestData.Routings);
        _ = api.Setup(a => a.GetOeeSummaryAsync(ManufacturingTestData.WorkCenterA, It.IsAny<DateTime>(), It.IsAny<DateTime>(), Any)).ReturnsAsync(ManufacturingTestData.OeeSummary);
        _ = api.Setup(a => a.GetOeeSummaryAsync(ManufacturingTestData.WorkCenterB, It.IsAny<DateTime>(), It.IsAny<DateTime>(), Any)).ThrowsAsync(new HttpRequestException("none", inner: null, HttpStatusCode.UnprocessableEntity));
    }

    /// <summary>
    /// A successful pull replaces the cache once with everything pulled; a work centre the server cannot summarise is left out.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Online_ReplacesTheCacheOnce()
    {
        ManufacturingMasterData? saved = null;
        _ = store.Setup(s => s.ReplaceAsync(It.IsAny<ManufacturingMasterData>(), Any)).Callback<ManufacturingMasterData, CancellationToken>((d, _) => saved = d).Returns(Task.CompletedTask);

        var result = await Create().SyncAsync(CancellationToken.None);

        Assert.Equal(ManufacturingSyncStatus.Synced, result.Status);
        Assert.Equal(Now.UtcDateTime, result.SyncedAtUtc);
        store.Verify(s => s.ReplaceAsync(It.IsAny<ManufacturingMasterData>(), Any), Times.Once);
        Assert.NotNull(saved);
        Assert.Equal(2, saved.WorkCenters.Count);
        _ = Assert.Single(saved.Machines);
        _ = Assert.Single(saved.Calendars);
        _ = Assert.Single(saved.Routings);
        var oee = Assert.Single(saved.OeeSnapshots);
        Assert.Equal(ManufacturingTestData.WorkCenterA, oee.WorkCenterId);
        Assert.Equal(Now.UtcDateTime, oee.SyncedAtUtc);
    }

    /// <summary>
    /// The OEE summary covers the last 30 days up to the sync instant.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_RequestsTheLastThirtyDaysOfOee()
    {
        _ = await Create().SyncAsync(CancellationToken.None);

        api.Verify(a => a.GetOeeSummaryAsync(ManufacturingTestData.WorkCenterA, Now.UtcDateTime.AddDays(-ManufacturingSyncService.OeeWindowDays), Now.UtcDateTime, Any), Times.Once);
    }

    /// <summary>
    /// Without a session nothing is requested and the user is asked to sign in.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WithoutSession_IsUnauthorizedAndAsksNothing()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        var result = await Create().SyncAsync(CancellationToken.None);

        Assert.Equal(ManufacturingSyncStatus.Unauthorized, result.Status);
        api.Verify(a => a.GetListAsync(It.IsAny<string>(), Any), Times.Never);
        store.Verify(s => s.ReplaceAsync(It.IsAny<ManufacturingMasterData>(), Any), Times.Never);
    }

    /// <summary>
    /// A server that cannot be reached leaves the cache untouched and reports offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_ServerUnreachable_IsOfflineAndKeepsTheCache()
    {
        _ = api.Setup(a => a.GetListAsync("machines", Any)).ThrowsAsync(new HttpRequestException("no route"));

        var result = await Create().SyncAsync(CancellationToken.None);

        Assert.Equal(ManufacturingSyncStatus.Offline, result.Status);
        Assert.Contains("no route", result.Detail, StringComparison.Ordinal);
        store.Verify(s => s.ReplaceAsync(It.IsAny<ManufacturingMasterData>(), Any), Times.Never);
    }

    /// <summary>
    /// A timeout counts as offline too.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Timeout_IsOffline()
    {
        _ = api.Setup(a => a.GetListAsync("routings", Any)).ThrowsAsync(new TaskCanceledException("timed out"));

        var result = await Create().SyncAsync(CancellationToken.None);

        Assert.Equal(ManufacturingSyncStatus.Offline, result.Status);
        store.Verify(s => s.ReplaceAsync(It.IsAny<ManufacturingMasterData>(), Any), Times.Never);
    }

    /// <summary>
    /// The statuses 401, 403 and 500 are told apart.
    /// </summary>
    /// <param name="code">The HTTP status.</param>
    /// <param name="expected">The expected outcome.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, ManufacturingSyncStatus.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, ManufacturingSyncStatus.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError, ManufacturingSyncStatus.Failed)]
    public async Task Sync_HttpFailure_IsClassified(HttpStatusCode code, ManufacturingSyncStatus expected)
    {
        _ = api.Setup(a => a.GetListAsync("work-centers", Any)).ThrowsAsync(new HttpRequestException("failed", inner: null, code));

        var result = await Create().SyncAsync(CancellationToken.None);

        Assert.Equal(expected, result.Status);
        store.Verify(s => s.ReplaceAsync(It.IsAny<ManufacturingMasterData>(), Any), Times.Never);
    }

    /// <summary>
    /// A payload that does not parse fails the sync without touching the cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_MalformedPayload_FailsAndKeepsTheCache()
    {
        _ = api.Setup(a => a.GetListAsync("routings", Any)).ReturnsAsync("""{ "not": "a list" }""");

        var result = await Create().SyncAsync(CancellationToken.None);

        Assert.Equal(ManufacturingSyncStatus.Failed, result.Status);
        store.Verify(s => s.ReplaceAsync(It.IsAny<ManufacturingMasterData>(), Any), Times.Never);
    }

    /// <summary>
    /// An OEE summary that does not parse is left out instead of failing the whole sync.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_MalformedOeeSummary_IsLeftOut()
    {
        _ = api.Setup(a => a.GetOeeSummaryAsync(ManufacturingTestData.WorkCenterA, It.IsAny<DateTime>(), It.IsAny<DateTime>(), Any)).ReturnsAsync("{}");

        var result = await Create().SyncAsync(CancellationToken.None);

        Assert.Equal(ManufacturingSyncStatus.Synced, result.Status);
        store.Verify(s => s.ReplaceAsync(It.Is<ManufacturingMasterData>(d => d.OeeSnapshots.Count == 0 && d.WorkCenters.Count == 2), Any), Times.Once);
    }

    private ManufacturingSyncService Create() => new(api.Object, store.Object, new FixedTime(Now));

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
