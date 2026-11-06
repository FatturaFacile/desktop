// <copyright file="PurchaseInvoiceLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A line of a supplier invoice with expense account and VAT.
/// </summary>
public class PurchaseInvoiceLine : ITenantScopedEntity
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
    /// Gets or sets the supplier invoice identifier.
    /// </summary>
    public Guid PurchaseInvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expense or asset account.
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    /// Gets or sets the net amount.
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Gets or sets the VAT rate in percent.
    /// </summary>
    public decimal VatRate { get; set; }

    /// <summary>
    /// Gets or sets the VAT amount.
    /// </summary>
    public decimal VatAmount { get; set; }

    /// <summary>
    /// Gets or sets the deductible share of the VAT in percent.
    /// </summary>
    public decimal DeductiblePercent { get; set; } = 100m;

    /// <summary>
    /// Gets or sets the Italian VAT nature (N1-N7) for exempt operations.
    /// </summary>
    public string? VatNature { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
