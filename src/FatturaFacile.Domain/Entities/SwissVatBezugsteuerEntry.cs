// <copyright file="SwissVatBezugsteuerEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Foreign service acquisition subject to Swiss Bezugsteuer (Art. 45 LIVA -> Box 382/383).
/// </summary>
public class SwissVatBezugsteuerEntry : ITenantScopedEntity
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
    /// Gets or sets the foreign supplier name.
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-2 country code of the foreign supplier.
    /// </summary>
    public string SupplierCountry { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice number.
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice date.
    /// </summary>
    public DateOnly InvoiceDate { get; set; }

    /// <summary>
    /// Gets or sets the service category.
    /// </summary>
    public BezugsteuerCategory ServiceCategory { get; set; } = BezugsteuerCategory.OtherCrossBorderService;

    /// <summary>
    /// Gets or sets the service description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the net amount in CHF.
    /// </summary>
    public decimal NetAmountChf { get; set; }

    /// <summary>
    /// Gets or sets the applied Swiss VAT rate percentage.
    /// </summary>
    public decimal VatRatePercent { get; set; } = 8.10m;

    /// <summary>
    /// Gets or sets the computed Bezugsteuer amount in CHF.
    /// </summary>
    public decimal TaxAmountChf { get; set; }

    /// <summary>
    /// Gets or sets the deductible input tax amount in CHF (zero under Saldosteuersatz).
    /// </summary>
    public decimal DeductibleInputTaxChf { get; set; }

    /// <summary>
    /// Gets or sets the AFC box code (382 or 383).
    /// </summary>
    public string AfcBox { get; set; } = "383";

    /// <summary>
    /// Gets or sets the review status.
    /// </summary>
    public BezugsteuerStatus Status { get; set; } = BezugsteuerStatus.Detected;

    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC last-updated timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
