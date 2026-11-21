// <copyright file="ManufacturingHandlersTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using FatturaFacile.Application.Features.Manufacturing;
using FatturaFacile.Application.Manufacturing;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Mod 061 MediatR handlers.
/// </summary>
public sealed class ManufacturingHandlersTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime At = new(2026, 11, 21, 9, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IManufacturingStore> store = new();

    /// <summary>
    /// The overview handler returns the cache as it is.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Overview_ReturnsTheCache()
    {
        var overview = Pulled();
        _ = store.Setup(s => s.GetOverviewAsync(Any)).ReturnsAsync(overview);

        var result = await new GetManufacturingOverviewQueryHandler(store.Object).Handle(new GetManufacturingOverviewQuery(), CancellationToken.None);

        Assert.Same(overview, result);
    }

    /// <summary>
    /// The sync handler delegates to the sync service.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_DelegatesToTheService()
    {
        var sync = new Mock<IManufacturingSyncService>();
        var expected = new ManufacturingSyncResult(ManufacturingSyncStatus.Synced, At);
        _ = sync.Setup(s => s.SyncAsync(Any)).ReturnsAsync(expected);

        var result = await new SyncManufacturingCommandHandler(sync.Object).Handle(new SyncManufacturingCommand(), CancellationToken.None);

        Assert.Same(expected, result);
    }

    /// <summary>
    /// The routing calculation runs on the cached data and gives the lead time of the steps.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CalculateRouting_UsesTheCachedData()
    {
        _ = store.Setup(s => s.GetOverviewAsync(Any)).ReturnsAsync(Pulled());
        var handler = new CalculateRoutingQueryHandler(store.Object);

        var result = await handler.Handle(new CalculateRoutingQuery(ManufacturingTestData.Routing, 10m, IncludeOverlap: false), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Steps.Count);
        Assert.Equal([10, 20], result.Steps.Select(s => s.Sequence));
        Assert.True(result.TotalMinutes > 0d);
        Assert.True(result.Cost.Total > 0m);
    }

    /// <summary>
    /// An unknown routing gives no calculation.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CalculateRouting_UnknownRouting_ReturnsNull()
    {
        _ = store.Setup(s => s.GetOverviewAsync(Any)).ReturnsAsync(Pulled());

        var result = await new CalculateRoutingQueryHandler(store.Object).Handle(new CalculateRoutingQuery(Guid.NewGuid(), 1m), CancellationToken.None);

        Assert.Null(result);
    }

    private static ManufacturingOverview Pulled() => new(
        ManufacturingParser.ParseWorkCenters(ManufacturingTestData.WorkCenters, At),
        ManufacturingParser.ParseMachines(ManufacturingTestData.Machines, At),
        ManufacturingParser.ParseCalendars(ManufacturingTestData.Calendars, At),
        ManufacturingParser.ParseRoutings(ManufacturingTestData.Routings, At),
        [],
        At);
}
