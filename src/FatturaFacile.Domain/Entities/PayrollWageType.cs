// <copyright file="PayrollWageType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A payroll wage type (Lohnart / genere salariale) with its subjection to social insurances and withholding tax and its box on the salary certificate.
/// </summary>
public class PayrollWageType : ITenantScopedEntity
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
    /// Gets or sets the code (e.g. "1000"), unique per company.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description printed on payslips.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the kind: Earning, NonCash, Expense, Deduction, Employer or Information.
    /// </summary>
    public string Kind { get; set; } = "Earning";

    /// <summary>
    /// Gets or sets a value indicating whether the amount is subject to AHV/IV/EO (AVS/AI/IPG).
    /// </summary>
    public bool SubjectToAhv { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the amount is subject to unemployment insurance (ALV / AD).
    /// </summary>
    public bool SubjectToAlv { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the amount is subject to accident insurance (UVG / LAINF).
    /// </summary>
    public bool SubjectToUvg { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the amount is subject to daily sickness insurance (KTG / IGM).
    /// </summary>
    public bool SubjectToKtg { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the amount is part of the BVG (LPP) annual salary.
    /// </summary>
    public bool SubjectToBvg { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the amount is subject to withholding tax (Quellensteuer / imposta alla fonte).
    /// </summary>
    public bool SubjectToWithholdingTax { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the amount is an irregular payment (bonus, 13th salary in the annual withholding tax model).
    /// </summary>
    public bool IsIrregular { get; set; }

    /// <summary>
    /// Gets or sets the salary certificate (Lohnausweis, form 11) box, e.g. "1", "2.1", "3", "7", "13.1.1".
    /// </summary>
    public string? CertificateBox { get; set; }

    /// <summary>
    /// Gets or sets the print order on payslips.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the wage type is used by the calculation engine and cannot be deleted.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the wage type can be used for new inputs.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
