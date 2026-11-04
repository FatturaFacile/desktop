// <copyright file="Invoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Primitives;
using FatturaFacile.Invoicing.Domain.Entities;
using FatturaFacile.Invoicing.Domain.Enums;
using FatturaFacile.Invoicing.Domain.Events;

namespace FatturaFacile.Invoicing.Domain.Aggregates;

/// <summary>
/// Represents an invoice aggregate root supporting multi-currency,
/// multi-jurisdiction (IT SDI and Swiss QR-bill),
/// item line management and settlement tracking.
/// </summary>
public sealed class Invoice : AggregateRoot<Guid>, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private const int CurrencyDecimalPlaces = 2;
    private const int CurrencyCodeLength = 3;
    private const int WholeNumberDecimalPlaces = 0;

    private const decimal DefaultExchangeRate = 1.0m;
    private const decimal ZeroAmount = 0m;
    private const decimal SwissCashRoundingFactor = 20m;

    private readonly List<InvoiceItem> _items = [];

    private Invoice()
    {
    }

    /// <inheritdoc />
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Gets the foreign key identifier of the billing client contact.
    /// </summary>
    public Guid ContactId { get; private set; }

    /// <summary>
    /// Gets the legal format and classification of the invoice.
    /// </summary>
    public InvoiceType InvoiceType { get; private set; }

    /// <summary>
    /// Gets the unique invoice document reference number.
    /// </summary>
    public string InvoiceNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the invoice issue date.
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Gets the optional payment deadline.
    /// </summary>
    public DateOnly? DueDate { get; private set; }

    /// <summary>
    /// Gets the ISO 4217 currency code.
    /// </summary>
    public string Currency { get; private set; } = "EUR";

    /// <summary>
    /// Gets the exchange rate multiplier.
    /// </summary>
    public decimal ExchangeRate { get; private set; } = DefaultExchangeRate;

    /// <summary>
    /// Gets the net taxable amount.
    /// </summary>
    public decimal TotalNet { get; private set; }

    /// <summary>
    /// Gets the tax amount.
    /// </summary>
    public decimal TotalTax { get; private set; }

    /// <summary>
    /// Gets the total amount payable.
    /// </summary>
    public decimal TotalGross { get; private set; }

    /// <summary>
    /// Gets the rounding adjustment.
    /// </summary>
    public decimal RoundingAmount { get; private set; }

    /// <summary>
    /// Gets the amount paid.
    /// </summary>
    public decimal TotalPaid { get; private set; }

    /// <summary>
    /// Gets the payment status.
    /// </summary>
    public PaymentStatus PaymentStatus { get; private set; }

    /// <summary>
    /// Gets the fiscal jurisdiction.
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets a value indicating whether split payment applies.
    /// </summary>
    public bool SplitPayment { get; private set; }

    /// <summary>
    /// Gets a value indicating whether reverse charge applies.
    /// </summary>
    public bool ReverseCharge { get; private set; }

    /// <summary>
    /// Gets invoice notes.
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
    /// Gets the invoice items.
    /// </summary>
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <param name="request">The invoice create request.</param>
    /// <returns>A new <see cref="Invoice" /> instance.</returns>
    public static Invoice Create(
        InvoiceCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.TenantId == Guid.Empty
            ? throw new ArgumentException("Tenant ID is required.", nameof(request.TenantId))
            : request.ContactId == Guid.Empty
            ? throw new ArgumentException("Contact ID is required.", nameof(request.ContactId))
            : string.IsNullOrWhiteSpace(request.InvoiceNumber)
            ? throw new ArgumentException("Invoice number is required.", nameof(request.InvoiceNumber))
            : string.IsNullOrWhiteSpace(request.Currency) ||
            request.Currency.Length != CurrencyCodeLength
            ? throw new ArgumentException("Currency must be 3 letters.", nameof(request.Currency))
            : new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ContactId = request.ContactId,
            InvoiceType = request.InvoiceType,
            InvoiceNumber = request.InvoiceNumber.Trim(),
            Date = request.Date,
            DueDate = request.DueDate,
            Currency = request.Currency.ToUpperInvariant(),
            Jurisdiction = request.Jurisdiction,
            SplitPayment = request.SplitPayment,
            ReverseCharge = request.ReverseCharge,
            Notes = request.Notes?.Trim(),
            ExchangeRate = request.ExchangeRate > ZeroAmount
                ? request.ExchangeRate
                : DefaultExchangeRate,
            PaymentStatus = PaymentStatus.Unpaid,
            TotalPaid = ZeroAmount,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Adds an invoice item.
    /// </summary>
    /// <param name="productId">The optional product identifier.</param>
    /// <param name="description">The item description.</param>
    /// <param name="quantity">The item quantity.</param>
    /// <param name="unitPrice">The item unit price.</param>
    /// <param name="taxRate">The item tax rate.</param>
    /// <param name="discountPercentage">The item discount percentage.</param>
    public void AddItem(
        Guid? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate,
        decimal discountPercentage = ZeroAmount)
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
    /// Recalculates all totals.
    /// </summary>
    public void RecalculateTotals()
    {
        TotalNet = decimal.Round(
            _items.Sum(i => i.LineTotal),
            CurrencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        TotalTax = decimal.Round(
            _items.Sum(i => i.TaxAmount),
            CurrencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var unroundedGross = TotalNet + TotalTax;

        if (string.Equals(
            Currency,
            "CHF",
            StringComparison.OrdinalIgnoreCase))
        {
            TotalGross =
                decimal.Round(
                    unroundedGross * SwissCashRoundingFactor,
                    WholeNumberDecimalPlaces,
                    MidpointRounding.AwayFromZero)
                / SwissCashRoundingFactor;

            RoundingAmount = decimal.Round(
                TotalGross - unroundedGross,
                CurrencyDecimalPlaces,
                MidpointRounding.AwayFromZero);
        }
        else
        {
            TotalGross = unroundedGross;
            RoundingAmount = ZeroAmount;
        }

        RaiseDomainEvent(
            new InvoiceCreatedDomainEvent(
                Id,
                TenantId,
                InvoiceNumber,
                TotalGross,
                Currency));
    }

    /// <summary>
    /// Registers a payment.
    /// </summary>
    public void RegisterPayment(
        decimal amount,
        DateTimeOffset paidAt)
    {
        if (amount <= ZeroAmount)
        {
            throw new ArgumentException(
                "Payment amount must be greater than zero.",
                nameof(amount));
        }

        TotalPaid = decimal.Round(
            TotalPaid + amount,
            CurrencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var oldStatus = PaymentStatus.ToString();

        if (TotalPaid >= TotalGross)
        {
            PaymentStatus = PaymentStatus.Paid;
        }
        else
        {
            PaymentStatus = PaymentStatus.PartiallyPaid;
        }

        RaiseDomainEvent(
            new InvoicePaidDomainEvent(
                Id,
                TenantId,
                amount,
                paidAt));

        RaiseDomainEvent(
            new InvoiceStatusChangedDomainEvent(
                Id,
                oldStatus,
                PaymentStatus.ToString()));
    }

    /// <summary>
    /// Cancels the invoice.
    /// </summary>
    public void Cancel(string reason)
    {
        if (PaymentStatus == PaymentStatus.Paid)
        {
            throw new InvalidOperationException(
                "Cannot cancel an invoice that is already fully paid. Issue a Credit Note instead.");
        }

        var oldStatus = PaymentStatus.ToString();

        PaymentStatus = PaymentStatus.Cancelled;

        Notes = string.IsNullOrWhiteSpace(Notes)
            ? $"Cancelled: {reason}"
            : $"{Notes} | Cancelled: {reason}";

        RaiseDomainEvent(
            new InvoiceStatusChangedDomainEvent(
                Id,
                oldStatus,
                PaymentStatus.Cancelled.ToString()));
    }
}
