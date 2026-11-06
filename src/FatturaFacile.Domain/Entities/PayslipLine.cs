// <copyright file="PayslipLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A line of a payslip.
/// </summary>
public class PayslipLine : ITenantScopedEntity
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
    /// Gets or sets the payslip identifier.
    /// </summary>
    public Guid PayslipId { get; set; }

    /// <summary>
    /// Gets or sets the wage type code.
    /// </summary>
    public string WageTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the kind (Earning, NonCash, Expense, Deduction, Employer, Information).
    /// </summary>
    public string Kind { get; set; } = "Earning";

    /// <summary>
    /// Gets or sets the quantity or basis.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the rate (amount per unit or percent).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Gets or sets the amount (deductions are negative).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the salary certificate box.
    /// </summary>
    public string? CertificateBox { get; set; }

    /// <summary>
    /// Gets or sets the print order.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
