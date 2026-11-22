// <copyright file="DependencyGraph.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// Structural checks on the dependency network of a project.
/// </summary>
public static class DependencyGraph
{
    /// <summary>
    /// Tells whether adding a link would close a cycle (a node can never precede itself, directly or through other nodes).
    /// </summary>
    /// <param name="existing">The links already in place, without the one being replaced.</param>
    /// <param name="predecessorId">The predecessor of the new link.</param>
    /// <param name="successorId">The successor of the new link.</param>
    /// <returns><see langword="true"/> when the new link would create a cycle.</returns>
    public static bool WouldCreateCycle(IEnumerable<TaskDependency> existing, Guid predecessorId, Guid successorId)
    {
        ArgumentNullException.ThrowIfNull(existing);

        if (predecessorId == successorId)
        {
            return true;
        }

        var next = existing
            .GroupBy(link => link.PredecessorNodeId)
            .ToDictionary(group => group.Key, group => group.Select(link => link.SuccessorNodeId).ToList());
        var stack = new Stack<Guid>();
        var seen = new HashSet<Guid> { successorId };
        stack.Push(successorId);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == predecessorId)
            {
                return true;
            }

            if (!next.TryGetValue(current, out var targets))
            {
                continue;
            }

            foreach (var target in targets.Where(seen.Add))
            {
                stack.Push(target);
            }
        }

        return false;
    }
}
