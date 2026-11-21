// <copyright file="Quote.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a sales quote (preventivo / Offerte) sent to a customer.
/// </summary>
public class Quote : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the mandatory owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the customer contact identifier.
    /// </summary>
    public Guid ContactId { get; set; }

    /// <summary>
    /// Gets or sets the formatted quote number (for example "QT-2026-0001").
    /// </summary>
    public string QuoteNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document date.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the date until which the quote is valid.
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Gets or sets the ISO-4217 currency code.
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets the subtotal before discounts and taxes.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Gets or sets the total line discount amount.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Gets or sets the net total after discounts.
    /// </summary>
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Gets or sets the total VAT amount.
    /// </summary>
    public decimal TotalTax { get; set; }

    /// <summary>
    /// Gets or sets the grand total including VAT.
    /// </summary>
    public decimal TotalGross { get; set; }

    /// <summary>
    /// Gets or sets the workflow status.
    /// </summary>
    public string Status { get; set; } = "draft";

    /// <summary>
    /// Gets or sets the payment terms.
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Gets or sets the notes shown on the document.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation reference to the customer.
    /// </summary>
    public Contact? Contact { get; set; }

    /// <summary>
    /// Gets or sets the shipping terms.
    /// </summary>
    public string? ShippingTerms { get; set; }

    /// <summary>
    /// Gets or sets the revision number.
    /// </summary>
    public int RevisionNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the internal approval timestamp.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the sales order created from this quote.
    /// </summary>
    public Guid? ConvertedToOrderId { get; set; }

    /// <summary>
    /// Gets or sets the lines of this quote.
    /// </summary>
    public ICollection<QuoteItem> Items { get; set; } = [];
}
