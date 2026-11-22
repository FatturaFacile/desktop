// <copyright file="FieldServiceSyncServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Field Service sync: each scope lands in its own cache slot, an unreadable payload never replaces a good cache, and
/// the failures are classified instead of thrown.
/// </summary>
public sealed class FieldServiceSyncServiceTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private readonly ManualTimeProvider clock = new();
    private readonly Mock<IFieldServiceApiClient> api = new();
    private readonly Mock<IFieldServiceStore> store = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldServiceSyncServiceTests"/> class.
    /// </summary>
    public FieldServiceSyncServiceTests()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.GetBoardAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), Any)).ReturnsAsync(FieldServiceTestData.Board);
        _ = api.Setup(a => a.GetContractsAsync(Any)).ReturnsAsync(FieldServiceTestData.Contracts);
        _ = api.Setup(a => a.GetEquipmentAsync(Any)).ReturnsAsync(FieldServiceTestData.Equipment);
        _ = api.Setup(a => a.GetWorkOrdersAsync(Any)).ReturnsAsync(FieldServiceTestData.WorkOrderList);
        _ = api.Setup(a => a.GetWorkOrderAsync(FieldServiceTestData.OrderId, Any)).ReturnsAsync(FieldServiceTestData.WorkOrder);
        _ = api.Setup(a => a.GetEquipmentHistoryAsync(FieldServiceTestData.EquipmentId, Any)).ReturnsAsync(FieldServiceTestData.History);
        _ = api.Setup(a => a.GetTechnicianBundleAsync(FieldServiceTestData.TechnicianId, Any)).ReturnsAsync(FieldServiceTestData.Bundle);
    }

    /// <summary>
    /// Each scope stores its payload in its own kind and key, stamped with the clock.
    /// </summary>
    /// <param name="scope">The scope.</param>
    /// <param name="key">The key given to the sync.</param>
    /// <param name="kind">The expected cache kind.</param>
    /// <param name="cacheKey">The expected cache key.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(FieldServiceSyncScope.Board, null, FieldServiceCacheKinds.Board, FieldServiceCacheKinds.All)]
    [InlineData(FieldServiceSyncScope.Contracts, null, FieldServiceCacheKinds.Contracts, FieldServiceCacheKinds.All)]
    [InlineData(FieldServiceSyncScope.Equipment, null, FieldServiceCacheKinds.Equipment, FieldServiceCacheKinds.All)]
    [InlineData(FieldServiceSyncScope.WorkOrders, null, FieldServiceCacheKinds.WorkOrders, FieldServiceCacheKinds.All)]
    [InlineData(FieldServiceSyncScope.WorkOrder, FieldServiceTestData.OrderId, FieldServiceCacheKinds.WorkOrder, FieldServiceTestData.OrderId)]
    [InlineData(FieldServiceSyncScope.History, FieldServiceTestData.EquipmentId, FieldServiceCacheKinds.History, FieldServiceTestData.EquipmentId)]
    [InlineData(FieldServiceSyncScope.Bundle, FieldServiceTestData.TechnicianId, FieldServiceCacheKinds.Bundle, FieldServiceTestData.TechnicianId)]
    public async Task Sync_StoresEachScopeInItsOwnSlot(FieldServiceSyncScope scope, string? key, string kind, string cacheKey)
    {
        var result = await Create().SyncAsync(scope, key, from: null, to: null, CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Ok, result.Status);
        Assert.Equal(clock.GetUtcNow().UtcDateTime, result.AtUtc);
        store.Verify(s => s.SaveAsync(kind, cacheKey, It.Is<string>(json => json.Length > 2), clock.GetUtcNow().UtcDateTime, Any), Times.Once);
    }

    /// <summary>
    /// Without a window the board is pulled from the Monday of the current week for seven days.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Board_DefaultsToTheCurrentWeek()
    {
        _ = await Create().SyncAsync(FieldServiceSyncScope.Board, key: null, from: null, to: null, CancellationToken.None);

        var monday = new DateTimeOffset(2026, 11, 16, 0, 0, 0, TimeSpan.Zero);
        api.Verify(a => a.GetBoardAsync(monday, monday.AddDays(7), Any), Times.Once);
    }

    /// <summary>
    /// An explicit window is passed to the server as given.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Board_UsesTheGivenWindow()
    {
        var from = new DateTimeOffset(2027, 4, 5, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(3);

        _ = await Create().SyncAsync(FieldServiceSyncScope.Board, key: null, from, to, CancellationToken.None);

        api.Verify(a => a.GetBoardAsync(from, to, Any), Times.Once);
    }

    /// <summary>
    /// Without a session nothing is requested and the user is asked to sign in.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WithoutSession_IsUnauthorizedAndAsksNothing()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        var result = await Create().SyncAsync(FieldServiceSyncScope.Contracts, key: null, from: null, to: null, CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Unauthorized, result.Status);
        api.Verify(a => a.GetContractsAsync(Any), Times.Never);
    }

    /// <summary>
    /// A server that cannot be reached, or a timeout, leaves the cache untouched and reports offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_ServerUnreachable_IsOfflineAndSavesNothing()
    {
        _ = api.Setup(a => a.GetContractsAsync(Any)).ThrowsAsync(new HttpRequestException("no route"));
        _ = api.Setup(a => a.GetEquipmentAsync(Any)).ThrowsAsync(new TaskCanceledException("timeout"));

        var contracts = await Create().SyncAsync(FieldServiceSyncScope.Contracts, key: null, from: null, to: null, CancellationToken.None);
        var equipment = await Create().SyncAsync(FieldServiceSyncScope.Equipment, key: null, from: null, to: null, CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Offline, contracts.Status);
        Assert.Equal(FieldServiceCallStatus.Offline, equipment.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A refusal is classified, and any other error is a failure.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <param name="expected">The expected outcome.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, FieldServiceCallStatus.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, FieldServiceCallStatus.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, FieldServiceCallStatus.Rejected)]
    [InlineData(HttpStatusCode.InternalServerError, FieldServiceCallStatus.Failed)]
    public async Task Sync_HttpError_IsClassified(HttpStatusCode status, FieldServiceCallStatus expected)
    {
        _ = api.Setup(a => a.GetBoardAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), Any)).ThrowsAsync(new HttpRequestException("refused", inner: null, status));

        var result = await Create().SyncAsync(FieldServiceSyncScope.Board, key: null, from: null, to: null, CancellationToken.None);

        Assert.Equal(expected, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A payload that does not parse never replaces a good cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_UnparsablePayload_FailsAndSavesNothing()
    {
        _ = api.Setup(a => a.GetBoardAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), Any)).ReturnsAsync("garbage");

        var result = await Create().SyncAsync(FieldServiceSyncScope.Board, key: null, from: null, to: null, CancellationToken.None);

        Assert.Equal(FieldServiceCallStatus.Failed, result.Status);
        store.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// The scopes that need an identifier refuse a blank one.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WithoutTheRequiredKey_Throws()
    {
        var service = Create();

        _ = await Assert.ThrowsAsync<ArgumentException>(() => service.SyncAsync(FieldServiceSyncScope.WorkOrder, key: " ", from: null, to: null, CancellationToken.None));
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => service.SyncAsync(FieldServiceSyncScope.Bundle, key: null, from: null, to: null, CancellationToken.None));
    }

    /// <summary>
    /// The sync command forwards to the service.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SyncCommand_ForwardsToTheService()
    {
        Mock<IFieldServiceSyncService> sync = new();
        var expected = new FieldServiceCallResult(FieldServiceCallStatus.Ok);
        _ = sync.Setup(s => s.SyncAsync(FieldServiceSyncScope.History, FieldServiceTestData.EquipmentId, from: null, to: null, Any)).ReturnsAsync(expected);

        var result = await new SyncFieldServiceCommandHandler(sync.Object).Handle(new SyncFieldServiceCommand(FieldServiceSyncScope.History, FieldServiceTestData.EquipmentId, From: null, To: null), CancellationToken.None);

        Assert.Same(expected, result);
    }

    private FieldServiceSyncService Create() => new(api.Object, store.Object, clock);
}
