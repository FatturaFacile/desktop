// <copyright file="CapacitySnapshotsViewModelTests.cs" company="Fattura Facile">
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
/// Tests of the capacity snapshots view model: the stored snapshots of the window and the recalculation, for all employees or one.
/// </summary>
public sealed class CapacitySnapshotsViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private PlanningCommandOutcome<SnapshotRecalculationResult> recalculated = new(HttpStatusCode.OK, new SnapshotRecalculationResult(5, 20), []);

    /// <summary>
    /// Initializes a new instance of the <see cref="CapacitySnapshotsViewModelTests"/> class.
    /// </summary>
    public CapacitySnapshotsViewModelTests()
    {
        IReadOnlyList<PlanningResource> resources =
        [
            ResourcePlanningTestData.Resource(ResourcePlanningTestData.Bruno, "Bruno Bianchi"),
            ResourcePlanningTestData.Resource(ResourcePlanningTestData.Anna, "Anna Rossi"),
        ];
        IReadOnlyList<ResourceCapacitySnapshot> snapshots =
        [
            new ResourceCapacitySnapshot(Guid.NewGuid(), ResourcePlanningTestData.Bruno, new DateOnly(2026, 11, 16), 40m, 8m, 32m, 20m, 4m, 75d, null, null),
            new ResourceCapacitySnapshot(Guid.NewGuid(), ResourcePlanningTestData.Anna, new DateOnly(2026, 11, 16), 40m, 0m, 40m, 30m, 5m, 87.5d, null, null),
            new ResourceCapacitySnapshot(Guid.NewGuid(), ResourcePlanningTestData.Anna, new DateOnly(2026, 11, 9), 40m, 0m, 40m, 10m, 0m, 25d, null, null),
        ];
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningResourcesQuery>(), Any)).ReturnsAsync(resources);
        _ = sender.Setup(s => s.Send(It.IsAny<GetCapacitySnapshotsQuery>(), Any)).ReturnsAsync(snapshots);
        _ = sender.Setup(s => s.Send(It.IsAny<RecalculateCapacitySnapshotsCommand>(), Any)).ReturnsAsync(() => recalculated);
    }

    /// <summary>
    /// Loading asks for the window around today (four weeks each way) and lists the snapshots by week and employee with the figures.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheStoredSnapshots()
    {
        var vm = Create();

        await vm.LoadAsync();

        sender.Verify(s => s.Send(It.Is<GetCapacitySnapshotsQuery>(q => q.From == new DateOnly(2026, 10, 24) && q.To == new DateOnly(2026, 12, 19) && q.EmployeeId == null), Any), Times.Once);
        Assert.Equal(3, vm.EmployeeChoices.Count);
        Assert.True(vm.HasData);
        Assert.Equal(new[] { "Anna Rossi", "Anna Rossi", "Bruno Bianchi" }, vm.Rows.Select(row => row.EmployeeName), StringComparer.Ordinal);
        Assert.Equal(new[] { new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 16) }, vm.Rows.Select(row => row.Snapshot.WeekStartDate));
        Assert.Equal(87.5d.ToString("0.#", loc.CurrentCulture) + "%", vm.Rows[1].UtilizationText);
        Assert.Equal(string.Empty, vm.StatusMessage);
    }

    /// <summary>
    /// Recalculating sends the window and the employee, reports the weeks and snapshots and reloads the stored snapshots.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Recalculate_SendsTheWindowAndReportsTheResult()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedEmployee = vm.EmployeeChoices.Single(choice => string.Equals(choice.Label, "Anna Rossi", StringComparison.Ordinal));
        vm.FromDate = new DateTime(2026, 11, 2);
        vm.ToDate = new DateTime(2026, 11, 29);

        await vm.RecalculateAsync();

        sender.Verify(s => s.Send(It.Is<RecalculateCapacitySnapshotsCommand>(c => c.Recalculation == new SnapshotRecalculationRequest(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 29), ResourcePlanningTestData.Anna)), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetCapacitySnapshotsQuery>(), Any), Times.Exactly(2));
        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_Snapshots_Recalculated"], "5", "20"), vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// A window that is missing or ends before it starts is explained and not sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Recalculate_InvalidWindow_IsExplainedAndNotSent()
    {
        var vm = Create();
        vm.FromDate = new DateTime(2026, 11, 30);
        vm.ToDate = new DateTime(2026, 11, 2);
        await vm.RecalculateAsync();
        Assert.Equal(loc["ResourcePlanning_Setup_snapshot_invalid_period"], vm.StatusMessage);

        vm.FromDate = null;
        await vm.RecalculateAsync();

        Assert.Equal(loc["ResourcePlanning_Setup_snapshot_invalid_period"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<RecalculateCapacitySnapshotsCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// Loading with an invalid window is explained and does not ask the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_InvalidWindow_IsExplained()
    {
        var vm = Create();
        vm.ToDate = new DateTime(2026, 1, 1);

        await vm.LoadAsync();

        Assert.Equal(loc["ResourcePlanning_Setup_snapshot_invalid_period"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<GetCapacitySnapshotsQuery>(), Any), Times.Never);
    }

    /// <summary>
    /// A refusal of the server is shown and the snapshots are not reloaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Recalculate_Refused_ShowsTheViolation()
    {
        recalculated = new PlanningCommandOutcome<SnapshotRecalculationResult>(HttpStatusCode.UnprocessableEntity, null, [new PlanningIssue("invalid_period", null)]);
        var vm = Create();

        await vm.RecalculateAsync();

        Assert.Equal(loc["ResourcePlanning_Error_invalid_period"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<GetCapacitySnapshotsQuery>(), Any), Times.Never);
    }

    /// <summary>
    /// An accepted answer without a result is reported as a failed save instead of a success.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Recalculate_AcceptedWithoutResult_IsAFailure()
    {
        recalculated = new PlanningCommandOutcome<SnapshotRecalculationResult>(HttpStatusCode.OK, null, []);
        var vm = Create();

        await vm.RecalculateAsync();

        Assert.Equal(loc["ResourcePlanning_SaveFailed"], vm.StatusMessage);
    }

    /// <summary>
    /// A server that cannot be reached shows the save and load failures.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ServerDown_ShowsTheFailures()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<RecalculateCapacitySnapshotsCommand>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        _ = sender.Setup(s => s.Send(It.IsAny<GetCapacitySnapshotsQuery>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = Create();

        await vm.RecalculateAsync();
        Assert.Equal(loc["ResourcePlanning_SaveFailed"], vm.StatusMessage);

        await vm.LoadAsync();
        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    private CapacitySnapshotsViewModel Create() => new(sender.Object, loc, time);
}
