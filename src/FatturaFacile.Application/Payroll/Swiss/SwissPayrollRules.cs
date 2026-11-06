// <copyright file="SwissPayrollRules.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Payroll.Swiss;

/// <summary>
/// Statutory rules of Swiss payroll that do not depend on company settings.
/// </summary>
public static class SwissPayrollRules
{
    /// <summary>
    /// BVG entry threshold (annual salary), valid from 2025.
    /// </summary>
    public const decimal BvgEntryThreshold = 22_680m;

    /// <summary>
    /// BVG coordination deduction, valid from 2025.
    /// </summary>
    public const decimal BvgCoordinationDeduction = 26_460m;

    /// <summary>
    /// BVG upper limit of the annual salary, valid from 2025.
    /// </summary>
    public const decimal BvgMaximumSalary = 90_720m;

    /// <summary>
    /// BVG minimum coordinated salary, valid from 2025.
    /// </summary>
    public const decimal BvgMinimumCoordinatedSalary = 3780m;

    /// <summary>
    /// Minimum weekly hours for compulsory non-occupational accident insurance.
    /// </summary>
    public const decimal NbuMinimumWeeklyHours = 8m;

    private static readonly string[] _annualModelCantons = ["FR", "GE", "TI", "VD", "VS"];

    /// <summary>
    /// Returns the AHV reference age date (AHV 21 reform: 65 for men; for women born 1961-1963 a transitional age).
    /// </summary>
    /// <param name="dateOfBirth">The date of birth.</param>
    /// <param name="gender">"M" or "F".</param>
    /// <returns>The date on which the reference age is reached.</returns>
    public static DateTime ReferenceAgeDate(DateTime dateOfBirth, string? gender)
    {
        if (!string.Equals(gender, "F", StringComparison.OrdinalIgnoreCase))
        {
            return dateOfBirth.AddYears(65);
        }

        var months = dateOfBirth.Year switch
        {
            <= 1960 => -12,
            1961 => -9,
            1962 => -6,
            1963 => -3,
            _ => 0,
        };

        return dateOfBirth.AddYears(65).AddMonths(months);
    }

    /// <summary>
    /// Returns whether AHV/ALV contributions are due in a year: from 1 January following the 17th birthday.
    /// </summary>
    /// <param name="dateOfBirth">The date of birth.</param>
    /// <param name="year">The year.</param>
    /// <returns>True when contributions are due.</returns>
    public static bool IsContributionAge(DateTime? dateOfBirth, int year) => dateOfBirth is not { } birth || year >= birth.Year + 18;

    /// <summary>
    /// Returns the BVG age credit (savings contribution) in percent of the coordinated salary.
    /// </summary>
    /// <param name="dateOfBirth">The date of birth.</param>
    /// <param name="year">The year (BVG age = year - year of birth).</param>
    /// <returns>The credit (0 below 25).</returns>
    public static decimal BvgAgeCreditPercent(DateTime? dateOfBirth, int year)
    {
        var age = dateOfBirth is { } birth ? year - birth.Year : 40;

        return age switch
        {
            < 25 => 0m,
            < 35 => 7m,
            < 45 => 10m,
            < 55 => 15m,
            _ => 18m,
        };
    }

    /// <summary>
    /// Returns the annual coordinated (insured) BVG salary for an annual salary.
    /// </summary>
    /// <param name="annualSalary">The annual salary.</param>
    /// <returns>The coordinated salary (0 below the entry threshold).</returns>
    public static decimal BvgCoordinatedSalary(decimal annualSalary)
    {
        if (annualSalary < BvgEntryThreshold)
        {
            return 0m;
        }

        var capped = Math.Min(annualSalary, BvgMaximumSalary);
        return Math.Max(capped - BvgCoordinationDeduction, BvgMinimumCoordinatedSalary);
    }

    /// <summary>
    /// Returns whether a canton applies the annual withholding tax model (FR, GE, TI, VD, VS).
    /// </summary>
    /// <param name="canton">The canton.</param>
    /// <returns>True for the annual model, false for the monthly model.</returns>
    public static bool UsesAnnualWithholdingTaxModel(string? canton) =>
        canton is not null && _annualModelCantons.Contains(canton.ToUpperInvariant(), StringComparer.Ordinal);

    /// <summary>
    /// Returns the holiday compensation rate of hourly paid employees (weeks / (52 - weeks)).
    /// </summary>
    /// <param name="holidayWeeks">The holiday weeks per year.</param>
    /// <returns>The rate in percent (4 weeks = 8.33 %).</returns>
    public static decimal HolidayCompensationPercent(int holidayWeeks) =>
        holidayWeeks <= 0 ? 0m : Math.Round(holidayWeeks * 100m / (52m - holidayWeeks), 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Rounds a contribution to 5 centimes.
    /// </summary>
    /// <param name="amount">The amount.</param>
    /// <returns>The rounded amount.</returns>
    public static decimal RoundTo5Centimes(decimal amount) => Math.Round(amount * 20m, MidpointRounding.AwayFromZero) / 20m;

    /// <summary>
    /// Returns the paid days of a month with the 30-day convention for an employment period.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="hireDate">The hire date.</param>
    /// <param name="terminationDate">The termination date.</param>
    /// <returns>The paid days (0 when not employed in the month).</returns>
    public static decimal PaidDays(int year, int month, DateTime hireDate, DateTime? terminationDate)
    {
        var first = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var last = first.AddMonths(1).AddDays(-1);
        var start = hireDate.Date > first ? hireDate.Date : first;
        var end = terminationDate is { } termination && termination.Date < last ? termination.Date : last;

        if (end < start)
        {
            return 0m;
        }

        var startDay = Math.Min(start.Day, 30);
        var endDay = end == last ? 30 : Math.Min(end.Day, 30);
        return Math.Max(endDay - startDay + 1, 0);
    }

    /// <summary>
    /// Returns the number of absence days of a type in a month (30-day convention, weighted by the percentage).
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="startDate">The first absence day.</param>
    /// <param name="endDate">The last absence day.</param>
    /// <param name="percent">The absence percentage.</param>
    /// <returns>The weighted days.</returns>
    public static decimal AbsenceDays(int year, int month, DateTime startDate, DateTime endDate, decimal percent) =>
        PaidDays(year, month, startDate, endDate) * percent / 100m;
}
