// <copyright file="IResourcePlanningApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Typed port to the Mod 079 endpoints of the web-api (/api/v1/resource-planning). Reads throw <see cref="System.Net.Http.HttpRequestException"/>
/// on a failing status; writes return the violations of the server in a <see cref="PlanningCommandOutcome{T}"/>.
/// </summary>
public interface IResourcePlanningApiClient
{
    /// <summary>
    /// Gets a value indicating whether a session exists.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the skills of the taxonomy.
    /// </summary>
    /// <param name="category">The category filter, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The skills.</returns>
    public Task<IReadOnlyList<PlanningSkill>> GetSkillsAsync(string? category, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a skill.
    /// </summary>
    /// <param name="skill">The skill.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<PlanningSkill>> CreateSkillAsync(PlanningSkillRequest skill, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a skill.
    /// </summary>
    /// <param name="skillId">The skill.</param>
    /// <param name="skill">The new values.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<PlanningSkill>> UpdateSkillAsync(Guid skillId, PlanningSkillRequest skill, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a skill.
    /// </summary>
    /// <param name="skillId">The skill.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<bool>> DeleteSkillAsync(Guid skillId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the skill profiles of the employees.
    /// </summary>
    /// <param name="employeeId">The employee filter, or null.</param>
    /// <param name="skillId">The skill filter, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The profiles.</returns>
    public Task<IReadOnlyList<EmployeeSkillProfile>> GetSkillProfilesAsync(Guid? employeeId, Guid? skillId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the proficiency of an employee in a skill (upsert).
    /// </summary>
    /// <param name="profile">The profile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<EmployeeSkillProfile>> UpsertSkillProfileAsync(EmployeeSkillProfileRequest profile, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a skill profile.
    /// </summary>
    /// <param name="profileId">The profile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<bool>> DeleteSkillProfileAsync(Guid profileId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the employees with an active contract.
    /// </summary>
    /// <param name="departmentId">The department filter, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resources.</returns>
    public Task<IReadOnlyList<PlanningResource>> GetResourcesAsync(Guid? departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the allocations that overlap a window.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="employeeId">The employee filter, or null.</param>
    /// <param name="projectId">The project filter, or null.</param>
    /// <param name="bookingType">The booking type filter, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The allocations.</returns>
    public Task<IReadOnlyList<ResourceAllocation>> GetAllocationsAsync(DateOnly from, DateOnly to, Guid? employeeId, Guid? projectId, ResourceBookingType? bookingType, CancellationToken cancellationToken);

    /// <summary>
    /// Creates an allocation.
    /// </summary>
    /// <param name="allocation">The allocation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<AllocationMutationResult>> CreateAllocationAsync(AllocationRequest allocation, CancellationToken cancellationToken);

    /// <summary>
    /// Converts a soft booking into a hard commitment.
    /// </summary>
    /// <param name="allocationId">The allocation.</param>
    /// <param name="conversion">The project and WBS node.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<AllocationMutationResult>> ConvertToHardAsync(Guid allocationId, ConvertToHardRequest conversion, CancellationToken cancellationToken);

    /// <summary>
    /// Rebalances an allocation.
    /// </summary>
    /// <param name="allocationId">The allocation.</param>
    /// <param name="rebalance">The strategy and its parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<AllocationMutationResult>> RebalanceAsync(Guid allocationId, RebalanceRequest rebalance, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an allocation.
    /// </summary>
    /// <param name="allocationId">The allocation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<bool>> DeleteAllocationAsync(Guid allocationId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the capacity saturation heatmap.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="granularity">The cell size.</param>
    /// <param name="departmentId">The department filter, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The heatmap.</returns>
    public Task<CapacityHeatmap> GetHeatmapAsync(DateOnly from, DateOnly to, HeatmapGranularity granularity, Guid? departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Searches the candidates for a skill requirement.
    /// </summary>
    /// <param name="search">The search criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The candidates, best first.</returns>
    public Task<IReadOnlyList<ResourceCandidate>> SearchCandidatesAsync(CandidateSearchRequest search, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the billable utilization report.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="groupBy">The grouping.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The report.</returns>
    public Task<UtilizationReport> GetBillableUtilizationAsync(DateOnly from, DateOnly to, UtilizationGroupBy groupBy, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the utilization targets.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The targets.</returns>
    public Task<IReadOnlyList<UtilizationTarget>> GetUtilizationTargetsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sets a utilization target (upsert).
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<UtilizationTarget>> SetUtilizationTargetAsync(UtilizationTargetRequest target, CancellationToken cancellationToken);

    /// <summary>
    /// Recalculates the capacity snapshots of a window.
    /// </summary>
    /// <param name="recalculation">The window and the optional employee.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<SnapshotRecalculationResult>> RecalculateSnapshotsAsync(SnapshotRecalculationRequest recalculation, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the stored capacity snapshots.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="employeeId">The employee filter, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The snapshots.</returns>
    public Task<IReadOnlyList<ResourceCapacitySnapshot>> GetSnapshotsAsync(DateOnly from, DateOnly to, Guid? employeeId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the holidays of a country in a year.
    /// </summary>
    /// <param name="country">The country code, CH or IT.</param>
    /// <param name="region">The canton or region, or null.</param>
    /// <param name="municipality">The municipality, or null.</param>
    /// <param name="year">The year.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The holidays.</returns>
    public Task<IReadOnlyList<PlanningHoliday>> GetHolidaysAsync(string country, string? region, string? municipality, int year, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the local holidays of the tenant.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The local holidays.</returns>
    public Task<IReadOnlyList<LocalHoliday>> GetLocalHolidaysAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds a local holiday.
    /// </summary>
    /// <param name="holiday">The holiday.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<LocalHoliday>> CreateLocalHolidayAsync(LocalHolidayRequest holiday, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a local holiday.
    /// </summary>
    /// <param name="holidayId">The holiday.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PlanningCommandOutcome<bool>> DeleteLocalHolidayAsync(Guid holidayId, CancellationToken cancellationToken);
}
