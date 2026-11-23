// <copyright file="ProjectTestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Builders shared by the project portfolio and WBS tests. Day 1 is 2027-01-01.
/// </summary>
internal static class ProjectTestData
{
    /// <summary>
    /// The tenant of the test data.
    /// </summary>
    public static readonly Guid Tenant = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);

    /// <summary>
    /// Gets a date of the test calendar.
    /// </summary>
    /// <param name="dayNumber">The day, 1 being 2027-01-01.</param>
    /// <returns>The date.</returns>
    public static DateOnly Day(int dayNumber) => new DateOnly(2027, 1, 1).AddDays(dayNumber - 1);

    /// <summary>
    /// Builds a project of the test tenant.
    /// </summary>
    /// <param name="code">The project code.</param>
    /// <param name="id">The id, or null for a new one.</param>
    /// <returns>The project.</returns>
    public static Project Project(string code = "P-1", Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        TenantId = Tenant,
        ProjectCode = code,
        Name = "Project " + code,
        StartDate = Day(1),
        TargetEndDate = Day(60),
    };

    /// <summary>
    /// Builds a WBS node of the test tenant.
    /// </summary>
    /// <param name="code">The WBS code.</param>
    /// <param name="startDay">The start day.</param>
    /// <param name="endDay">The end day.</param>
    /// <param name="projectId">The project, or null for none.</param>
    /// <param name="parent">The parent node, or null for a root.</param>
    /// <param name="type">The node type.</param>
    /// <param name="hours">The planned hours.</param>
    /// <param name="cost">The planned cost.</param>
    /// <param name="revenue">The planned revenue.</param>
    /// <param name="percent">The percent complete.</param>
    /// <returns>The node.</returns>
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "A builder with one optional argument per field of the node.")]
    public static WbsNode Node(
        string code,
        int startDay,
        int endDay,
        Guid? projectId = null,
        Guid? parent = null,
        WbsNodeType type = WbsNodeType.WorkPackage,
        decimal hours = 0m,
        decimal cost = 0m,
        decimal revenue = 0m,
        decimal percent = 0m) => new()
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ProjectId = projectId ?? Guid.Empty,
            ParentNodeId = parent,
            WbsCode = code,
            Title = "Node " + code,
            NodeType = type,
            StartDate = Day(startDay),
            EndDate = Day(endDay),
            PlannedHours = hours,
            PlannedCost = cost,
            PlannedRevenue = revenue,
            PercentComplete = percent,
        };

    /// <summary>
    /// Builds a dependency of the test tenant.
    /// </summary>
    /// <param name="predecessor">The predecessor.</param>
    /// <param name="successor">The successor.</param>
    /// <param name="type">The dependency type.</param>
    /// <param name="lag">The lag in days.</param>
    /// <returns>The dependency.</returns>
    public static TaskDependency Link(WbsNode predecessor, WbsNode successor, DependencyType type = DependencyType.FS, int lag = 0) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = Tenant,
        ProjectId = predecessor.ProjectId,
        PredecessorNodeId = predecessor.Id,
        SuccessorNodeId = successor.Id,
        DependencyType = type,
        LagDays = lag,
    };
}
