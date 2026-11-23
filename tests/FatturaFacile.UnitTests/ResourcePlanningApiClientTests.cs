// <copyright file="ResourcePlanningApiClientTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Application.Sync;
using FatturaFacile.Infrastructure.ResourcePlanning;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Mod 079 HTTP client: routes and query strings, bodies, parsing of the data member and the mapping of refusals.
/// </summary>
public sealed class ResourcePlanningApiClientTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISyncApiClient> sync = new();

    /// <summary>
    /// The heatmap is requested for the window, granularity and department and the data object is parsed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetHeatmap_SendsTheWindowAndParsesTheAnswer()
    {
        const string expectedUrl = "resource-planning/capacity/heatmap?from=2026-11-16&to=2026-12-06&granularity=week&department_id=77777777-7777-7777-7777-777777777777";
        const string body = """{ "success": true, "message": "", "data": { "from": "2026-11-16", "to": "2026-12-06", "granularity": "week", "thresholds": { "yellow": 80, "red": 100 }, "periods": ["2026-11-16"], "rows": [] }, "meta": {} }""";
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, expectedUrl, null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, body));

        var heatmap = await new ResourcePlanningApiClient(sync.Object).GetHeatmapAsync(new DateOnly(2026, 11, 16), new DateOnly(2026, 12, 6), HeatmapGranularity.Week, ResourcePlanningTestData.Department, CancellationToken.None);

        Assert.Equal(100d, heatmap.Thresholds.Red);
        Assert.Equal(new DateOnly(2026, 11, 16), Assert.Single(heatmap.Periods));
        Assert.Empty(heatmap.Rows);
    }

    /// <summary>
    /// Filters that are not set are left out of the query string and the booking type is sent with its contract name.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetAllocations_LeavesOutEmptyFiltersAndNamesTheBookingType()
    {
        const string expectedUrl = "resource-planning/allocations?from=2026-11-16&to=2026-12-06&booking_type=SoftTentative";
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, expectedUrl, null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, """{ "data": [] }"""));

        var allocations = await new ResourcePlanningApiClient(sync.Object).GetAllocationsAsync(new DateOnly(2026, 11, 16), new DateOnly(2026, 12, 6), null, null, ResourceBookingType.SoftTentative, CancellationToken.None);

        Assert.Empty(allocations);
        sync.Verify(s => s.SendAuthorizedAsync(HttpMethod.Get, expectedUrl, null, Any), Times.Once);
    }

    /// <summary>
    /// The utilization report is requested with the lower snake case grouping.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetBillableUtilization_SendsTheGrouping()
    {
        const string expectedUrl = "resource-planning/reports/billable-utilization?from=2026-11-01&to=2026-11-30&group_by=cost_center";
        const string row = """{ "key_id": null, "key_name": "All", "net_available_hours": 0, "billable_committed_hours": 0, "billable_booked_hours": 0, "internal_booked_hours": 0, "tentative_booked_hours": 0, "actual_billable_percent": 0, "forecast_billable_percent": 0, "target_percent": 70, "gap_percent": -70, "status": "below_target" }""";
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, expectedUrl, null, Any))
            .ReturnsAsync(Reply(HttpStatusCode.OK, $$"""{ "data": { "from": "2026-11-01", "to": "2026-11-30", "group_by": "cost_center", "default_target_percent": 70, "rows": [], "totals": {{row}} } }"""));

        var report = await new ResourcePlanningApiClient(sync.Object).GetBillableUtilizationAsync(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), UtilizationGroupBy.CostCenter, CancellationToken.None);

        Assert.Equal(UtilizationGroupBy.CostCenter, report.GroupBy);
        Assert.Equal(UtilizationStatus.BelowTarget, report.Totals.Status);
    }

    /// <summary>
    /// A rebalance is posted to the allocation with the strategy body and the mutation result is returned.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Rebalance_PostsTheStrategyAndReturnsTheMutation()
    {
        string? sent = null;
        var allocation = ResourcePlanningTestData.Allocation(ResourcePlanningTestData.AllocationOne, ResourceBookingType.HardCommitted, ResourcePlanningTestData.ProjectA);
        var data = ResourcePlanningJson.Write(ResourcePlanningTestData.Mutation(allocation));
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Post, "resource-planning/allocations/88888888-8888-8888-8888-888888888888/rebalance", It.IsAny<string?>(), Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, _, json, _) => sent = json)
            .ReturnsAsync(Reply(HttpStatusCode.OK, $$"""{ "data": {{data}} }"""));

        var outcome = await new ResourcePlanningApiClient(sync.Object).RebalanceAsync(ResourcePlanningTestData.AllocationOne, new RebalanceRequest(RebalanceStrategy.Shift, null, null, null, null, 7, null), CancellationToken.None);

        Assert.True(outcome.IsSuccess);
        Assert.Equal(ResourcePlanningTestData.AllocationOne, outcome.Value!.Allocation.Id);
        using var document = JsonDocument.Parse(sent!);
        Assert.Equal("shift", document.RootElement.GetProperty("strategy").GetString());
        Assert.Equal(7, document.RootElement.GetProperty("shift_days").GetInt32());
    }

    /// <summary>
    /// A refused write carries the status and the invalid_params of the problem document; it does not throw.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertToHard_UnprocessableAnswer_ReturnsTheIssues()
    {
        const string problem = """{ "status": 422, "invalid_params": [ { "code": "hard_booking_requires_project", "params": {} } ] }""";
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Post, "resource-planning/allocations/88888888-8888-8888-8888-888888888888/convert-to-hard", It.IsAny<string?>(), Any))
            .ReturnsAsync(Reply(HttpStatusCode.UnprocessableEntity, problem));

        var outcome = await new ResourcePlanningApiClient(sync.Object).ConvertToHardAsync(ResourcePlanningTestData.AllocationOne, new ConvertToHardRequest(null, null), CancellationToken.None);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, outcome.Status);
        Assert.Null(outcome.Value);
        Assert.Equal("hard_booking_requires_project", Assert.Single(outcome.Issues).Code);
    }

    /// <summary>
    /// Creating an allocation posts the snake case body and parses the created mutation.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CreateAllocation_PostsTheBody()
    {
        string? sent = null;
        var allocation = ResourcePlanningTestData.Allocation(ResourcePlanningTestData.AllocationTwo, ResourceBookingType.SoftTentative, projectName: null, opportunityId: ResourcePlanningTestData.Opportunity, opportunityName: "Tender");
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Post, "resource-planning/allocations", It.IsAny<string?>(), Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, _, json, _) => sent = json)
            .ReturnsAsync(Reply(HttpStatusCode.Created, $$"""{ "data": {{ResourcePlanningJson.Write(ResourcePlanningTestData.Mutation(allocation))}} }"""));
        var request = new AllocationRequest(null, null, ResourcePlanningTestData.Anna, null, ResourceBookingType.SoftTentative, new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 27), 6m, true, ResourcePlanningTestData.Opportunity, null, true);

        var outcome = await new ResourcePlanningApiClient(sync.Object).CreateAllocationAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, outcome.Status);
        Assert.Equal("Tender", outcome.Value!.Allocation.CrmOpportunityName);
        using var document = JsonDocument.Parse(sent!);
        Assert.Equal("SoftTentative", document.RootElement.GetProperty("booking_type").GetString());
        Assert.Equal(ResourcePlanningTestData.Opportunity, document.RootElement.GetProperty("crm_opportunity_id").GetGuid());
    }

    /// <summary>
    /// A candidate search posts its body and parses the list of candidates.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SearchCandidates_PostsTheRequirements()
    {
        string? sent = null;
        var candidate = ResourcePlanningTestData.Candidate(ResourcePlanningTestData.Bruno, "Bruno Bianchi", eligible: true, 88d, 75d);
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Post, "resource-planning/candidates/search", It.IsAny<string?>(), Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, _, json, _) => sent = json)
            .ReturnsAsync(Reply(HttpStatusCode.OK, $$"""{ "data": [ {{ResourcePlanningJson.Write(candidate)}} ] }"""));
        var search = new CandidateSearchRequest(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 27), 8m, [new CandidateRequirement(ResourcePlanningTestData.SkillWelding, 3)], null, null, null, true, false, 10);

        var candidates = await new ResourcePlanningApiClient(sync.Object).SearchCandidatesAsync(search, CancellationToken.None);

        Assert.Equal("Bruno Bianchi", Assert.Single(candidates).EmployeeName);
        using var document = JsonDocument.Parse(sent!);
        Assert.Equal(3, document.RootElement.GetProperty("requirements")[0].GetProperty("min_level").GetInt32());
        Assert.Equal(10, document.RootElement.GetProperty("limit").GetInt32());
    }

    /// <summary>
    /// A delete answered with 204 succeeds and one that is refused returns the status.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task DeleteAllocation_MapsSuccessAndRefusal()
    {
        _ = sync.SetupSequence(s => s.SendAuthorizedAsync(HttpMethod.Delete, "resource-planning/allocations/88888888-8888-8888-8888-888888888888", null, Any))
            .ReturnsAsync(Reply(HttpStatusCode.NoContent, string.Empty))
            .ReturnsAsync(Reply(HttpStatusCode.Forbidden, "{}"));
        var client = new ResourcePlanningApiClient(sync.Object);

        var deleted = await client.DeleteAllocationAsync(ResourcePlanningTestData.AllocationOne, CancellationToken.None);
        var refused = await client.DeleteAllocationAsync(ResourcePlanningTestData.AllocationOne, CancellationToken.None);

        Assert.True(deleted.IsSuccess);
        Assert.True(deleted.Value);
        Assert.False(refused.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, refused.Status);
        Assert.Empty(refused.Issues);
    }

    /// <summary>
    /// A read that fails surfaces as an HTTP exception that carries the status code.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Read_ForbiddenAnswer_ThrowsWithTheStatus()
    {
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, It.IsAny<string>(), null, Any)).ReturnsAsync(Reply(HttpStatusCode.Forbidden, "{}"));

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => new ResourcePlanningApiClient(sync.Object).GetResourcesAsync(null, CancellationToken.None));

        Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
    }

    /// <summary>
    /// An answer without a data member is rejected.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Read_WithoutData_Throws()
    {
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, "resource-planning/skills", null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, """{ "message": "hi" }"""));

        _ = await Assert.ThrowsAsync<JsonException>(() => new ResourcePlanningApiClient(sync.Object).GetSkillsAsync(null, CancellationToken.None));
    }

    /// <summary>
    /// Values in a query string are escaped and the skill filters use their contract names.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetSkillsAndProfiles_EscapeAndNameTheFilters()
    {
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, "resource-planning/skills?category=Heavy%20lifting%20%26%20rigging", null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, """{ "data": [ { "id": "66666666-6666-6666-6666-666666666666", "category": "Heavy lifting & rigging", "skill_name": "Crane", "requires_certification": true, "certification_code": "CRANE" } ] }"""));
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, "resource-planning/employee-skill-profiles?employee_id=11111111-1111-1111-1111-111111111111&skill_id=66666666-6666-6666-6666-666666666666", null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, """{ "data": [] }"""));
        var client = new ResourcePlanningApiClient(sync.Object);

        var skills = await client.GetSkillsAsync("Heavy lifting & rigging", CancellationToken.None);
        var profiles = await client.GetSkillProfilesAsync(ResourcePlanningTestData.Anna, ResourcePlanningTestData.SkillCrane, CancellationToken.None);

        Assert.True(Assert.Single(skills).RequiresCertification);
        Assert.Empty(profiles);
    }

    /// <summary>
    /// The remaining endpoints use their contract routes.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task OtherEndpoints_UseTheirContractRoutes()
    {
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, It.IsAny<string>(), null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, """{ "data": [] }"""));
        var client = new ResourcePlanningApiClient(sync.Object);

        _ = await client.GetUtilizationTargetsAsync(CancellationToken.None);
        _ = await client.GetSnapshotsAsync(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), null, CancellationToken.None);
        _ = await client.GetHolidaysAsync("CH", "TI", null, 2026, CancellationToken.None);
        _ = await client.GetLocalHolidaysAsync(CancellationToken.None);
        _ = await client.GetResourcesAsync(ResourcePlanningTestData.Department, CancellationToken.None);

        sync.Verify(s => s.SendAuthorizedAsync(HttpMethod.Get, "resource-planning/utilization-targets", null, Any), Times.Once);
        sync.Verify(s => s.SendAuthorizedAsync(HttpMethod.Get, "resource-planning/capacity/snapshots?from=2026-11-01&to=2026-11-30", null, Any), Times.Once);
        sync.Verify(s => s.SendAuthorizedAsync(HttpMethod.Get, "resource-planning/holidays?country=CH&region=TI&year=2026", null, Any), Times.Once);
        sync.Verify(s => s.SendAuthorizedAsync(HttpMethod.Get, "resource-planning/local-holidays", null, Any), Times.Once);
        sync.Verify(s => s.SendAuthorizedAsync(HttpMethod.Get, "resource-planning/resources?department_id=77777777-7777-7777-7777-777777777777", null, Any), Times.Once);
    }

    /// <summary>
    /// The client reports the session state of the underlying API client.
    /// </summary>
    [Fact]
    public void IsAuthenticated_FollowsTheSession()
    {
        _ = sync.SetupGet(s => s.IsAuthenticated).Returns(value: true);

        Assert.True(new ResourcePlanningApiClient(sync.Object).IsAuthenticated);
    }

    private static SyncApiReply Reply(HttpStatusCode status, string body) => new(status, Encoding.UTF8.GetBytes(body));
}
