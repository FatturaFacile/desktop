// <copyright file="WorkingTimeContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// The data needed to evaluate the working time of an employee.
/// </summary>
public sealed class WorkingTimeContext
{
    /// <summary>
    /// Gets or sets the employee.
    /// </summary>
    public HrEmployee Employee { get; set; } = new();

    /// <summary>
    /// Gets or sets the first day.
    /// </summary>
    public DateTime From { get; set; }

    /// <summary>
    /// Gets or sets the last day.
    /// </summary>
    public DateTime To { get; set; }

    /// <summary>
    /// Gets or sets the valid (not deleted) clock events, including the day before the period for the daily rest.
    /// </summary>
    public IReadOnlyList<TimeClockEvent> Events { get; set; } = [];

    /// <summary>
    /// Gets or sets the daily records (simplified recording).
    /// </summary>
    public IReadOnlyList<TimeDailyRecord> DailyRecords { get; set; } = [];

    /// <summary>
    /// Gets or sets the absences.
    /// </summary>
    public IReadOnlyList<HrAbsence> Absences { get; set; } = [];

    /// <summary>
    /// Gets or sets the public holidays.
    /// </summary>
    public IReadOnlyList<PublicHoliday> Holidays { get; set; } = [];

    /// <summary>
    /// Gets or sets the statutory overtime of the same year before the period, in minutes.
    /// </summary>
    public int StatutoryOvertimeBeforeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the current time (open entries of today are not reported as missing exits).
    /// </summary>
    public DateTime Now { get; set; } = DateTime.Now;
}
