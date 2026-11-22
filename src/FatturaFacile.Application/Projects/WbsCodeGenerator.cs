// <copyright file="WbsCodeGenerator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// Builds the hierarchical WBS codes (1, 1.1, 1.1.1).
/// </summary>
public static class WbsCodeGenerator
{
    /// <summary>
    /// Returns the next free code under a parent (the first level when the parent code is empty).
    /// </summary>
    /// <param name="parentCode">The code of the parent node, or null for a top-level node.</param>
    /// <param name="siblingCodes">The codes of the existing siblings.</param>
    /// <returns>The code of a new last child.</returns>
    public static string NextChildCode(string? parentCode, IEnumerable<string> siblingCodes)
    {
        ArgumentNullException.ThrowIfNull(siblingCodes);

        var highest = 0;
        foreach (var code in siblingCodes)
        {
            var tail = code[(code.LastIndexOf('.') + 1)..];
            if (int.TryParse(tail, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            {
                highest = Math.Max(highest, number);
            }
        }

        var next = (highest + 1).ToString(CultureInfo.InvariantCulture);

        return string.IsNullOrWhiteSpace(parentCode) ? next : parentCode + "." + next;
    }

    /// <summary>
    /// Renumbers a whole tree from the order of the siblings, so the codes follow the structure after a node was moved or deleted.
    /// </summary>
    /// <param name="nodes">The nodes of one project.</param>
    public static void Renumber(IReadOnlyList<WbsNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        var ids = nodes.Select(node => node.Id).ToHashSet();
        var children = nodes
            .Where(node => node.ParentNodeId is { } parent && parent != node.Id && ids.Contains(parent))
            .GroupBy(node => node.ParentNodeId!.Value)
            .ToDictionary(group => group.Key, group => Ordered(group));
        var roots = Ordered(nodes.Where(node => node.ParentNodeId is null || node.ParentNodeId == node.Id || !ids.Contains(node.ParentNodeId.Value)));
        var seen = new HashSet<Guid>();

        Assign(roots, string.Empty, children, seen);
    }

    private static List<WbsNode> Ordered(IEnumerable<WbsNode> group) =>
        [.. group.OrderBy(node => node.SortOrder).ThenBy(node => node.WbsCode, StringComparer.Ordinal)];

    private static void Assign(List<WbsNode> level, string prefix, Dictionary<Guid, List<WbsNode>> children, HashSet<Guid> seen)
    {
        for (var index = 0; index < level.Count; index++)
        {
            var node = level[index];
            if (!seen.Add(node.Id))
            {
                continue;
            }

            var position = (index + 1).ToString(CultureInfo.InvariantCulture);
            node.WbsCode = prefix.Length == 0 ? position : prefix + "." + position;
            node.SortOrder = index + 1;
            if (children.TryGetValue(node.Id, out var kids))
            {
                Assign(kids, node.WbsCode, children, seen);
            }
        }
    }
}
