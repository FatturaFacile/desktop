// <copyright file="SwissWorkingTimeEngine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// Evaluates clock events against the Swiss Labour Act: working time, breaks (art. 15), daily span (art. 10), daily rest
/// (art. 15a), night and Sunday work (art. 16-18), legal weekly maximum and statutory overtime (art. 9 and 12), and the
/// contractual time balance.
/// </summary>
public static class SwissWorkingTimeEngine
{
    private static readonly string[] _paidAbsences = ["Vacation", "Sickness", "Accident", "Maternity", "Paternity", "Military", "Other"];

    /// <summary>
    /// Evaluates a period.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The report.</returns>
    public static WorkingTimeReport Evaluate(WorkingTimeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var employee = context.Employee;
        var mode = string.IsNullOrWhiteSpace(employee.TimeRecordingMode) ? TimeRecordingMode.Detailed : employee.TimeRecordingMode;
        var legalMaximum = employee.LegalMaxWeeklyHours <= 0m ? 45m : employee.LegalMaxWeeklyHours;

        WorkingTimeReport report = new()
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            RecordingMode = mode,
            From = context.From.Date,
            To = context.To.Date,
            StatutoryOvertimeBeforeMinutes = context.StatutoryOvertimeBeforeMinutes,
            StatutoryOvertimeCapMinutes = SwissWorkingTimeRules.YearlyOvertimeCapMinutes(legalMaximum),
        };

        var issues = BuildIntervals(context.Events, context.Now, out var intervals);
        var simplified = string.Equals(mode, TimeRecordingMode.Simplified, StringComparison.Ordinal);

        for (var day = report.From; day <= report.To; day = day.AddDays(1))
        {
            var result = simplified
                ? EvaluateSimplifiedDay(day, context.DailyRecords)
                : EvaluateDetailedDay(day, intervals, issues);
            ApplyTarget(result, employee, context);
            report.Days.Add(result);
        }

        AddWeeks(report, legalMaximum, employee.WorkDaysPerWeek);
        AddPeriodIssues(report, mode, employee);
        return report;
    }

    private static Dictionary<DateTime, List<string>> BuildIntervals(IReadOnlyList<TimeClockEvent> events, DateTime now, out List<WorkInterval> intervals)
    {
        intervals = [];
        Dictionary<DateTime, List<string>> issues = [];
        DateTime? open = null;

        foreach (var clock in events.Where(static e => e.DeletedAt is null).OrderBy(static e => e.Timestamp))
        {
            if (TimeEventType.StartsWork(clock.EventType))
            {
                if (open is { } previous)
                {
                    AddIssue(issues, previous.Date, "Entrata senza uscita prima di una nuova entrata");
                }

                open = clock.Timestamp;
            }
            else if (open is { } start)
            {
                intervals.Add(new WorkInterval(start, clock.Timestamp));
                open = null;
            }
            else
            {
                AddIssue(issues, clock.Timestamp.Date, "Uscita senza entrata");
            }
        }

        if (open is { } pending && pending.Date < now.Date)
        {
            AddIssue(issues, pending.Date, "Timbratura di uscita mancante");
        }

        return issues;
    }

    private static void AddIssue(Dictionary<DateTime, List<string>> issues, DateTime day, string issue)
    {
        if (!issues.TryGetValue(day, out var list))
        {
            list = [];
            issues[day] = list;
        }

        list.Add(issue);
    }

    private static WorkingDayResult EvaluateDetailedDay(DateTime day, List<WorkInterval> intervals, Dictionary<DateTime, List<string>> issues)
    {
        WorkingDayResult result = new() { Date = day };
        var today = intervals.Where(i => i.Start.Date == day).OrderBy(static i => i.Start).ToList();

        if (issues.TryGetValue(day, out var dayIssues))
        {
            foreach (var issue in dayIssues)
            {
                result.Issues.Add(issue);
            }
        }

        if (today.Count == 0)
        {
            return result;
        }

        result.FirstIn = today[0].Start;
        result.LastOut = today[^1].End;
        result.WorkedMinutes = today.Sum(static i => i.Minutes);
        result.NightMinutes = today.Sum(static i => SwissWorkingTimeRules.NightMinutes(i.Start, i.End));
        result.SundayWork = today.Exists(static i => SwissWorkingTimeRules.IsSundayWork(i.Start, i.End));

        for (var i = 1; i < today.Count; i++)
        {
            var gap = (int)Math.Round((today[i].Start - today[i - 1].End).TotalMinutes, MidpointRounding.AwayFromZero);

            if (gap >= SwissWorkingTimeRules.MinimumBreakMinutes)
            {
                result.BreakMinutes += gap;
            }
        }

        var previous = intervals.Where(i => i.End <= today[0].Start).OrderByDescending(static i => i.End).FirstOrDefault();
        result.RestBeforeMinutes = previous is null ? null : (int)Math.Round((today[0].Start - previous.End).TotalMinutes, MidpointRounding.AwayFromZero);

        CheckDetailedRules(result);
        return result;
    }

    private static void CheckDetailedRules(WorkingDayResult result)
    {
        var requiredBreak = SwissWorkingTimeRules.RequiredBreakMinutes(result.WorkedMinutes);

        if (result.BreakMinutes < requiredBreak)
        {
            result.Issues.Add($"Pausa insufficiente: {result.BreakMinutes.ToString(CultureInfo.InvariantCulture)} min invece di {requiredBreak.ToString(CultureInfo.InvariantCulture)} (art. 15 LL)");
        }

        if (result.FirstIn is { } first && result.LastOut is { } last && (last - first).TotalMinutes > SwissWorkingTimeRules.MaximumDailySpanMinutes)
        {
            result.Issues.Add("Spazio temporale del lavoro superiore a 14 ore (art. 10 LL)");
        }

        if (result.RestBeforeMinutes is { } rest && rest < SwissWorkingTimeRules.ReducedDailyRestMinutes)
        {
            result.Issues.Add($"Riposo giornaliero di {WorkingTimeReport.FormatMinutes(rest)} h: inferiore a 8 ore (art. 15a LL)");
        }
        else if (result.RestBeforeMinutes is { } shortRest && shortRest < SwissWorkingTimeRules.DailyRestMinutes)
        {
            result.Issues.Add($"Riposo giornaliero ridotto a {WorkingTimeReport.FormatMinutes(shortRest)} h: ammesso una volta a settimana con media di 11 ore su due settimane (art. 15a LL)");
        }

        AddNightAndSundayIssues(result);
    }

    private static void AddNightAndSundayIssues(WorkingDayResult result)
    {
        if (result.NightMinutes > 0)
        {
            result.Issues.Add("Lavoro notturno (23-06): richiede un permesso e il supplemento o compensazione previsti (art. 16-17b LL)");
        }

        if (result.SundayWork)
        {
            result.Issues.Add("Lavoro domenicale (sabato 23 - domenica 23): richiede un permesso (art. 18-19 LL)");
        }
    }

    private static WorkingDayResult EvaluateSimplifiedDay(DateTime day, IReadOnlyList<TimeDailyRecord> records)
    {
        WorkingDayResult result = new() { Date = day };
        var record = records.FirstOrDefault(r => r.Date.Date == day);

        if (record is null)
        {
            return result;
        }

        result.WorkedMinutes = record.WorkedMinutes;

        if (record.StartTime is { } start && record.EndTime is { } end && end > start)
        {
            result.FirstIn = start;
            result.LastOut = end;
            result.NightMinutes = SwissWorkingTimeRules.NightMinutes(start, end);
            result.SundayWork = SwissWorkingTimeRules.IsSundayWork(start, end);
            AddNightAndSundayIssues(result);
        }
        else if (day.DayOfWeek == DayOfWeek.Sunday && record.WorkedMinutes > 0)
        {
            result.SundayWork = true;
            result.Issues.Add("Lavoro domenicale: con la registrazione semplificata vanno indicati inizio e fine (art. 73b OLL 1)");
        }

        return result;
    }

    private static void ApplyTarget(WorkingDayResult result, HrEmployee employee, WorkingTimeContext context)
    {
        var workDays = Math.Clamp(employee.WorkDaysPerWeek, 1, 7);
        var dayIndex = ((int)result.Date.DayOfWeek + 6) % 7;

        if (dayIndex >= workDays || result.Date < employee.HireDate.Date || (employee.TerminationDate is { } exit && result.Date > exit.Date))
        {
            return;
        }

        var target = (int)Math.Round(employee.WeeklyHours * 60m / workDays, MidpointRounding.AwayFromZero);
        var holiday = context.Holidays.FirstOrDefault(h => h.Date.Date == result.Date);

        if (holiday is not null)
        {
            target = (int)Math.Round(target * (1m - Math.Clamp(holiday.DayFraction, 0m, 1m)), MidpointRounding.AwayFromZero);
            result.Absence = holiday.Name;
        }

        var absence = context.Absences.FirstOrDefault(a => a.StartDate.Date <= result.Date && a.EndDate.Date >= result.Date);

        if (absence is not null)
        {
            var share = (int)Math.Round(target * Math.Clamp(absence.Percent, 0m, 100m) / 100m, MidpointRounding.AwayFromZero);
            result.Absence = absence.AbsenceType;

            if (_paidAbsences.Contains(absence.AbsenceType, StringComparer.Ordinal))
            {
                result.CreditedMinutes = share;
            }
            else
            {
                target -= share;
            }
        }

        result.TargetMinutes = target;
    }

    private static void AddWeeks(WorkingTimeReport report, decimal legalMaximumHours, int workDaysPerWeek)
    {
        var legalMaximum = (int)Math.Round(legalMaximumHours * 60m, MidpointRounding.AwayFromZero);
        var dailyLegal = legalMaximum / Math.Clamp(workDaysPerWeek, 1, 7);

        foreach (var week in report.Days.GroupBy(static d => d.Date.AddDays(-(((int)d.Date.DayOfWeek + 6) % 7))))
        {
            var worked = week.Sum(static d => d.WorkedMinutes);
            var overtime = Math.Max(0, worked - legalMaximum);
            report.Weeks.Add(new WorkingWeekResult(week.Key, worked, legalMaximum, overtime));

            if (overtime <= 0)
            {
                continue;
            }

            foreach (var day in week.Where(d => d.WorkedMinutes - dailyLegal > SwissWorkingTimeRules.MaximumDailyOvertimeMinutes))
            {
                day.Issues.Add("Lavoro straordinario superiore a 2 ore nel giorno (art. 12 LL)");
            }
        }
    }

    private static void AddPeriodIssues(WorkingTimeReport report, string mode, HrEmployee employee)
    {
        if (report.StatutoryOvertimeYearMinutes > report.StatutoryOvertimeCapMinutes)
        {
            report.Issues.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"Lavoro straordinario annuo {WorkingTimeReport.FormatMinutes(report.StatutoryOvertimeYearMinutes)} h oltre il limite di {report.StatutoryOvertimeCapMinutes / 60} h (art. 12 cpv. 2 LL)"));
        }

        if (string.Equals(mode, TimeRecordingMode.Waived, StringComparison.Ordinal))
        {
            report.Issues.Add("Rinuncia alla registrazione (art. 73a OLL 1): verificare CCL, accordo scritto individuale e salario annuo lordo sopra la soglia.");
        }
        else if (string.Equals(mode, TimeRecordingMode.Simplified, StringComparison.Ordinal) && employee.TimeRecordingAgreementDate is null)
        {
            report.Issues.Add("Registrazione semplificata senza data dell'accordo (art. 73b OLL 1).");
        }
    }
}
