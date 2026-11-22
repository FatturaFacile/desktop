// <copyright file="BaselineSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The frozen plan of a project stored in <see cref="ProjectBaseline.SnapshotJson"/>.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="ProjectCode">The project code.</param>
/// <param name="StartDate">The planned project start.</param>
/// <param name="TargetEndDate">The target project end.</param>
/// <param name="PlannedHours">The planned hours of the project.</param>
/// <param name="PlannedCost">The planned cost of the project.</param>
/// <param name="PlannedRevenue">The planned revenue of the project.</param>
/// <param name="Nodes">The frozen nodes.</param>
public sealed record BaselineSnapshot(
    Guid ProjectId,
    string ProjectCode,
    DateOnly? StartDate,
    DateOnly? TargetEndDate,
    decimal PlannedHours,
    decimal PlannedCost,
    decimal PlannedRevenue,
    IReadOnlyList<BaselineNodeSnapshot> Nodes)
{
    /// <summary>
    /// Freezes the current plan of a project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="nodes">The nodes of the project.</param>
    /// <returns>The snapshot.</returns>
    public static BaselineSnapshot Capture(Project project, IReadOnlyList<WbsNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(nodes);

        return new BaselineSnapshot(
            project.Id,
            project.ProjectCode,
            project.StartDate,
            project.TargetEndDate,
            project.PlannedHours,
            project.PlannedCost,
            project.PlannedRevenue,
            [.. nodes.Select(node => new BaselineNodeSnapshot(node.Id, node.WbsCode, node.Title, node.NodeType, node.StartDate, node.EndDate, node.PlannedHours, node.PlannedCost, node.PlannedRevenue))]);
    }
}
