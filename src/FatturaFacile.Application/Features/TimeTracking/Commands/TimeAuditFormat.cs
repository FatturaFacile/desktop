// <copyright file="TimeAuditFormat.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Describes working time records in the audit trail.
/// </summary>
public static class TimeAuditFormat
{
    /// <summary>
    /// Describes a clock event.
    /// </summary>
    /// <param name="clock">The event.</param>
    /// <returns>The description.</returns>
    public static string Describe(TimeClockEvent clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return string.Create(CultureInfo.InvariantCulture, $"{clock.Timestamp:yyyy-MM-dd HH:mm} {clock.EventType} ({clock.Source}) {clock.Note}").Trim();
    }

    /// <summary>
    /// Describes a daily record.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <returns>The description.</returns>
    public static string Describe(TimeDailyRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return string.Create(CultureInfo.InvariantCulture, $"{record.Date:yyyy-MM-dd} {record.WorkedMinutes} min {record.StartTime:HH:mm}-{record.EndTime:HH:mm} {record.Note}").Trim();
    }
}
