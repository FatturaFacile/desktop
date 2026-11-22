// <copyright file="ResourcePlanningHandlersTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Net;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Mod 079 CQRS handlers: each one forwards its request to the API client, and reads and writes need the right permission.
/// </summary>
public sealed class ResourcePlanningHandlersTests
{
    private static readonly DateOnly From = new(2026, 11, 16);
    private static readonly DateOnly To = new(2026, 12, 6);

    private readonly Mock<IResourcePlanningApiClient> client = new(MockBehavior.Strict);

    /// <summary>
    /// The heatmap query forwards the window, the granularity and the department.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task HeatmapQuery_ForwardsToTheClient()
    {
        var heatmap = ResourcePlanningTestData.Heatmap();
        _ = client.Setup(c => c.GetHeatmapAsync(From, To, HeatmapGranularity.Day, ResourcePlanningTestData.Department, CancellationToken.None)).ReturnsAsync(heatmap);

        var result = await new GetCapacityHeatmapQueryHandler(client.Object).Handle(new GetCapacityHeatmapQuery(From, To, HeatmapGranularity.Day, ResourcePlanningTestData.Department), CancellationToken.None);

        Assert.Same(heatmap, result);
    }

    /// <summary>
    /// The allocation, resource, skill, profile, candidate and utilization queries forward their filters.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ReadQueries_ForwardTheirFilters()
    {
        IReadOnlyList<ResourceAllocation> allocations = [];
        IReadOnlyList<PlanningResource> resources = [];
        IReadOnlyList<PlanningSkill> skills = [];
        IReadOnlyList<EmployeeSkillProfile> profiles = [];
        IReadOnlyList<ResourceCandidate> candidates = [];
        var report = new UtilizationReport(From, To, UtilizationGroupBy.Department, 70d, [], ResourcePlanningTestData.Utilization("Total", 0d, 0d, 70d, UtilizationStatus.BelowTarget));
        var search = new CandidateSearchRequest(From, To, 8m, [], null, null, null, true, false, 10);
        _ = client.Setup(c => c.GetAllocationsAsync(From, To, ResourcePlanningTestData.Anna, null, ResourceBookingType.HardCommitted, CancellationToken.None)).ReturnsAsync(allocations);
        _ = client.Setup(c => c.GetResourcesAsync(null, CancellationToken.None)).ReturnsAsync(resources);
        _ = client.Setup(c => c.GetSkillsAsync("Site", CancellationToken.None)).ReturnsAsync(skills);
        _ = client.Setup(c => c.GetSkillProfilesAsync(ResourcePlanningTestData.Anna, null, CancellationToken.None)).ReturnsAsync(profiles);
        _ = client.Setup(c => c.SearchCandidatesAsync(search, CancellationToken.None)).ReturnsAsync(candidates);
        _ = client.Setup(c => c.GetBillableUtilizationAsync(From, To, UtilizationGroupBy.Department, CancellationToken.None)).ReturnsAsync(report);

        Assert.Same(allocations, await new GetResourceAllocationsQueryHandler(client.Object).Handle(new GetResourceAllocationsQuery(From, To, ResourcePlanningTestData.Anna, null, ResourceBookingType.HardCommitted), CancellationToken.None));
        Assert.Same(resources, await new GetPlanningResourcesQueryHandler(client.Object).Handle(new GetPlanningResourcesQuery(null), CancellationToken.None));
        Assert.Same(skills, await new GetPlanningSkillsQueryHandler(client.Object).Handle(new GetPlanningSkillsQuery("Site"), CancellationToken.None));
        Assert.Same(profiles, await new GetEmployeeSkillProfilesQueryHandler(client.Object).Handle(new GetEmployeeSkillProfilesQuery(ResourcePlanningTestData.Anna, null), CancellationToken.None));
        Assert.Same(candidates, await new SearchResourceCandidatesQueryHandler(client.Object).Handle(new SearchResourceCandidatesQuery(search), CancellationToken.None));
        Assert.Same(report, await new GetBillableUtilizationQueryHandler(client.Object).Handle(new GetBillableUtilizationQuery(From, To, UtilizationGroupBy.Department), CancellationToken.None));
    }

    /// <summary>
    /// The write commands forward their bodies and return the outcome of the client unchanged.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task WriteCommands_ForwardTheirBodies()
    {
        var allocation = ResourcePlanningTestData.Allocation(ResourcePlanningTestData.AllocationOne, ResourceBookingType.SoftTentative, ResourcePlanningTestData.ProjectA);
        var mutation = new PlanningCommandOutcome<AllocationMutationResult>(HttpStatusCode.OK, ResourcePlanningTestData.Mutation(allocation), []);
        var deleted = new PlanningCommandOutcome<bool>(HttpStatusCode.NoContent, true, []);
        var create = new AllocationRequest(null, null, ResourcePlanningTestData.Anna, null, ResourceBookingType.SoftTentative, From, To, 4m, true, ResourcePlanningTestData.Opportunity, null, true);
        var convert = new ConvertToHardRequest(ResourcePlanningTestData.ProjectA, null);
        var rebalance = new RebalanceRequest(RebalanceStrategy.Shift, null, null, null, null, 3, null);
        _ = client.Setup(c => c.CreateAllocationAsync(create, CancellationToken.None)).ReturnsAsync(mutation);
        _ = client.Setup(c => c.ConvertToHardAsync(ResourcePlanningTestData.AllocationOne, convert, CancellationToken.None)).ReturnsAsync(mutation);
        _ = client.Setup(c => c.RebalanceAsync(ResourcePlanningTestData.AllocationOne, rebalance, CancellationToken.None)).ReturnsAsync(mutation);
        _ = client.Setup(c => c.DeleteAllocationAsync(ResourcePlanningTestData.AllocationOne, CancellationToken.None)).ReturnsAsync(deleted);

        Assert.Same(mutation, await new CreateResourceAllocationCommandHandler(client.Object).Handle(new CreateResourceAllocationCommand(create), CancellationToken.None));
        Assert.Same(mutation, await new ConvertAllocationToHardCommandHandler(client.Object).Handle(new ConvertAllocationToHardCommand(ResourcePlanningTestData.AllocationOne, convert), CancellationToken.None));
        Assert.Same(mutation, await new RebalanceAllocationCommandHandler(client.Object).Handle(new RebalanceAllocationCommand(ResourcePlanningTestData.AllocationOne, rebalance), CancellationToken.None));
        Assert.Same(deleted, await new DeleteResourceAllocationCommandHandler(client.Object).Handle(new DeleteResourceAllocationCommand(ResourcePlanningTestData.AllocationOne), CancellationToken.None));
    }

    /// <summary>
    /// A null request is rejected.
    /// </summary>
    [Fact]
    public void Handler_NullRequest_Throws()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new GetCapacityHeatmapQueryHandler(client.Object).Handle(null!, CancellationToken.None));
    }

    /// <summary>
    /// Reads need the read permission and writes need the master data write permission.
    /// </summary>
    [Fact]
    public void Requests_RequireTheirPermission()
    {
        IAuthorizedRequest[] reads =
        [
            new GetCapacityHeatmapQuery(From, To, HeatmapGranularity.Week, null),
            new GetResourceAllocationsQuery(From, To, null, null, null),
            new GetPlanningResourcesQuery(null),
            new GetPlanningSkillsQuery(null),
            new GetEmployeeSkillProfilesQuery(null, null),
            new GetBillableUtilizationQuery(From, To, UtilizationGroupBy.Resource),
            new SearchResourceCandidatesQuery(new CandidateSearchRequest(From, To, 8m, [], null, null, null, true, false, 10)),
        ];
        IAuthorizedRequest[] writes =
        [
            new CreateResourceAllocationCommand(new AllocationRequest(null, null, ResourcePlanningTestData.Anna, null, ResourceBookingType.SoftTentative, From, To, 4m, true, null, null, true)),
            new ConvertAllocationToHardCommand(ResourcePlanningTestData.AllocationOne, new ConvertToHardRequest(null, null)),
            new RebalanceAllocationCommand(ResourcePlanningTestData.AllocationOne, new RebalanceRequest(RebalanceStrategy.Shift, null, null, null, null, 1, null)),
            new DeleteResourceAllocationCommand(ResourcePlanningTestData.AllocationOne),
        ];

        Assert.All(reads, request => Assert.Equal(Permission.CommercialRead, request.RequiredPermission));
        Assert.All(writes, request => Assert.Equal(Permission.MasterDataWrite, request.RequiredPermission));
    }
}
