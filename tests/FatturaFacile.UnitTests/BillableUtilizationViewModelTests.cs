// <copyright file="BillableUtilizationViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Net.Http;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the billable utilization view model: target versus actual, the gap, the status and the grouping that is requested.
/// </summary>
public sealed class BillableUtilizationViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private UtilizationReport report;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillableUtilizationViewModelTests"/> class.
    /// </summary>
    public BillableUtilizationViewModelTests()
    {
        var below = ResourcePlanningTestData.Utilization("Anna Rossi", 62.5d, 75d, 70d, UtilizationStatus.BelowTarget) with { GapPercent = -7.5d };
        var above = ResourcePlanningTestData.Utilization("Bruno Bianchi", 130d, 140d, 70d, UtilizationStatus.AboveTarget) with { GapPercent = 60d };
        var on = ResourcePlanningTestData.Utilization("Carla Verdi", 68d, 70d, 70d, UtilizationStatus.OnTarget) with { GapPercent = -2d };
        report = new UtilizationReport(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), UtilizationGroupBy.Resource, 70d, [below, above, on], ResourcePlanningTestData.Utilization("Total", 70d, 80d, 70d, UtilizationStatus.OnTarget) with { GapPercent = 0d });
        _ = sender.Setup(s => s.Send(It.IsAny<GetBillableUtilizationQuery>(), Any)).ReturnsAsync(() => report);
    }

    /// <summary>
    /// The report of the current month is requested grouped by resource.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_AsksForTheCurrentMonthByResource()
    {
        var vm = Create();

        await vm.LoadAsync();

        sender.Verify(s => s.Send(It.Is<GetBillableUtilizationQuery>(q => q.From == new DateOnly(2026, 11, 1) && q.To == new DateOnly(2026, 11, 30) && q.GroupBy == UtilizationGroupBy.Resource), Any), Times.Once);
    }

    /// <summary>
    /// Each row shows actual, forecast, target and gap with the status, and the bar is limited to 100.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsActualAgainstTarget()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(3, vm.Rows.Count);
        var below = vm.Rows[0];
        Assert.Equal("BelowTarget", below.StatusName);
        Assert.Equal(loc["ResourcePlanning_Status_BelowTarget"], below.StatusText);
        Assert.Equal(Pct(62.5d), below.ActualText);
        Assert.Equal(Pct(75d), below.ForecastText);
        Assert.Equal(Pct(70d), below.TargetText);
        Assert.Equal(Pct(-7.5d), below.GapText);
        Assert.Equal(62.5d, below.ActualValue);
        var above = vm.Rows[1];
        Assert.Equal("AboveTarget", above.StatusName);
        Assert.Equal("+" + Pct(60d), above.GapText);
        Assert.Equal(100d, above.ActualValue);
        Assert.Equal("OnTarget", vm.Rows[2].StatusName);
        Assert.True(vm.HasData);
    }

    /// <summary>
    /// The totals line shows the totals and the default target.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheTotals()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_Utilization_Totals"], Pct(70d), Pct(80d), Pct(70d), Pct(70d)), vm.TotalsText);
    }

    /// <summary>
    /// The selected grouping is sent and a report without rows shows the empty text.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ByCostCenterWithoutRows_ShowsTheEmptyText()
    {
        report = report with { GroupBy = UtilizationGroupBy.CostCenter, Rows = [] };
        var vm = Create();
        vm.SelectedGroup = vm.GroupChoices[2];

        await vm.LoadAsync();

        sender.Verify(s => s.Send(It.Is<GetBillableUtilizationQuery>(q => q.GroupBy == UtilizationGroupBy.CostCenter), Any), Times.Once);
        Assert.False(vm.HasData);
        Assert.Equal(loc["ResourcePlanning_Utilization_Empty"], vm.TotalsText);
    }

    /// <summary>
    /// A server that cannot be reached shows the load failure.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ServerDown_ShowsTheFailure()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetBillableUtilizationQuery>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    private string Pct(double value) => value.ToString("0.#", loc.CurrentCulture) + "%";

    private BillableUtilizationViewModel Create() => new(sender.Object, loc, time);
}
