// <copyright file="HrAbsence.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// An employee absence (sickness, accident, maternity, military service, holidays, unpaid leave).
/// </summary>
public class HrAbsence : ITenantScopedEntity
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
    /// Gets or sets the type: Sickness, Accident, Maternity, Paternity, Military, Vacation, Unpaid or Other.
    /// </summary>
    public string AbsenceType { get; set; } = "Sickness";

    /// <summary>
    /// Gets or sets the first day.
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the last day (inclusive).
    /// </summary>
    public DateTime EndDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the incapacity / absence percentage.
    /// </summary>
    public decimal Percent { get; set; } = 100m;

    /// <summary>
    /// Gets or sets a note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
