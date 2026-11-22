// <copyright file="AllocationBoardViewModelTests.cs" company="Fattura Facile">
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
/// Tests of the allocation board and of the helpers of the drag and drop: the weekly load the cells show, the window that is asked
/// for, and the identifiers the forms resolve.
/// </summary>
public sealed class AllocationBoardViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;

    /// <summary>
    /// The board asks for eight weeks from the Monday of the day and shows each employee with the load and band of every week.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheWeeklyLoadOfTheEmployees()
    {
        var week1 = new DateOnly(2026, 11, 16);
        var heatmap = ResourcePlanningTestData.Heatmap(
            ResourcePlanningTestData.Row(
                ResourcePlanningTestData.Anna,
                "Anna Rossi",
                "Workshop",
                ResourcePlanningTestData.Cell(week1, 40m, 20m, 0m, 50d, CapacityBand.Green),
                ResourcePlanningTestData.Cell(week1.AddDays(7), 40m, 30m, 20m, 125d, CapacityBand.Red),
                ResourcePlanningTestData.Cell(week1.AddDays(14), 0m, 0m, 0m, 0d, CapacityBand.None)));
        _ = sender.Setup(s => s.Send(It.IsAny<GetCapacityHeatmapQuery>(), Any)).ReturnsAsync(heatmap);
        var vm = new AllocationBoardViewModel(sender.Object, loc);

        await vm.LoadAsync(new DateOnly(2026, 11, 19));

        sender.Verify(s => s.Send(It.Is<GetCapacityHeatmapQuery>(q => q.From == week1 && q.To == new DateOnly(2027, 1, 10) && q.Granularity == HeatmapGranularity.Week && q.DepartmentId == null), Any), Times.Once);
        Assert.True(vm.HasData);
        Assert.Equal(3, vm.PeriodHeaders.Count);
        var row = Assert.Single(vm.Rows);
        Assert.Equal("Anna Rossi", row.EmployeeName);
        Assert.Equal(new[] { "Green", "Red", "None" }, row.Cells.Select(cell => cell.BandName));
        Assert.Equal(new[] { false, true, false }, row.Cells.Select(cell => cell.IsOverloaded));
        Assert.Equal("-", row.Cells[2].LoadText);
        Assert.All(row.Cells, cell => Assert.Equal(ResourcePlanningTestData.Anna, cell.EmployeeId));
        Assert.Equal(new[] { week1, week1.AddDays(7), week1.AddDays(14) }, row.Cells.Select(cell => cell.WeekStart));
        Assert.Contains("Anna Rossi", row.Cells[0].ToolTip, StringComparison.Ordinal);
        Assert.Equal(string.Empty, vm.StatusMessage);
    }

    /// <summary>
    /// A heatmap without employees leaves the board empty.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NoEmployees_HidesTheBoard()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetCapacityHeatmapQuery>(), Any)).ReturnsAsync(ResourcePlanningTestData.Heatmap());
        var vm = new AllocationBoardViewModel(sender.Object, loc);

        await vm.LoadAsync(new DateOnly(2026, 11, 16));

        Assert.False(vm.HasData);
        Assert.Empty(vm.Rows);
    }

    /// <summary>
    /// A server that cannot be reached, a missing right or an unreadable answer is explained and keeps the board from lying.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Failures_AreExplained()
    {
        var vm = new AllocationBoardViewModel(sender.Object, loc);
        _ = sender.SetupSequence(s => s.Send(It.IsAny<GetCapacityHeatmapQuery>(), Any))
            .ThrowsAsync(new HttpRequestException("down"))
            .ThrowsAsync(new UnauthorizedAccessException("no right"))
            .ThrowsAsync(new System.Text.Json.JsonException("bad"));

        await vm.LoadAsync(new DateOnly(2026, 11, 16));
        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);

        await vm.LoadAsync(new DateOnly(2026, 11, 16));
        Assert.Equal("no right", vm.StatusMessage);

        await vm.LoadAsync(new DateOnly(2026, 11, 16));
        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
    }

    /// <summary>
    /// An identifier is taken from the entry picked from a list, else from the text typed in; a text that is not an identifier is refused.
    /// </summary>
    [Fact]
    public void PlanningIds_ResolvesPickedThenTyped()
    {
        var picked = Guid.NewGuid();
        var typed = Guid.NewGuid();

        Assert.True(PlanningIds.TryResolve(picked.ToString(), typed.ToString(), out var first));
        Assert.Equal(picked, first);
        Assert.True(PlanningIds.TryResolve(string.Empty, $"  {typed}  ", out var second));
        Assert.Equal(typed, second);
        Assert.True(PlanningIds.TryResolve(null, " ", out var none));
        Assert.Null(none);
        Assert.False(PlanningIds.TryResolve(string.Empty, "nonsense", out var bad));
        Assert.Null(bad);
    }
}
