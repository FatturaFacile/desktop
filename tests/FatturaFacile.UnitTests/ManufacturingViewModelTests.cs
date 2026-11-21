// <copyright file="ManufacturingViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using FatturaFacile.Application.Features.Manufacturing;
using FatturaFacile.Application.Manufacturing;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Mod 061 production view model: cached display, offline behaviour, routing steps with the offline calculation and OEE.
/// </summary>
public sealed class ManufacturingViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime SyncedAt = new(2026, 11, 21, 9, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly Mock<IManufacturingStore> store = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private ManufacturingOverview cached = Empty();
    private ManufacturingSyncResult syncResult = new(ManufacturingSyncStatus.Synced, SyncedAt);

    /// <summary>
    /// Initializes a new instance of the <see cref="ManufacturingViewModelTests"/> class.
    /// </summary>
    public ManufacturingViewModelTests()
    {
        _ = store.Setup(s => s.GetOverviewAsync(Any)).ReturnsAsync(() => cached);
        _ = sender.Setup(s => s.Send(It.IsAny<GetManufacturingOverviewQuery>(), Any)).ReturnsAsync(() => cached);
        _ = sender.Setup(s => s.Send(It.IsAny<SyncManufacturingCommand>(), Any)).ReturnsAsync(() => syncResult);
        _ = sender.Setup(s => s.Send(It.IsAny<CalculateRoutingQuery>(), Any)).Returns(
            (CalculateRoutingQuery query, CancellationToken token) => new CalculateRoutingQueryHandler(store.Object).Handle(query, token));
    }

    /// <summary>
    /// With cached data the view shows work centres with their OEE, the machines of the selected one, the routings and the calendars.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Cached_PopulatesTheLists()
    {
        cached = Pulled();
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.HasData);
        Assert.False(vm.IsOffline);
        Assert.Equal(["WC-01", "WC-02"], vm.WorkCenters.Select(w => w.Code));
        Assert.Equal(Percent(66.67d), vm.WorkCenters[0].Oee);
        Assert.Equal("–", vm.WorkCenters[1].Oee);
        Assert.Equal("1", vm.WorkCenters[0].MachineCount);
        Assert.Equal("WC-01", vm.SelectedWorkCenter?.Code);
        Assert.Equal("M-01", Assert.Single(vm.Machines).Code);
        Assert.Contains(loc["Mfg_FLAG_INDUSTRIA40"], vm.Machines[0].Flags, StringComparison.Ordinal);
        Assert.Equal("RT-100", Assert.Single(vm.Routings).Number);
        Assert.Equal("STD", Assert.Single(vm.Calendars).Code);
        Assert.Contains(SyncedAt.ToLocalTime().ToString("g", loc.CurrentCulture), vm.LastSyncedText, StringComparison.Ordinal);
        sender.Verify(s => s.Send(It.IsAny<SyncManufacturingCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// Nothing cached triggers a pull on load; the freshly cached data is shown.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NothingCached_PullsThenShows()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<SyncManufacturingCommand>(), Any)).Callback(() => cached = Pulled()).ReturnsAsync(() => syncResult);
        var vm = Create();

        await vm.LoadAsync();

        sender.Verify(s => s.Send(It.IsAny<SyncManufacturingCommand>(), Any), Times.Once);
        Assert.True(vm.HasData);
        Assert.Equal(loc["Mfg_Synced"], vm.StatusMessage);
        Assert.Equal(2, vm.WorkCenters.Count);
    }

    /// <summary>
    /// Offline without any cache says so; with a cache the last data stays on screen with an offline notice.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Offline_KeepsTheCachedDataAndSaysSo()
    {
        syncResult = new ManufacturingSyncResult(ManufacturingSyncStatus.Offline, Detail: "no route");
        var empty = Create();
        await empty.LoadAsync();
        Assert.False(empty.HasData);
        Assert.True(empty.IsOffline);
        Assert.Equal(loc["Mfg_OfflineNoCache"], empty.StatusMessage);

        cached = Pulled();
        var vm = Create();
        await vm.LoadAsync();
        await vm.SyncAsync();

        Assert.True(vm.IsOffline);
        Assert.True(vm.HasData);
        Assert.Equal(2, vm.WorkCenters.Count);
        Assert.Equal(loc["Mfg_OfflineUsingCache"], vm.StatusMessage);
    }

    /// <summary>
    /// A failed sync shows the server detail; a missing session or permission has its own message.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_FailureStatuses_AreExplained()
    {
        cached = Pulled();
        var vm = Create();
        await vm.LoadAsync();

        syncResult = new ManufacturingSyncResult(ManufacturingSyncStatus.Failed, Detail: "HTTP 500");
        await vm.SyncAsync();
        Assert.Contains("HTTP 500", vm.StatusMessage, StringComparison.Ordinal);

        syncResult = new ManufacturingSyncResult(ManufacturingSyncStatus.Unauthorized);
        await vm.SyncAsync();
        Assert.Equal(loc["Mfg_SignInRequired"], vm.StatusMessage);

        syncResult = new ManufacturingSyncResult(ManufacturingSyncStatus.Forbidden);
        await vm.SyncAsync();
        Assert.Equal(loc["Mfg_Forbidden"], vm.StatusMessage);
        Assert.True(vm.IsOffline);
    }

    /// <summary>
    /// Selecting a work centre shows its machines, its OEE headline and the six big losses.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectWorkCenter_ShowsMachinesAndOeeLosses()
    {
        cached = Pulled();
        var vm = Create();
        await vm.LoadAsync();

        Assert.True(vm.HasOee);
        Assert.False(vm.OeeMeetsTarget);
        Assert.Contains(Percent(66.67d), vm.OeeHeadline, StringComparison.Ordinal);
        Assert.Contains(Percent(85d), vm.OeeHeadline, StringComparison.Ordinal);
        Assert.Equal(6, vm.Losses.Count);
        Assert.Equal(loc["Mfg_LOSS_EQUIPMENT_FAILURE"], vm.Losses[0].Label);
        Assert.Equal(loc["Mfg_FACTOR_AVAILABILITY"], vm.Losses[0].Factor);

        vm.SelectedWorkCenter = vm.WorkCenters[1];

        Assert.Empty(vm.Machines);
        Assert.False(vm.HasOee);
        Assert.Empty(vm.Losses);
        _ = Assert.Single(vm.OeeRows);
    }

    /// <summary>
    /// The selected routing shows its steps in sequence with the linked components and the offline lead time and cost.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectRouting_ShowsStepsWithTheOfflineCalculation()
    {
        cached = Pulled();
        var vm = Create();
        await vm.LoadAsync();
        vm.Quantity = 10m;
        await vm.RecalculateAsync();

        Assert.Equal([10, 20], vm.Operations.Select(o => o.Sequence));
        var mill = vm.Operations[0];
        Assert.Equal("WC-01", mill.WorkCenter);
        Assert.Equal("M-01", mill.Machine);
        Assert.Equal(loc["Mfg_OPTYPE_INTERNAL_MACHINING"], mill.Type);
        Assert.Contains(string.Format(loc.CurrentCulture, loc["Mfg_LinkBom"], 1), mill.Links, StringComparison.Ordinal);
        Assert.Contains(loc["Mfg_LinkQc"], mill.Links, StringComparison.Ordinal);
        Assert.Contains(loc["Mfg_LinkWorkInstruction"], mill.Links, StringComparison.Ordinal);
        Assert.NotEqual("–", mill.Duration);
        Assert.NotEqual(string.Empty, vm.CalculationSummary);
        Assert.Contains("CHF", vm.CalculationSummary, StringComparison.Ordinal);
    }

    /// <summary>
    /// The overlap switch changes the calculated lead time.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ToggleOverlap_ChangesTheCalculation()
    {
        cached = Pulled();
        var vm = Create();
        await vm.LoadAsync();
        vm.Quantity = 10m;
        var withOverlap = vm.CalculationSummary;

        vm.IncludeOverlap = false;
        var withoutOverlap = vm.CalculationSummary;

        Assert.NotEqual(withOverlap, withoutOverlap);
    }

    /// <summary>
    /// A quantity that is not positive is reported and the steps are still listed without figures.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task InvalidQuantity_IsReported()
    {
        cached = Pulled();
        var vm = Create();
        await vm.LoadAsync();

        vm.Quantity = 0m;

        Assert.Equal(loc["Mfg_QuantityInvalid"], vm.CalculationSummary);
        Assert.Equal(2, vm.Operations.Count);
        Assert.All(vm.Operations, o => Assert.Equal("–", o.Duration));
    }

    /// <summary>
    /// A routing that refers to a work centre that is not cached is explained instead of failing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task MissingWorkCentre_IsExplained()
    {
        var pulled = Pulled();
        cached = pulled with { WorkCenters = [.. pulled.WorkCenters.Where(w => w.Id != ManufacturingTestData.WorkCenterB)] };
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(loc["Mfg_CalcMissingData"], vm.CalculationSummary);
        Assert.Equal(2, vm.Operations.Count);
    }

    private static string Percent(double value) => value.ToString("0.#", LocalizationService.Instance.CurrentCulture) + " %";

    private static ManufacturingOverview Empty() => new([], [], [], [], [], null);

    private static ManufacturingOverview Pulled() => new(
        ManufacturingParser.ParseWorkCenters(ManufacturingTestData.WorkCenters, SyncedAt),
        ManufacturingParser.ParseMachines(ManufacturingTestData.Machines, SyncedAt),
        ManufacturingParser.ParseCalendars(ManufacturingTestData.Calendars, SyncedAt),
        ManufacturingParser.ParseRoutings(ManufacturingTestData.Routings, SyncedAt),
        [ManufacturingParser.ParseOeeSnapshot(ManufacturingTestData.OeeSummary, SyncedAt)],
        SyncedAt);

    private ManufacturingViewModel Create() => new(sender.Object, loc);
}
