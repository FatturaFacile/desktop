// <copyright file="Invoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a fiscal billing document
/// (invoice, proforma invoice, credit note, etc.).
/// </summary>
public class Invoice : EntityBase, IMustHaveTenant, ISoftDeletable
{
    /// <summary>
    /// Default payment term in days.
    /// </summary>
    private const int DefaultPaymentTermDays = 30;

    /// <summary>
    /// Default exchange rate.
    /// </summary>
    private const decimal DefaultExchangeRate = 1m;

    /// <summary>
    /// Default currency.
    /// </summary>
    private const string DefaultCurrency = "CHF";

    /// <inheritdoc />
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique customer identifier.
    /// </summary>
    public Guid ContactId { get; set; }

    /// <summary>
    /// Gets or sets the customer contact.
    /// </summary>
    public Contact? Contact { get; set; }

    /// <summary>
    /// Gets or sets the invoice type.
    /// </summary>
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Invoice;

    /// <summary>
    /// Gets or sets the invoice number.
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fiscal year.
    /// </summary>
    public int FiscalYear { get; set; }

    /// <summary>
    /// Gets or sets the sequential number.
    /// </summary>
    public int SequentialNumber { get; set; }

    /// <summary>
    /// Gets or sets the issue date.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the invoice due date.
    /// </summary>
    public DateTime DueDate { get; set; } =
        DateTime.Today.AddDays(DefaultPaymentTermDays);

    /// <summary>
    /// Gets or sets the ISO 4217 currency code.
    /// </summary>
    public string Currency { get; set; } = DefaultCurrency;

    /// <summary>
    /// Gets or sets the exchange rate.
    /// </summary>
    public decimal ExchangeRate { get; set; } = DefaultExchangeRate;

    /// <summary>
    /// Gets or sets the net total.
    /// </summary>
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Gets or sets the VAT total.
    /// </summary>
    public decimal TotalTax { get; set; }

    /// <summary>
    /// Gets or sets the gross total.
    /// </summary>
    public decimal TotalGross { get; set; }

    /// <summary>
    /// Gets or sets the rounding amount.
    /// </summary>
    public decimal RoundingAmount { get; set; }

    /// <summary>
    /// Gets or sets the paid amount.
    /// </summary>
    public decimal TotalPaid { get; set; }

    /// <summary>
    /// Gets or sets the payment status.
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; } =
        PaymentStatus.Unpaid;

    /// <summary>
    /// Gets or sets additional notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the payment method.
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the bank name.
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// Gets or sets the payment IBAN.
    /// </summary>
    public string? BankIban { get; set; }

    /// <summary>
    /// Gets or sets the Swiss QR-IBAN.
    /// </summary>
    public string? QrIban { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether split payment applies.
    /// </summary>
    public bool SplitPayment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether reverse charge applies.
    /// </summary>
    public bool ReverseCharge { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the owning tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Gets or sets the invoice line items.
    /// </summary>
    public ICollection<InvoiceItem> Items { get; set; } =
        [];
}
