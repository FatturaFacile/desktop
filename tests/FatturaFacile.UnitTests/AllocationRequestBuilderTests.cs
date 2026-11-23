// <copyright file="AllocationRequestBuilderTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.ResourcePlanning;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the checks of the form that creates an allocation: a hard commitment needs a project, a soft booking a project or an opportunity.
/// </summary>
public sealed class AllocationRequestBuilderTests
{
    private static readonly DateOnly Start = new(2026, 11, 16);
    private static readonly DateOnly End = new(2026, 11, 27);

    /// <summary>
    /// A complete hard commitment becomes the request, without an opportunity and with trimmed notes.
    /// </summary>
    [Fact]
    public void Build_HardCommitment_BuildsTheRequest()
    {
        var wbs = Guid.NewGuid();
        var draft = Draft() with { BookingType = ResourceBookingType.HardCommitted, ProjectId = ResourcePlanningTestData.ProjectA, WbsNodeId = wbs, CrmOpportunityId = ResourcePlanningTestData.Opportunity, Notes = "  night shift  ", IsBillable = false, AllowOverload = false };

        var result = AllocationRequestBuilder.Build(draft);

        Assert.True(result.IsValid);
        var request = result.Request!;
        Assert.Equal(ResourcePlanningTestData.ProjectA, request.ProjectId);
        Assert.Equal(wbs, request.WbsNodeId);
        Assert.Null(request.CrmOpportunityId);
        Assert.Equal("night shift", request.Notes);
        Assert.False(request.IsBillable);
        Assert.False(request.AllowOverload);
        Assert.Equal(ResourcePlanningTestData.Anna, request.EmployeeId);
        Assert.Equal(6m, request.AllocatedHoursPerDay);
        Assert.Null(request.RequiredSkillIds);
    }

    /// <summary>
    /// A soft booking can have an opportunity only, and the required skills are passed on once each.
    /// </summary>
    [Fact]
    public void Build_SoftBookingWithOpportunity_KeepsTheOpportunityAndTheSkills()
    {
        var draft = Draft() with { CrmOpportunityId = ResourcePlanningTestData.Opportunity, RequiredSkillIds = [ResourcePlanningTestData.SkillWelding, ResourcePlanningTestData.SkillWelding, ResourcePlanningTestData.SkillCrane] };

        var request = AllocationRequestBuilder.Build(draft).Request!;

        Assert.Null(request.ProjectId);
        Assert.Equal(ResourcePlanningTestData.Opportunity, request.CrmOpportunityId);
        Assert.Equal(new[] { ResourcePlanningTestData.SkillWelding, ResourcePlanningTestData.SkillCrane }, request.RequiredSkillIds);
        Assert.Null(request.Notes);
    }

    /// <summary>
    /// Each rule of the contract is explained with its code, the first problem winning.
    /// </summary>
    /// <param name="code">The expected problem code.</param>
    /// <param name="mutation">Which field of a valid draft is broken.</param>
    [Theory]
    [InlineData(AllocationRequestBuilder.NeedsEmployee, "employee")]
    [InlineData(AllocationRequestBuilder.HardNeedsProject, "hard")]
    [InlineData(AllocationRequestBuilder.SoftNeedsReference, "soft")]
    [InlineData(AllocationRequestBuilder.WbsNeedsProject, "wbs")]
    [InlineData(AllocationRequestBuilder.InvalidPeriod, "no-start")]
    [InlineData(AllocationRequestBuilder.InvalidPeriod, "reversed")]
    [InlineData(AllocationRequestBuilder.InvalidHours, "no-hours")]
    [InlineData(AllocationRequestBuilder.InvalidHours, "zero-hours")]
    [InlineData(AllocationRequestBuilder.InvalidHours, "too-many-hours")]
    public void Build_Problems_AreExplainedWithTheirCode(string code, string mutation)
    {
        var draft = mutation switch
        {
            "employee" => Draft() with { EmployeeId = null },
            "hard" => Draft() with { BookingType = ResourceBookingType.HardCommitted, CrmOpportunityId = ResourcePlanningTestData.Opportunity },
            "soft" => Draft(),
            "wbs" => Draft() with { WbsNodeId = Guid.NewGuid(), CrmOpportunityId = ResourcePlanningTestData.Opportunity },
            "no-start" => Valid() with { StartDate = null },
            "reversed" => Valid() with { StartDate = End, EndDate = Start },
            "no-hours" => Valid() with { AllocatedHoursPerDay = null },
            "zero-hours" => Valid() with { AllocatedHoursPerDay = 0m },
            _ => Valid() with { AllocatedHoursPerDay = 24.5m },
        };

        var result = AllocationRequestBuilder.Build(draft);

        Assert.False(result.IsValid);
        Assert.Equal(code, result.ErrorCode);
    }

    /// <summary>
    /// A whole day of 24 hours is accepted.
    /// </summary>
    [Fact]
    public void Build_TwentyFourHours_IsAccepted()
    {
        Assert.True(AllocationRequestBuilder.Build(Valid() with { AllocatedHoursPerDay = 24m }).IsValid);
    }

    /// <summary>
    /// A missing draft is rejected.
    /// </summary>
    [Fact]
    public void Build_NullDraft_Throws()
    {
        _ = Assert.Throws<ArgumentNullException>(() => AllocationRequestBuilder.Build(null!));
    }

    private static AllocationDraft Valid() => Draft() with { ProjectId = ResourcePlanningTestData.ProjectA };

    private static AllocationDraft Draft() =>
        new(ResourcePlanningTestData.Anna, ResourceBookingType.SoftTentative, null, null, null, Start, End, 6m, true, null, true, []);
}
