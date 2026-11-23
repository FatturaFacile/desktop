// <copyright file="CriticalPathEngine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The critical path method (CPM) over the leaf nodes of a WBS: forward and backward pass, FS/SS/FF/SF links with lag or lead,
/// total and free float, critical path and cycle detection. Durations count calendar days (a milestone lasts zero days) and the
/// planned start date of a node is a "start no earlier than" constraint. Summary nodes aggregate their descendants.
/// </summary>
public static class CriticalPathEngine
{
    /// <summary>
    /// Computes the schedule of a project.
    /// </summary>
    /// <param name="nodes">The nodes of the project.</param>
    /// <param name="dependencies">The dependencies between the nodes.</param>
    /// <param name="projectStart">The day the project starts; no node starts earlier.</param>
    /// <returns>The schedule, or a result that lists the nodes of a dependency cycle.</returns>
    public static CriticalPathResult Calculate(IReadOnlyList<WbsNode> nodes, IReadOnlyList<TaskDependency> dependencies, DateOnly projectStart)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(dependencies);

        var parents = nodes.Where(node => node.ParentNodeId.HasValue).Select(node => node.ParentNodeId!.Value).ToHashSet();
        var leaves = nodes.Where(node => !parents.Contains(node.Id)).ToList();
        var leafIds = leaves.Select(leaf => leaf.Id).ToHashSet();
        var links = dependencies
            .Where(link => leafIds.Contains(link.PredecessorNodeId) && leafIds.Contains(link.SuccessorNodeId))
            .ToList();

        var incoming = leaves.ToDictionary(leaf => leaf.Id, _ => new List<TaskDependency>());
        var outgoing = leaves.ToDictionary(leaf => leaf.Id, _ => new List<TaskDependency>());
        foreach (var link in links)
        {
            incoming[link.SuccessorNodeId].Add(link);
            outgoing[link.PredecessorNodeId].Add(link);
        }

        var order = TopologicalOrder(leaves, incoming, outgoing);
        if (order.Count < leaves.Count)
        {
            var sorted = order.ToHashSet();
            var cycle = leaves.Where(leaf => !sorted.Contains(leaf.Id)).Select(leaf => leaf.Id).ToList();

            return new CriticalPathResult([], [], ProjectFinish: null, cycle);
        }

        var baseDay = projectStart.DayNumber;
        var duration = leaves.ToDictionary(leaf => leaf.Id, DurationOf);
        var byId = leaves.ToDictionary(leaf => leaf.Id);
        var earliest = new Dictionary<Guid, int>();
        foreach (var id in order)
        {
            var start = byId[id].StartDate is { } planned ? Math.Max(0, planned.DayNumber - baseDay) : 0;
            foreach (var link in incoming[id])
            {
                start = Math.Max(start, LowerBoundStart(link, earliest[link.PredecessorNodeId], duration[link.PredecessorNodeId], duration[id]));
            }

            earliest[id] = start;
        }

        var projectFinish = leaves.Count == 0 ? 0 : leaves.Max(leaf => earliest[leaf.Id] + duration[leaf.Id]);
        var latestFinish = new Dictionary<Guid, int>();
        foreach (var id in Enumerable.Reverse(order))
        {
            var finish = projectFinish;
            foreach (var link in outgoing[id])
            {
                var successor = link.SuccessorNodeId;
                var successorFinish = latestFinish[successor];
                finish = Math.Min(finish, UpperBoundFinish(link, successorFinish - duration[successor], successorFinish, duration[id]));
            }

            latestFinish[id] = finish;
        }

        var schedules = new Dictionary<Guid, NodeSchedule>();
        foreach (var leaf in leaves)
        {
            var id = leaf.Id;
            var length = duration[id];
            var lateStart = latestFinish[id] - length;
            var totalFloat = lateStart - earliest[id];
            var freeFloat = outgoing[id].Count == 0
                ? projectFinish - (earliest[id] + length)
                : outgoing[id].Min(link => Slack(link, earliest, duration));

            schedules[id] = new NodeSchedule(
                id,
                Day(baseDay, earliest[id]),
                FinishDay(baseDay, earliest[id] + length, length),
                Day(baseDay, lateStart),
                FinishDay(baseDay, latestFinish[id], length),
                totalFloat,
                Math.Max(0, freeFloat),
                totalFloat <= 0);
        }

        var all = AggregateSummaries(nodes, parents, schedules);
        var critical = leaves
            .Where(leaf => schedules[leaf.Id].IsCritical)
            .OrderBy(leaf => schedules[leaf.Id].EarlyStart)
            .Select(leaf => leaf.Id)
            .ToList();
        DateOnly? finishDate = schedules.Count == 0 ? null : schedules.Values.Max(schedule => schedule.EarlyFinish);

        return new CriticalPathResult(all, critical, finishDate, []);
    }

    /// <summary>
    /// Writes the early/late dates, floats and the critical flag of a result onto the nodes; nodes without a result are cleared.
    /// </summary>
    /// <param name="nodes">The nodes to update.</param>
    /// <param name="result">The calculation result.</param>
    public static void Apply(IReadOnlyList<WbsNode> nodes, CriticalPathResult result)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(result);

        var byNode = result.Nodes.ToDictionary(schedule => schedule.NodeId);
        foreach (var node in nodes)
        {
            if (byNode.TryGetValue(node.Id, out var schedule))
            {
                node.EarlyStart = schedule.EarlyStart;
                node.EarlyFinish = schedule.EarlyFinish;
                node.LateStart = schedule.LateStart;
                node.LateFinish = schedule.LateFinish;
                node.TotalFloatDays = schedule.TotalFloatDays;
                node.FreeFloatDays = schedule.FreeFloatDays;
                node.IsCritical = schedule.IsCritical;
            }
            else
            {
                node.EarlyStart = null;
                node.EarlyFinish = null;
                node.LateStart = null;
                node.LateFinish = null;
                node.TotalFloatDays = null;
                node.FreeFloatDays = null;
                node.IsCritical = false;
            }
        }
    }

    private static List<Guid> TopologicalOrder(List<WbsNode> leaves, Dictionary<Guid, List<TaskDependency>> incoming, Dictionary<Guid, List<TaskDependency>> outgoing)
    {
        var pending = leaves.ToDictionary(leaf => leaf.Id, leaf => incoming[leaf.Id].Count);
        var queue = new Queue<Guid>(leaves.Where(leaf => pending[leaf.Id] == 0).Select(leaf => leaf.Id));
        var order = new List<Guid>(leaves.Count);
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            order.Add(id);
            foreach (var link in outgoing[id])
            {
                pending[link.SuccessorNodeId]--;
                if (pending[link.SuccessorNodeId] == 0)
                {
                    queue.Enqueue(link.SuccessorNodeId);
                }
            }
        }

        return order;
    }

    private static List<NodeSchedule> AggregateSummaries(IReadOnlyList<WbsNode> nodes, HashSet<Guid> parents, Dictionary<Guid, NodeSchedule> schedules)
    {
        var children = nodes
            .Where(node => node.ParentNodeId.HasValue)
            .GroupBy(node => node.ParentNodeId!.Value)
            .ToDictionary(group => group.Key, group => group.Select(node => node.Id).ToList());
        var result = new List<NodeSchedule>(nodes.Count);
        foreach (var node in nodes)
        {
            if (!parents.Contains(node.Id))
            {
                result.Add(schedules[node.Id]);
                continue;
            }

            var leaves = LeafSchedules(node.Id, children, schedules, []);
            if (leaves.Count == 0)
            {
                continue;
            }

            result.Add(new NodeSchedule(
                node.Id,
                leaves.Min(schedule => schedule.EarlyStart),
                leaves.Max(schedule => schedule.EarlyFinish),
                leaves.Min(schedule => schedule.LateStart),
                leaves.Max(schedule => schedule.LateFinish),
                leaves.Min(schedule => schedule.TotalFloatDays),
                leaves.Min(schedule => schedule.FreeFloatDays),
                leaves.Exists(schedule => schedule.IsCritical)));
        }

        return result;
    }

    private static List<NodeSchedule> LeafSchedules(Guid id, Dictionary<Guid, List<Guid>> children, Dictionary<Guid, NodeSchedule> schedules, HashSet<Guid> seen)
    {
        var found = new List<NodeSchedule>();
        if (!seen.Add(id))
        {
            return found;
        }

        if (!children.TryGetValue(id, out var kids))
        {
            if (schedules.TryGetValue(id, out var own))
            {
                found.Add(own);
            }

            return found;
        }

        foreach (var kid in kids)
        {
            found.AddRange(LeafSchedules(kid, children, schedules, seen));
        }

        return found;
    }

    private static int DurationOf(WbsNode node) => node.NodeType == WbsNodeType.Milestone ? 0 : ScheduledDays(node);

    private static int ScheduledDays(WbsNode node) => node is { StartDate: { } start, EndDate: { } end } ? Math.Max(1, end.DayNumber - start.DayNumber + 1) : 1;

    private static int LowerBoundStart(TaskDependency link, int predecessorStart, int predecessorDuration, int successorDuration) => link.DependencyType switch
    {
        DependencyType.SS => predecessorStart + link.LagDays,
        DependencyType.FF => predecessorStart + predecessorDuration + link.LagDays - successorDuration,
        DependencyType.SF => predecessorStart + link.LagDays - successorDuration,
        DependencyType.FS or _ => predecessorStart + predecessorDuration + link.LagDays,
    };

    private static int UpperBoundFinish(TaskDependency link, int successorLateStart, int successorLateFinish, int predecessorDuration) => link.DependencyType switch
    {
        DependencyType.SS => successorLateStart - link.LagDays + predecessorDuration,
        DependencyType.FF => successorLateFinish - link.LagDays,
        DependencyType.SF => successorLateFinish - link.LagDays + predecessorDuration,
        DependencyType.FS or _ => successorLateStart - link.LagDays,
    };

    private static int Slack(TaskDependency link, Dictionary<Guid, int> earliest, Dictionary<Guid, int> duration)
    {
        var predecessor = link.PredecessorNodeId;
        var successor = link.SuccessorNodeId;
        var predecessorStart = earliest[predecessor];
        var predecessorFinish = predecessorStart + duration[predecessor];
        var successorStart = earliest[successor];
        var successorFinish = successorStart + duration[successor];

        return link.DependencyType switch
        {
            DependencyType.SS => successorStart - (predecessorStart + link.LagDays),
            DependencyType.FF => successorFinish - (predecessorFinish + link.LagDays),
            DependencyType.SF => successorFinish - (predecessorStart + link.LagDays),
            DependencyType.FS or _ => successorStart - (predecessorFinish + link.LagDays),
        };
    }

    private static DateOnly Day(int baseDay, int offset) => DateOnly.FromDayNumber(baseDay + offset);

    private static DateOnly FinishDay(int baseDay, int finishExclusive, int duration) => Day(baseDay, duration > 0 ? finishExclusive - 1 : finishExclusive);
}
