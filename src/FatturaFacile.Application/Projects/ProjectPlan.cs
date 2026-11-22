// <copyright file="ProjectPlan.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The whole plan of a project: WBS, dependencies, baselines and the critical path computed from them.
/// </summary>
/// <param name="Project">The project.</param>
/// <param name="Nodes">The nodes in tree order.</param>
/// <param name="Dependencies">The dependencies.</param>
/// <param name="Baselines">The baselines, newest first.</param>
/// <param name="Schedule">The CPM result.</param>
public sealed record ProjectPlan(
    Project Project,
    IReadOnlyList<WbsNode> Nodes,
    IReadOnlyList<TaskDependency> Dependencies,
    IReadOnlyList<ProjectBaseline> Baselines,
    CriticalPathResult Schedule);
