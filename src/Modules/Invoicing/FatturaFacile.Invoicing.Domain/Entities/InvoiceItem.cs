// <copyright file="InvoiceItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Invoicing.Domain.Entities;

/// <summary>
/// Represents a billable item line within an invoice.
/// </summary>
public sealed class InvoiceItem : Entity<Guid>, IHasTenant
{
    private const decimal MinPercentage = 0m;
    private const decimal MaxPercentage = 100m;
    private const int CurrencyDecimalPlaces = 2;

    private InvoiceItem()
    {
    }

    /// <inheritdoc />
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Gets the parent invoice identifier.
    /// </summary>
    public Guid InvoiceId { get; private set; }

    /// <summary>
    /// Gets the product identifier if linked to a catalog item.
    /// </summary>
    public Guid? ProductId { get; private set; }

    /// <summary>
    /// Gets the line description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the item quantity.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// Gets the unit price before taxes and discounts.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Gets the VAT rate percentage.
    /// </summary>
    public decimal TaxRate { get; private set; }

    /// <summary>
    /// Gets the discount percentage.
    /// </summary>
    public decimal DiscountPercentage { get; private set; }

    /// <summary>
    /// Gets the taxable line amount after discounts.
    /// </summary>
    public decimal LineTotal { get; private set; }

    /// <summary>
    /// Gets the calculated tax amount.
    /// </summary>
    public decimal TaxAmount { get; private set; }

    /// <summary>
    /// Creates a new invoice item.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="productId">Optional product identifier.</param>
    /// <param name="description">Line description.</param>
    /// <param name="quantity">Quantity greater than zero.</param>
    /// <param name="unitPrice">Unit price greater than or equal to zero.</param>
    /// <param name="taxRate">VAT rate between 0 and 100.</param>
    /// <param name="discountPercentage">Discount percentage between 0 and 100.</param>
    /// <returns>A new <see cref="InvoiceItem"/> instance.</returns>
    public static InvoiceItem Create(
        Guid tenantId,
        Guid? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate,
        decimal discountPercentage = MinPercentage)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Tenant ID is required.",
                nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Item description cannot be empty.",
                nameof(description));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.",
                nameof(quantity));
        }

        if (unitPrice < 0)
        {
            throw new ArgumentException(
                "Unit price cannot be negative.",
                nameof(unitPrice));
        }

        ValidatePercentage(
            taxRate,
            nameof(taxRate),
            "Tax rate must be between 0 and 100.");

        ValidatePercentage(
            discountPercentage,
            nameof(discountPercentage),
            "Discount percentage must be between 0 and 100.");

        var subtotal = quantity * unitPrice;

        if (discountPercentage > MinPercentage)
        {
            subtotal -= subtotal * (discountPercentage / MaxPercentage);
        }

        var lineTotal = decimal.Round(
            subtotal,
            CurrencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var taxAmount = decimal.Round(
            lineTotal * (taxRate / MaxPercentage),
            CurrencyDecimalPlaces,
            MidpointRounding.AwayFromZero);

        return new InvoiceItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductId = productId,
            Description = description.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            TaxRate = taxRate,
            DiscountPercentage = discountPercentage,
            LineTotal = lineTotal,
            TaxAmount = taxAmount,
        };
    }

    /// <summary>
    /// Validates a percentage value.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <param name="paramName">Parameter name.</param>
    /// <param name="message">Validation message.</param>
    private static void ValidatePercentage(
        decimal value,
        string paramName,
        string message)
    {
        if (value < MinPercentage || value > MaxPercentage)
        {
            throw new ArgumentException(message, paramName);
        }
    }
}
