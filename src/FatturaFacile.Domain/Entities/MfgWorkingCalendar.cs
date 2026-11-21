// <copyright file="MfgWorkingCalendar.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A working calendar mirrored from the web-api (Mod 061), with its shifts and closures.
/// </summary>
public class MfgWorkingCalendar : ITenantScopedEntity
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
    /// Gets or sets the calendar code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IANA time zone.
    /// </summary>
    public string Timezone { get; set; } = "UTC";

    /// <summary>
    /// Gets or sets the country code.
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Gets or sets the region code.
    /// </summary>
    public string? RegionCode { get; set; }

    /// <summary>
    /// Gets or sets the holiday calendar the closures inherit.
    /// </summary>
    public Guid? InheritHolidayCalendarId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the calendar is the default one.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets when the row was pulled from the server.
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the weekly shifts.
    /// </summary>
    public ICollection<MfgCalendarShift> Shifts { get; set; } = [];

    /// <summary>
    /// Gets or sets the closures.
    /// </summary>
    public ICollection<MfgCalendarClosure> Closures { get; set; } = [];
}
