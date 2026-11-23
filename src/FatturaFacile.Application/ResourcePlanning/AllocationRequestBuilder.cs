// <copyright file="AllocationRequestBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Checks the form that creates an allocation before it is sent and builds the request the way the contract wants it: a hard commitment
/// needs a project, a soft booking needs a project or an opportunity, and a WBS node belongs to a project.
/// </summary>
public static class AllocationRequestBuilder
{
    /// <summary>
    /// The problem code when no employee is chosen.
    /// </summary>
    public const string NeedsEmployee = "needs_employee";

    /// <summary>
    /// The problem code when a hard commitment has no project.
    /// </summary>
    public const string HardNeedsProject = "hard_needs_project";

    /// <summary>
    /// The problem code when a soft booking has neither a project nor an opportunity.
    /// </summary>
    public const string SoftNeedsReference = "soft_needs_reference";

    /// <summary>
    /// The problem code when a WBS node is chosen without its project.
    /// </summary>
    public const string WbsNeedsProject = "wbs_needs_project";

    /// <summary>
    /// The problem code when a day is missing or the period ends before it starts.
    /// </summary>
    public const string InvalidPeriod = "invalid_period";

    /// <summary>
    /// The problem code when the hours per day are not between 0 and 24.
    /// </summary>
    public const string InvalidHours = "invalid_hours";

    private const decimal HoursPerDayMax = 24m;

    /// <summary>
    /// Builds the request that creates an allocation.
    /// </summary>
    /// <param name="draft">What the planner entered.</param>
    /// <returns>The request or the first problem.</returns>
    public static PlanningDraftResult<AllocationRequest> Build(AllocationDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (draft.EmployeeId is not { } employee)
        {
            return PlanningDraftResults.Fail<AllocationRequest>(NeedsEmployee);
        }

        var hard = draft.BookingType == ResourceBookingType.HardCommitted;
        if (hard && draft.ProjectId is null)
        {
            return PlanningDraftResults.Fail<AllocationRequest>(HardNeedsProject);
        }

        if (!hard && draft.ProjectId is null && draft.CrmOpportunityId is null)
        {
            return PlanningDraftResults.Fail<AllocationRequest>(SoftNeedsReference);
        }

        if (draft.WbsNodeId is not null && draft.ProjectId is null)
        {
            return PlanningDraftResults.Fail<AllocationRequest>(WbsNeedsProject);
        }

        if (draft.StartDate is not { } start || draft.EndDate is not { } end || end < start)
        {
            return PlanningDraftResults.Fail<AllocationRequest>(InvalidPeriod);
        }

        if (draft.AllocatedHoursPerDay is not { } hours || hours <= 0m || hours > HoursPerDayMax)
        {
            return PlanningDraftResults.Fail<AllocationRequest>(InvalidHours);
        }

        var skills = draft.RequiredSkillIds.Distinct().ToList();
        var notes = string.IsNullOrWhiteSpace(draft.Notes) ? null : draft.Notes.Trim();

        return PlanningDraftResults.Ok(new AllocationRequest(
            draft.ProjectId,
            draft.WbsNodeId,
            employee,
            RoleId: null,
            draft.BookingType,
            start,
            end,
            hours,
            draft.IsBillable,
            hard ? null : draft.CrmOpportunityId,
            notes,
            draft.AllowOverload,
            skills.Count > 0 ? skills : null));
    }
}
