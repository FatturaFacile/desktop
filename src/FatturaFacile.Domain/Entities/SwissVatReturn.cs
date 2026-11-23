// <copyright file="SwissVatReturn.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Periodic Swiss VAT return (Form 050 Effective / Form 051 Saldosteuersatz).
/// </summary>
public class SwissVatReturn : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the fiscal year.
    /// </summary>
    public int FiscalYear { get; set; }

    /// <summary>
    /// Gets or sets the period type.
    /// </summary>
    public SwissVatFrequency PeriodType { get; set; } = SwissVatFrequency.Quarterly;

    /// <summary>
    /// Gets or sets the period number within the year.
    /// </summary>
    public int PeriodNumber { get; set; }

    /// <summary>
    /// Gets or sets the human-readable period label (e.g., 2026-Q1).
    /// </summary>
    public string PeriodLabel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the period start date.
    /// </summary>
    public DateOnly PeriodFrom { get; set; }

    /// <summary>
    /// Gets or sets the period end date.
    /// </summary>
    public DateOnly PeriodTo { get; set; }

    /// <summary>
    /// Gets or sets the accounting method used for this return.
    /// </summary>
    public SwissVatMethod AccountingMethod { get; set; } = SwissVatMethod.Effective;

    /// <summary>
    /// Gets or sets the consideration basis used for this return.
    /// </summary>
    public SwissVatConsiderationBasis ConsiderationBasis { get; set; } = SwissVatConsiderationBasis.Agreed;

    /// <summary>
    /// Gets or sets the lifecycle status.
    /// </summary>
    public SwissVatReturnStatus Status { get; set; } = SwissVatReturnStatus.Draft;

    /// <summary>
    /// Gets or sets AFC Box 200 total turnover.
    /// </summary>
    public decimal Box200TotalTurnover { get; set; }

    /// <summary>
    /// Gets or sets AFC Box 289 total deductions.
    /// </summary>
    public decimal Box289TotalDeductions { get; set; }

    /// <summary>
    /// Gets or sets AFC Box 299 taxable turnover.
    /// </summary>
    public decimal Box299TaxableTurnover { get; set; }

    /// <summary>
    /// Gets or sets AFC Box 399 total output tax.
    /// </summary>
    public decimal Box399TotalOutputTax { get; set; }

    /// <summary>
    /// Gets or sets AFC Box 479 total deductible input tax.
    /// </summary>
    public decimal Box479TotalInputTax { get; set; }

    /// <summary>
    /// Gets or sets AFC Box 500 net payable VAT.
    /// </summary>
    public decimal Box500PayableAmount { get; set; }

    /// <summary>
    /// Gets or sets AFC Box 510 net credit VAT.
    /// </summary>
    public decimal Box510CreditAmount { get; set; }

    /// <summary>
    /// Gets or sets the gross turnover including invoiced VAT.
    /// </summary>
    public decimal GrossTurnover { get; set; }

    /// <summary>
    /// Gets or sets the forfeited input VAT under Saldosteuersatz.
    /// </summary>
    public decimal ForfeitedInputVat { get; set; }

    /// <summary>
    /// Gets or sets the Bezugsteuer taxable base (Box 382/383).
    /// </summary>
    public decimal BezugsteuerBase { get; set; }

    /// <summary>
    /// Gets or sets the Bezugsteuer tax amount (Box 382/383).
    /// </summary>
    public decimal BezugsteuerTax { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether accounting quadratura checks passed.
    /// </summary>
    public bool QuadraturaPassed { get; set; }

    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC last-updated timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
