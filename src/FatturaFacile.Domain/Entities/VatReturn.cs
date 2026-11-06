// <copyright file="VatReturn.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A periodic VAT return: Italian periodic liquidation (LIPE) or Swiss MWST statement (effective method).
/// </summary>
public class VatReturn : ITenantScopedEntity
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
    /// Gets or sets the country (CH or IT).
    /// </summary>
    public string Country { get; set; } = "CH";

    /// <summary>
    /// Gets or sets the first day of the period.
    /// </summary>
    public DateTime PeriodFrom { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the last day of the period.
    /// </summary>
    public DateTime PeriodTo { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the turnover of the period (CH figure 200).
    /// </summary>
    public decimal Turnover { get; set; }

    /// <summary>
    /// Gets or sets the taxable turnover.
    /// </summary>
    public decimal TaxableTurnover { get; set; }

    /// <summary>
    /// Gets or sets the VAT due on sales.
    /// </summary>
    public decimal OutputVat { get; set; }

    /// <summary>
    /// Gets or sets the deductible input VAT.
    /// </summary>
    public decimal InputVat { get; set; }

    /// <summary>
    /// Gets or sets the credit carried over from the previous period.
    /// </summary>
    public decimal PreviousCredit { get; set; }

    /// <summary>
    /// Gets or sets the interest due (Italian quarterly liquidation, 1 %).
    /// </summary>
    public decimal Interest { get; set; }

    /// <summary>
    /// Gets or sets the amount payable (negative = credit).
    /// </summary>
    public decimal Payable { get; set; }

    /// <summary>
    /// Gets or sets the status: Draft or Final.
    /// </summary>
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// Gets or sets the computation details by rate.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
