// <copyright file="FieldServiceDispatchPreflightTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the dispatch preview of the board (schedule overlap, missing skills, van stock) and of the way the server blockers are
/// read.
/// </summary>
public sealed class FieldServiceDispatchPreflightTests
{
    private static readonly DateTimeOffset Day = new(2027, 3, 2, 0, 0, 0, TimeSpan.Zero);

    private readonly FieldServiceBoard board = FieldServiceBoardParser.Parse(FieldServiceTestData.Board);

    /// <summary>
    /// A technician with the skills, a van and a free window raises no warning.
    /// </summary>
    [Fact]
    public void Evaluate_FreeSkilledTechnicianWithVan_RaisesNothing()
    {
        var warnings = FieldServiceDispatchPreflight.Evaluate(board.Lanes[0], board.Unassigned[0], Day.AddHours(13), Day.AddHours(15));

        Assert.Empty(warnings);
    }

    /// <summary>
    /// A window that overlaps a visit of the technician names the visit.
    /// </summary>
    [Fact]
    public void Evaluate_OverlappingWindow_WarnsWithTheOtherVisit()
    {
        var warnings = FieldServiceDispatchPreflight.Evaluate(board.Lanes[0], board.Unassigned[0], Day.AddHours(11), Day.AddHours(14));

        var warning = Assert.Single(warnings);
        Assert.Equal(FieldServiceWarningKind.Schedule, warning.Kind);
        Assert.Equal("WO-2", warning.Detail);
    }

    /// <summary>
    /// A window that only touches the end of another visit does not overlap it.
    /// </summary>
    [Fact]
    public void Evaluate_WindowStartingWhenTheOtherEnds_IsFree()
    {
        var warnings = FieldServiceDispatchPreflight.Evaluate(board.Lanes[0], board.Unassigned[0], Day.AddHours(12), Day.AddHours(14));

        Assert.Empty(warnings);
    }

    /// <summary>
    /// Rescheduling a work order does not collide with its own bar.
    /// </summary>
    [Fact]
    public void Evaluate_IgnoresTheBarOfTheSameWorkOrder()
    {
        var lane = board.Lanes[0];
        var self = lane.Blocks[0] with { RequiredSkills = [], RequiredParts = 0 };

        Assert.Empty(FieldServiceDispatchPreflight.Evaluate(lane, self, Day.AddHours(9), Day.AddHours(10)));
    }

    /// <summary>
    /// A technician without the skills and without a van gets a skill warning and a van warning.
    /// </summary>
    [Fact]
    public void Evaluate_JuniorWithoutVan_WarnsAboutSkillsAndStock()
    {
        var warnings = FieldServiceDispatchPreflight.Evaluate(board.Lanes[1], board.Unassigned[0], Day.AddHours(13), Day.AddHours(15));

        Assert.Equal([FieldServiceWarningKind.Skills, FieldServiceWarningKind.VanStock], warnings.Select(w => w.Kind));
        Assert.Equal("electrical", warnings[0].Detail);
    }

    /// <summary>
    /// A van that has fewer distinct products than the visit needs is a stock warning, and skills are compared without case.
    /// </summary>
    [Fact]
    public void Evaluate_ShortVan_WarnsAndSkillsIgnoreCase()
    {
        var lane = board.Lanes[0] with { VanProducts = 1, Skills = ["HVAC", "Electrical"] };

        var warnings = FieldServiceDispatchPreflight.Evaluate(lane, board.Unassigned[0], Day.AddHours(13), Day.AddHours(15));

        Assert.Equal(FieldServiceWarningKind.VanStock, Assert.Single(warnings).Kind);
        Assert.Empty(FieldServiceDispatchPreflight.MissingSkills(lane, board.Unassigned[0]));
    }

    /// <summary>
    /// The blockers of a 422 answer become warnings of the matching kind, and unknown keys are kept as other.
    /// </summary>
    [Fact]
    public void FromServer_MapsTheBlockerKeys()
    {
        var blockers = FieldServiceErrorReader.Blockers(FieldServiceTestData.DispatchBlocked);

        var warnings = FieldServiceDispatchPreflight.FromServer(blockers);

        Assert.Equal([FieldServiceWarningKind.Schedule, FieldServiceWarningKind.Skills, FieldServiceWarningKind.VanStock], warnings.Select(w => w.Kind));
        Assert.Equal("Overlaps WO-2", warnings[0].Detail);

        var other = FieldServiceDispatchPreflight.FromServer(FieldServiceErrorReader.Blockers("{\"errors\":{\"work_order\":[\"Not dispatchable\"]}}"));
        Assert.Equal(FieldServiceWarningKind.Other, Assert.Single(other).Kind);
    }

    /// <summary>
    /// An error body is described in one line, whatever its shape.
    /// </summary>
    [Fact]
    public void Describe_HandlesValidationProblemAndGarbage()
    {
        Assert.Contains("schedule: Overlaps WO-2", FieldServiceErrorReader.Describe(FieldServiceTestData.DispatchBlocked), StringComparison.Ordinal);
        Assert.Equal("Nope", FieldServiceErrorReader.Describe("{\"detail\":\"Nope\"}"));
        Assert.Equal("boom", FieldServiceErrorReader.Describe("boom"));
        Assert.Empty(FieldServiceErrorReader.Blockers("boom"));
    }
}
