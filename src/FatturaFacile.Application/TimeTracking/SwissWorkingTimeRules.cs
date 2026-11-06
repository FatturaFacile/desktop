// <copyright file="SwissWorkingTimeRules.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// Working and rest time rules of the Swiss Labour Act (LL / ArG) and its ordinance 1 (OLL 1 / ArGV 1).
/// </summary>
public static class SwissWorkingTimeRules
{
    /// <summary>
    /// Maximum span of day and evening work including breaks, in minutes (art. 10 cpv. 3 LL: 14 hours).
    /// </summary>
    public const int MaximumDailySpanMinutes = 14 * 60;

    /// <summary>
    /// Minimum daily rest in minutes (art. 15a LL: 11 consecutive hours).
    /// </summary>
    public const int DailyRestMinutes = 11 * 60;

    /// <summary>
    /// Daily rest that can be granted once a week if the two-week average stays at 11 hours (art. 15a cpv. 2 LL).
    /// </summary>
    public const int ReducedDailyRestMinutes = 8 * 60;

    /// <summary>
    /// Maximum statutory overtime per day, in minutes (art. 12 cpv. 2 LL: 2 hours).
    /// </summary>
    public const int MaximumDailyOvertimeMinutes = 2 * 60;

    /// <summary>
    /// Minimum duration of a work interruption counted as a break, in minutes.
    /// </summary>
    public const int MinimumBreakMinutes = 15;

    /// <summary>
    /// Start of night work (art. 10 and 16 LL: 23:00 - 06:00).
    /// </summary>
    public static readonly TimeSpan NightStart = TimeSpan.FromHours(23);

    /// <summary>
    /// End of night work.
    /// </summary>
    public static readonly TimeSpan NightEnd = TimeSpan.FromHours(6);

    /// <summary>
    /// Returns the minimum break for a daily working time (art. 15 LL: 15 minutes above 5.5 hours, 30 above 7, 60 above 9).
    /// </summary>
    /// <param name="workedMinutes">The worked minutes of the day.</param>
    /// <returns>The minimum break in minutes.</returns>
    public static int RequiredBreakMinutes(int workedMinutes) => workedMinutes switch
    {
        > 9 * 60 => 60,
        > 7 * 60 => 30,
        > 330 => 15,
        _ => 0,
    };

    /// <summary>
    /// Returns the yearly cap of statutory overtime (art. 12 cpv. 2 LL: 170 hours with a 45-hour week, 140 with 50 hours).
    /// </summary>
    /// <param name="legalMaximumWeeklyHours">The legal maximum weekly hours (45 or 50).</param>
    /// <returns>The yearly cap in minutes.</returns>
    public static int YearlyOvertimeCapMinutes(decimal legalMaximumWeeklyHours) =>
        legalMaximumWeeklyHours >= 50m ? 140 * 60 : 170 * 60;

    /// <summary>
    /// Returns the minutes of an interval falling in the night period (23:00 - 06:00).
    /// </summary>
    /// <param name="start">The interval start.</param>
    /// <param name="end">The interval end.</param>
    /// <returns>The night minutes.</returns>
    public static int NightMinutes(DateTime start, DateTime end)
    {
        var minutes = 0;

        for (var day = start.Date.AddDays(-1); day <= end.Date; day = day.AddDays(1))
        {
            minutes += OverlapMinutes(start, end, day + NightStart, day.AddDays(1) + NightEnd);
        }

        return minutes;
    }

    /// <summary>
    /// Returns whether an interval touches the Swiss legal Sunday (Saturday 23:00 - Sunday 23:00, art. 18 LL).
    /// </summary>
    /// <param name="start">The interval start.</param>
    /// <param name="end">The interval end.</param>
    /// <returns>True for Sunday work.</returns>
    public static bool IsSundayWork(DateTime start, DateTime end)
    {
        for (var day = start.Date.AddDays(-1); day <= end.Date.AddDays(1); day = day.AddDays(1))
        {
            if (day.DayOfWeek == DayOfWeek.Sunday && OverlapMinutes(start, end, day.AddHours(-1), day.AddHours(23)) > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the overlap of two intervals in minutes.
    /// </summary>
    /// <param name="start">The first interval start.</param>
    /// <param name="end">The first interval end.</param>
    /// <param name="otherStart">The second interval start.</param>
    /// <param name="otherEnd">The second interval end.</param>
    /// <returns>The overlapping minutes.</returns>
    public static int OverlapMinutes(DateTime start, DateTime end, DateTime otherStart, DateTime otherEnd)
    {
        var from = start > otherStart ? start : otherStart;
        var to = end < otherEnd ? end : otherEnd;
        return to > from ? (int)Math.Round((to - from).TotalMinutes, MidpointRounding.AwayFromZero) : 0;
    }
}
