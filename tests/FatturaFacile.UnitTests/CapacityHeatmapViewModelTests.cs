// <copyright file="CapacityHeatmapViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Globalization;
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
/// Tests of the capacity heatmap view model: the window, the bands and the overload highlight of the cells, the filters and the failure notice.
/// </summary>
public sealed class CapacityHeatmapViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateOnly Monday = new(2026, 11, 16);

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private CapacityHeatmap heatmap;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapacityHeatmapViewModelTests"/> class.
    /// </summary>
    public CapacityHeatmapViewModelTests()
    {
        var anna = ResourcePlanningTestData.Row(
            ResourcePlanningTestData.Anna,
            "Anna Rossi",
            "Workshop",
            ResourcePlanningTestData.Cell(Monday, 40m, 20m, 0m, 50d, CapacityBand.Green),
            ResourcePlanningTestData.Cell(Monday.AddDays(7), 32m, 30m, 10m, 125d, CapacityBand.Red),
            ResourcePlanningTestData.Cell(Monday.AddDays(14), 0m, 0m, 0m, 0d, CapacityBand.None));
        var bruno = ResourcePlanningTestData.Row(
            ResourcePlanningTestData.Bruno,
            "Bruno Bianchi",
            null,
            ResourcePlanningTestData.Cell(Monday, 40m, 30m, 4m, 85d, CapacityBand.Yellow),
            ResourcePlanningTestData.Cell(Monday.AddDays(7), 40m, 40m, 0m, 100d, CapacityBand.Yellow),
            ResourcePlanningTestData.Cell(Monday.AddDays(14), 40m, 0m, 0m, 0d, CapacityBand.Green));
        heatmap = ResourcePlanningTestData.Heatmap(anna, bruno);
        IReadOnlyList<PlanningResource> resources =
        [
            ResourcePlanningTestData.Resource(ResourcePlanningTestData.Anna, "Anna Rossi", ResourcePlanningTestData.Department, "Workshop"),
            ResourcePlanningTestData.Resource(ResourcePlanningTestData.Bruno, "Bruno Bianchi"),
        ];
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningResourcesQuery>(), Any)).ReturnsAsync(resources);
        _ = sender.Setup(s => s.Send(It.IsAny<GetCapacityHeatmapQuery>(), Any)).ReturnsAsync(() => heatmap);
    }

    /// <summary>
    /// The window starts on the Monday of the current week and shows eight weeks.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_AsksForEightWeeksFromTheMonday()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(Monday.ToDateTime(TimeOnly.MinValue), vm.FromDate);
        sender.Verify(s => s.Send(It.Is<GetCapacityHeatmapQuery>(q => q.From == Monday && q.To == Monday.AddDays(55) && q.Granularity == HeatmapGranularity.Week && q.DepartmentId == null), Any), Times.Once);
    }

    /// <summary>
    /// Cells are green below 80 percent, yellow up to 100, red above; only the red cell is highlighted as overload and an empty cell shows a dash.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ColoursTheCellsAndHighlightsOverload()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(2, vm.Rows.Count);
        Assert.Equal(3, vm.PeriodHeaders.Count);
        var anna = vm.Rows[0];
        Assert.Equal(new[] { "Green", "Red", "None" }, anna.Cells.Select(cell => cell.BandName));
        Assert.Equal(new[] { false, true, false }, anna.Cells.Select(cell => cell.IsOverloaded));
        Assert.True(anna.HasOverload);
        Assert.Equal("125%", anna.Cells[1].LoadText);
        Assert.Equal("-", anna.Cells[2].LoadText);
        var bruno = vm.Rows[1];
        Assert.Equal(new[] { "Yellow", "Yellow", "Green" }, bruno.Cells.Select(cell => cell.BandName));
        Assert.False(bruno.HasOverload);
        Assert.Equal("-", bruno.DepartmentName);
        Assert.True(vm.HasData);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_Heatmap_Summary"], "2", "1", Monday.ToString("d", loc.CurrentCulture) + " - " + new DateOnly(2026, 12, 6).ToString("d", loc.CurrentCulture)), vm.SummaryText);
    }

    /// <summary>
    /// The tooltip of a cell lists the day and the hours behind the load.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_TooltipShowsTheHours()
    {
        var vm = Create();

        await vm.LoadAsync();

        var tip = vm.Rows[0].Cells[1].ToolTip;
        Assert.Contains(Monday.AddDays(7).ToString("d", loc.CurrentCulture), tip, StringComparison.Ordinal);
        Assert.Contains(string.Format(loc.CurrentCulture, loc["ResourcePlanning_HoursFormat"], "32"), tip, StringComparison.Ordinal);
        Assert.Contains(string.Format(loc.CurrentCulture, loc["ResourcePlanning_HoursFormat"], "30"), tip, StringComparison.Ordinal);
    }

    /// <summary>
    /// The day granularity asks for two weeks and the department choice is passed on.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_DayGranularityAndDepartment_AreSent()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedGranularity = vm.GranularityChoices[1];
        vm.SelectedDepartment = vm.DepartmentChoices.Single(choice => choice.Key == ResourcePlanningTestData.Department.ToString("D", CultureInfo.InvariantCulture));

        await vm.LoadAsync();

        Assert.Equal(2, vm.DepartmentChoices.Count);
        sender.Verify(s => s.Send(It.Is<GetCapacityHeatmapQuery>(q => q.To == Monday.AddDays(13) && q.Granularity == HeatmapGranularity.Day && q.DepartmentId == ResourcePlanningTestData.Department), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetPlanningResourcesQuery>(), Any), Times.Once);
    }

    /// <summary>
    /// An answer without rows shows the empty text and hides the grid.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NoRows_ShowsTheEmptyText()
    {
        heatmap = ResourcePlanningTestData.Heatmap();
        var vm = Create();

        await vm.LoadAsync();

        Assert.False(vm.HasData);
        Assert.Equal(loc["ResourcePlanning_Heatmap_Empty"], vm.SummaryText);
    }

    /// <summary>
    /// A server that cannot be reached shows a notice and the view stops being busy.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ServerError_ShowsTheNotice()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetCapacityHeatmapQuery>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// A permission failure shows its message.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Forbidden_ShowsTheMessage()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningResourcesQuery>(), Any)).ThrowsAsync(new UnauthorizedAccessException("no access"));
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal("no access", vm.StatusMessage);
    }

    /// <summary>
    /// The week of a day starts on the Monday before it.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="day">The day.</param>
    /// <param name="expectedDay">The Monday of that week in the same month.</param>
    [Theory]
    [InlineData(2026, 11, 16, 16)]
    [InlineData(2026, 11, 18, 16)]
    [InlineData(2026, 11, 22, 16)]
    [InlineData(2026, 11, 23, 23)]
    public void MondayOf_IsTheStartOfTheIsoWeek(int year, int month, int day, int expectedDay)
    {
        Assert.Equal(new DateOnly(year, month, expectedDay), CapacityHeatmapViewModel.MondayOf(new DateOnly(year, month, day)));
    }

    private CapacityHeatmapViewModel Create() => new(sender.Object, loc, time);
}
