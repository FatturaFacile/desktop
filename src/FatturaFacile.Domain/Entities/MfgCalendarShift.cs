// <copyright file="MfgCalendarShift.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A weekly shift of a working calendar.
/// </summary>
public class MfgCalendarShift : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the server identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the local creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the local update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the calendar identifier.
    /// </summary>
    public Guid CalendarId { get; set; }

    /// <summary>
    /// Gets or sets the ISO weekday (1 = Monday ... 7 = Sunday).
    /// </summary>
    public int DayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the shift code.
    /// </summary>
    public string ShiftCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time (HH:mm).
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the end time (HH:mm).
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the shift ends the next day.
    /// </summary>
    public bool CrossesMidnight { get; set; }

    /// <summary>
    /// Gets or sets the unpaid break in minutes.
    /// </summary>
    public int BreakMinutes { get; set; }

    /// <summary>
    /// Gets or sets the net available minutes of the shift.
    /// </summary>
    public int NetAvailableMinutes { get; set; }

    /// <summary>
    /// Gets or sets the labour cost multiplier.
    /// </summary>
    public decimal LaborMultiplier { get; set; } = 1m;
}
