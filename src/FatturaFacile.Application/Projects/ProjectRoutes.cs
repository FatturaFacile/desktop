// <copyright file="ProjectRoutes.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The relative web-api routes of the project portfolio and WBS (Mod 078, `/api/v1`). Kept in one place so a contract change is one edit.
/// </summary>
public static class ProjectRoutes
{
    /// <summary>
    /// Gets the route of the project list.
    /// </summary>
    public const string Projects = "projects";

    /// <summary>
    /// Gets the route of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>The route.</returns>
    public static string Project(Guid projectId) => Projects + "/" + Id(projectId);

    /// <summary>
    /// Gets the route of the WBS nodes of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>The route.</returns>
    public static string Nodes(Guid projectId) => Project(projectId) + "/wbs-nodes";

    /// <summary>
    /// Gets the route of one WBS node.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="nodeId">The node identifier.</param>
    /// <returns>The route.</returns>
    public static string Node(Guid projectId, Guid nodeId) => Nodes(projectId) + "/" + Id(nodeId);

    /// <summary>
    /// Gets the route of the dependencies of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>The route.</returns>
    public static string Dependencies(Guid projectId) => Project(projectId) + "/dependencies";

    /// <summary>
    /// Gets the route of one dependency.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="dependencyId">The dependency identifier.</param>
    /// <returns>The route.</returns>
    public static string Dependency(Guid projectId, Guid dependencyId) => Dependencies(projectId) + "/" + Id(dependencyId);

    /// <summary>
    /// Gets the route of the baselines of a project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>The route.</returns>
    public static string Baselines(Guid projectId) => Project(projectId) + "/baselines";

    /// <summary>
    /// Gets the route that creates a project from a template (#186).
    /// </summary>
    /// <param name="templateId">The template identifier.</param>
    /// <returns>The route.</returns>
    public static string Instantiate(Guid templateId) => Project(templateId) + "/instantiate";

    private static string Id(Guid id) => id.ToString("D", CultureInfo.InvariantCulture);
}
