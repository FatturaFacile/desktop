// <copyright file="CriticalPathEngineTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the critical path method: link types, lag and lead, float, critical path and cycles.
/// </summary>
public sealed class CriticalPathEngineTests
{
    private static readonly DateOnly Start = ProjectTestData.Day(1);

    /// <summary>
    /// A finish-to-start chain schedules the successor right after the predecessor, and a side task gets float.
    /// </summary>
    [Fact]
    public void Calculate_FinishToStartChain_SchedulesAndFindsTheCriticalPath()
    {
        var a = ProjectTestData.Node("1", 1, 3);
        var b = ProjectTestData.Node("2", 1, 2);
        var c = ProjectTestData.Node("3", 1, 1);

        var result = CriticalPathEngine.Calculate([a, b, c], [ProjectTestData.Link(a, b)], Start);

        var scheduleB = Of(result, b);
        Assert.Equal(ProjectTestData.Day(4), scheduleB.EarlyStart);
        Assert.Equal(ProjectTestData.Day(5), scheduleB.EarlyFinish);
        Assert.Equal(ProjectTestData.Day(5), result.ProjectFinish);
        Assert.Equal([a.Id, b.Id], result.CriticalPath.Where(id => id != c.Id).ToArray());
        Assert.False(result.HasCycle);
        var scheduleC = Of(result, c);
        Assert.Equal(4, scheduleC.TotalFloatDays);
        Assert.Equal(4, scheduleC.FreeFloatDays);
        Assert.False(scheduleC.IsCritical);
        Assert.Equal(ProjectTestData.Day(5), scheduleC.LateFinish);
    }

    /// <summary>
    /// With two branches only the longest one is critical and the shorter one carries the difference as float.
    /// </summary>
    [Fact]
    public void Calculate_ParallelBranches_ShortBranchHasFloat()
    {
        var a = ProjectTestData.Node("1", 1, 3);
        var b = ProjectTestData.Node("2", 1, 2);
        var c = ProjectTestData.Node("3", 1, 5);

        var result = CriticalPathEngine.Calculate([a, b, c], [ProjectTestData.Link(a, b), ProjectTestData.Link(a, c)], Start);

        Assert.Equal([a.Id, c.Id], result.CriticalPath.ToArray());
        Assert.Equal(3, Of(result, b).TotalFloatDays);
        Assert.Equal(3, Of(result, b).FreeFloatDays);
        Assert.Equal(0, Of(result, c).TotalFloatDays);
    }

    /// <summary>
    /// A lag delays the successor and a lead (negative lag) overlaps it.
    /// </summary>
    [Theory]
    [InlineData(2, 6)]
    [InlineData(0, 4)]
    [InlineData(-1, 3)]
    public void Calculate_FinishToStartWithLagOrLead_ShiftsTheSuccessor(int lag, int expectedStartDay)
    {
        var a = ProjectTestData.Node("1", 1, 3);
        var b = ProjectTestData.Node("2", 1, 2);

        var result = CriticalPathEngine.Calculate([a, b], [ProjectTestData.Link(a, b, DependencyType.FS, lag)], Start);

        Assert.Equal(ProjectTestData.Day(expectedStartDay), Of(result, b).EarlyStart);
    }

    /// <summary>
    /// A start-to-start link starts the successor relative to the start of the predecessor.
    /// </summary>
    [Fact]
    public void Calculate_StartToStart_UsesThePredecessorStart()
    {
        var a = ProjectTestData.Node("1", 1, 3);
        var b = ProjectTestData.Node("2", 1, 2);

        var result = CriticalPathEngine.Calculate([a, b], [ProjectTestData.Link(a, b, DependencyType.SS, 1)], Start);

        Assert.Equal(ProjectTestData.Day(2), Of(result, b).EarlyStart);
        Assert.Equal(ProjectTestData.Day(3), Of(result, b).EarlyFinish);
        Assert.Equal(ProjectTestData.Day(3), result.ProjectFinish);
    }

    /// <summary>
    /// A finish-to-finish link ends the successor with the predecessor.
    /// </summary>
    [Fact]
    public void Calculate_FinishToFinish_AlignsTheFinishes()
    {
        var a = ProjectTestData.Node("1", 1, 3);
        var b = ProjectTestData.Node("2", 1, 2);

        var result = CriticalPathEngine.Calculate([a, b], [ProjectTestData.Link(a, b, DependencyType.FF)], Start);

        Assert.Equal(ProjectTestData.Day(2), Of(result, b).EarlyStart);
        Assert.Equal(ProjectTestData.Day(3), Of(result, b).EarlyFinish);
    }

    /// <summary>
    /// A start-to-finish link makes the successor finish after the predecessor started plus the lag.
    /// </summary>
    [Fact]
    public void Calculate_StartToFinish_UsesLagToPushTheSuccessor()
    {
        var a = ProjectTestData.Node("1", 1, 3);
        var b = ProjectTestData.Node("2", 1, 2);

        var result = CriticalPathEngine.Calculate([a, b], [ProjectTestData.Link(a, b, DependencyType.SF, 4)], Start);

        Assert.Equal(ProjectTestData.Day(3), Of(result, b).EarlyStart);
        Assert.Equal(ProjectTestData.Day(4), Of(result, b).EarlyFinish);
    }

    /// <summary>
    /// A milestone lasts zero days: it starts and finishes on the day its predecessor ends.
    /// </summary>
    [Fact]
    public void Calculate_Milestone_HasZeroDuration()
    {
        var a = ProjectTestData.Node("1", 1, 3);
        var m = ProjectTestData.Node("2", 1, 1, type: WbsNodeType.Milestone);

        var result = CriticalPathEngine.Calculate([a, m], [ProjectTestData.Link(a, m)], Start);

        var schedule = Of(result, m);
        Assert.Equal(ProjectTestData.Day(4), schedule.EarlyStart);
        Assert.Equal(ProjectTestData.Day(4), schedule.EarlyFinish);
        Assert.True(schedule.IsCritical);
    }

    /// <summary>
    /// The planned start of a node without predecessors is a start-no-earlier-than constraint.
    /// </summary>
    [Fact]
    public void Calculate_PlannedStart_ActsAsAConstraint()
    {
        var a = ProjectTestData.Node("1", 10, 12);

        var result = CriticalPathEngine.Calculate([a], [], Start);

        Assert.Equal(ProjectTestData.Day(10), Of(result, a).EarlyStart);
        Assert.Equal(ProjectTestData.Day(12), Of(result, a).EarlyFinish);
    }

    /// <summary>
    /// A summary node spans its children and is critical when one of them is.
    /// </summary>
    [Fact]
    public void Calculate_SummaryNode_AggregatesItsLeaves()
    {
        var phase = ProjectTestData.Node("1", 1, 1, type: WbsNodeType.Phase);
        var a = ProjectTestData.Node("1.1", 1, 3, parent: phase.Id);
        var b = ProjectTestData.Node("1.2", 1, 2, parent: phase.Id);
        var side = ProjectTestData.Node("2", 1, 1);

        var result = CriticalPathEngine.Calculate([phase, a, b, side], [ProjectTestData.Link(a, b), ProjectTestData.Link(a, side)], Start);

        var summary = Of(result, phase);
        Assert.Equal(ProjectTestData.Day(1), summary.EarlyStart);
        Assert.Equal(ProjectTestData.Day(5), summary.EarlyFinish);
        Assert.True(summary.IsCritical);
        Assert.Equal(4, result.Nodes.Count);
    }

    /// <summary>
    /// A direct two-node cycle is reported and no schedule is produced.
    /// </summary>
    [Fact]
    public void Calculate_Cycle_IsDetected()
    {
        var a = ProjectTestData.Node("1", 1, 2);
        var b = ProjectTestData.Node("2", 1, 2);

        var result = CriticalPathEngine.Calculate([a, b], [ProjectTestData.Link(a, b), ProjectTestData.Link(b, a)], Start);

        Assert.True(result.HasCycle);
        Assert.Contains(a.Id, result.CycleNodeIds);
        Assert.Contains(b.Id, result.CycleNodeIds);
        Assert.Empty(result.Nodes);
        Assert.Null(result.ProjectFinish);
    }

    /// <summary>
    /// A longer cycle is detected without blaming a node outside it, and a self link counts as a cycle.
    /// </summary>
    [Fact]
    public void Calculate_LongCycleAndSelfLink_AreDetected()
    {
        var a = ProjectTestData.Node("1", 1, 2);
        var b = ProjectTestData.Node("2", 1, 2);
        var c = ProjectTestData.Node("3", 1, 2);
        var free = ProjectTestData.Node("4", 1, 2);

        var ring = CriticalPathEngine.Calculate(
            [a, b, c, free],
            [ProjectTestData.Link(a, b), ProjectTestData.Link(b, c), ProjectTestData.Link(c, a)],
            Start);
        var self = CriticalPathEngine.Calculate([a], [ProjectTestData.Link(a, a)], Start);

        Assert.True(ring.HasCycle);
        Assert.DoesNotContain(free.Id, ring.CycleNodeIds);
        Assert.Equal(3, ring.CycleNodeIds.Count);
        Assert.True(self.HasCycle);
    }

    /// <summary>
    /// A dependency that touches a summary node is ignored instead of breaking the schedule.
    /// </summary>
    [Fact]
    public void Calculate_LinkOnSummaryNode_IsIgnored()
    {
        var phase = ProjectTestData.Node("1", 1, 1, type: WbsNodeType.Phase);
        var child = ProjectTestData.Node("1.1", 1, 2, parent: phase.Id);
        var other = ProjectTestData.Node("2", 1, 2);

        var result = CriticalPathEngine.Calculate([phase, child, other], [ProjectTestData.Link(phase, other)], Start);

        Assert.False(result.HasCycle);
        Assert.Equal(ProjectTestData.Day(1), Of(result, other).EarlyStart);
    }

    /// <summary>
    /// Applying a result writes the schedule onto the nodes and clears nodes without one.
    /// </summary>
    [Fact]
    public void Apply_WritesTheScheduleAndClearsMissingNodes()
    {
        var a = ProjectTestData.Node("1", 1, 3);
        var b = ProjectTestData.Node("2", 1, 2);
        IReadOnlyList<WbsNode> nodes = [a, b];
        var result = CriticalPathEngine.Calculate(nodes, [ProjectTestData.Link(a, b)], Start);

        CriticalPathEngine.Apply(nodes, result);

        Assert.Equal(ProjectTestData.Day(4), b.EarlyStart);
        Assert.Equal(0, b.TotalFloatDays);
        Assert.True(b.IsCritical);

        CriticalPathEngine.Apply(nodes, CriticalPathEngine.Calculate(nodes, [ProjectTestData.Link(a, b), ProjectTestData.Link(b, a)], Start));

        Assert.Null(b.EarlyStart);
        Assert.Null(b.TotalFloatDays);
        Assert.False(b.IsCritical);
    }

    private static NodeSchedule Of(CriticalPathResult result, WbsNode node) => Assert.Single(result.Nodes, schedule => schedule.NodeId == node.Id);
}
