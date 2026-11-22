// <copyright file="AllocationsEditingViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Net;
using System.Net.Http;
using FatturaFacile.Application.Features.ProjectEvm;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ProjectEvm;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the editing side of the allocations view model: the form that creates an allocation with its project picker, the deletion
/// with its confirmation, the project picker of the conversion, and the drag and drop of an allocation onto a week of the board.
/// </summary>
public sealed class AllocationsEditingViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid ProjectB = Guid.NewGuid();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private readonly ResourceAllocation soft = ResourcePlanningTestData.Allocation(ResourcePlanningTestData.AllocationOne, ResourceBookingType.SoftTentative, projectId: ResourcePlanningTestData.ProjectA);
    private readonly ResourceAllocation tender = ResourcePlanningTestData.Allocation(ResourcePlanningTestData.AllocationTwo, ResourceBookingType.SoftTentative, employeeId: ResourcePlanningTestData.Bruno, employeeName: "Bruno Bianchi", projectName: null, opportunityId: ResourcePlanningTestData.Opportunity, opportunityName: "Tender 7");
    private PlanningCommandOutcome<AllocationMutationResult> outcome;
    private PlanningCommandOutcome<bool> deleteOutcome = new(HttpStatusCode.NoContent, true, []);

    /// <summary>
    /// Initializes a new instance of the <see cref="AllocationsEditingViewModelTests"/> class.
    /// </summary>
    public AllocationsEditingViewModelTests()
    {
        IReadOnlyList<ResourceAllocation> allocations = [tender, soft];
        IReadOnlyList<PlanningResource> resources =
        [
            ResourcePlanningTestData.Resource(ResourcePlanningTestData.Bruno, "Bruno Bianchi"),
            ResourcePlanningTestData.Resource(ResourcePlanningTestData.Anna, "Anna Rossi"),
        ];
        IReadOnlyList<PlanningSkill> skills =
        [
            new PlanningSkill(ResourcePlanningTestData.SkillWelding, "Site", "Welding", true, "WELD-EN"),
            new PlanningSkill(ResourcePlanningTestData.SkillCrane, "Site", "Crane", false, null),
        ];
        IReadOnlyList<ProjectEvmProject> projects =
        [
            new ProjectEvmProject(ResourcePlanningTestData.ProjectA.ToString(), "P-1", "Plant retrofit"),
            new ProjectEvmProject(ProjectB.ToString(), "P-2", "Warehouse"),
            new ProjectEvmProject("not-a-guid", "P-3", "Legacy"),
        ];
        outcome = Success(soft);
        _ = sender.Setup(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any)).ReturnsAsync(allocations);
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningResourcesQuery>(), Any)).ReturnsAsync(resources);
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningSkillsQuery>(), Any)).ReturnsAsync(skills);
        _ = sender.Setup(s => s.Send(It.IsAny<GetProjectEvmProjectsQuery>(), Any)).ReturnsAsync(projects);
        _ = sender.Setup(s => s.Send(It.IsAny<GetCapacityHeatmapQuery>(), Any)).ReturnsAsync(ResourcePlanningTestData.Heatmap());
        _ = sender.Setup(s => s.Send(It.IsAny<CreateResourceAllocationCommand>(), Any)).ReturnsAsync(() => outcome);
        _ = sender.Setup(s => s.Send(It.IsAny<ConvertAllocationToHardCommand>(), Any)).ReturnsAsync(() => outcome);
        _ = sender.Setup(s => s.Send(It.IsAny<RebalanceAllocationCommand>(), Any)).ReturnsAsync(() => outcome);
        _ = sender.Setup(s => s.Send(It.IsAny<DeleteResourceAllocationCommand>(), Any)).ReturnsAsync(() => deleteOutcome);
    }

    /// <summary>
    /// Loading fills the choices of the form: employees, the skills to require and the projects of the cache (those with a valid identifier).
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_FillsTheChoicesOfTheForm()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(2, vm.Form.EmployeeChoices.Count);
        Assert.Equal(new[] { "Site / Crane", "Site / Welding" }, vm.Form.SkillChoices.Select(choice => choice.Label));
        Assert.True(vm.Form.HasSkills);
        Assert.True(vm.Form.HasProjectPicker);
        Assert.Equal(new[] { loc["ResourcePlanning_Project_None"], "P-1 · Plant retrofit", "P-2 · Warehouse" }, vm.Form.ProjectChoices.Select(choice => choice.Label));
        Assert.True(vm.Form.IsSoft);
    }

    /// <summary>
    /// The choices are loaded once, not on every refresh.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Twice_LoadsTheChoicesOnce()
    {
        var vm = Create();

        await vm.LoadAsync();
        await vm.LoadAsync();

        sender.Verify(s => s.Send(It.IsAny<GetPlanningSkillsQuery>(), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetProjectEvmProjectsQuery>(), Any), Times.Once);
        Assert.Equal(3, vm.Form.ProjectChoices.Count);
    }

    /// <summary>
    /// A user who may not read the profitability cache gets an identifier field instead of the picker.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ProjectsNotReadable_FallsBackToTheTypedIdentifier()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetProjectEvmProjectsQuery>(), Any)).ThrowsAsync(new UnauthorizedAccessException("no access"));
        var vm = Create();

        await vm.LoadAsync();

        Assert.False(vm.Form.HasProjectPicker);
        Assert.Single(vm.Form.ProjectChoices);
        Assert.Equal(2, vm.Rows.Count);
        Assert.NotEqual("no access", vm.StatusMessage);
    }

    /// <summary>
    /// A complete form creates the allocation with the project picked from the list and the skills ticked, shows the warnings of the
    /// server, reloads the list and clears what describes the booking.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_SendsTheFormAndResetsIt()
    {
        outcome = Success(soft, new OverloadSummary(true, 105d, [new OverloadWeek(new DateOnly(2026, 11, 23), 105d)]));
        var vm = Create();
        await vm.LoadAsync();
        vm.Form.SelectedEmployee = vm.Form.EmployeeChoices.Single(choice => choice.Label == "Anna Rossi");
        vm.Form.SelectedBookingType = vm.Form.BookingChoices[1];
        vm.Form.SelectedProject = vm.Form.ProjectChoices.Single(choice => choice.Label.StartsWith("P-1", StringComparison.Ordinal));
        vm.Form.SkillChoices.Single(choice => choice.Label == "Site / Welding").IsSelected = true;
        vm.Form.Notes = " night shift ";
        vm.Form.IsBillable = false;
        vm.Form.AllowOverload = false;
        vm.Form.HoursPerDayText = 7.5m.ToString(loc.CurrentCulture);

        await vm.CreateAsync();

        sender.Verify(
            s => s.Send(
                It.Is<CreateResourceAllocationCommand>(c =>
                    c.Allocation.EmployeeId == ResourcePlanningTestData.Anna
                    && c.Allocation.BookingType == ResourceBookingType.HardCommitted
                    && c.Allocation.ProjectId == ResourcePlanningTestData.ProjectA
                    && c.Allocation.CrmOpportunityId == null
                    && c.Allocation.AllocatedHoursPerDay == 7.5m
                    && !c.Allocation.IsBillable
                    && !c.Allocation.AllowOverload
                    && c.Allocation.Notes == "night shift"
                    && c.Allocation.StartDate == new DateOnly(2026, 11, 21)
                    && c.Allocation.EndDate == new DateOnly(2026, 12, 4)
                    && c.Allocation.RequiredSkillIds != null
                    && c.Allocation.RequiredSkillIds.Count == 1
                    && c.Allocation.RequiredSkillIds[0] == ResourcePlanningTestData.SkillWelding),
                Any),
            Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any), Times.Exactly(2));
        Assert.Equal(loc["ResourcePlanning_Created"], vm.StatusMessage);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_OverloadWarning"], 105d.ToString("0.#", loc.CurrentCulture) + "%"), vm.OverloadMessage);
        Assert.Equal(vm.Form.ProjectChoices[0], vm.Form.SelectedProject);
        Assert.Equal(string.Empty, vm.Form.Notes);
        Assert.All(vm.Form.SkillChoices, choice => Assert.False(choice.IsSelected));
        Assert.Equal("Anna Rossi", vm.Form.SelectedEmployee?.Label);
    }

    /// <summary>
    /// A soft booking with an opportunity typed in is created without a project.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_SoftBookingWithOpportunity_SendsTheOpportunity()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.Form.SelectedEmployee = vm.Form.EmployeeChoices[0];
        vm.Form.OpportunityIdText = ResourcePlanningTestData.Opportunity.ToString();

        await vm.CreateAsync();

        sender.Verify(s => s.Send(It.Is<CreateResourceAllocationCommand>(c => c.Allocation.ProjectId == null && c.Allocation.CrmOpportunityId == ResourcePlanningTestData.Opportunity && c.Allocation.BookingType == ResourceBookingType.SoftTentative), Any), Times.Once);
    }

    /// <summary>
    /// Without a picker the project is typed in as an identifier, and an identifier that is not valid is explained.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_TypedProject_IsUsedWhenThereIsNoPicker()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetProjectEvmProjectsQuery>(), Any)).ThrowsAsync(new UnauthorizedAccessException("no access"));
        var vm = Create();
        await vm.LoadAsync();
        vm.Form.SelectedEmployee = vm.Form.EmployeeChoices[0];
        vm.Form.SelectedBookingType = vm.Form.BookingChoices[1];
        vm.Form.ProjectIdText = "not an id";

        await vm.CreateAsync();

        Assert.Equal(loc["ResourcePlanning_Create_invalid_project_id"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<CreateResourceAllocationCommand>(), Any), Times.Never);

        vm.Form.ProjectIdText = ResourcePlanningTestData.ProjectA.ToString();
        await vm.CreateAsync();

        sender.Verify(s => s.Send(It.Is<CreateResourceAllocationCommand>(c => c.Allocation.ProjectId == ResourcePlanningTestData.ProjectA), Any), Times.Once);
    }

    /// <summary>
    /// A form that is not valid is explained in the language of the user and nothing is sent.
    /// </summary>
    /// <param name="problem">What is wrong with the form.</param>
    /// <param name="code">The expected problem code.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData("employee", "needs_employee")]
    [InlineData("hard", "hard_needs_project")]
    [InlineData("soft", "soft_needs_reference")]
    [InlineData("wbs-id", "invalid_wbs_id")]
    [InlineData("opportunity-id", "invalid_opportunity_id")]
    [InlineData("wbs", "wbs_needs_project")]
    [InlineData("period", "invalid_period")]
    [InlineData("hours", "invalid_hours")]
    public async Task Create_InvalidForm_IsExplainedAndNotSent(string problem, string code)
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.Form.SelectedEmployee = vm.Form.EmployeeChoices[0];
        vm.Form.SelectedProject = vm.Form.ProjectChoices[1];
        switch (problem)
        {
            case "employee":
                vm.Form.SelectedEmployee = null;
                break;
            case "hard":
                vm.Form.SelectedBookingType = vm.Form.BookingChoices[1];
                vm.Form.SelectedProject = vm.Form.ProjectChoices[0];
                break;
            case "soft":
                vm.Form.SelectedProject = vm.Form.ProjectChoices[0];
                break;
            case "wbs-id":
                vm.Form.WbsNodeIdText = "x";
                break;
            case "opportunity-id":
                vm.Form.OpportunityIdText = "x";
                break;
            case "wbs":
                vm.Form.SelectedProject = vm.Form.ProjectChoices[0];
                vm.Form.OpportunityIdText = ResourcePlanningTestData.Opportunity.ToString();
                vm.Form.WbsNodeIdText = Guid.NewGuid().ToString();
                break;
            case "period":
                vm.Form.EndDate = vm.Form.StartDate!.Value.AddDays(-1);
                break;
            default:
                vm.Form.HoursPerDayText = "25";
                break;
        }

        await vm.CreateAsync();

        Assert.Equal(loc["ResourcePlanning_Create_" + code], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<CreateResourceAllocationCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A refusal of the server is shown with its violations, the form is kept and the list is not reloaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_Refused_ShowsTheViolationsAndKeepsTheForm()
    {
        outcome = new PlanningCommandOutcome<AllocationMutationResult>(HttpStatusCode.UnprocessableEntity, null, [new PlanningIssue("overload", null)]);
        var vm = Create();
        await vm.LoadAsync();
        vm.Form.SelectedEmployee = vm.Form.EmployeeChoices[0];
        vm.Form.SelectedProject = vm.Form.ProjectChoices[1];
        vm.Form.Notes = "keep me";

        await vm.CreateAsync();

        Assert.Equal(loc["ResourcePlanning_Error_overload"], vm.StatusMessage);
        Assert.Equal("keep me", vm.Form.Notes);
        sender.Verify(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any), Times.Once);
    }

    /// <summary>
    /// A server that cannot be reached shows the save failure and the view stops being busy.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_ServerDown_ShowsTheSaveFailure()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<CreateResourceAllocationCommand>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = Create();
        await vm.LoadAsync();
        vm.Form.SelectedEmployee = vm.Form.EmployeeChoices[0];
        vm.Form.SelectedProject = vm.Form.ProjectChoices[1];

        await vm.CreateAsync();

        Assert.Equal(loc["ResourcePlanning_SaveFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// Deleting asks for a confirmation first: nothing is sent until the user confirms, and then the list is reloaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Delete_NeedsAConfirmation()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.RequestDelete();
        Assert.Equal(loc["ResourcePlanning_Delete_NeedsSelection"], vm.StatusMessage);

        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationOne);
        vm.RequestDelete();

        Assert.True(vm.IsDeletePending);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_Delete_Confirm"], "Anna Rossi", vm.SelectedRow.PeriodText), vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<DeleteResourceAllocationCommand>(), Any), Times.Never);

        await vm.ConfirmDeleteAsync();

        sender.Verify(s => s.Send(It.Is<DeleteResourceAllocationCommand>(c => c.AllocationId == ResourcePlanningTestData.AllocationOne), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any), Times.Exactly(2));
        Assert.False(vm.IsDeletePending);
        Assert.Equal(loc["ResourcePlanning_Deleted"], vm.StatusMessage);
    }

    /// <summary>
    /// Cancelling, selecting another allocation, or confirming without a request deletes nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Delete_CancelledOrUnrequested_DeletesNothing()
    {
        var vm = Create();
        await vm.LoadAsync();
        await vm.ConfirmDeleteAsync();
        vm.SelectedRow = vm.Rows[0];
        vm.RequestDelete();
        vm.CancelDelete();
        await vm.ConfirmDeleteAsync();
        vm.RequestDelete();
        vm.SelectedRow = vm.Rows[1];
        await vm.ConfirmDeleteAsync();

        Assert.False(vm.IsDeletePending);
        sender.Verify(s => s.Send(It.IsAny<DeleteResourceAllocationCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A refused deletion shows the violation of the server and leaves the list alone.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Delete_Refused_ShowsTheViolation()
    {
        deleteOutcome = new PlanningCommandOutcome<bool>(HttpStatusCode.NotFound, false, [new PlanningIssue("allocation_not_found", null)]);
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows[0];
        vm.RequestDelete();

        await vm.ConfirmDeleteAsync();

        Assert.Equal(loc["ResourcePlanning_Error_allocation_not_found"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any), Times.Once);
    }

    /// <summary>
    /// A soft booking that only has an opportunity is converted with the project picked from the list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertToHard_ProjectPickedFromTheList_IsPassedOn()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationTwo);
        vm.SelectedConvertProject = vm.Form.ProjectChoices.Single(choice => choice.Label.StartsWith("P-2", StringComparison.Ordinal));

        await vm.ConvertToHardAsync();

        sender.Verify(s => s.Send(It.Is<ConvertAllocationToHardCommand>(c => c.AllocationId == ResourcePlanningTestData.AllocationTwo && c.Conversion.ProjectId == ProjectB), Any), Times.Once);
    }

    /// <summary>
    /// A typed project identifier that is not an identifier is explained instead of being sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertToHard_InvalidTypedProject_IsExplained()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Allocation.Id == ResourcePlanningTestData.AllocationTwo);
        vm.ConvertProjectIdText = "nonsense";

        await vm.ConvertToHardAsync();

        Assert.Equal(loc["ResourcePlanning_Convert_InvalidProject"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<ConvertAllocationToHardCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// Dropping an allocation on a later week of the same employee shifts it by whole weeks and shows the overload that results.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Drop_OnAnotherWeek_ShiftsByWholeWeeks()
    {
        outcome = Success(soft, new OverloadSummary(true, 120d, [new OverloadWeek(new DateOnly(2026, 11, 30), 120d)]));
        var vm = Create();
        await vm.LoadAsync();
        var row = vm.Rows.Single(item => item.Allocation.Id == ResourcePlanningTestData.AllocationOne);

        await vm.DropAsync(new DropRequest(row, Cell(ResourcePlanningTestData.Anna, new DateOnly(2026, 11, 30))));

        sender.Verify(s => s.Send(It.Is<RebalanceAllocationCommand>(c => c.AllocationId == ResourcePlanningTestData.AllocationOne && c.Rebalance.Strategy == RebalanceStrategy.Shift && c.Rebalance.ShiftDays == 14 && c.Rebalance.EmployeeId == null && c.Rebalance.StartDate == null), Any), Times.Once);
        Assert.Equal(loc["ResourcePlanning_Moved"], vm.StatusMessage);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_OverloadWarning"], 120d.ToString("0.#", loc.CurrentCulture) + "%"), vm.OverloadMessage);
        sender.Verify(s => s.Send(It.IsAny<GetResourceAllocationsQuery>(), Any), Times.Exactly(2));
    }

    /// <summary>
    /// Dropping on an earlier week shifts backwards; the weekday stays because the shift is in whole weeks.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Drop_OnAnEarlierWeek_ShiftsBackwards()
    {
        var vm = Create();
        await vm.LoadAsync();
        var row = vm.Rows.Single(item => item.Allocation.Id == ResourcePlanningTestData.AllocationOne);

        await vm.DropAsync(new DropRequest(row, Cell(ResourcePlanningTestData.Anna, new DateOnly(2026, 11, 9))));

        sender.Verify(s => s.Send(It.Is<RebalanceAllocationCommand>(c => c.Rebalance.Strategy == RebalanceStrategy.Shift && c.Rebalance.ShiftDays == -7), Any), Times.Once);
    }

    /// <summary>
    /// Dropping on the same week of a colleague gives the allocation to the colleague without moving it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Drop_OnTheSameWeekOfAColleague_Reassigns()
    {
        var vm = Create();
        await vm.LoadAsync();
        var row = vm.Rows.Single(item => item.Allocation.Id == ResourcePlanningTestData.AllocationOne);

        await vm.DropAsync(new DropRequest(row, Cell(ResourcePlanningTestData.Bruno, new DateOnly(2026, 11, 16))));

        sender.Verify(s => s.Send(It.Is<RebalanceAllocationCommand>(c => c.Rebalance.Strategy == RebalanceStrategy.Reassign && c.Rebalance.EmployeeId == ResourcePlanningTestData.Bruno), Any), Times.Once);
    }

    /// <summary>
    /// Dropping on another week of a colleague moves and reassigns in one step.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Drop_OnAnotherWeekOfAColleague_ShiftsAndReassigns()
    {
        var vm = Create();
        await vm.LoadAsync();
        var row = vm.Rows.Single(item => item.Allocation.Id == ResourcePlanningTestData.AllocationOne);

        await vm.DropAsync(new DropRequest(row, Cell(ResourcePlanningTestData.Bruno, new DateOnly(2026, 11, 23))));

        sender.Verify(s => s.Send(It.Is<RebalanceAllocationCommand>(c => c.Rebalance.Strategy == RebalanceStrategy.Shift && c.Rebalance.ShiftDays == 7 && c.Rebalance.EmployeeId == ResourcePlanningTestData.Bruno), Any), Times.Once);
    }

    /// <summary>
    /// Dropping on the week and the employee the allocation already has changes nothing and says so.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Drop_OnTheSameCell_ChangesNothing()
    {
        var vm = Create();
        await vm.LoadAsync();
        var row = vm.Rows.Single(item => item.Allocation.Id == ResourcePlanningTestData.AllocationOne);

        await vm.DropAsync(new DropRequest(row, Cell(ResourcePlanningTestData.Anna, new DateOnly(2026, 11, 16))));

        Assert.Equal(loc["ResourcePlanning_Drop_NoChange"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<RebalanceAllocationCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A drop that does not carry an allocation and a cell, or no drop at all, is ignored.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Drop_OfSomethingElse_IsIgnored()
    {
        var vm = Create();
        await vm.LoadAsync();
        var row = vm.Rows[0];

        await vm.DropAsync(null);
        await vm.DropAsync(new DropRequest("text", Cell(ResourcePlanningTestData.Anna, new DateOnly(2026, 11, 23))));
        await vm.DropAsync(new DropRequest(row, "not a cell"));

        sender.Verify(s => s.Send(It.IsAny<RebalanceAllocationCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A drop the server refuses shows its violations.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Drop_Refused_ShowsTheViolations()
    {
        outcome = new PlanningCommandOutcome<AllocationMutationResult>(HttpStatusCode.UnprocessableEntity, null, [new PlanningIssue("no_working_days", null)]);
        var vm = Create();
        await vm.LoadAsync();
        var row = vm.Rows.Single(item => item.Allocation.Id == ResourcePlanningTestData.AllocationOne);

        await vm.DropAsync(new DropRequest(row, Cell(ResourcePlanningTestData.Anna, new DateOnly(2026, 11, 30))));

        Assert.Equal(loc["ResourcePlanning_Error_no_working_days"], vm.StatusMessage);
    }

    private static BoardCell Cell(Guid employee, DateOnly week) => new(employee, week, "70%", "Green", false, "tip");

    private static PlanningCommandOutcome<AllocationMutationResult> Success(ResourceAllocation allocation, OverloadSummary? overload = null) =>
        new(HttpStatusCode.OK, ResourcePlanningTestData.Mutation(allocation, overload), []);

    private AllocationsViewModel Create() => new(sender.Object, loc, time, new AllocationFormViewModel(sender.Object, loc, time), new AllocationBoardViewModel(sender.Object, loc));
}
