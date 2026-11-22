// <copyright file="BaselineVarianceCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the baseline snapshot and the variance against it.
/// </summary>
public sealed class BaselineVarianceCalculatorTests
{
    /// <summary>
    /// The snapshot survives a JSON round trip.
    /// </summary>
    [Fact]
    public void Serializer_RoundTripsTheSnapshot()
    {
        var project = ProjectTestData.Project();
        var node = ProjectTestData.Node("1", 1, 5, project.Id, hours: 8m, cost: 80m, revenue: 120m);
        var snapshot = BaselineSnapshot.Capture(project, [node]);

        var json = BaselineSnapshotSerializer.Serialize(snapshot);
        var back = BaselineSnapshotSerializer.Deserialize(json);

        Assert.Contains("\"planned_hours\"", json, StringComparison.Ordinal);
        Assert.Contains("\"work_package\"", json, StringComparison.Ordinal);
        Assert.NotNull(back);
        Assert.Equal(project.Id, back.ProjectId);
        var frozen = Assert.Single(back.Nodes);
        Assert.Equal(node.Id, frozen.NodeId);
        Assert.Equal(ProjectTestData.Day(5), frozen.EndDate);
        Assert.Equal(120m, frozen.PlannedRevenue);
        Assert.Null(BaselineSnapshotSerializer.Deserialize("not json"));
        Assert.Null(BaselineSnapshotSerializer.Deserialize(string.Empty));
    }

    /// <summary>
    /// Moved, resized, added and removed nodes show up with the right slip and amounts, and unchanged ones stay unchanged.
    /// </summary>
    [Fact]
    public void Compare_ReportsSlipAndVariance()
    {
        var project = ProjectTestData.Project();
        var kept = ProjectTestData.Node("1", 1, 5, project.Id, hours: 10m, cost: 100m, revenue: 150m);
        var moved = ProjectTestData.Node("2", 6, 10, project.Id, hours: 20m, cost: 200m, revenue: 300m);
        var removed = ProjectTestData.Node("3", 11, 12, project.Id, hours: 4m, cost: 40m, revenue: 60m);
        project.PlannedHours = 34m;
        project.PlannedCost = 340m;
        project.PlannedRevenue = 510m;
        project.TargetEndDate = ProjectTestData.Day(12);
        var baseline = new ProjectBaseline { Id = Guid.NewGuid(), BaselineNumber = 1, Name = "Initial", CapturedAt = DateTime.UtcNow, ProjectId = project.Id };
        var snapshot = BaselineSnapshot.Capture(project, [kept, moved, removed]);

        moved.StartDate = ProjectTestData.Day(9);
        moved.EndDate = ProjectTestData.Day(14);
        moved.PlannedHours = 25m;
        moved.PlannedCost = 260m;
        var added = ProjectTestData.Node("4", 15, 16, project.Id, hours: 6m, cost: 30m, revenue: 90m);
        project.PlannedHours = 41m;
        project.PlannedCost = 390m;
        project.PlannedRevenue = 540m;

        var variance = BaselineVarianceCalculator.Compare(baseline, snapshot, project, [kept, moved, added]);

        Assert.Equal(VarianceStatus.Unchanged, Row(variance, kept.Id).Status);
        var movedRow = Row(variance, moved.Id);
        Assert.Equal(VarianceStatus.Changed, movedRow.Status);
        Assert.Equal(3, movedRow.StartVarianceDays);
        Assert.Equal(4, movedRow.FinishVarianceDays);
        Assert.Equal(5m, movedRow.HoursVariance);
        Assert.Equal(60m, movedRow.CostVariance);
        var addedRow = Row(variance, added.Id);
        Assert.Equal(VarianceStatus.Added, addedRow.Status);
        Assert.Equal(6m, addedRow.HoursVariance);
        Assert.Null(addedRow.StartVarianceDays);
        var removedRow = Row(variance, removed.Id);
        Assert.Equal(VarianceStatus.Removed, removedRow.Status);
        Assert.Equal(-4m, removedRow.HoursVariance);
        Assert.Equal(-60m, removedRow.RevenueVariance);
        Assert.Equal(7m, variance.HoursVariance);
        Assert.Equal(50m, variance.CostVariance);
        Assert.Equal(30m, variance.RevenueVariance);
        Assert.Equal(0, variance.StartVarianceDays);
        Assert.Equal(4, variance.FinishVarianceDays);
        Assert.Equal("Initial", variance.BaselineName);
    }

    private static NodeVariance Row(BaselineVariance variance, Guid id) => Assert.Single(variance.Nodes, row => row.NodeId == id);
}
