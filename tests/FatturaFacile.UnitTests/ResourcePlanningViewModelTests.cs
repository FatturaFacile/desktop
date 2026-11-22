// <copyright file="ResourcePlanningViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Mod 079 host view model: each tab loads its own data when it is shown.
/// </summary>
public sealed class ResourcePlanningViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePlanningViewModelTests"/> class.
    /// </summary>
    public ResourcePlanningViewModelTests()
    {
        IReadOnlyList<PlanningResource> resources = [];
        IReadOnlyList<ResourceAllocation> allocations = [];
        IReadOnlyList<PlanningSkill> skills = [];
        IReadOnlyList<EmployeeSkillProfile> profiles = [];
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningResourcesQuery>(), Any)).ReturnsAsync(resources);
        _ = sender.Setup(s => s.Send(It.IsAny<GetCapacityHeatmapQuery>(), Any)).ReturnsAsync(ResourcePlanningTestData.Heatmap());
        _ = sender.Setup(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any)).ReturnsAsync(allocations);
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningSkillsQuery>(), Any)).ReturnsAsync(skills);
        _ = sender.Setup(s => s.Send(It.IsAny<GetEmployeeSkillProfilesQuery>(), Any)).ReturnsAsync(profiles);
        _ = sender.Setup(s => s.Send(It.IsAny<GetBillableUtilizationQuery>(), Any)).ReturnsAsync(new UtilizationReport(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), UtilizationGroupBy.Resource, 70d, [], ResourcePlanningTestData.Utilization("Total", 0d, 0d, 70d, UtilizationStatus.BelowTarget)));
    }

    /// <summary>
    /// The first tab is the heatmap and loading it asks for the heatmap only.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_FirstTab_LoadsTheHeatmapOnly()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(ResourcePlanningViewModel.HeatmapTab, vm.SelectedTabIndex);
        sender.Verify(s => s.Send(It.IsAny<GetCapacityHeatmapQuery>(), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any), Times.Never);
        sender.Verify(s => s.Send(It.IsAny<GetBillableUtilizationQuery>(), Any), Times.Never);
    }

    /// <summary>
    /// Showing another tab loads the data of that tab.
    /// </summary>
    /// <param name="tab">The tab index.</param>
    [Theory]
    [InlineData(ResourcePlanningViewModel.AllocationsTab)]
    [InlineData(ResourcePlanningViewModel.CandidatesTab)]
    [InlineData(ResourcePlanningViewModel.UtilizationTab)]
    [InlineData(ResourcePlanningViewModel.SkillsTab)]
    public async Task SelectingATab_LoadsItsData(int tab)
    {
        var vm = Create();

        vm.SelectedTabIndex = tab;
        await vm.LoadAsync();

        switch (tab)
        {
            case ResourcePlanningViewModel.AllocationsTab:
                sender.Verify(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any), Times.AtLeastOnce);
                break;
            case ResourcePlanningViewModel.CandidatesTab:
                sender.Verify(s => s.Send(It.IsAny<GetPlanningSkillsQuery>(), Any), Times.AtLeastOnce);
                break;
            case ResourcePlanningViewModel.UtilizationTab:
                sender.Verify(s => s.Send(It.IsAny<GetBillableUtilizationQuery>(), Any), Times.AtLeastOnce);
                break;
            default:
                sender.Verify(s => s.Send(It.IsAny<GetEmployeeSkillProfilesQuery>(), Any), Times.AtLeastOnce);
                break;
        }

        sender.Verify(s => s.Send(It.IsAny<GetCapacityHeatmapQuery>(), Any), Times.Never);
    }

    private ResourcePlanningViewModel Create() =>
        new(
            new CapacityHeatmapViewModel(sender.Object, loc, time),
            new AllocationsViewModel(sender.Object, loc, time),
            new CandidateSuggestionsViewModel(sender.Object, loc, time),
            new BillableUtilizationViewModel(sender.Object, loc, time),
            new SkillMatrixViewModel(sender.Object, loc));
}
