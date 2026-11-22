// <copyright file="WbsRollupCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the bottom-up WBS roll-up.
/// </summary>
public sealed class WbsRollupCalculatorTests
{
    /// <summary>
    /// Summary nodes and the project sum their leaves, with progress weighted by planned hours and the dates spanning the children.
    /// </summary>
    [Fact]
    public void Apply_RollsLeavesUpThroughEveryLevel()
    {
        var project = ProjectTestData.Project();
        var phase = ProjectTestData.Node("1", 1, 1, type: WbsNodeType.Phase);
        var first = ProjectTestData.Node("1.1", 2, 5, parent: phase.Id, hours: 10m, cost: 100m, revenue: 200m, percent: 50m);
        var second = ProjectTestData.Node("1.2", 4, 9, parent: phase.Id, hours: 30m, cost: 300m, revenue: 400m, percent: 100m);
        var other = ProjectTestData.Node("2", 1, 1, type: WbsNodeType.Phase);
        var deliverable = ProjectTestData.Node("2.1", 1, 1, parent: other.Id, type: WbsNodeType.Deliverable);
        var leaf = ProjectTestData.Node("2.1.1", 10, 12, parent: deliverable.Id, hours: 20m, cost: 50m, percent: 0m);
        WbsNode[] nodes = [phase, first, second, other, deliverable, leaf];

        WbsRollupCalculator.Apply(project, nodes);

        Assert.Equal(40m, phase.PlannedHours);
        Assert.Equal(400m, phase.PlannedCost);
        Assert.Equal(600m, phase.PlannedRevenue);
        Assert.Equal(87.5m, phase.PercentComplete);
        Assert.Equal(ProjectTestData.Day(2), phase.StartDate);
        Assert.Equal(ProjectTestData.Day(9), phase.EndDate);
        Assert.Equal(20m, deliverable.PlannedHours);
        Assert.Equal(20m, other.PlannedHours);
        Assert.Equal(ProjectTestData.Day(12), other.EndDate);
        Assert.Equal(60m, project.PlannedHours);
        Assert.Equal(450m, project.PlannedCost);
        Assert.Equal(600m, project.PlannedRevenue);
        Assert.Equal(58.33m, project.PercentComplete);
    }

    /// <summary>
    /// Actual hours and costs roll up as well.
    /// </summary>
    [Fact]
    public void Apply_RollsActualsUp()
    {
        var project = ProjectTestData.Project();
        var phase = ProjectTestData.Node("1", 1, 1, type: WbsNodeType.Phase);
        var first = ProjectTestData.Node("1.1", 1, 2, parent: phase.Id);
        var second = ProjectTestData.Node("1.2", 3, 4, parent: phase.Id);
        first.ActualHours = 3m;
        first.ActualCost = 30m;
        second.ActualHours = 4.5m;
        second.ActualCost = 45m;

        WbsRollupCalculator.Apply(project, [phase, first, second]);

        Assert.Equal(7.5m, project.ActualHours);
        Assert.Equal(75m, phase.ActualCost);
    }

    /// <summary>
    /// Without planned hours the progress of a group is the plain average.
    /// </summary>
    [Fact]
    public void Weighted_WithoutHours_AveragesTheChildren()
    {
        var first = ProjectTestData.Node("1", 1, 2, percent: 20m);
        var second = ProjectTestData.Node("2", 1, 2, percent: 60m);

        Assert.Equal(40m, WbsRollupCalculator.Weighted([first, second]));
        Assert.Equal(0m, WbsRollupCalculator.Weighted([]));
    }

    /// <summary>
    /// A project without nodes has zero totals.
    /// </summary>
    [Fact]
    public void Apply_WithoutNodes_ZeroesTheProject()
    {
        var project = ProjectTestData.Project();
        project.PlannedHours = 99m;
        project.PercentComplete = 40m;

        WbsRollupCalculator.Apply(project, []);

        Assert.Equal(0m, project.PlannedHours);
        Assert.Equal(0m, project.PercentComplete);
    }

    /// <summary>
    /// A node that is its own parent does not loop and is treated as a top-level leaf.
    /// </summary>
    [Fact]
    public void Apply_SelfParent_DoesNotLoop()
    {
        var project = ProjectTestData.Project();
        var node = ProjectTestData.Node("1", 1, 2, hours: 5m);
        node.ParentNodeId = node.Id;

        WbsRollupCalculator.Apply(project, [node]);

        Assert.Equal(5m, project.PlannedHours);
    }
}
