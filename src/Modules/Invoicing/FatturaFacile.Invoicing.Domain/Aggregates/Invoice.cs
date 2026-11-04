// <copyright file="Invoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Primitives;
using FatturaFacile.Invoicing.Domain.Entities;
using FatturaFacile.Invoicing.Domain.Enums;
using FatturaFacile.Invoicing.Domain.Events;

namespace FatturaFacile.Invoicing.Domain.Aggregates;

/// <summary>
/// Represents an invoice aggregate root supporting multi-currency, multi-jurisdiction (IT SDI and Swiss QR-bill),
/// item line management, and settlement tracking.
/// </summary>
public sealed class Invoice : AggregateRoot<Guid>, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<InvoiceItem> _items = [];

    private Invoice() { }

    /// <inheritdoc />
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Gets the foreign key identifier of the billing client contact.
    /// </summary>
    public Guid ContactId { get; private set; }

    /// <summary>
    /// Gets the legal format and classification of the invoice (Standard, SdiElectronic, ChQrBill, etc.).
    /// </summary>
    public InvoiceType InvoiceType { get; private set; }

    /// <summary>
    /// Gets the unique invoice document reference number within the tenant (e.g., "INV-2026-0001").
    /// </summary>
    public string InvoiceNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date of invoice issuance.
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Gets the optional payment deadline date.
    /// </summary>
    public DateOnly? DueDate { get; private set; }

    /// <summary>
    /// Gets the 3-letter ISO 4217 billing currency code (e.g., "EUR", "CHF").
    /// </summary>
    public string Currency { get; private set; } = "EUR";

    /// <summary>
    /// Gets the exchange rate multiplier to the base accounting currency.
    /// </summary>
    public decimal ExchangeRate { get; private set; } = 1.0m;

    /// <summary>
    /// Gets the net taxable total across all line items before taxes.
    /// </summary>
    public decimal TotalNet { get; private set; }

    /// <summary>
    /// Gets the cumulative value-added tax amount.
    /// </summary>
    public decimal TotalTax { get; private set; }

    /// <summary>
    /// Gets the gross total payable amount after applying tax and Swiss 0.05 rounding when applicable.
    /// </summary>
    public decimal TotalGross { get; private set; }

    /// <summary>
    /// Gets the rounding adjustment difference (particularly relevant for Swiss CHF 0.05 cash rounding).
    /// </summary>
    public decimal RoundingAmount { get; private set; }

    /// <summary>
    /// Gets the total cumulative monetary amount received towards this invoice.
    /// </summary>
    public decimal TotalPaid { get; private set; }

    /// <summary>
    /// Gets the settlement lifecycle status of the invoice.
    /// </summary>
    public PaymentStatus PaymentStatus { get; private set; }

    /// <summary>
    /// Gets the fiscal jurisdiction (Italian IT or Swiss CH).
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets a value indicating whether Italian public administration Split Payment (Scissione dei Pagamenti) applies.
    /// </summary>
    public bool SplitPayment { get; private set; }

    /// <summary>
    /// Gets a value indicating whether Reverse Charge (Inversione Contabile) tax rules apply.
    /// </summary>
    public bool ReverseCharge { get; private set; }

    /// <summary>
    /// Gets optional administrative notes or payment instructions.
    /// </summary>
    public string? Notes { get; private set; }

    /// <inheritdoc />
    public bool IsDeleted { get; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; private set; }

    /// <inheritdoc />
    public string? CreatedBy { get; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <inheritdoc />
    public string? UpdatedBy { get; }

    /// <summary>
    /// Gets the read-only collection of line items comprising this invoice.
    /// </summary>
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Factory method to construct a new <see cref="Invoice"/> aggregate root.
    /// </summary>
    /// <param name="tenantId">The owning tenant identifier.</param>
    /// <param name="contactId">The customer contact identifier.</param>
    /// <param name="invoiceType">The invoice type format.</param>
    /// <param name="invoiceNumber">The unique invoice document number.</param>
    /// <param name="date">The invoice issue date.</param>
    /// <param name="dueDate">The optional payment due date.</param>
    /// <param name="currency">The 3-letter ISO currency code.</param>
    /// <param name="jurisdiction">The fiscal jurisdiction.</param>
    /// <param name="splitPayment">Whether Italian split payment applies.</param>
    /// <param name="reverseCharge">Whether reverse charge applies.</param>
    /// <param name="notes">Optional notes or payment terms.</param>
    /// <param name="exchangeRate">The currency exchange rate (defaults to 1.0).</param>
    /// <returns>A new <see cref="Invoice"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when validation constraints are not met.</exception>
    public static Invoice Create(
        Guid tenantId,
        Guid contactId,
        InvoiceType invoiceType,
        string invoiceNumber,
        DateOnly date,
        DateOnly? dueDate,
        string currency,
        Jurisdiction jurisdiction,
        bool splitPayment = false,
        bool reverseCharge = false,
        string? notes = null,
        decimal exchangeRate = 1.0m)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID is required.", nameof(tenantId));
        }

        if (contactId == Guid.Empty)
        {
            throw new ArgumentException("Contact ID is required.", nameof(contactId));
        }

        if (string.IsNullOrWhiteSpace(invoiceNumber))
        {
            throw new ArgumentException("Invoice number is required.", nameof(invoiceNumber));
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new ArgumentException("Currency must be 3 letters.", nameof(currency));
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContactId = contactId,
            InvoiceType = invoiceType,
            InvoiceNumber = invoiceNumber.Trim(),
            Date = date,
            DueDate = dueDate,
            Currency = currency.ToUpperInvariant(),
            Jurisdiction = jurisdiction,
            SplitPayment = splitPayment,
            ReverseCharge = reverseCharge,
            Notes = notes?.Trim(),
            ExchangeRate = exchangeRate > 0 ? exchangeRate : 1.0m,
            PaymentStatus = PaymentStatus.Unpaid,
            TotalPaid = 0m,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        return invoice;
    }

    /// <summary>
    /// Adds a line item to the invoice and recalculates financial totals.
    /// </summary>
    /// <param name="productId">The optional product catalog identifier.</param>
    /// <param name="description">The item description.</param>
    /// <param name="quantity">The item quantity.</param>
    /// <param name="unitPrice">The net unit price.</param>
    /// <param name="taxRate">The VAT percentage rate.</param>
    /// <param name="discountPercentage">The discount percentage (default: 0).</param>
    public void AddItem(
        Guid? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate,
        decimal discountPercentage = 0m)
    {
        var item = InvoiceItem.Create(
            TenantId,
            productId,
            description,
            quantity,
            unitPrice,
            taxRate,
            discountPercentage);

        _items.Add(item);
        RecalculateTotals();
    }

    /// <summary>
    /// Recalculates net, tax, gross totals, and applies Swiss 0.05 rounding when billing in CHF.
    /// </summary>
    public void RecalculateTotals()
    {
        TotalNet = decimal.Round(_items.Sum(i => i.LineTotal), 2, MidpointRounding.AwayFromZero);
        TotalTax = decimal.Round(_items.Sum(i => i.TaxAmount), 2, MidpointRounding.AwayFromZero);

        var unroundedGross = TotalNet + TotalTax;

        // Swiss CHF 0.05 rounding rule (round to nearest 0.05 CHF)
        if (string.Equals(Currency, "CHF", StringComparison.OrdinalIgnoreCase))
        {
            TotalGross = decimal.Round(unroundedGross * 20m, 0, MidpointRounding.AwayFromZero) / 20m;
            RoundingAmount = decimal.Round(TotalGross - unroundedGross, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            TotalGross = unroundedGross;
            RoundingAmount = 0m;
        }

        RaiseDomainEvent(new InvoiceCreatedDomainEvent(Id, TenantId, InvoiceNumber, TotalGross, Currency));
    }

    /// <summary>
    /// Records a payment against this invoice and transitions its payment status accordingly.
    /// </summary>
    /// <param name="amount">The payment amount received, must be positive.</param>
    /// <param name="paidAt">The UTC timestamp of the payment transaction.</param>
    /// <exception cref="ArgumentException">Thrown when amount is zero or negative.</exception>
    public void RegisterPayment(decimal amount, DateTimeOffset paidAt)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));
        }

        TotalPaid = decimal.Round(TotalPaid + amount, 2, MidpointRounding.AwayFromZero);

        var oldStatus = PaymentStatus.ToString();
        if (TotalPaid >= TotalGross)
        {
            PaymentStatus = PaymentStatus.Paid;
        }
        else
        {
            PaymentStatus = PaymentStatus.PartiallyPaid;
        }

        RaiseDomainEvent(new InvoicePaidDomainEvent(Id, TenantId, amount, paidAt));
        RaiseDomainEvent(new InvoiceStatusChangedDomainEvent(Id, oldStatus, PaymentStatus.ToString()));
    }

    /// <summary>
    /// Voids or cancels the invoice, recording the cancellation reason.
    /// </summary>
    /// <param name="reason">The reason for cancellation.</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting to cancel an already paid invoice.</exception>
    public void Cancel(string reason)
    {
        if (PaymentStatus == PaymentStatus.Paid)
        {
            throw new InvalidOperationException("Cannot cancel an invoice that is already fully paid. Issue a Credit Note instead.");
        }

        var oldStatus = PaymentStatus.ToString();
        PaymentStatus = PaymentStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? $"Cancelled: {reason}" : $"{Notes} | Cancelled: {reason}";

        RaiseDomainEvent(new InvoiceStatusChangedDomainEvent(Id, oldStatus, PaymentStatus.Cancelled.ToString()));
    }
}
