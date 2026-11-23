// <copyright file="ProjectTemplateInstantiatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the creation of a project from a template (#186).
/// </summary>
public sealed class ProjectTemplateInstantiatorTests
{
    private static readonly Guid NewTenant = Guid.NewGuid();

    /// <summary>
    /// The copy has new identifiers, keeps the structure and the dependencies, shifts the dates and starts from zero progress.
    /// </summary>
    [Fact]
    public void Instantiate_CopiesTheStructureWithShiftedDatesAndFreshActuals()
    {
        var template = ProjectTestData.Project("TPL");
        template.IsTemplate = true;
        template.ProjectType = ProjectType.CapexInvestment;
        template.CupCode = "OLD0CUP0000001X";
        var phase = ProjectTestData.Node("1", 1, 10, template.Id, type: WbsNodeType.Phase);
        var work = ProjectTestData.Node("1.1", 1, 5, template.Id, phase.Id, hours: 8m, cost: 80m, revenue: 120m, percent: 60m);
        var follow = ProjectTestData.Node("1.2", 6, 10, template.Id, phase.Id, hours: 4m, percent: 20m);
        work.ActualHours = 7m;
        work.ActualCost = 70m;
        work.IsCritical = true;
        work.EarlyStart = ProjectTestData.Day(1);
        var link = ProjectTestData.Link(work, follow, DependencyType.SS, 2);
        var request = new TemplateInstantiationRequest(template.Id, " NEW-1 ", " New project ", new DateOnly(2027, 2, 1), CupCode: " abcdefghij12345 ");

        var instance = ProjectTemplateInstantiator.Instantiate(template, [phase, work, follow], [link], request, NewTenant);

        Assert.NotEqual(template.Id, instance.Project.Id);
        Assert.Equal(NewTenant, instance.Project.TenantId);
        Assert.Equal("NEW-1", instance.Project.ProjectCode);
        Assert.Equal("New project", instance.Project.Name);
        Assert.False(instance.Project.IsTemplate);
        Assert.Equal(template.Id, instance.Project.TemplateSourceId);
        Assert.Equal(ProjectStatus.Planning, instance.Project.Status);
        Assert.Equal(ProjectType.CapexInvestment, instance.Project.ProjectType);
        Assert.Equal("ABCDEFGHIJ12345", instance.Project.CupCode);
        Assert.Equal(new DateOnly(2027, 2, 1), instance.Project.StartDate);
        Assert.Equal(ProjectTestData.Day(60).AddDays(31), instance.Project.TargetEndDate);

        Assert.Equal(3, instance.Nodes.Count);
        Assert.All(instance.Nodes, node => Assert.Equal(instance.Project.Id, node.ProjectId));
        Assert.DoesNotContain(instance.Nodes, node => node.Id == phase.Id || node.Id == work.Id || node.Id == follow.Id);
        var copiedPhase = instance.Nodes.Single(node => string.Equals(node.WbsCode, "1", StringComparison.Ordinal));
        var copiedWork = instance.Nodes.Single(node => string.Equals(node.WbsCode, "1.1", StringComparison.Ordinal));
        Assert.Equal(copiedPhase.Id, copiedWork.ParentNodeId);
        Assert.Equal(new DateOnly(2027, 2, 1), copiedWork.StartDate);
        Assert.Equal(new DateOnly(2027, 2, 5), copiedWork.EndDate);
        Assert.Equal(8m, copiedWork.PlannedHours);
        Assert.Equal(0m, copiedWork.ActualHours);
        Assert.Equal(0m, copiedWork.ActualCost);
        Assert.Equal(0m, copiedWork.PercentComplete);
        Assert.False(copiedWork.IsCritical);
        Assert.Null(copiedWork.EarlyStart);

        var copiedLink = Assert.Single(instance.Dependencies);
        Assert.Equal(copiedWork.Id, copiedLink.PredecessorNodeId);
        Assert.Equal(instance.Nodes.Single(node => string.Equals(node.WbsCode, "1.2", StringComparison.Ordinal)).Id, copiedLink.SuccessorNodeId);
        Assert.Equal(DependencyType.SS, copiedLink.DependencyType);
        Assert.Equal(2, copiedLink.LagDays);
        Assert.NotEqual(link.Id, copiedLink.Id);
    }

    /// <summary>
    /// A dependency that points outside the copied nodes is dropped.
    /// </summary>
    [Fact]
    public void Instantiate_DropsDependenciesToUnknownNodes()
    {
        var template = ProjectTestData.Project("TPL");
        var work = ProjectTestData.Node("1", 1, 5, template.Id);
        var stranger = ProjectTestData.Node("9", 1, 5, template.Id);
        var request = new TemplateInstantiationRequest(template.Id, "N", "N", ProjectTestData.Day(1));

        var instance = ProjectTemplateInstantiator.Instantiate(template, [work], [ProjectTestData.Link(work, stranger)], request, NewTenant);

        Assert.Empty(instance.Dependencies);
    }
}
