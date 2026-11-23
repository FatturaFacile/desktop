// <copyright file="WbsTreeOrder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// Flattens a WBS into display order (parents before their children, siblings by order).
/// </summary>
public static class WbsTreeOrder
{
    /// <summary>
    /// Flattens the tree depth first.
    /// </summary>
    /// <param name="nodes">The nodes of one project.</param>
    /// <returns>The nodes with their depth.</returns>
    public static IReadOnlyList<WbsTreeItem> Flatten(IReadOnlyList<WbsNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        var ids = nodes.Select(node => node.Id).ToHashSet();
        var children = nodes
            .Where(node => node.ParentNodeId is { } parent && parent != node.Id && ids.Contains(parent))
            .GroupBy(node => node.ParentNodeId!.Value)
            .ToDictionary(group => group.Key, Ordered);
        var roots = Ordered(nodes.Where(node => node.ParentNodeId is null || node.ParentNodeId == node.Id || !ids.Contains(node.ParentNodeId.Value)));
        var result = new List<WbsTreeItem>(nodes.Count);
        var seen = new HashSet<Guid>();

        Walk(roots, 0, children, seen, result);

        return result;
    }

    private static List<WbsNode> Ordered(IEnumerable<WbsNode> group) =>
        [.. group.OrderBy(node => node.SortOrder).ThenBy(node => node.WbsCode, StringComparer.Ordinal)];

    private static void Walk(List<WbsNode> level, int depth, Dictionary<Guid, List<WbsNode>> children, HashSet<Guid> seen, List<WbsTreeItem> result)
    {
        foreach (var node in level.Where(node => seen.Add(node.Id)))
        {
            var kids = children.GetValueOrDefault(node.Id);
            result.Add(new WbsTreeItem(node, depth, kids is { Count: > 0 }));
            if (kids is not null)
            {
                Walk(kids, depth + 1, children, seen, result);
            }
        }
    }
}
