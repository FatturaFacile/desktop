// <copyright file="PayrollRun.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A monthly payroll run (Lohnlauf / elaborazione paghe) of a company.
/// </summary>
public class PayrollRun : ITenantScopedEntity
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
    /// Gets or sets the year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the month (1-12).
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets the status: Draft (can be recalculated) or Final (closed, used for the salary certificate).
    /// </summary>
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// Gets or sets the payment date.
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Gets or sets the last calculation time.
    /// </summary>
    public DateTime? CalculatedAt { get; set; }

    /// <summary>
    /// Gets or sets the closing time.
    /// </summary>
    public DateTime? FinalizedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
