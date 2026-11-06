// <copyright file="TimeDailyRecord.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A daily working time record for employees with simplified recording (art. 73b OLL 1): only the daily duration, plus start and end for night or Sunday work.
/// </summary>
public class TimeDailyRecord : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the company (tenant) identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the employee identifier.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the day.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the worked minutes.
    /// </summary>
    public int WorkedMinutes { get; set; }

    /// <summary>
    /// Gets or sets the start time (mandatory for night or Sunday work).
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time (mandatory for night or Sunday work).
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets a note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the user who recorded the duration.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
