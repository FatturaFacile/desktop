// <copyright file="MfgCalendarClosure.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A closure (holiday or planned shutdown) of a working calendar.
/// </summary>
public class MfgCalendarClosure : ITenantScopedEntity
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
    /// Gets or sets the first closed day.
    /// </summary>
    public DateTime StartsOn { get; set; }

    /// <summary>
    /// Gets or sets the last closed day.
    /// </summary>
    public DateTime EndsOn { get; set; }

    /// <summary>
    /// Gets or sets the reason.
    /// </summary>
    public string? Reason { get; set; }
}
