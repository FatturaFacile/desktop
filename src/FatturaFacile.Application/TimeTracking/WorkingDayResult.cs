// <copyright file="WorkingDayResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// The evaluation of one day of an employee.
/// </summary>
public sealed class WorkingDayResult
{
    /// <summary>
    /// Gets or sets the day.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the first start of work.
    /// </summary>
    public DateTime? FirstIn { get; set; }

    /// <summary>
    /// Gets or sets the last end of work.
    /// </summary>
    public DateTime? LastOut { get; set; }

    /// <summary>
    /// Gets or sets the worked minutes.
    /// </summary>
    public int WorkedMinutes { get; set; }

    /// <summary>
    /// Gets or sets the break minutes (interruptions of at least 15 minutes).
    /// </summary>
    public int BreakMinutes { get; set; }

    /// <summary>
    /// Gets or sets the target minutes (contract hours on working days, without holidays).
    /// </summary>
    public int TargetMinutes { get; set; }

    /// <summary>
    /// Gets or sets the minutes credited by paid absences (holidays, sickness, accident, military service).
    /// </summary>
    public int CreditedMinutes { get; set; }

    /// <summary>
    /// Gets or sets the night work minutes (23:00 - 06:00).
    /// </summary>
    public int NightMinutes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there is Sunday work (Saturday 23:00 - Sunday 23:00).
    /// </summary>
    public bool SundayWork { get; set; }

    /// <summary>
    /// Gets or sets the rest since the previous working period, in minutes.
    /// </summary>
    public int? RestBeforeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the absence or holiday description of the day.
    /// </summary>
    public string? Absence { get; set; }

    /// <summary>
    /// Gets the daily balance (worked + credited - target).
    /// </summary>
    public int BalanceMinutes => WorkedMinutes + CreditedMinutes - TargetMinutes;

    /// <summary>
    /// Gets the violations and warnings of the day.
    /// </summary>
    public IList<string> Issues { get; } = [];

    /// <summary>
    /// Gets the issues as one line.
    /// </summary>
    public string IssuesText => string.Join("; ", Issues);
}
