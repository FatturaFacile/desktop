// <copyright file="ResourcePlanningSetupHandlersTests.cs" company="Fattura Facile">
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
/// Tests of the CQRS handlers of the Mod 079 setup screens (skill taxonomy, utilization targets, capacity snapshots, holidays): each one
/// forwards its request to the API client, and reads and writes need the right permission.
/// </summary>
public sealed class ResourcePlanningSetupHandlersTests
{
    private static readonly DateOnly From = new(2026, 11, 16);
    private static readonly DateOnly To = new(2026, 12, 6);

    private readonly Mock<IResourcePlanningApiClient> client = new(MockBehavior.Strict);

    /// <summary>
    /// The skill commands forward their bodies and return the outcome of the client unchanged.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SkillCommands_ForwardTheirBodies()
    {
        var body = new PlanningSkillRequest("Site", "Welding", true, "WELD-EN");
        var skill = new PlanningSkill(ResourcePlanningTestData.SkillWelding, "Site", "Welding", true, "WELD-EN");
        var saved = new PlanningCommandOutcome<PlanningSkill>(HttpStatusCode.OK, skill, []);
        var deleted = new PlanningCommandOutcome<bool>(HttpStatusCode.NoContent, true, []);
        _ = client.Setup(c => c.CreateSkillAsync(body, CancellationToken.None)).ReturnsAsync(saved);
        _ = client.Setup(c => c.UpdateSkillAsync(skill.Id, body, CancellationToken.None)).ReturnsAsync(saved);
        _ = client.Setup(c => c.DeleteSkillAsync(skill.Id, CancellationToken.None)).ReturnsAsync(deleted);

        Assert.Same(saved, await new CreatePlanningSkillCommandHandler(client.Object).Handle(new CreatePlanningSkillCommand(body), CancellationToken.None));
        Assert.Same(saved, await new UpdatePlanningSkillCommandHandler(client.Object).Handle(new UpdatePlanningSkillCommand(skill.Id, body), CancellationToken.None));
        Assert.Same(deleted, await new DeletePlanningSkillCommandHandler(client.Object).Handle(new DeletePlanningSkillCommand(skill.Id), CancellationToken.None));
    }

    /// <summary>
    /// The target, snapshot and holiday writes forward their bodies and return the outcome of the client unchanged.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SetupWrites_ForwardTheirBodies()
    {
        var target = new UtilizationTargetRequest(ResourcePlanningTestData.Department, null, 75d);
        var targetOutcome = new PlanningCommandOutcome<UtilizationTarget>(HttpStatusCode.OK, new UtilizationTarget(Guid.NewGuid(), ResourcePlanningTestData.Department, null, 75d), []);
        var window = new SnapshotRecalculationRequest(From, To, null);
        var windowOutcome = new PlanningCommandOutcome<SnapshotRecalculationResult>(HttpStatusCode.OK, new SnapshotRecalculationResult(3, 12), []);
        var holiday = new LocalHolidayRequest("CH", "TI", null, 8, 15, null, "Local fair", 0.5m);
        var holidayOutcome = new PlanningCommandOutcome<LocalHoliday>(HttpStatusCode.Created, new LocalHoliday(Guid.NewGuid(), "CH", "TI", null, 8, 15, null, "Local fair", 0.5m), []);
        var removed = new PlanningCommandOutcome<bool>(HttpStatusCode.NoContent, true, []);
        var holidayId = Guid.NewGuid();
        _ = client.Setup(c => c.SetUtilizationTargetAsync(target, CancellationToken.None)).ReturnsAsync(targetOutcome);
        _ = client.Setup(c => c.RecalculateSnapshotsAsync(window, CancellationToken.None)).ReturnsAsync(windowOutcome);
        _ = client.Setup(c => c.CreateLocalHolidayAsync(holiday, CancellationToken.None)).ReturnsAsync(holidayOutcome);
        _ = client.Setup(c => c.DeleteLocalHolidayAsync(holidayId, CancellationToken.None)).ReturnsAsync(removed);

        Assert.Same(targetOutcome, await new SetUtilizationTargetCommandHandler(client.Object).Handle(new SetUtilizationTargetCommand(target), CancellationToken.None));
        Assert.Same(windowOutcome, await new RecalculateCapacitySnapshotsCommandHandler(client.Object).Handle(new RecalculateCapacitySnapshotsCommand(window), CancellationToken.None));
        Assert.Same(holidayOutcome, await new CreateLocalHolidayCommandHandler(client.Object).Handle(new CreateLocalHolidayCommand(holiday), CancellationToken.None));
        Assert.Same(removed, await new DeleteLocalHolidayCommandHandler(client.Object).Handle(new DeleteLocalHolidayCommand(holidayId), CancellationToken.None));
    }

    /// <summary>
    /// The target, snapshot and holiday queries forward their filters.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SetupQueries_ForwardTheirFilters()
    {
        IReadOnlyList<UtilizationTarget> targets = [];
        IReadOnlyList<ResourceCapacitySnapshot> snapshots = [];
        IReadOnlyList<PlanningHoliday> resolved = [];
        IReadOnlyList<LocalHoliday> local = [];
        _ = client.Setup(c => c.GetUtilizationTargetsAsync(CancellationToken.None)).ReturnsAsync(targets);
        _ = client.Setup(c => c.GetSnapshotsAsync(From, To, ResourcePlanningTestData.Anna, CancellationToken.None)).ReturnsAsync(snapshots);
        _ = client.Setup(c => c.GetHolidaysAsync("CH", "TI", "Lugano", 2027, CancellationToken.None)).ReturnsAsync(resolved);
        _ = client.Setup(c => c.GetLocalHolidaysAsync(CancellationToken.None)).ReturnsAsync(local);

        Assert.Same(targets, await new GetUtilizationTargetsQueryHandler(client.Object).Handle(new GetUtilizationTargetsQuery(), CancellationToken.None));
        Assert.Same(snapshots, await new GetCapacitySnapshotsQueryHandler(client.Object).Handle(new GetCapacitySnapshotsQuery(From, To, ResourcePlanningTestData.Anna), CancellationToken.None));
        Assert.Same(resolved, await new GetPlanningHolidaysQueryHandler(client.Object).Handle(new GetPlanningHolidaysQuery("CH", "TI", "Lugano", 2027), CancellationToken.None));
        Assert.Same(local, await new GetLocalHolidaysQueryHandler(client.Object).Handle(new GetLocalHolidaysQuery(), CancellationToken.None));
    }

    /// <summary>
    /// A null request is rejected by every handler.
    /// </summary>
    [Fact]
    public void Handlers_NullRequest_Throw()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new CreatePlanningSkillCommandHandler(client.Object).Handle(null!, CancellationToken.None));
        _ = Assert.Throws<ArgumentNullException>(() => new UpdatePlanningSkillCommandHandler(client.Object).Handle(null!, CancellationToken.None));
        _ = Assert.Throws<ArgumentNullException>(() => new DeletePlanningSkillCommandHandler(client.Object).Handle(null!, CancellationToken.None));
        _ = Assert.Throws<ArgumentNullException>(() => new GetUtilizationTargetsQueryHandler(client.Object).Handle(null!, CancellationToken.None));
        _ = Assert.Throws<ArgumentNullException>(() => new SetUtilizationTargetCommandHandler(client.Object).Handle(null!, CancellationToken.None));
        _ = Assert.Throws<ArgumentNullException>(() => new RecalculateCapacitySnapshotsCommandHandler(client.Object).Handle(null!, CancellationToken.None));
        _ = Assert.Throws<ArgumentNullException>(() => new GetCapacitySnapshotsQueryHandler(client.Object).Handle(null!, CancellationToken.None));
        _ = Assert.Throws<ArgumentNullException>(() => new GetPlanningHolidaysQueryHandler(client.Object).Handle(null!, CancellationToken.None));
        _ = Assert.Throws<ArgumentNullException>(() => new GetLocalHolidaysQueryHandler(client.Object).Handle(null!, CancellationToken.None));
        _ = Assert.Throws<ArgumentNullException>(() => new CreateLocalHolidayCommandHandler(client.Object).Handle(null!, CancellationToken.None));
        _ = Assert.Throws<ArgumentNullException>(() => new DeleteLocalHolidayCommandHandler(client.Object).Handle(null!, CancellationToken.None));
    }

    /// <summary>
    /// Reads need the read permission and writes need the master data write permission.
    /// </summary>
    [Fact]
    public void Requests_RequireTheirPermission()
    {
        var body = new PlanningSkillRequest("Site", "Welding", false, null);
        IAuthorizedRequest[] reads =
        [
            new GetUtilizationTargetsQuery(),
            new GetCapacitySnapshotsQuery(From, To, null),
            new GetPlanningHolidaysQuery("IT", null, null, 2027),
            new GetLocalHolidaysQuery(),
        ];
        IAuthorizedRequest[] writes =
        [
            new CreatePlanningSkillCommand(body),
            new UpdatePlanningSkillCommand(Guid.NewGuid(), body),
            new DeletePlanningSkillCommand(Guid.NewGuid()),
            new SetUtilizationTargetCommand(new UtilizationTargetRequest(null, null, 70d)),
            new RecalculateCapacitySnapshotsCommand(new SnapshotRecalculationRequest(From, To, null)),
            new CreateLocalHolidayCommand(new LocalHolidayRequest("IT", null, null, 6, 2, null, "Local", 1m)),
            new DeleteLocalHolidayCommand(Guid.NewGuid()),
        ];

        Assert.All(reads, request => Assert.Equal(Permission.CommercialRead, request.RequiredPermission));
        Assert.All(writes, request => Assert.Equal(Permission.MasterDataWrite, request.RequiredPermission));
    }
}
