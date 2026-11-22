// <copyright file="WbsRollupCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// Rolls the effort, cost, revenue, dates and progress of the leaf nodes of a WBS up to their summary nodes and to the project (bottom-up).
/// </summary>
public static class WbsRollupCalculator
{
    /// <summary>
    /// Recomputes every summary node and the rolled-up figures of the project from the leaves. A leaf keeps its own figures.
    /// </summary>
    /// <param name="project">The project that receives the totals.</param>
    /// <param name="nodes">The nodes of the project.</param>
    public static void Apply(Project project, IReadOnlyList<WbsNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(nodes);

        var ids = nodes.Select(node => node.Id).ToHashSet();
        var children = nodes
            .Where(node => node.ParentNodeId is { } parent && parent != node.Id && ids.Contains(parent))
            .GroupBy(node => node.ParentNodeId!.Value)
            .ToDictionary(group => group.Key, group => group.ToList());
        var roots = nodes
            .Where(node => node.ParentNodeId is null || node.ParentNodeId == node.Id || !ids.Contains(node.ParentNodeId.Value))
            .ToList();
        var visited = new HashSet<Guid>();

        foreach (var root in roots)
        {
            Roll(root, children, visited);
        }

        project.PlannedHours = roots.Sum(node => node.PlannedHours);
        project.ActualHours = roots.Sum(node => node.ActualHours);
        project.PlannedCost = roots.Sum(node => node.PlannedCost);
        project.ActualCost = roots.Sum(node => node.ActualCost);
        project.PlannedRevenue = roots.Sum(node => node.PlannedRevenue);
        project.PercentComplete = roots.Count == 0 ? 0m : Weighted(roots);
    }

    /// <summary>
    /// Computes the progress of a group of nodes weighted by their planned hours (a plain average when none has hours).
    /// </summary>
    /// <param name="group">The nodes.</param>
    /// <returns>The percent complete between 0 and 100, rounded to two decimals.</returns>
    public static decimal Weighted(IReadOnlyCollection<WbsNode> group)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (group.Count == 0)
        {
            return 0m;
        }

        var hours = group.Sum(node => node.PlannedHours);
        var percent = hours > 0m
            ? group.Sum(node => Clamp(node.PercentComplete) * node.PlannedHours) / hours
            : group.Average(node => Clamp(node.PercentComplete));

        return Math.Round(percent, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal Clamp(decimal percent) => Math.Clamp(percent, 0m, 100m);

    private static void Roll(WbsNode node, Dictionary<Guid, List<WbsNode>> children, HashSet<Guid> visited)
    {
        if (!visited.Add(node.Id) || !children.TryGetValue(node.Id, out var kids) || kids.Count == 0)
        {
            return;
        }

        foreach (var kid in kids)
        {
            Roll(kid, children, visited);
        }

        node.PlannedHours = kids.Sum(kid => kid.PlannedHours);
        node.ActualHours = kids.Sum(kid => kid.ActualHours);
        node.PlannedCost = kids.Sum(kid => kid.PlannedCost);
        node.ActualCost = kids.Sum(kid => kid.ActualCost);
        node.PlannedRevenue = kids.Sum(kid => kid.PlannedRevenue);
        node.PercentComplete = Weighted(kids);

        var starts = kids.Where(kid => kid.StartDate.HasValue).Select(kid => kid.StartDate!.Value).ToList();
        var ends = kids.Where(kid => kid.EndDate.HasValue).Select(kid => kid.EndDate!.Value).ToList();
        if (starts.Count > 0)
        {
            node.StartDate = starts.Min();
        }

        if (ends.Count > 0)
        {
            node.EndDate = ends.Max();
        }
    }
}
