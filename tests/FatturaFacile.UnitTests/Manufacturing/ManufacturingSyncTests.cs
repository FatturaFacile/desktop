// <copyright file="ManufacturingSyncTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net.Http;
using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Application.Manufacturing.Sync;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Manufacturing;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests.Manufacturing;

/// <summary>
/// Tests of the reconnect procedure: push first, then the delta pull, protecting orders that still have local mutations.
/// </summary>
public sealed class ManufacturingSyncTests : IDisposable
{
    private readonly ManufacturingHarness harness = new();

    /// <inheritdoc/>
    public void Dispose() => harness.Dispose();

    /// <summary>
    /// Without a session nothing is pushed or pulled.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WithoutSession_AsksForSignIn()
    {
        _ = harness.Gateway.SetupGet(g => g.IsAuthenticated).Returns(value: false);

        var result = await CreateService().SyncAsync(CancellationToken.None);

        Assert.True(result.Push.AuthRequired);
        harness.Gateway.Verify(g => g.GetPageAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// The pull stores masters, prices the BOM from the item costs, caches the cockpit, and a second pass skips rows whose version did not change.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_PullsMastersPricesBomAndSkipsUnchangedRows()
    {
        var itemId = Guid.NewGuid();
        var centerId = Guid.NewGuid();
        var bomId = Guid.NewGuid();
        var componentId = Guid.NewGuid();
        SetupEmptyPages();
        SetupPage("items", "{\"id\":\"" + itemId.ToString("D") + "\",\"item_code\":\"D-1\",\"name\":\"Screw\",\"standard_cost\":\"2.5000\",\"updated_at\":\"v1\"}");
        SetupPage("work-centers", "{\"id\":\"" + centerId.ToString("D") + "\",\"work_center_code\":\"WC1\",\"name\":\"Lathe\",\"capacity_hours_per_day\":\"8.00\",\"efficiency_rate\":\"85\",\"availability_rate\":\"100\",\"hourly_cost_rate\":\"40.00\",\"hourly_overhead_rate\":\"60.00\",\"updated_at\":\"v1\"}");
        SetupPage("boms", "{\"id\":\"" + bomId.ToString("D") + "\",\"updated_at\":\"v1\"}");
        _ = harness.Gateway
            .Setup(g => g.GetJsonAsync("boms/" + bomId.ToString("D"), It.IsAny<CancellationToken>()))
            .ReturnsAsync("{\"id\":\"" + bomId.ToString("D") + "\",\"bom_number\":\"BOM-1\",\"mfg_item_id\":\"" + Guid.NewGuid().ToString("D") + "\",\"item_code\":\"TOP\",\"status\":\"active\",\"base_quantity\":\"1\",\"updated_at\":\"v1\",\"components\":[{\"id\":\"" + componentId.ToString("D") + "\",\"component_item_id\":\"" + itemId.ToString("D") + "\",\"component_item_code\":\"D-1\",\"sequence\":10,\"quantity_per\":\"4\",\"scrap_percentage\":\"0\"}]}");
        _ = harness.Gateway.Setup(g => g.GetJsonAsync("cockpit", It.IsAny<CancellationToken>())).ReturnsAsync("{\"orders\":{\"open\":3},\"machines\":[1,2]}");

        var first = await CreateService().SyncAsync(CancellationToken.None);

        Assert.False(first.Offline);
        Assert.Equal(3, first.Pulled);
        var workbench = await harness.Store.GetWorkbenchAsync(CancellationToken.None);
        var center = Assert.Single(workbench.WorkCenters);
        Assert.Equal(0.85m, center.EfficiencyRate);
        Assert.Equal(1m, center.AvailabilityRate);
        var line = Assert.Single(Assert.Single(workbench.Boms).Lines);
        Assert.Equal(2.5m, line.StandardUnitCost);
        Assert.NotNull(workbench.Telemetry);
        _ = Assert.NotNull(await harness.Store.GetCheckpointAsync("work-centers", CancellationToken.None));

        var second = await CreateService().SyncAsync(CancellationToken.None);

        Assert.Equal(0, second.Pulled);
        Assert.Equal(3, second.Skipped);
    }

    /// <summary>
    /// A failing network after the push is reported as offline; the pushed outbox work is kept.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_NetworkDownDuringPull_ReportsOfflineWithoutThrowing()
    {
        _ = harness.Gateway
            .Setup(g => g.GetPageAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("offline"));

        var result = await CreateService().SyncAsync(CancellationToken.None);

        Assert.True(result.Offline);
        Assert.Contains(result.Errors, e => string.Equals(e, "offline", StringComparison.Ordinal));
    }

    /// <summary>
    /// A pull never overwrites an order that still has local mutations; once they are delivered, the server version is adopted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Pull_DoesNotOverwriteOrdersWithPendingMutations()
    {
        var order = await harness.SeedOrderAsync(ProductionOrderStatus.Released);
        _ = await harness.Queue.QueueTransitionAsync(order.Id, ManufacturingMutationKind.Start, CancellationToken.None);
        var row = ManufacturingHarness.Json("{\"id\":\"" + order.Id.ToString("D") + "\",\"order_number\":\"SERVER\",\"status\":\"released\",\"target_quantity\":\"10\",\"updated_at\":\"v2\"}");

        var protectedIds = await harness.Store.GetProtectedOrderIdsAsync(CancellationToken.None);
        Assert.Contains(order.Id, protectedIds);
        var blocked = await harness.Store.ApplyAsync("production-orders", [row], protectedIds, CancellationToken.None);

        Assert.Equal((0, 1), blocked);
        Assert.Equal(ProductionOrderStatus.InProgress, (await harness.OrderAsync(order.Id)).Status);

        _ = harness.Gateway.Setup(g => g.SendAsync(It.IsAny<ManufacturingRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ManufacturingHarness.Outcome(SyncSendResult.Success, 200));
        _ = await harness.Queue.FlushAsync(CancellationToken.None);
        var open = await harness.Store.ApplyAsync("production-orders", [row], await harness.Store.GetProtectedOrderIdsAsync(CancellationToken.None), CancellationToken.None);

        Assert.Equal((1, 0), open);
        var adopted = await harness.OrderAsync(order.Id);
        Assert.Equal("SERVER", adopted.OrderNumber);
        Assert.Equal(ProductionOrderStatus.Released, adopted.Status);
    }

    private WorkbenchSyncService CreateService() => new(harness.Queue, harness.Gateway.Object, harness.Store, harness.Clock);

    private void SetupEmptyPages() => harness.Gateway
        .Setup(g => g.GetPageAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ManufacturingPage([], HasMore: false, harness.Clock.GetUtcNow().UtcDateTime));

    private void SetupPage(string resource, string rowJson) => harness.Gateway
        .Setup(g => g.GetPageAsync(resource, It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ManufacturingPage([ManufacturingHarness.Json(rowJson)], HasMore: false, harness.Clock.GetUtcNow().UtcDateTime));
}
