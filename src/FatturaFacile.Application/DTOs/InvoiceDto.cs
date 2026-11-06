// <copyright file="InvoiceDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for a commercial invoice including contact summary and line items.
/// </summary>
public sealed class InvoiceDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the mandatory owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key identifier of the customer or supplier contact.
    /// </summary>
    public Guid ContactId { get; set; }

    /// <summary>
    /// Gets or sets the contact display name.
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice type ("standard", "proforma", "credit_note").
    /// </summary>
    public string InvoiceType { get; set; } = "standard";

    /// <summary>
    /// Gets or sets the formatted invoice document number.
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document issue date.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the payment due date.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the currency ISO code ("CHF", "EUR").
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets the exchange rate.
    /// </summary>
    public decimal ExchangeRate { get; set; } = 1.0m;

    /// <summary>
    /// Gets or sets the net taxable subtotal.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Gets or sets the discount amount.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Gets or sets the net total.
    /// </summary>
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Gets or sets the total VAT tax amount.
    /// </summary>
    public decimal TotalTax { get; set; }

    /// <summary>
    /// Gets or sets the grand gross total.
    /// </summary>
    public decimal TotalGross { get; set; }

    /// <summary>
    /// Gets or sets the Swiss 0.05 rounding adjustment amount.
    /// </summary>
    public decimal RoundingAmount { get; set; }

    /// <summary>
    /// Gets or sets the payment status ("unpaid", "partially_paid", "paid").
    /// </summary>
    public string PaymentStatus { get; set; } = "unpaid";

    /// <summary>
    /// Gets or sets the legal jurisdiction ("CH" or "IT").
    /// </summary>
    public string Jurisdiction { get; set; } = "IT";

    /// <summary>
    /// Gets or sets the workflow status ("draft", "issued", "paid", "cancelled").
    /// </summary>
    public string Status { get; set; } = "draft";

    /// <summary>
    /// Gets or sets a value indicating whether Italian Split Payment is active.
    /// </summary>
    public bool SplitPayment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether withholding tax (Ritenuta d'acconto) is applied.
    /// </summary>
    public bool WithholdingTaxEnabled { get; set; }

    /// <summary>
    /// Gets or sets withholding tax percentage.
    /// </summary>
    public decimal? WithholdingTaxPercent { get; set; }

    /// <summary>
    /// Gets or sets computed withholding tax amount.
    /// </summary>
    public decimal? WithholdingTaxAmount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether stamp duty (bollo virtuale) was applied.
    /// </summary>
    public bool StampDutyApplied { get; set; }

    /// <summary>
    /// Gets or sets stamp duty amount.
    /// </summary>
    public decimal? StampDutyAmount { get; set; }

    /// <summary>
    /// Gets or sets the payment method.
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the payment bank IBAN.
    /// </summary>
    public string? BankIban { get; set; }

    /// <summary>
    /// Gets or sets document notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the line items on this invoice.
    /// </summary>
    public IList<InvoiceItemDto> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the Swiss QR-Bill data if applicable.
    /// </summary>
    public InvoiceQrBillDto? QrBillData { get; set; }

    /// <summary>
    /// Gets or sets the Italian SDI metadata if applicable.
    /// </summary>
    public InvoiceSdiDto? SdiMetadata { get; set; }
}
