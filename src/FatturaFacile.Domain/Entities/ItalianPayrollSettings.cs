// <copyright file="ItalianPayrollSettings.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The Italian payroll settings of a company (INPS position and contribution rates).
/// </summary>
public class ItalianPayrollSettings : ITenantScopedEntity
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
    /// Gets or sets the INPS company number (matricola aziendale).
    /// </summary>
    public string? InpsCompanyNumber { get; set; }

    /// <summary>
    /// Gets or sets the INPS office code used in the F24 (codice sede).
    /// </summary>
    public string? InpsOfficeCode { get; set; }

    /// <summary>
    /// Gets or sets the INAIL customer and position code.
    /// </summary>
    public string? InailCode { get; set; }

    /// <summary>
    /// Gets or sets the employee INPS rate (9.19 for most employees).
    /// </summary>
    public decimal EmployeeInpsRatePercent { get; set; } = 9.19m;

    /// <summary>
    /// Gets or sets the employer INPS rate of the company.
    /// </summary>
    public decimal EmployerInpsRatePercent { get; set; } = 29.0m;

    /// <summary>
    /// Gets or sets the date and time when the record was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
