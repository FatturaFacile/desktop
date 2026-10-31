// <copyright file="Invoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a fiscal billing document (e.g. invoice, proforma, credit note) issued by a tenant organization.
/// </summary>
public class Invoice : EntityBase, IMustHaveTenant, ISoftDeletable
{
    /// <inheritdoc />
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the customer or recipient contact.
    /// </summary>
    public Guid ContactId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the customer contact recipient.
    /// </summary>
    public Contact? Contact { get; set; }

    /// <summary>
    /// Gets or sets the document type classification (e.g. Invoice, CreditNote, Proforma).
    /// </summary>
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Invoice;

    /// <summary>
    /// Gets or sets the formatted alphanumeric invoice document number (e.g. "INV-2026-0001").
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fiscal calendar year to which the document sequence belongs.
    /// </summary>
    public int FiscalYear { get; set; }

    /// <summary>
    /// Gets or sets the strictly increasing sequential integer number within the fiscal year sequence.
    /// </summary>
    public int SequentialNumber { get; set; }

    /// <summary>
    /// Gets or sets the formal document issue date.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the payment maturity or due date.
    /// </summary>
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);

    /// <summary>
    /// Gets or sets the ISO 4217 three-letter currency code (e.g. "CHF", "EUR").
    /// </summary>
    public string Currency { get; set; } = "CHF";

    /// <summary>
    /// Gets or sets the foreign currency conversion exchange rate relative to the tenant's base accounting currency.
    /// </summary>
    public decimal ExchangeRate { get; set; } = 1.0m;

    /// <summary>
    /// Gets or sets the aggregate net monetary amount of all invoice lines before taxes and after line discounts.
    /// </summary>
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Gets or sets the total cumulative VAT amount across all invoice line items.
    /// </summary>
    public decimal TotalTax { get; set; }

    /// <summary>
    /// Gets or sets the final gross total amount payable by the client, including VAT and rounding adjustments.
    /// </summary>
    public decimal TotalGross { get; set; }

    /// <summary>
    /// Gets or sets the currency-specific fractional rounding adjustment (e.g. Swiss 0.05 CHF rounding).
    /// </summary>
    public decimal RoundingAmount { get; set; }

    /// <summary>
    /// Gets or sets the total cumulative payments collected against this invoice.
    /// </summary>
    public decimal TotalPaid { get; set; }

    /// <summary>
    /// Gets or sets the current settlement status of the invoice.
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    /// <summary>
    /// Gets or sets optional free-form remarks, payment terms, or legal footers displayed on the document.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the agreed payment channel (e.g. "BankTransfer", "CreditCard", "Cash").
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the name of the financial institution for remittance.
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// Gets or sets the International Bank Account Number (IBAN) for payment transfer.
    /// </summary>
    public string? BankIban { get; set; }

    /// <summary>
    /// Gets or sets the specialized Swiss QR-IBAN used for structured QR-bill references.
    /// </summary>
    public string? QrIban { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Italian Public Administration split payment rules apply (Scissione dei pagamenti).
    /// </summary>
    public bool SplitPayment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether VAT reverse charge mechanisms apply (Inversione contabile).
    /// </summary>
    public bool ReverseCharge { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the owning tenant organization.
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Gets or sets the collection of itemized billing line items included in this invoice.
    /// </summary>
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
