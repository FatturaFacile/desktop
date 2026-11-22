// <copyright file="DependencyGraphTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the dependency cycle check.
/// </summary>
public sealed class DependencyGraphTests
{
    /// <summary>
    /// A link back to an ancestor closes a cycle, a link to an unrelated node does not.
    /// </summary>
    [Fact]
    public void WouldCreateCycle_DetectsDirectAndTransitiveCycles()
    {
        var a = ProjectTestData.Node("1", 1, 2);
        var b = ProjectTestData.Node("2", 1, 2);
        var c = ProjectTestData.Node("3", 1, 2);
        var d = ProjectTestData.Node("4", 1, 2);
        TaskDependency[] existing = [ProjectTestData.Link(a, b), ProjectTestData.Link(b, c)];

        Assert.True(DependencyGraph.WouldCreateCycle(existing, c.Id, a.Id));
        Assert.True(DependencyGraph.WouldCreateCycle(existing, b.Id, a.Id));
        Assert.True(DependencyGraph.WouldCreateCycle(existing, a.Id, a.Id));
        Assert.False(DependencyGraph.WouldCreateCycle(existing, a.Id, c.Id));
        Assert.False(DependencyGraph.WouldCreateCycle(existing, d.Id, a.Id));
    }
}
