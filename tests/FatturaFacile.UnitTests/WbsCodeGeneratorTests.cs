// <copyright file="WbsCodeGeneratorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the WBS codes and the tree order.
/// </summary>
public sealed class WbsCodeGeneratorTests
{
    /// <summary>
    /// The next code continues after the highest sibling, at the first level or under a parent.
    /// </summary>
    [Fact]
    public void NextChildCode_ContinuesAfterTheHighestSibling()
    {
        Assert.Equal("1", WbsCodeGenerator.NextChildCode(parentCode: null, []));
        Assert.Equal("4", WbsCodeGenerator.NextChildCode(parentCode: null, ["1", "3"]));
        Assert.Equal("2.1", WbsCodeGenerator.NextChildCode("2", []));
        Assert.Equal("2.11", WbsCodeGenerator.NextChildCode("2", ["2.1", "2.10"]));
    }

    /// <summary>
    /// Renumbering rebuilds the codes from the structure and the order of the siblings.
    /// </summary>
    [Fact]
    public void Renumber_FollowsTheTree()
    {
        var first = ProjectTestData.Node("7", 1, 2);
        first.SortOrder = 2;
        var second = ProjectTestData.Node("3", 1, 2);
        second.SortOrder = 1;
        var child = ProjectTestData.Node("9.9", 1, 2, parent: first.Id);

        WbsCodeGenerator.Renumber([first, second, child]);

        Assert.Equal("1", second.WbsCode);
        Assert.Equal("2", first.WbsCode);
        Assert.Equal("2.1", child.WbsCode);
    }

    /// <summary>
    /// The flattened tree lists parents before their children with the depth and the summary flag.
    /// </summary>
    [Fact]
    public void Flatten_ListsParentsBeforeChildren()
    {
        var root = ProjectTestData.Node("1", 1, 2);
        var child = ProjectTestData.Node("1.1", 1, 2, parent: root.Id);
        var grandchild = ProjectTestData.Node("1.1.1", 1, 2, parent: child.Id);
        var sibling = ProjectTestData.Node("2", 1, 2);
        sibling.SortOrder = 2;

        var items = WbsTreeOrder.Flatten([sibling, grandchild, child, root]);

        Assert.Equal(["1", "1.1", "1.1.1", "2"], items.Select(item => item.Node.WbsCode).ToArray());
        Assert.Equal([0, 1, 2, 0], items.Select(item => item.Depth).ToArray());
        Assert.True(items[0].HasChildren);
        Assert.False(items[2].HasChildren);
    }
}
