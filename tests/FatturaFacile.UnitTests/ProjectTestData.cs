// <copyright file="ProjectTestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Builders shared by the project portfolio and WBS tests. Day 1 is 2027-01-01.
/// </summary>
internal static class ProjectTestData
{
    public static readonly Guid Tenant = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);

    public static DateOnly Day(int day) => new DateOnly(2027, 1, 1).AddDays(day - 1);

    public static Project Project(string code = "P-1", Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        TenantId = Tenant,
        ProjectCode = code,
        Name = "Project " + code,
        StartDate = Day(1),
        TargetEndDate = Day(60),
    };

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
