// <copyright file="CriticalPathResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The outcome of a critical path calculation.
/// </summary>
/// <param name="Nodes">The schedule of every node (summary nodes aggregate their descendants); empty when the network has a cycle.</param>
/// <param name="CriticalPath">The identifiers of the critical leaf nodes in start order.</param>
/// <param name="ProjectFinish">The finish of the project, or null when nothing is scheduled.</param>
/// <param name="CycleNodeIds">The nodes that take part in or depend on a dependency cycle; empty when the network is acyclic.</param>
public sealed record CriticalPathResult(
    IReadOnlyList<NodeSchedule> Nodes,
    IReadOnlyList<Guid> CriticalPath,
    DateOnly? ProjectFinish,
    IReadOnlyList<Guid> CycleNodeIds)
{
    /// <summary>
    /// Gets a value indicating whether the dependencies contain a cycle, so no schedule could be computed.
    /// </summary>
    public bool HasCycle => CycleNodeIds.Count > 0;
}
