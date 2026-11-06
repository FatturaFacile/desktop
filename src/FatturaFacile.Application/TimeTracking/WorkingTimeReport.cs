// <copyright file="WorkingTimeReport.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// The working time evaluation of an employee for a period, as required for the labour inspectorate.
/// </summary>
public sealed class WorkingTimeReport
{
    /// <summary>
    /// Gets or sets the employee identifier.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the employee name.
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recording mode.
    /// </summary>
    public string RecordingMode { get; set; } = TimeRecordingMode.Detailed;

    /// <summary>
    /// Gets or sets the first day.
    /// </summary>
    public DateTime From { get; set; }

    /// <summary>
    /// Gets or sets the last day.
    /// </summary>
    public DateTime To { get; set; }

    /// <summary>
    /// Gets the days.
    /// </summary>
    public IList<WorkingDayResult> Days { get; } = [];

    /// <summary>
    /// Gets the weeks.
    /// </summary>
    public IList<WorkingWeekResult> Weeks { get; } = [];

    /// <summary>
    /// Gets the period issues (yearly overtime cap, recording mode).
    /// </summary>
    public IList<string> Issues { get; } = [];

    /// <summary>
    /// Gets or sets the statutory overtime of the year before the period, in minutes.
    /// </summary>
    public int StatutoryOvertimeBeforeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the yearly statutory overtime cap in minutes.
    /// </summary>
    public int StatutoryOvertimeCapMinutes { get; set; }

    /// <summary>
    /// Gets the worked minutes of the period.
    /// </summary>
    public int WorkedMinutes => Days.Sum(static d => d.WorkedMinutes);

    /// <summary>
    /// Gets the target minutes of the period.
    /// </summary>
    public int TargetMinutes => Days.Sum(static d => d.TargetMinutes);

    /// <summary>
    /// Gets the credited minutes of the period.
    /// </summary>
    public int CreditedMinutes => Days.Sum(static d => d.CreditedMinutes);

    /// <summary>
    /// Gets the time balance of the period (overtime hours if positive).
    /// </summary>
    public int BalanceMinutes => Days.Sum(static d => d.BalanceMinutes);

    /// <summary>
    /// Gets the statutory overtime of the period.
    /// </summary>
    public int StatutoryOvertimeMinutes => Weeks.Sum(static w => w.StatutoryOvertimeMinutes);

    /// <summary>
    /// Gets the statutory overtime of the year up to the end of the period.
    /// </summary>
    public int StatutoryOvertimeYearMinutes => StatutoryOvertimeBeforeMinutes + StatutoryOvertimeMinutes;

    /// <summary>
    /// Gets the number of days with issues.
    /// </summary>
    public int DaysWithIssues => Days.Count(static d => d.Issues.Count > 0);

    /// <summary>
    /// Formats minutes as hours and minutes (e.g. "8:30", "-1:15").
    /// </summary>
    /// <param name="minutes">The minutes.</param>
    /// <returns>The formatted duration.</returns>
    public static string FormatMinutes(int minutes)
    {
        var sign = minutes < 0 ? "-" : string.Empty;
        var absolute = Math.Abs(minutes);
        return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{sign}{absolute / 60}:{absolute % 60:00}");
    }
}
