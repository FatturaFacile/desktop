// <copyright file="ResourcePlanningApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net.Http;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.Infrastructure.ResourcePlanning;

/// <summary>
/// Calls the Mod 079 endpoints of the web-api (<c>resource-planning/</c>) with the authorized sync session. Reads throw an
/// <see cref="HttpRequestException"/> with the status; writes return the violations of the server in the outcome.
/// </summary>
/// <param name="api">The authorized web-api client.</param>
public sealed class ResourcePlanningApiClient(ISyncApiClient api) : IResourcePlanningApiClient
{
    private const string Base = "resource-planning/";

    /// <inheritdoc/>
    public bool IsAuthenticated => api.IsAuthenticated;

    /// <inheritdoc/>
    public Task<IReadOnlyList<PlanningSkill>> GetSkillsAsync(string? category, CancellationToken cancellationToken) =>
        ReadListAsync<PlanningSkill>(Url("skills", ("category", category)), cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<PlanningSkill>> CreateSkillAsync(PlanningSkillRequest skill, CancellationToken cancellationToken) =>
        WriteAsync<PlanningSkill>(HttpMethod.Post, Base + "skills", skill, cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<PlanningSkill>> UpdateSkillAsync(Guid skillId, PlanningSkillRequest skill, CancellationToken cancellationToken) =>
        WriteAsync<PlanningSkill>(HttpMethod.Put, Base + "skills/" + Id(skillId), skill, cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<bool>> DeleteSkillAsync(Guid skillId, CancellationToken cancellationToken) =>
        DeleteAsync(Base + "skills/" + Id(skillId), cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<EmployeeSkillProfile>> GetSkillProfilesAsync(Guid? employeeId, Guid? skillId, CancellationToken cancellationToken) =>
        ReadListAsync<EmployeeSkillProfile>(Url("employee-skill-profiles", ("employee_id", Id(employeeId)), ("skill_id", Id(skillId))), cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<EmployeeSkillProfile>> UpsertSkillProfileAsync(EmployeeSkillProfileRequest profile, CancellationToken cancellationToken) =>
        WriteAsync<EmployeeSkillProfile>(HttpMethod.Post, Base + "employee-skill-profiles", profile, cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<bool>> DeleteSkillProfileAsync(Guid profileId, CancellationToken cancellationToken) =>
        DeleteAsync(Base + "employee-skill-profiles/" + Id(profileId), cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<PlanningResource>> GetResourcesAsync(Guid? departmentId, CancellationToken cancellationToken) =>
        ReadListAsync<PlanningResource>(Url("resources", ("department_id", Id(departmentId))), cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<ResourceAllocation>> GetAllocationsAsync(DateOnly from, DateOnly to, Guid? employeeId, Guid? projectId, ResourceBookingType? bookingType, CancellationToken cancellationToken) =>
        ReadListAsync<ResourceAllocation>(
            Url(
                "allocations",
                ("from", ResourcePlanningJson.Day(from)),
                ("to", ResourcePlanningJson.Day(to)),
                ("employee_id", Id(employeeId)),
                ("project_id", Id(projectId)),
                ("booking_type", bookingType is { } type ? ResourceBookingTypeJsonConverter.ToWire(type) : null)),
            cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<AllocationMutationResult>> CreateAllocationAsync(AllocationRequest allocation, CancellationToken cancellationToken) =>
        WriteAsync<AllocationMutationResult>(HttpMethod.Post, Base + "allocations", allocation, cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<AllocationMutationResult>> ConvertToHardAsync(Guid allocationId, ConvertToHardRequest conversion, CancellationToken cancellationToken) =>
        WriteAsync<AllocationMutationResult>(HttpMethod.Post, Base + "allocations/" + Id(allocationId) + "/convert-to-hard", conversion, cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<AllocationMutationResult>> RebalanceAsync(Guid allocationId, RebalanceRequest rebalance, CancellationToken cancellationToken) =>
        WriteAsync<AllocationMutationResult>(HttpMethod.Post, Base + "allocations/" + Id(allocationId) + "/rebalance", rebalance, cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<bool>> DeleteAllocationAsync(Guid allocationId, CancellationToken cancellationToken) =>
        DeleteAsync(Base + "allocations/" + Id(allocationId), cancellationToken);

    /// <inheritdoc/>
    public Task<CapacityHeatmap> GetHeatmapAsync(DateOnly from, DateOnly to, HeatmapGranularity granularity, Guid? departmentId, CancellationToken cancellationToken) =>
        ReadAsync<CapacityHeatmap>(
            Url(
                "capacity/heatmap",
                ("from", ResourcePlanningJson.Day(from)),
                ("to", ResourcePlanningJson.Day(to)),
                ("granularity", ResourcePlanningJson.Wire(granularity)),
                ("department_id", Id(departmentId))),
            cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ResourceCandidate>> SearchCandidatesAsync(CandidateSearchRequest search, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(search);

        var reply = await api.SendAuthorizedAsync(HttpMethod.Post, Base + "candidates/search", ResourcePlanningJson.Write(search), cancellationToken).ConfigureAwait(false);

        return Parse<IReadOnlyList<ResourceCandidate>>(reply);
    }

    /// <inheritdoc/>
    public Task<UtilizationReport> GetBillableUtilizationAsync(DateOnly from, DateOnly to, UtilizationGroupBy groupBy, CancellationToken cancellationToken) =>
        ReadAsync<UtilizationReport>(
            Url(
                "reports/billable-utilization",
                ("from", ResourcePlanningJson.Day(from)),
                ("to", ResourcePlanningJson.Day(to)),
                ("group_by", ResourcePlanningJson.Wire(groupBy))),
            cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<UtilizationTarget>> GetUtilizationTargetsAsync(CancellationToken cancellationToken) =>
        ReadListAsync<UtilizationTarget>(Base + "utilization-targets", cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<UtilizationTarget>> SetUtilizationTargetAsync(UtilizationTargetRequest target, CancellationToken cancellationToken) =>
        WriteAsync<UtilizationTarget>(HttpMethod.Put, Base + "utilization-targets", target, cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<SnapshotRecalculationResult>> RecalculateSnapshotsAsync(SnapshotRecalculationRequest recalculation, CancellationToken cancellationToken) =>
        WriteAsync<SnapshotRecalculationResult>(HttpMethod.Post, Base + "capacity/snapshots/recalculate", recalculation, cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<ResourceCapacitySnapshot>> GetSnapshotsAsync(DateOnly from, DateOnly to, Guid? employeeId, CancellationToken cancellationToken) =>
        ReadListAsync<ResourceCapacitySnapshot>(
            Url("capacity/snapshots", ("from", ResourcePlanningJson.Day(from)), ("to", ResourcePlanningJson.Day(to)), ("employee_id", Id(employeeId))),
            cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<PlanningHoliday>> GetHolidaysAsync(string country, string? region, string? municipality, int year, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        return ReadListAsync<PlanningHoliday>(
            Url("holidays", ("country", country), ("region", region), ("municipality", municipality), ("year", year.ToString(CultureInfo.InvariantCulture))),
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<LocalHoliday>> GetLocalHolidaysAsync(CancellationToken cancellationToken) =>
        ReadListAsync<LocalHoliday>(Base + "local-holidays", cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<LocalHoliday>> CreateLocalHolidayAsync(LocalHolidayRequest holiday, CancellationToken cancellationToken) =>
        WriteAsync<LocalHoliday>(HttpMethod.Post, Base + "local-holidays", holiday, cancellationToken);

    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<bool>> DeleteLocalHolidayAsync(Guid holidayId, CancellationToken cancellationToken) =>
        DeleteAsync(Base + "local-holidays/" + Id(holidayId), cancellationToken);

    private static string Id(Guid id) => id.ToString("D", CultureInfo.InvariantCulture);

    private static string? Id(Guid? id) => id is { } value ? Id(value) : null;

    private static string Url(string path, params (string Name, string? Value)[] query)
    {
        var pairs = query.Where(pair => !string.IsNullOrEmpty(pair.Value)).Select(pair => pair.Name + "=" + Uri.EscapeDataString(pair.Value!)).ToList();

        return Base + path + (pairs.Count == 0 ? string.Empty : "?" + string.Join('&', pairs));
    }

    private static PlanningCommandOutcome<T> Refused<T>(SyncApiReply reply) =>
        new(reply.Status, default, ResourcePlanningJson.ReadIssues(reply.Text));

    private static T Parse<T>(SyncApiReply reply)
    {
        if (!reply.IsSuccess)
        {
            throw new HttpRequestException("Resource planning request failed with HTTP " + ((int)reply.Status).ToString(CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);
        }

        return ResourcePlanningJson.Read<T>(ResourcePlanningJson.ReadData(reply.Text));
    }

    private async Task<T> ReadAsync<T>(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Get, url, jsonBody: null, cancellationToken).ConfigureAwait(false);

        return Parse<T>(reply);
    }

    private Task<IReadOnlyList<T>> ReadListAsync<T>(string url, CancellationToken cancellationToken) =>
        ReadAsync<IReadOnlyList<T>>(url, cancellationToken);

    private async Task<PlanningCommandOutcome<T>> WriteAsync<T>(HttpMethod method, string url, object body, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);

        var reply = await api.SendAuthorizedAsync(method, url, ResourcePlanningJson.Write(body), cancellationToken).ConfigureAwait(false);

        return reply.IsSuccess
            ? new PlanningCommandOutcome<T>(reply.Status, ResourcePlanningJson.Read<T>(ResourcePlanningJson.ReadData(reply.Text)), [])
            : Refused<T>(reply);
    }

    private async Task<PlanningCommandOutcome<bool>> DeleteAsync(string url, CancellationToken cancellationToken)
    {
        var reply = await api.SendAuthorizedAsync(HttpMethod.Delete, url, jsonBody: null, cancellationToken).ConfigureAwait(false);

        return reply.IsSuccess ? new PlanningCommandOutcome<bool>(reply.Status, Value: true, []) : Refused<bool>(reply);
    }
}
