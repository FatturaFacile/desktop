// <copyright file="AnalyticalParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.AnalyticalAccounting;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the parsing of the analytical endpoints into the typed read models.
/// </summary>
public sealed class AnalyticalParserTests
{
    /// <summary>
    /// The center tree keeps its hierarchy, and Flatten lists parents before children.
    /// </summary>
    [Fact]
    public void ParseCenters_BuildsTheTreeAndFlattensDepthFirst()
    {
        var roots = AnalyticalParser.ParseCenters(AnalyticalTestData.Centers);
        var flat = AnalyticalParser.Flatten(roots);

        _ = Assert.Single(roots);
        Assert.Equal(["ROOT", "IT", "ASM"], flat.Select(c => c.Code), StringComparer.Ordinal);
        Assert.Equal("Rita Rossi", roots[0].ManagerName);
        Assert.False(flat[2].IsActive);
        Assert.Equal(1, flat[1].Depth);
    }

    /// <summary>
    /// The margin statement is read with its levels and passes the consistency check.
    /// </summary>
    [Fact]
    public void ParseMargin_ReadsTheLevelsAndIsConsistent()
    {
        var statement = AnalyticalParser.ParseMargin(AnalyticalTestData.Margin);

        Assert.Equal(5000m, statement.OperatingRevenues);
        Assert.Equal(4600m, statement.ContributionMargin1);
        Assert.Equal(3600m, statement.ContributionMargin2);
        Assert.Equal(3350m, statement.OperatingResult);
        Assert.Equal(67m, statement.OperatingResultPercent);
        Assert.True(statement.IsConsistent);
        Assert.False((statement with { OperatingResult = 1m }).IsConsistent);
    }

    /// <summary>
    /// The comparison rows and the run history are read.
    /// </summary>
    [Fact]
    public void ParseComparisonAndRuns_ReadTheRows()
    {
        var row = Assert.Single(AnalyticalParser.ParseComparison(AnalyticalTestData.Comparison));
        var run = Assert.Single(AnalyticalParser.ParseRuns(AnalyticalTestData.Runs));

        Assert.Equal(-1000m, row.Variance);
        Assert.Equal(1000m, row.ApportionedOut);
        Assert.Equal("CONSOLIDATED", run.Status);
        Assert.Equal(1000m, run.TotalAllocated);
    }

    /// <summary>
    /// A payload without the expected shape is an error, so it can never replace a good cache.
    /// </summary>
    [Fact]
    public void Parse_WrongShape_Throws()
    {
        _ = Assert.Throws<JsonException>(() => AnalyticalParser.ParseCenters("{}"));
        _ = Assert.Throws<JsonException>(() => AnalyticalParser.ParseMargin("{}"));
        _ = Assert.Throws<JsonException>(() => AnalyticalParser.ParseComparison("[]"));
        _ = Assert.Throws<JsonException>(() => AnalyticalParser.ParseRuns("{}"));
        _ = Assert.ThrowsAny<JsonException>(() => AnalyticalParser.ParseCenters("garbage"));
    }
}
