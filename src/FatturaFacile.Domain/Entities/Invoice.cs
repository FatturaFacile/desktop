// <copyright file="Invoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a commercial sales invoice, credit note, or proforma document.
/// Mandatory tenant-isolated financial document.
/// </summary>
public class Invoice : ITenantScopedEntity
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
    /// Gets or sets the foreign key identifier of the recipient contact.
    /// </summary>
    public Guid ContactId { get; set; }

    /// <summary>
    /// Gets or sets the invoice type classification ("standard", "proforma", "credit_note", "debit_note").
    /// </summary>
    public string InvoiceType { get; set; } = "standard";

    /// <summary>
    /// Gets or sets the unique formatted document number (e.g. "INV-2026-0001" or "2026/001").
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fiscal document issue date.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the payment due date.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the ISO-4217 currency code (CHF, EUR, USD).
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets the exchange rate against functional currency.
    /// </summary>
    public decimal ExchangeRate { get; set; } = 1.0m;

    /// <summary>
    /// Gets or sets the net taxable subtotal before discounts and taxes.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Gets or sets document level discount amount.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Gets or sets the net total after discounts.
    /// </summary>
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Gets or sets the total value added tax amount.
    /// </summary>
    public decimal TotalTax { get; set; }

    /// <summary>
    /// Gets or sets the grand total amount including VAT and adjustments.
    /// </summary>
    public decimal TotalGross { get; set; }

    /// <summary>
    /// Gets or sets the Swiss 0.05 rounding adjustment amount or cash rounding.
    /// </summary>
    public decimal RoundingAmount { get; set; }

    /// <summary>
    /// Gets or sets total settled amount.
    /// </summary>
    public decimal TotalPaid { get; set; }

    /// <summary>
    /// Gets or sets remaining net receivable amount.
    /// </summary>
    public decimal ReceivableAmount { get; set; }

    /// <summary>
    /// Gets or sets the payment settlement status ("unpaid", "partially_paid", "paid").
    /// </summary>
    public string PaymentStatus { get; set; } = "unpaid";

    /// <summary>
    /// Gets or sets the legal jurisdiction ("CH" or "IT").
    /// </summary>
    public string Jurisdiction { get; set; } = "IT";

    /// <summary>
    /// Gets or sets the document workflow status ("draft", "issued", "paid", "cancelled").
    /// </summary>
    public string Status { get; set; } = "draft";

    /// <summary>
    /// Gets or sets a value indicating whether Italian Split Payment (Scissione dei pagamenti PA) applies.
    /// </summary>
    public bool SplitPayment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether reverse charge VAT applies.
    /// </summary>
    public bool ReverseCharge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether withholding tax (Ritenuta d'acconto) is enabled.
    /// </summary>
    public bool WithholdingTaxEnabled { get; set; }

    /// <summary>
    /// Gets or sets the withholding tax percentage (e.g. 20%).
    /// </summary>
    public decimal? WithholdingTaxPercent { get; set; }

    /// <summary>
    /// Gets or sets the computed withholding tax amount deducted from final payment.
    /// </summary>
    public decimal? WithholdingTaxAmount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Italian bollo virtuale / stamp duty is applied.
    /// </summary>
    public bool StampDutyApplied { get; set; }

    /// <summary>
    /// Gets or sets the stamp duty amount (typically 2.00 EUR).
    /// </summary>
    public decimal? StampDutyAmount { get; set; }

    /// <summary>
    /// Gets or sets the fiscal calendar year.
    /// </summary>
    public int? FiscalYear { get; set; }

    /// <summary>
    /// Gets or sets the sequential integer counter within the fiscal year.
    /// </summary>
    public int? SequentialNumber { get; set; }

    /// <summary>
    /// Gets or sets payment method code or description.
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets bank account IBAN printed on the invoice.
    /// </summary>
    public string? BankIban { get; set; }

    /// <summary>
    /// Gets or sets notes or terms displayed on the document.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the invoice was posted to the general ledger.
    /// </summary>
    public DateTime? PostedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the invoice was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the invoice was updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the navigation reference to the recipient contact.
    /// </summary>
    public Contact? Contact { get; set; }

    /// <summary>
    /// Gets or sets the collection of line items on this invoice.
    /// </summary>
    public ICollection<InvoiceItem> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the Swiss QR-bill compliance data record if Swiss jurisdiction.
    /// </summary>
    public InvoiceQrBillData? QrBillData { get; set; }

    /// <summary>
    /// Gets or sets the Italian SDI electronic invoicing metadata if Italian jurisdiction.
    /// </summary>
    public InvoiceSdiMetadata? SdiMetadata { get; set; }
}
