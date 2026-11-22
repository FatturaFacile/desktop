// <copyright file="AllocationsViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Net;
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
/// Tests of the allocations view model: the list, the soft to hard conversion, the rebalance strategies, overload feedback and refusals.
/// </summary>
public sealed class AllocationsViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private readonly ResourceAllocation soft = ResourcePlanningTestData.Allocation(ResourcePlanningTestData.AllocationOne, ResourceBookingType.SoftTentative, projectId: ResourcePlanningTestData.ProjectA);
    private readonly ResourceAllocation tender = ResourcePlanningTestData.Allocation(ResourcePlanningTestData.AllocationTwo, ResourceBookingType.SoftTentative, employeeId: ResourcePlanningTestData.Bruno, employeeName: "Bruno Bianchi", projectName: null, opportunityId: ResourcePlanningTestData.Opportunity, opportunityName: "Tender 7", overloaded: true, peak: 118d);
    private readonly ResourceAllocation hard = ResourcePlanningTestData.Allocation(Guid.NewGuid(), ResourceBookingType.HardCommitted, ResourcePlanningTestData.ProjectA, employeeName: "Carla Verdi");
    private PlanningCommandOutcome<AllocationMutationResult> outcome;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllocationsViewModelTests"/> class.
    /// </summary>
    public AllocationsViewModelTests()
    {
        IReadOnlyList<ResourceAllocation> allocations = [tender, hard, soft];
        IReadOnlyList<PlanningResource> resources =
        [
            ResourcePlanningTestData.Resource(ResourcePlanningTestData.Bruno, "Bruno Bianchi"),
            ResourcePlanningTestData.Resource(ResourcePlanningTestData.Anna, "Anna Rossi"),
        ];
        outcome = Success(soft);
        _ = sender.Setup(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any)).ReturnsAsync(allocations);
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningResourcesQuery>(), Any)).ReturnsAsync(resources);
        _ = sender.Setup(s => s.Send(It.IsAny<ConvertAllocationToHardCommand>(), Any)).ReturnsAsync(() => outcome);
        _ = sender.Setup(s => s.Send(It.IsAny<RebalanceAllocationCommand>(), Any)).ReturnsAsync(() => outcome);
    }

    /// <summary>
    /// The list shows the allocations by start day and employee with the booking type, the reference and the overload mark.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheAllocations()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(3, vm.Rows.Count);
        Assert.Equal(new[] { "Anna Rossi", "Bruno Bianchi", "Carla Verdi" }, vm.Rows.Select(row => row.Allocation.EmployeeName));
        var tenderRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationTwo);
        Assert.True(tenderRow.IsSoft);
        Assert.True(tenderRow.IsOverloaded);
        Assert.Equal("Tender 7", tenderRow.ReferenceText);
        Assert.Equal(loc["ResourcePlanning_Booking_SoftTentative"], tenderRow.BookingText);
        var hardRow = vm.Rows.Single(row => row.Allocation.EmployeeName == "Carla Verdi");
        Assert.False(hardRow.IsSoft);
        Assert.Equal("Plant retrofit", hardRow.ReferenceText);
        Assert.Equal(loc["ResourcePlanning_Booking_HardCommitted"], hardRow.BookingText);
        Assert.Equal(2, vm.EmployeeChoices.Count);
        Assert.True(vm.HasData);
    }

    /// <summary>
    /// The booking type filter is sent to the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_SendsTheBookingFilter()
    {
        var vm = Create();
        vm.SelectedBookingFilter = vm.BookingFilters[2];

        await vm.LoadAsync();

        sender.Verify(s => s.Send(It.Is<GetResourceAllocationsQuery>(q => q.BookingType == ResourceBookingType.HardCommitted), Any), Times.Once);
    }

    /// <summary>
    /// Selecting a soft booking enables the conversion and fills the form with the values of the allocation.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Select_SoftBooking_EnablesConversionAndPrefillsTheForm()
    {
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationOne);

        Assert.True(vm.HasSelection);
        Assert.True(vm.CanConvert);
        Assert.False(vm.NeedsProjectForConversion);
        Assert.Equal(new DateTime(2026, 11, 16), vm.NewStartDate);
        Assert.Equal(new DateTime(2026, 11, 27), vm.NewEndDate);
        Assert.Equal(6m.ToString("0.##", loc.CurrentCulture), vm.HoursPerDayText);
    }

    /// <summary>
    /// A soft booking with a project is converted without typing anything and the list is reloaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertToHard_WithProject_SendsTheCommandAndReloads()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationOne);

        await vm.ConvertToHardAsync();

        sender.Verify(s => s.Send(It.Is<ConvertAllocationToHardCommand>(c => c.AllocationId == ResourcePlanningTestData.AllocationOne && c.Conversion.ProjectId == null), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any), Times.Exactly(2));
        Assert.Equal(loc["ResourcePlanning_Converted"], vm.StatusMessage);
        Assert.Equal(string.Empty, vm.OverloadMessage);
    }

    /// <summary>
    /// A soft booking that only has an opportunity needs a project: without it nothing is sent, with it the project is passed on.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertToHard_OpportunityOnly_NeedsAProject()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationTwo);
        Assert.True(vm.NeedsProjectForConversion);

        await vm.ConvertToHardAsync();

        Assert.Equal(loc["ResourcePlanning_Convert_NeedsProject"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<ConvertAllocationToHardCommand>(), Any), Times.Never);

        vm.ConvertProjectIdText = ResourcePlanningTestData.ProjectA.ToString();
        await vm.ConvertToHardAsync();

        sender.Verify(s => s.Send(It.Is<ConvertAllocationToHardCommand>(c => c.AllocationId == ResourcePlanningTestData.AllocationTwo && c.Conversion.ProjectId == ResourcePlanningTestData.ProjectA), Any), Times.Once);
    }

    /// <summary>
    /// A hard booking or no selection cannot be converted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertToHard_HardBooking_IsNotSent()
    {
        var vm = Create();
        await vm.LoadAsync();

        await vm.ConvertToHardAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.EmployeeName == "Carla Verdi");
        await vm.ConvertToHardAsync();

        Assert.Equal(loc["ResourcePlanning_Convert_NeedsSoft"], vm.StatusMessage);
        Assert.False(vm.CanConvert);
        sender.Verify(s => s.Send(It.IsAny<ConvertAllocationToHardCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A shift by days is sent as a rebalance and the overload that comes back is shown with its peak.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Rebalance_Shift_SendsTheDaysAndShowsTheOverload()
    {
        outcome = Success(soft, new OverloadSummary(true, 112.5d, [new OverloadWeek(new DateOnly(2026, 11, 23), 112.5d)]));
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationOne);
        vm.SelectedStrategy = vm.StrategyChoices[0];
        vm.ShiftDaysText = "7";
        vm.NewStartDate = null;

        await vm.RebalanceAsync();

        sender.Verify(s => s.Send(It.Is<RebalanceAllocationCommand>(c => c.AllocationId == ResourcePlanningTestData.AllocationOne && c.Rebalance.Strategy == RebalanceStrategy.Shift && c.Rebalance.ShiftDays == 7 && c.Rebalance.StartDate == null), Any), Times.Once);
        Assert.Equal(loc["ResourcePlanning_Rebalanced"], vm.StatusMessage);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_OverloadWarning"], 112.5d.ToString("0.#", loc.CurrentCulture) + "%"), vm.OverloadMessage);
    }

    /// <summary>
    /// Reassigning gives the allocation to the chosen colleague and a warning of the server is shown in the language.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Rebalance_Reassign_SendsTheColleagueAndShowsWarnings()
    {
        outcome = Success(soft, null, new PlanningIssue("skill_certificate_invalid", null));
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationOne);
        vm.SelectedStrategy = vm.StrategyChoices[1];
        vm.SelectedEmployee = vm.EmployeeChoices.Single(choice => choice.Label == "Bruno Bianchi");

        await vm.RebalanceAsync();

        sender.Verify(s => s.Send(It.Is<RebalanceAllocationCommand>(c => c.Rebalance.Strategy == RebalanceStrategy.Reassign && c.Rebalance.EmployeeId == ResourcePlanningTestData.Bruno), Any), Times.Once);
        Assert.Equal(loc["ResourcePlanning_Warning_skill_certificate_invalid"], vm.OverloadMessage);
    }

    /// <summary>
    /// A split and a resize go through the builder with their own parameters.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Rebalance_SplitAndResize_SendTheirParameters()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationOne);
        vm.SelectedStrategy = vm.StrategyChoices[2];
        vm.SplitDate = new DateTime(2026, 11, 23);

        await vm.RebalanceAsync();

        vm.SelectedStrategy = vm.StrategyChoices[3];
        vm.HoursPerDayText = "4";
        await vm.RebalanceAsync();

        sender.Verify(s => s.Send(It.Is<RebalanceAllocationCommand>(c => c.Rebalance.Strategy == RebalanceStrategy.Split && c.Rebalance.SplitDate == new DateOnly(2026, 11, 23) && c.Rebalance.StartDate == null), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<RebalanceAllocationCommand>(c => c.Rebalance.Strategy == RebalanceStrategy.Resize && c.Rebalance.AllocatedHoursPerDay == 4m && c.Rebalance.EndDate == null), Any), Times.Once);
    }

    /// <summary>
    /// A draft that is not valid is explained and not sent; no selection is explained too.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Rebalance_InvalidDraft_IsExplainedAndNotSent()
    {
        var vm = Create();
        await vm.LoadAsync();

        await vm.RebalanceAsync();
        Assert.Equal(loc["ResourcePlanning_Rebalance_NeedsSelection"], vm.StatusMessage);

        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationOne);
        vm.SelectedStrategy = vm.StrategyChoices[1];
        await vm.RebalanceAsync();

        Assert.Equal(loc["ResourcePlanning_Rebalance_reassign_needs_colleague"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<RebalanceAllocationCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A refusal of the server is shown with the texts of its violation codes and nothing is reloaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Rebalance_Refused_ShowsTheViolations()
    {
        outcome = new PlanningCommandOutcome<AllocationMutationResult>(HttpStatusCode.UnprocessableEntity, null, [new PlanningIssue("overload", null), new PlanningIssue("no_working_days", null)]);
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationOne);
        vm.ShiftDaysText = "3";
        vm.NewStartDate = null;

        await vm.RebalanceAsync();

        Assert.Equal(loc["ResourcePlanning_Error_overload"] + "; " + loc["ResourcePlanning_Error_no_working_days"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any), Times.Once);
    }

    /// <summary>
    /// A refusal without violations shows the general text with the status and an unknown code is shown as it is.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Rebalance_RefusedWithoutIssues_ShowsTheStatus()
    {
        outcome = new PlanningCommandOutcome<AllocationMutationResult>(HttpStatusCode.Forbidden, null, []);
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationOne);
        vm.ShiftDaysText = "3";
        vm.NewStartDate = null;

        await vm.RebalanceAsync();

        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_Error_Generic"], "403"), vm.StatusMessage);
    }

    /// <summary>
    /// A server that cannot be reached shows the save failure and the view stops being busy.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Rebalance_ServerDown_ShowsTheSaveFailure()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<RebalanceAllocationCommand>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationOne);
        vm.ShiftDaysText = "3";
        vm.NewStartDate = null;

        await vm.RebalanceAsync();

        Assert.Equal(loc["ResourcePlanning_SaveFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// A list that cannot be loaded shows the load failure.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ServerDown_ShowsTheLoadFailure()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
        Assert.Empty(vm.Rows);
    }

    private static PlanningCommandOutcome<AllocationMutationResult> Success(ResourceAllocation allocation, OverloadSummary? overload = null, params PlanningIssue[] warnings) =>
        new(HttpStatusCode.OK, ResourcePlanningTestData.Mutation(allocation, overload, warnings), []);

    private AllocationsViewModel Create() => new(sender.Object, loc, time);
}
