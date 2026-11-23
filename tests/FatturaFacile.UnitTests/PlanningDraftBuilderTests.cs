// <copyright file="PlanningDraftBuilderTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.ResourcePlanning;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the checks of the planning setup forms: skill taxonomy, utilization targets, local holidays and the snapshot window.
/// </summary>
public sealed class PlanningDraftBuilderTests
{
    /// <summary>
    /// A skill is trimmed and keeps its certificate type only when a certificate is required.
    /// </summary>
    [Fact]
    public void Skill_TrimsAndDropsTheCodeWithoutCertificate()
    {
        var plain = PlanningDraftBuilder.Skill("  Site  ", " Crane ", false, "WELD-EN").Request;
        var certified = PlanningDraftBuilder.Skill("Site", "Welding", true, " WELD-EN ").Request;

        Assert.Equal(new PlanningSkillRequest("Site", "Crane", false, null), plain);
        Assert.Equal(new PlanningSkillRequest("Site", "Welding", true, "WELD-EN"), certified);
    }

    /// <summary>
    /// A skill without category, name, or the certificate type it requires is explained.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="name">The name.</param>
    /// <param name="requires">Whether a certificate is required.</param>
    /// <param name="code">The certificate type.</param>
    /// <param name="expected">The expected problem code.</param>
    [Theory]
    [InlineData(null, "Welding", false, null, PlanningDraftBuilder.SkillNeedsCategory)]
    [InlineData("  ", "Welding", false, null, PlanningDraftBuilder.SkillNeedsCategory)]
    [InlineData("Site", null, false, null, PlanningDraftBuilder.SkillNeedsName)]
    [InlineData("Site", " ", false, null, PlanningDraftBuilder.SkillNeedsName)]
    [InlineData("Site", "Welding", true, null, PlanningDraftBuilder.CertificationCodeRequired)]
    [InlineData("Site", "Welding", true, "  ", PlanningDraftBuilder.CertificationCodeRequired)]
    public void Skill_Problems_AreExplained(string? category, string? name, bool requires, string? code, string expected)
    {
        var result = PlanningDraftBuilder.Skill(category, name, requires, code);

        Assert.False(result.IsValid);
        Assert.Equal(expected, result.ErrorCode);
    }

    /// <summary>
    /// A target is built for the tenant, a department or a cost center, and the limits are accepted.
    /// </summary>
    [Fact]
    public void Target_ValidScopes_AreBuilt()
    {
        Assert.Equal(new UtilizationTargetRequest(null, null, 70d), PlanningDraftBuilder.Target(null, null, 70d).Request);
        Assert.Equal(new UtilizationTargetRequest(ResourcePlanningTestData.Department, null, 0d), PlanningDraftBuilder.Target(ResourcePlanningTestData.Department, null, 0d).Request);
        Assert.Equal(new UtilizationTargetRequest(null, ResourcePlanningTestData.Opportunity, 100d), PlanningDraftBuilder.Target(null, ResourcePlanningTestData.Opportunity, 100d).Request);
    }

    /// <summary>
    /// A target outside 0 to 100, missing, or with two scopes is explained.
    /// </summary>
    [Fact]
    public void Target_Problems_AreExplained()
    {
        Assert.Equal(PlanningDraftBuilder.TargetInvalidPercent, PlanningDraftBuilder.Target(null, null, null).ErrorCode);
        Assert.Equal(PlanningDraftBuilder.TargetInvalidPercent, PlanningDraftBuilder.Target(null, null, -1d).ErrorCode);
        Assert.Equal(PlanningDraftBuilder.TargetInvalidPercent, PlanningDraftBuilder.Target(null, null, 100.5d).ErrorCode);
        Assert.Equal(PlanningDraftBuilder.TargetInvalidPercent, PlanningDraftBuilder.Target(null, null, double.NaN).ErrorCode);
        Assert.Equal(PlanningDraftBuilder.TargetSingleScope, PlanningDraftBuilder.Target(ResourcePlanningTestData.Department, ResourcePlanningTestData.Opportunity, 70d).ErrorCode);
    }

    /// <summary>
    /// A local holiday is normalized: the country and region in upper case, empty parts left out, the name trimmed.
    /// </summary>
    [Fact]
    public void LocalHoliday_IsNormalized()
    {
        var request = PlanningDraftBuilder.LocalHoliday(" ch ", " ti ", "  ", 8, 15, null, " Local fair ", 0.5m).Request;

        Assert.Equal(new LocalHolidayRequest("CH", "TI", null, 8, 15, null, "Local fair", 0.5m), request);
    }

    /// <summary>
    /// February 29 is a holiday of a leap year only when the year is given, and never of a common year.
    /// </summary>
    [Fact]
    public void LocalHoliday_LeapDay_NeedsALeapYear()
    {
        Assert.True(PlanningDraftBuilder.LocalHoliday("IT", null, null, 2, 29, null, "Leap", 1m).IsValid);
        Assert.True(PlanningDraftBuilder.LocalHoliday("IT", null, null, 2, 29, 2028, "Leap", 1m).IsValid);
        Assert.Equal(PlanningDraftBuilder.HolidayInvalidDate, PlanningDraftBuilder.LocalHoliday("IT", null, null, 2, 29, 2027, "Leap", 1m).ErrorCode);
    }

    /// <summary>
    /// Each rule of a local holiday is explained with its code.
    /// </summary>
    /// <param name="country">The country.</param>
    /// <param name="name">The name.</param>
    /// <param name="month">The month.</param>
    /// <param name="day">The day.</param>
    /// <param name="year">The year.</param>
    /// <param name="fraction">The part of the day that is off.</param>
    /// <param name="expected">The expected problem code.</param>
    [Theory]
    [InlineData(null, "Fair", 8, 15, null, 1, PlanningDraftBuilder.HolidayInvalidCountry)]
    [InlineData("FR", "Fair", 8, 15, null, 1, PlanningDraftBuilder.HolidayInvalidCountry)]
    [InlineData("CH", " ", 8, 15, null, 1, PlanningDraftBuilder.HolidayNeedsName)]
    [InlineData("CH", "Fair", null, 15, null, 1, PlanningDraftBuilder.HolidayInvalidDate)]
    [InlineData("CH", "Fair", 13, 15, null, 1, PlanningDraftBuilder.HolidayInvalidDate)]
    [InlineData("CH", "Fair", 4, 31, null, 1, PlanningDraftBuilder.HolidayInvalidDate)]
    [InlineData("CH", "Fair", 8, 0, null, 1, PlanningDraftBuilder.HolidayInvalidDate)]
    [InlineData("CH", "Fair", 8, 15, 1800, 1, PlanningDraftBuilder.HolidayInvalidDate)]
    [InlineData("CH", "Fair", 8, 15, null, null, PlanningDraftBuilder.HolidayInvalidFraction)]
    [InlineData("CH", "Fair", 8, 15, null, 0, PlanningDraftBuilder.HolidayInvalidFraction)]
    [InlineData("CH", "Fair", 8, 15, null, 2, PlanningDraftBuilder.HolidayInvalidFraction)]
    public void LocalHoliday_Problems_AreExplained(string? country, string? name, int? month, int? day, int? year, int? fraction, string expected)
    {
        var result = PlanningDraftBuilder.LocalHoliday(country, null, null, month, day, year, name, fraction);

        Assert.False(result.IsValid);
        Assert.Equal(expected, result.ErrorCode);
    }

    /// <summary>
    /// The snapshot window needs both days, in order; one day is a valid window.
    /// </summary>
    [Fact]
    public void SnapshotWindow_ChecksThePeriod()
    {
        var day = new DateOnly(2026, 11, 16);

        Assert.Equal(new SnapshotRecalculationRequest(day, day, ResourcePlanningTestData.Anna), PlanningDraftBuilder.SnapshotWindow(day, day, ResourcePlanningTestData.Anna).Request);
        Assert.Equal(PlanningDraftBuilder.SnapshotInvalidPeriod, PlanningDraftBuilder.SnapshotWindow(null, day, null).ErrorCode);
        Assert.Equal(PlanningDraftBuilder.SnapshotInvalidPeriod, PlanningDraftBuilder.SnapshotWindow(day, null, null).ErrorCode);
        Assert.Equal(PlanningDraftBuilder.SnapshotInvalidPeriod, PlanningDraftBuilder.SnapshotWindow(day.AddDays(1), day, null).ErrorCode);
    }

    /// <summary>
    /// The result carries either the request or the code.
    /// </summary>
    [Fact]
    public void Result_OkAndFail_AreExclusive()
    {
        var ok = PlanningDraftResults.Ok(new PlanningSkillRequest("a", "b", false, null));
        var fail = PlanningDraftResults.Fail<PlanningSkillRequest>("code");

        Assert.True(ok.IsValid);
        Assert.Null(ok.ErrorCode);
        Assert.False(fail.IsValid);
        Assert.Null(fail.Request);
    }
}
