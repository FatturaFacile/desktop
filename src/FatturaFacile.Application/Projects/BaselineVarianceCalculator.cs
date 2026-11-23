// <copyright file="BaselineVarianceCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// Compares the current plan of a project with a baseline: schedule slip in days and effort, cost and revenue variance.
/// </summary>
public static class BaselineVarianceCalculator
{
    /// <summary>
    /// Computes the variance of the current plan against a baseline.
    /// </summary>
    /// <param name="baseline">The stored baseline.</param>
    /// <param name="snapshot">The frozen plan of the baseline.</param>
    /// <param name="project">The project with its current rolled-up figures.</param>
    /// <param name="current">The current nodes.</param>
    /// <returns>The variance.</returns>
    public static BaselineVariance Compare(ProjectBaseline baseline, BaselineSnapshot snapshot, Project project, IReadOnlyList<WbsNode> current)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(current);

        var before = snapshot.Nodes.ToDictionary(node => node.NodeId);
        var rows = new List<NodeVariance>(current.Count);
        foreach (var node in current)
        {
            rows.Add(before.TryGetValue(node.Id, out var frozen) ? Changed(node, frozen) : Added(node));
        }

        var live = current.Select(node => node.Id).ToHashSet();
        rows.AddRange(snapshot.Nodes.Where(node => !live.Contains(node.NodeId)).Select(Removed));

        var currentStart = Min(current.Select(node => node.StartDate)) ?? project.StartDate;
        var currentFinish = Max(current.Select(node => node.EndDate)) ?? project.TargetEndDate;
        var frozenStart = Min(snapshot.Nodes.Select(node => node.StartDate)) ?? snapshot.StartDate;
        var frozenFinish = Max(snapshot.Nodes.Select(node => node.EndDate)) ?? snapshot.TargetEndDate;

        return new BaselineVariance(
            baseline.Id,
            baseline.BaselineNumber,
            baseline.Name,
            baseline.CapturedAt,
            Days(frozenStart, currentStart),
            Days(frozenFinish, currentFinish),
            project.PlannedHours - snapshot.PlannedHours,
            project.PlannedCost - snapshot.PlannedCost,
            project.PlannedRevenue - snapshot.PlannedRevenue,
            rows);
    }

    private static NodeVariance Changed(WbsNode node, BaselineNodeSnapshot frozen)
    {
        var start = Days(frozen.StartDate, node.StartDate);
        var finish = Days(frozen.EndDate, node.EndDate);
        var hours = node.PlannedHours - frozen.PlannedHours;
        var cost = node.PlannedCost - frozen.PlannedCost;
        var revenue = node.PlannedRevenue - frozen.PlannedRevenue;
        var same = start.GetValueOrDefault() == 0 && finish.GetValueOrDefault() == 0 && IsZero(hours, cost, revenue);

        return new NodeVariance(node.Id, node.WbsCode, node.Title, same ? VarianceStatus.Unchanged : VarianceStatus.Changed, start, finish, hours, cost, revenue);
    }

    private static NodeVariance Added(WbsNode node) =>
        new(node.Id, node.WbsCode, node.Title, VarianceStatus.Added, StartVarianceDays: null, FinishVarianceDays: null, node.PlannedHours, node.PlannedCost, node.PlannedRevenue);

    private static NodeVariance Removed(BaselineNodeSnapshot frozen) =>
        new(frozen.NodeId, frozen.WbsCode, frozen.Title, VarianceStatus.Removed, StartVarianceDays: null, FinishVarianceDays: null, -frozen.PlannedHours, -frozen.PlannedCost, -frozen.PlannedRevenue);

    private static bool IsZero(decimal hours, decimal cost, decimal revenue) => hours == 0m && cost == 0m && revenue == 0m;

    private static int? Days(DateOnly? from, DateOnly? to) =>
        from is { } start && to is { } end ? end.DayNumber - start.DayNumber : null;

    private static DateOnly? Min(IEnumerable<DateOnly?> dates)
    {
        var known = dates.Where(date => date.HasValue).Select(date => date!.Value).ToList();

        return known.Count == 0 ? null : known.Min();
    }

    private static DateOnly? Max(IEnumerable<DateOnly?> dates)
    {
        var known = dates.Where(date => date.HasValue).Select(date => date!.Value).ToList();

        return known.Count == 0 ? null : known.Max();
    }
}
