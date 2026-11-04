// <copyright file="InvoiceDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Invoicing.Application.DTOs;

/// <summary>
/// Data transfer object representing the full details of an invoice.
/// </summary>
public sealed record InvoiceDto
{
    /// <summary>
    /// Gets the unique invoice identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Gets the customer contact identifier.
    /// </summary>
    public Guid ContactId { get; init; }

    /// <summary>
    /// Gets the invoice format and type name string.
    /// </summary>
    public string InvoiceType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique document reference number.
    /// </summary>
    public string InvoiceNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets the date of invoice issuance.
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Gets the optional payment deadline date.
    /// </summary>
    public DateOnly? DueDate { get; init; }

    /// <summary>
    /// Gets the 3-letter ISO currency code.
    /// </summary>
    public string Currency { get; init; } = "EUR";

    /// <summary>
    /// Gets the currency exchange rate multiplier to base currency.
    /// </summary>
    public decimal ExchangeRate { get; init; } = 1.0m;

    /// <summary>
    /// Gets the subtotal net taxable amount across all line items.
    /// </summary>
    public decimal TotalNet { get; init; }

    /// <summary>
    /// Gets the total value-added tax amount.
    /// </summary>
    public decimal TotalTax { get; init; }

    /// <summary>
    /// Gets the gross total payable after rounding.
    /// </summary>
    public decimal TotalGross { get; init; }

    /// <summary>
    /// Gets the rounding adjustment difference (Swiss CHF 0.05 rule).
    /// </summary>
    public decimal RoundingAmount { get; init; }

    /// <summary>
    /// Gets the cumulative amount paid towards this invoice.
    /// </summary>
    public decimal TotalPaid { get; init; }

    /// <summary>
    /// Gets the current payment status name.
    /// </summary>
    public string PaymentStatus { get; init; } = string.Empty;

    /// <summary>
    /// Gets the fiscal jurisdiction code (IT or CH).
    /// </summary>
    public string Jurisdiction { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether Italian split payment applies.
    /// </summary>
    public bool SplitPayment { get; init; }

    /// <summary>
    /// Gets a value indicating whether Italian reverse charge applies.
    /// </summary>
    public bool ReverseCharge { get; init; }

    /// <summary>
    /// Gets any notes or terms recorded on the invoice.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Gets the collection of line items on the invoice.
    /// </summary>
    public IReadOnlyCollection<InvoiceItemDto> Items { get; init; } = [];
}
