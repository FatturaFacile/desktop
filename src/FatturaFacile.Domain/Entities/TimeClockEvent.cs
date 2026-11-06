// <copyright file="TimeClockEvent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A time clock event (timbratura): entry, exit, start or end of a break. Corrections are audited and deletions are logical, so that the records can be shown to the labour inspectorate (art. 73 OLL 1, kept at least five years).
/// </summary>
public class TimeClockEvent : ITenantScopedEntity
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
    /// Gets or sets the local date and time of the event.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the event type: In, Out, BreakStart or BreakEnd.
    /// </summary>
    public string EventType { get; set; } = "In";

    /// <summary>
    /// Gets or sets the source: Terminal (clocked by the employee), Manual (entered or corrected by a manager) or Import.
    /// </summary>
    public string Source { get; set; } = "Terminal";

    /// <summary>
    /// Gets or sets a note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the user who recorded the event.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the logical deletion time (deleted events stay in the database).
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the reason of the deletion.
    /// </summary>
    public string? DeletionReason { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
