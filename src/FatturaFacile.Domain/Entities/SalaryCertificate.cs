// <copyright file="SalaryCertificate.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The yearly salary certificate (Lohnausweis / certificato di salario, form 11) of an employee, computed from the closed payroll runs.
/// </summary>
public class SalaryCertificate : ITenantScopedEntity
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
    /// Gets or sets the year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the start of the certified period.
    /// </summary>
    public DateTime PeriodFrom { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the end of the certified period.
    /// </summary>
    public DateTime PeriodTo { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets box 1: salary.
    /// </summary>
    public decimal Box1Salary { get; set; }

    /// <summary>
    /// Gets or sets box 2: benefits in kind (2.1 + 2.2 + 2.3).
    /// </summary>
    public decimal Box2Benefits { get; set; }

    /// <summary>
    /// Gets or sets box 3: irregular payments.
    /// </summary>
    public decimal Box3Irregular { get; set; }

    /// <summary>
    /// Gets or sets box 4: capital payments.
    /// </summary>
    public decimal Box4Capital { get; set; }

    /// <summary>
    /// Gets or sets box 7: other payments (insurance daily allowances).
    /// </summary>
    public decimal Box7Other { get; set; }

    /// <summary>
    /// Gets or sets box 8: gross salary total.
    /// </summary>
    public decimal Box8Gross { get; set; }

    /// <summary>
    /// Gets or sets box 9: AHV/IV/EO/ALV/NBUV contributions.
    /// </summary>
    public decimal Box9SocialContributions { get; set; }

    /// <summary>
    /// Gets or sets box 10.1: ordinary BVG contributions.
    /// </summary>
    public decimal Box10Pension { get; set; }

    /// <summary>
    /// Gets or sets box 11: net salary.
    /// </summary>
    public decimal Box11Net { get; set; }

    /// <summary>
    /// Gets or sets box 12: withholding tax deducted.
    /// </summary>
    public decimal Box12WithholdingTax { get; set; }

    /// <summary>
    /// Gets or sets box 13: expense allowances.
    /// </summary>
    public decimal Box13Expenses { get; set; }

    /// <summary>
    /// Gets or sets box 15: remarks.
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Gets or sets the generation time.
    /// </summary>
    public DateTime? GeneratedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
