// <copyright file="PlanningDraftBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Checks the forms of the planning setup (skill taxonomy, utilization targets, local holidays, snapshot window) before they are sent
/// and builds their requests with the members the contract expects.
/// </summary>
public static class PlanningDraftBuilder
{
    /// <summary>
    /// The problem code when the skill has no category.
    /// </summary>
    public const string SkillNeedsCategory = "skill_needs_category";

    /// <summary>
    /// The problem code when the skill has no name.
    /// </summary>
    public const string SkillNeedsName = "skill_needs_name";

    /// <summary>
    /// The problem code when a skill that needs a certificate names no certificate code.
    /// </summary>
    public const string CertificationCodeRequired = "certification_code_required";

    /// <summary>
    /// The problem code when a target is not between 0 and 100 percent.
    /// </summary>
    public const string TargetInvalidPercent = "target_invalid_percent";

    /// <summary>
    /// The problem code when a target names both a department and a cost center.
    /// </summary>
    public const string TargetSingleScope = "target_single_scope";

    /// <summary>
    /// The problem code when the holiday country is not CH or IT.
    /// </summary>
    public const string HolidayInvalidCountry = "holiday_invalid_country";

    /// <summary>
    /// The problem code when the holiday has no name.
    /// </summary>
    public const string HolidayNeedsName = "holiday_needs_name";

    /// <summary>
    /// The problem code when the month, the day or the year do not make a date.
    /// </summary>
    public const string HolidayInvalidDate = "holiday_invalid_date";

    /// <summary>
    /// The problem code when the fraction of the day is not between 0 (excluded) and 1.
    /// </summary>
    public const string HolidayInvalidFraction = "holiday_invalid_fraction";

    /// <summary>
    /// The problem code when the snapshot window has no days or ends before it starts.
    /// </summary>
    public const string SnapshotInvalidPeriod = "snapshot_invalid_period";

    private const double PercentMax = 100d;
    private const int LeapYear = 2024;
    private const int FirstYear = 1900;
    private const int LastYear = 2200;

    /// <summary>
    /// The country codes a holiday can be given for.
    /// </summary>
    public static readonly IReadOnlyList<string> Countries = ["CH", "IT"];

    /// <summary>
    /// Builds the request that creates or updates a skill of the taxonomy.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="skillName">The name of the skill.</param>
    /// <param name="requiresCertification">Whether a valid certificate is needed for the skill to count.</param>
    /// <param name="certificationCode">The certificate type, needed when a certificate is required.</param>
    /// <returns>The request or the first problem.</returns>
    public static PlanningDraftResult<PlanningSkillRequest> Skill(string? category, string? skillName, bool requiresCertification, string? certificationCode)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return PlanningDraftResult<PlanningSkillRequest>.Fail(SkillNeedsCategory);
        }

        if (string.IsNullOrWhiteSpace(skillName))
        {
            return PlanningDraftResult<PlanningSkillRequest>.Fail(SkillNeedsName);
        }

        var code = string.IsNullOrWhiteSpace(certificationCode) ? null : certificationCode.Trim();
        if (requiresCertification && code is null)
        {
            return PlanningDraftResult<PlanningSkillRequest>.Fail(CertificationCodeRequired);
        }

        return PlanningDraftResult<PlanningSkillRequest>.Ok(new PlanningSkillRequest(category.Trim(), skillName.Trim(), requiresCertification, requiresCertification ? code : null));
    }

    /// <summary>
    /// Builds the request that sets a utilization target.
    /// </summary>
    /// <param name="departmentId">The department, or null.</param>
    /// <param name="costCenterId">The cost center, or null; both null is the default of the tenant.</param>
    /// <param name="targetPercent">The target in percent.</param>
    /// <returns>The request or the first problem.</returns>
    public static PlanningDraftResult<UtilizationTargetRequest> Target(Guid? departmentId, Guid? costCenterId, double? targetPercent)
    {
        if (departmentId is not null && costCenterId is not null)
        {
            return PlanningDraftResult<UtilizationTargetRequest>.Fail(TargetSingleScope);
        }

        if (targetPercent is not { } percent || double.IsNaN(percent) || percent < 0d || percent > PercentMax)
        {
            return PlanningDraftResult<UtilizationTargetRequest>.Fail(TargetInvalidPercent);
        }

        return PlanningDraftResult<UtilizationTargetRequest>.Ok(new UtilizationTargetRequest(departmentId, costCenterId, percent));
    }

    /// <summary>
    /// Builds the request that adds a local holiday of the tenant.
    /// </summary>
    /// <param name="country">The country, CH or IT.</param>
    /// <param name="regionCode">The canton or region, or empty for the whole country.</param>
    /// <param name="municipality">The municipality, or empty.</param>
    /// <param name="month">The month, 1 to 12.</param>
    /// <param name="day">The day of the month.</param>
    /// <param name="year">The year when the holiday is not yearly, or null.</param>
    /// <param name="name">The name.</param>
    /// <param name="dayFraction">The part of the day that is off, above 0 and up to 1.</param>
    /// <returns>The request or the first problem.</returns>
    public static PlanningDraftResult<LocalHolidayRequest> LocalHoliday(string? country, string? regionCode, string? municipality, int? month, int? day, int? year, string? name, decimal? dayFraction)
    {
        var upperCountry = country?.Trim().ToUpperInvariant();
        if (upperCountry is null || !Countries.Contains(upperCountry, StringComparer.Ordinal))
        {
            return PlanningDraftResult<LocalHolidayRequest>.Fail(HolidayInvalidCountry);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return PlanningDraftResult<LocalHolidayRequest>.Fail(HolidayNeedsName);
        }

        if (month is not { } m || day is not { } d || !IsDate(m, d, year))
        {
            return PlanningDraftResult<LocalHolidayRequest>.Fail(HolidayInvalidDate);
        }

        if (dayFraction is not { } fraction || fraction <= 0m || fraction > 1m)
        {
            return PlanningDraftResult<LocalHolidayRequest>.Fail(HolidayInvalidFraction);
        }

        return PlanningDraftResult<LocalHolidayRequest>.Ok(new LocalHolidayRequest(
            upperCountry,
            string.IsNullOrWhiteSpace(regionCode) ? null : regionCode.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(municipality) ? null : municipality.Trim(),
            m,
            d,
            year,
            name.Trim(),
            fraction));
    }

    /// <summary>
    /// Builds the request that recalculates the capacity snapshots of a window.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="employeeId">The employee, or null for all of them.</param>
    /// <returns>The request or the first problem.</returns>
    public static PlanningDraftResult<SnapshotRecalculationRequest> SnapshotWindow(DateOnly? from, DateOnly? to, Guid? employeeId)
    {
        if (from is not { } start || to is not { } end || end < start)
        {
            return PlanningDraftResult<SnapshotRecalculationRequest>.Fail(SnapshotInvalidPeriod);
        }

        return PlanningDraftResult<SnapshotRecalculationRequest>.Ok(new SnapshotRecalculationRequest(start, end, employeeId));
    }

    private static bool IsDate(int month, int day, int? year)
    {
        if (year is { } value && (value < FirstYear || value > LastYear))
        {
            return false;
        }

        return month is >= 1 and <= 12 && day >= 1 && day <= DateTime.DaysInMonth(year ?? LeapYear, month);
    }
}
