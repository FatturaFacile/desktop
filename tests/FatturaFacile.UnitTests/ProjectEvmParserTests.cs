// <copyright file="ProjectEvmParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.ProjectEvm;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the profitability payload parser and of the traffic-light rules.
/// </summary>
public sealed class ProjectEvmParserTests
{
    private static readonly DateTime SyncedAt = new(2027, 3, 1, 8, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The project figures are read from decimal strings and the thresholds are counted.
    /// </summary>
    [Fact]
    public void ParseSnapshot_ReadsProjectFiguresAndBreaches()
    {
        var snapshot = ProjectEvmParser.ParseSnapshot(ProjectEvmTestData.ProjectId, ProjectEvmTestData.Payload(), SyncedAt);

        Assert.Equal("CHF", snapshot.Currency);
        Assert.Equal(new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc), snapshot.SnapshotDate);
        var project = Assert.IsType<ProjectEvmFigures>(snapshot.Project);
        Assert.Equal(100000.0, project.Bac);
        Assert.Equal(0.8333, project.Cpi);
        Assert.Equal(120000.0, project.Eac);
        Assert.Equal(31000.0, project.Exposure);
        Assert.Equal(ProjectEvmLight.Red, project.CpiLight);
        Assert.Equal(ProjectEvmLight.Red, project.SpiLight);
        Assert.True(project.AbsorptionWarning);
        Assert.Equal(3, project.Breaches);
        Assert.Null(project.ManualEtc);
    }

    /// <summary>
    /// The WBS tree keeps its hierarchy; a node without snapshot has no figures; a manual ETC is carried.
    /// </summary>
    [Fact]
    public void ParseSnapshot_ReadsTheWbsTree()
    {
        var snapshot = ProjectEvmParser.ParseSnapshot(ProjectEvmTestData.ProjectId, ProjectEvmTestData.Payload(), SyncedAt);

        var phase = Assert.Single(snapshot.Nodes);
        Assert.Equal("1", phase.Code);
        Assert.Equal(65000.0, phase.Figures?.ManualEtc);
        Assert.Equal(ProjectEvmLight.Amber, phase.Figures?.SpiLight);
        var child = Assert.Single(phase.Children);
        Assert.Null(child.Figures);
    }

    /// <summary>
    /// The waterfall, the S-curve, the EAC projection and the reconciliation are read.
    /// </summary>
    [Fact]
    public void ParseSnapshot_ReadsWaterfallCurveAndReconciliation()
    {
        var snapshot = ProjectEvmParser.ParseSnapshot(ProjectEvmTestData.ProjectId, ProjectEvmTestData.Payload(), SyncedAt);

        Assert.Equal([true, false, true], snapshot.Waterfall.Select(step => step.IsTotal));
        Assert.Equal(-30000.0, snapshot.Waterfall[1].Amount);
        Assert.Equal(2, snapshot.Points.Count);
        Assert.Equal(170.0, snapshot.Points[1].Ac);
        Assert.Equal(600.0, snapshot.Forecast[^1].Eac);
        var reconciliation = Assert.IsType<ProjectEvmReconciliation>(snapshot.Reconciliation);
        Assert.Equal("Swiss GAAP FER 22", reconciliation.Standard);
        Assert.True(reconciliation.HasValuation);
        Assert.False(reconciliation.IsReconciled);
        Assert.Equal(1000.0, reconciliation.Difference);
    }

    /// <summary>
    /// A project without snapshot has no figures, and missing sections are tolerated.
    /// </summary>
    [Fact]
    public void ParseSnapshot_ProjectWithoutSnapshot_HasNoFigures()
    {
        ProjectEvmPayload payload = new(
            """{"project_id":"p","currency":null,"snapshot_date":null,"project":{"has_snapshot":false},"reconciliation":null,"nodes":[]}""",
            """{"steps":[]}""",
            """{"points":[],"forecast":[]}""");

        var snapshot = ProjectEvmParser.ParseSnapshot("p", payload, SyncedAt);

        Assert.Null(snapshot.Project);
        Assert.Null(snapshot.Reconciliation);
        Assert.Null(snapshot.SnapshotDate);
        Assert.Equal("EUR", snapshot.Currency);
        Assert.Empty(snapshot.Nodes);
    }

    /// <summary>
    /// An unreadable payload throws, so the sync never stores it.
    /// </summary>
    [Fact]
    public void ParseSnapshot_BrokenJson_Throws()
    {
        ProjectEvmPayload payload = new("not json", "{}", "{}");

        _ = Assert.ThrowsAny<JsonException>(() => ProjectEvmParser.ParseSnapshot("p", payload, SyncedAt));
    }

    /// <summary>
    /// The project picker is read, an empty payload gives no projects, and the label joins the number and the name.
    /// </summary>
    [Fact]
    public void ParseProjects_ReadsThePicker()
    {
        var projects = ProjectEvmParser.ParseProjects(ProjectEvmTestData.Projects);

        Assert.Equal(2, projects.Count);
        Assert.Equal("PRJ-1 · Microgrid", projects[0].Label);
        Assert.Equal("Retrofit", projects[1].Label);
        Assert.Empty(ProjectEvmParser.ParseProjects(json: null));
        Assert.Empty(ProjectEvmParser.ParseProjects("{}"));
    }

    /// <summary>
    /// Green from 1.00, amber from 0.90, red below; an undefined index is green.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="expected">The expected light.</param>
    [Theory]
    [InlineData(1.0, ProjectEvmLight.Green)]
    [InlineData(1.2, ProjectEvmLight.Green)]
    [InlineData(0.95, ProjectEvmLight.Amber)]
    [InlineData(0.9, ProjectEvmLight.Amber)]
    [InlineData(0.8999, ProjectEvmLight.Red)]
    [InlineData(null, ProjectEvmLight.Green)]
    public void LightOf_FollowsTheThresholds(double? index, ProjectEvmLight expected) =>
        Assert.Equal(expected, ProjectEvmFigures.LightOf(index));
}
