// <copyright file="ResourcePlanningLocalizationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0006 // Plain string comparisons by design.
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Wpf.Localization;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests that the Mod 079 strings exist in the four resource files with the same keys and placeholders, that every key the code builds
/// from a code or enum value is present, and that the five cultures of the application (it-IT, it-CH, de-CH, fr-CH and en) resolve.
/// </summary>
public sealed partial class ResourcePlanningLocalizationTests
{
    private static readonly string[] Files = ["Strings.resx", "Strings.en.resx", "Strings.de.resx", "Strings.fr.resx"];

    private static readonly string[] ErrorCodes =
    [
        "employee_not_found",
        "project_not_allocatable",
        "wbs_node_not_in_project",
        "opportunity_not_open",
        "hard_booking_requires_project",
        "soft_booking_requires_reference",
        "overload",
        "already_hard_committed",
        "invalid_period",
        "no_working_days",
        "certification_code_required",
        "skill_not_found",
        "skill_profile_not_found",
        "local_holiday_not_found",
        "allocation_not_found",
    ];

    private static readonly string[] CreateCodes =
    [
        AllocationRequestBuilder.NeedsEmployee,
        AllocationRequestBuilder.HardNeedsProject,
        AllocationRequestBuilder.SoftNeedsReference,
        AllocationRequestBuilder.WbsNeedsProject,
        AllocationRequestBuilder.InvalidPeriod,
        AllocationRequestBuilder.InvalidHours,
        "invalid_project_id",
        "invalid_wbs_id",
        "invalid_opportunity_id",
    ];

    private static readonly string[] SetupCodes =
    [
        PlanningDraftBuilder.SkillNeedsCategory,
        PlanningDraftBuilder.SkillNeedsName,
        PlanningDraftBuilder.CertificationCodeRequired,
        PlanningDraftBuilder.TargetInvalidPercent,
        PlanningDraftBuilder.TargetSingleScope,
        PlanningDraftBuilder.HolidayInvalidCountry,
        PlanningDraftBuilder.HolidayNeedsName,
        PlanningDraftBuilder.HolidayInvalidDate,
        PlanningDraftBuilder.HolidayInvalidFraction,
        PlanningDraftBuilder.SnapshotInvalidPeriod,
    ];

    private static readonly string[] BlockerCodes =
    [
        "skill_missing",
        "proficiency_too_low",
        "skill_expired",
        "certificate_missing",
        "certificate_expired",
        "insufficient_availability",
    ];

    private static readonly string[] RebalanceCodes =
    [
        RebalanceRequestBuilder.ShiftNeedsTarget,
        RebalanceRequestBuilder.ReassignNeedsColleague,
        RebalanceRequestBuilder.SplitDateOutside,
        RebalanceRequestBuilder.ResizeNeedsChange,
        RebalanceRequestBuilder.InvalidPeriod,
        RebalanceRequestBuilder.InvalidHours,
    ];

    /// <summary>
    /// The four resource files have the same Mod 079 keys, none is empty and the placeholders match.
    /// </summary>
    [Fact]
    public void ResourceFiles_HaveTheSameKeysAndPlaceholders()
    {
        var italian = Read("Strings.resx");
        Assert.NotEmpty(italian);

        foreach (var file in Files.Skip(1))
        {
            var other = Read(file);

            Assert.Equal(italian.Keys.OrderBy(key => key, StringComparer.Ordinal), other.Keys.OrderBy(key => key, StringComparer.Ordinal));
            foreach (var (key, value) in other)
            {
                Assert.False(string.IsNullOrWhiteSpace(value), file + ": " + key);
                Assert.Equal(Placeholders(italian[key]), Placeholders(value));
            }
        }
    }

    /// <summary>
    /// Every key the code builds from an enum value or a server code exists.
    /// </summary>
    [Fact]
    public void ResourceFiles_ContainTheKeysBuiltFromCodes()
    {
        var keys = new List<string> { "Nav_ResourcePlanning" };
        keys.AddRange(Enum.GetNames<ResourceBookingType>().Select(name => "ResourcePlanning_Booking_" + name));
        keys.Add("ResourcePlanning_Booking_All");
        keys.AddRange(Enum.GetNames<RebalanceStrategy>().Select(name => "ResourcePlanning_Strategy_" + name));
        keys.AddRange(Enum.GetNames<UtilizationStatus>().Select(name => "ResourcePlanning_Status_" + name));
        keys.AddRange(Enum.GetNames<UtilizationGroupBy>().Select(name => "ResourcePlanning_GroupBy_" + name));
        keys.AddRange(Enum.GetNames<HeatmapGranularity>().Select(name => "ResourcePlanning_Granularity_" + name));
        keys.AddRange(Enum.GetNames<SkillMatrixCellState>().Select(name => "ResourcePlanning_SkillState_" + name));
        keys.AddRange(ErrorCodes.Select(code => "ResourcePlanning_Error_" + code));
        keys.AddRange(BlockerCodes.Select(code => "ResourcePlanning_Blocker_" + code));
        keys.AddRange(RebalanceCodes.Select(code => "ResourcePlanning_Rebalance_" + code));
        keys.Add("ResourcePlanning_Warning_skill_certificate_invalid");
        keys.AddRange(CreateCodes.Select(code => "ResourcePlanning_Create_" + code));
        keys.AddRange(SetupCodes.Select(code => "ResourcePlanning_Setup_" + code));
        keys.AddRange(PlanningDraftBuilder.Countries.Select(code => "ResourcePlanning_Country_" + code));

        foreach (var file in Files)
        {
            var strings = Read(file);

            Assert.All(keys, key => Assert.True(strings.ContainsKey(key), file + " misses " + key));
        }
    }

    /// <summary>
    /// The title resolves in every culture of the application: it-IT and it-CH to Italian, en to English, de-CH to German and fr-CH to French.
    /// </summary>
    /// <param name="culture">The culture name.</param>
    /// <param name="file">The resource file the culture falls back to.</param>
    [Theory]
    [InlineData("it-IT", "Strings.resx")]
    [InlineData("it-CH", "Strings.resx")]
    [InlineData("en-US", "Strings.en.resx")]
    [InlineData("de-CH", "Strings.de.resx")]
    [InlineData("fr-CH", "Strings.fr.resx")]
    public void Cultures_ResolveTheirLanguage(string culture, string file)
    {
        var expected = Read(file);

        foreach (var key in new[] { "Nav_ResourcePlanning", "ResourcePlanning_Title", "ResourcePlanning_Legend_Red", "ResourcePlanning_Rebalance_split_date_outside" })
        {
            Assert.Equal(expected[key], Strings.ResourceManager.GetString(key, new CultureInfo(culture)));
        }
    }

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex PlaceholderPattern();

    private static string Placeholders(string value) =>
        string.Join(",", PlaceholderPattern().Matches(value).Select(match => match.Value).Distinct().OrderBy(text => text, StringComparer.Ordinal));

    private static Dictionary<string, string> Read(string file)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FatturaFacile.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        var path = Path.Combine(directory.FullName, "src", "FatturaFacile.Wpf", "Localization", file);

        return XDocument.Load(path).Root!
            .Elements("data")
            .Where(data => data.Attribute("name")!.Value.StartsWith("ResourcePlanning_", StringComparison.Ordinal) || data.Attribute("name")!.Value == "Nav_ResourcePlanning")
            .ToDictionary(data => data.Attribute("name")!.Value, data => data.Element("value")!.Value, StringComparer.Ordinal);
    }
}
