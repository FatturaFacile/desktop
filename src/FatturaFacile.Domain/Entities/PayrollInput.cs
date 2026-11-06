// <copyright file="PayrollInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A monthly payroll input of an employee (hours, overtime, bonus, expenses, benefits in kind, daily allowances).
/// </summary>
public class PayrollInput : ITenantScopedEntity
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
    /// Gets or sets the payroll year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the payroll month (1-12).
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets the wage type code.
    /// </summary>
    public string WageTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity (hours, days); 0 when the amount is entered directly.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the rate per unit.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Gets or sets the amount (quantity x rate when both are set).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets a note printed on the payslip.
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
