// <copyright file="QuoteDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object of a sales quote.
/// </summary>
public sealed class QuoteDto
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public Guid ContactId { get; set; }

    /// <summary>
    /// Gets or sets the customer display name.
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document number.
    /// </summary>
    public string QuoteNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document date.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets the subtotal.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Gets or sets the discount total.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Gets or sets the net total.
    /// </summary>
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Gets or sets the VAT total.
    /// </summary>
    public decimal TotalTax { get; set; }

    /// <summary>
    /// Gets or sets the grand total.
    /// </summary>
    public decimal TotalGross { get; set; }

    /// <summary>
    /// Gets or sets the workflow status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment terms.
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the validity end date.
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Gets or sets the shipping terms.
    /// </summary>
    public string? ShippingTerms { get; set; }

    /// <summary>
    /// Gets or sets the revision number.
    /// </summary>
    public int RevisionNumber { get; set; }

    /// <summary>
    /// Gets or sets the order created from the quote.
    /// </summary>
    public Guid? ConvertedToOrderId { get; set; }

    /// <summary>
    /// Gets or sets the lines.
    /// </summary>
    public IList<SalesLineDto> Items { get; set; } = [];
}
