// <copyright file="WithholdingLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// One withheld payment of a withholding settlement of the web-api (ritenuta operata), kept offline as the base of the Certificazione Unica.
/// </summary>
public class WithholdingLine : ITenantScopedEntity
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
    /// Gets or sets the withholding settlement of the web-api.
    /// </summary>
    public Guid SettlementServerId { get; set; }

    /// <summary>
    /// Gets or sets the year of the payment.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the month of the payment.
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets the supplier (percipient) contact.
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Gets or sets the number of the invoice paid.
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of the payment.
    /// </summary>
    public DateTime PaidAt { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the causale of the CU (A, M, L, ...).
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the F24 tax code of the withholding.
    /// </summary>
    public string F24Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gross amount paid.
    /// </summary>
    public decimal GrossPaid { get; set; }

    /// <summary>
    /// Gets or sets the taxable amount.
    /// </summary>
    public decimal TaxableAmount { get; set; }

    /// <summary>
    /// Gets or sets the withholding rate.
    /// </summary>
    public decimal RatePercent { get; set; }

    /// <summary>
    /// Gets or sets the amount withheld.
    /// </summary>
    public decimal WithheldAmount { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
