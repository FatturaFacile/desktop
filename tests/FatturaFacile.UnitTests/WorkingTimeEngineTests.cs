// <copyright file="WorkingTimeEngineTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Swiss working time evaluation (Labour Act).
/// </summary>
public sealed class WorkingTimeEngineTests
{
    private static readonly DateTime _monday = new(2026, 11, 2, 0, 0, 0, DateTimeKind.Unspecified);

    /// <summary>
    /// A regular day with a 45-minute lunch break: worked time, break and balance against 8h24 (42 h / 5 days).
    /// </summary>
    [Fact]
    public void RegularDay_ComputesWorkedBreakAndBalance()
    {
        var report = Evaluate([.. Day(_monday, "08:00", "12:00", "12:45", "17:15")]);
        var day = report.Days[0];

        Assert.Equal(510, day.WorkedMinutes);
        Assert.Equal(45, day.BreakMinutes);
        Assert.Equal(504, day.TargetMinutes);
        Assert.Equal(6, day.BalanceMinutes);
        Assert.Empty(day.Issues);
    }

    /// <summary>
    /// More than 9 hours of work requires a one-hour break (art. 15 LL).
    /// </summary>
    [Fact]
    public void LongDayWithShortBreak_IsReported()
    {
        var report = Evaluate([.. Day(_monday, "07:00", "12:00", "12:30", "17:30")]);

        Assert.Contains(report.Days[0].Issues, static i => i.Contains("art. 15 LL", StringComparison.Ordinal));
    }

    /// <summary>
    /// Less than 11 hours of rest between two days and night work are reported.
    /// </summary>
    [Fact]
    public void ShortRestAndNightWork_AreReported()
    {
        List<TimeClockEvent> events = [.. Day(_monday, "14:00", "23:30"), .. Day(_monday.AddDays(1), "07:00", "12:00")];

        var report = Evaluate(events, days: 2);

        Assert.Contains(report.Days[0].Issues, static i => i.Contains("notturno", StringComparison.Ordinal));
        Assert.Equal(30, report.Days[0].NightMinutes);
        Assert.Equal(450, report.Days[1].RestBeforeMinutes);
        Assert.Contains(report.Days[1].Issues, static i => i.Contains("inferiore a 8 ore", StringComparison.Ordinal));
    }

    /// <summary>
    /// A missing exit is reported, holidays and paid absences adjust the target, and Sunday work is detected.
    /// </summary>
    [Fact]
    public void MissingExitHolidayAbsenceAndSunday_AreHandled()
    {
        List<TimeClockEvent> events = [Clock(_monday.AddHours(8), TimeEventType.In), .. Day(_monday.AddDays(6), "09:00", "11:00")];
        var context = Context(events, days: 7);
        context.Holidays = [new PublicHoliday { Date = _monday.AddDays(1), Name = "Ognissanti" }];
        context.Absences = [new HrAbsence { AbsenceType = "Vacation", StartDate = _monday.AddDays(2), EndDate = _monday.AddDays(2), Percent = 100m }];

        var report = SwissWorkingTimeEngine.Evaluate(context);

        Assert.Contains(report.Days[0].Issues, static i => i.Contains("senza uscita", StringComparison.Ordinal));
        Assert.Equal(0, report.Days[1].TargetMinutes);
        Assert.Equal(504, report.Days[2].CreditedMinutes);
        Assert.Equal(0, report.Days[2].BalanceMinutes);
        Assert.True(report.Days[6].SundayWork);
        Assert.Equal(0, report.Days[6].TargetMinutes);
    }

    /// <summary>
    /// Work beyond the legal maximum of 45 hours is statutory overtime, also checked against the yearly cap of 170 hours.
    /// </summary>
    [Fact]
    public void WeekBeyondLegalMaximum_ProducesStatutoryOvertime()
    {
        List<TimeClockEvent> events = [];

        for (var i = 0; i < 5; i++)
        {
            events.AddRange(Day(_monday.AddDays(i), "07:00", "12:00", "13:00", "18:00"));
        }

        var context = Context(events, days: 7);
        context.StatutoryOvertimeBeforeMinutes = 169 * 60;

        var report = SwissWorkingTimeEngine.Evaluate(context);

        Assert.Equal(50 * 60, report.Weeks[0].WorkedMinutes);
        Assert.Equal(5 * 60, report.StatutoryOvertimeMinutes);
        Assert.Contains(report.Issues, static i => i.Contains("art. 12 cpv. 2", StringComparison.Ordinal));
    }

    /// <summary>
    /// Pause thresholds and the 50-hour week cap follow the Labour Act.
    /// </summary>
    [Fact]
    public void Rules_FollowLabourAct()
    {
        Assert.Equal(0, SwissWorkingTimeRules.RequiredBreakMinutes(330));
        Assert.Equal(15, SwissWorkingTimeRules.RequiredBreakMinutes(331));
        Assert.Equal(30, SwissWorkingTimeRules.RequiredBreakMinutes(421));
        Assert.Equal(60, SwissWorkingTimeRules.RequiredBreakMinutes(541));
        Assert.Equal(140 * 60, SwissWorkingTimeRules.YearlyOvertimeCapMinutes(50m));
        Assert.Equal(new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Unspecified), SwissHolidayCalendar.EasterSunday(2026));
        Assert.Contains(SwissHolidayCalendar.ForCanton("TI", 2026), static h => string.Equals(h.Name, "San Giuseppe", StringComparison.Ordinal));
    }

    private static WorkingTimeReport Evaluate(List<TimeClockEvent> events, int days = 1) => SwissWorkingTimeEngine.Evaluate(Context(events, days));

    private static WorkingTimeContext Context(List<TimeClockEvent> events, int days) => new()
    {
        Employee = new HrEmployee
        {
            FirstName = "Anna",
            LastName = "Bianchi",
            HireDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
            WeeklyHours = 42m,
            WorkDaysPerWeek = 5,
            LegalMaxWeeklyHours = 45m,
        },
        From = _monday,
        To = _monday.AddDays(days - 1),
        Events = events,
        Now = new DateTime(2026, 12, 31, 12, 0, 0, DateTimeKind.Unspecified),
    };

    private static IEnumerable<TimeClockEvent> Day(DateTime date, params string[] times)
    {
        for (var i = 0; i < times.Length; i++)
        {
            var type = (i % 2 == 0) ? TimeEventType.In : TimeEventType.Out;
            yield return Clock(date + TimeSpan.Parse(times[i], System.Globalization.CultureInfo.InvariantCulture), type);
        }
    }

    private static TimeClockEvent Clock(DateTime timestamp, string type) => new() { Timestamp = timestamp, EventType = type };
}
