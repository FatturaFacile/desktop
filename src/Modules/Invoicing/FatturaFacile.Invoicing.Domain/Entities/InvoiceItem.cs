// <copyright file="InvoiceItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Invoicing.Domain.Entities;

/// <summary>
/// Represents a billable item line within an <see cref="Aggregates.Invoice"/>.
/// </summary>
public sealed class InvoiceItem : Entity<Guid>, IHasTenant
{
    private InvoiceItem() { }

    /// <inheritdoc />
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Gets the foreign key identifier of the parent invoice.
    /// </summary>
    public Guid InvoiceId { get; }

    /// <summary>
    /// Gets the optional product or service catalogue identifier.
    /// </summary>
    public Guid? ProductId { get; private set; }

    /// <summary>
    /// Gets the descriptive narrative of the supplied goods or services.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the quantity of goods or units supplied.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// Gets the net price per unit before taxes and discounts.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Gets the applicable value-added tax (VAT) percentage rate (e.g., 22.0 or 8.1).
    /// </summary>
    public decimal TaxRate { get; private set; }

    /// <summary>
    /// Gets the line item discount percentage (0 to 100).
    /// </summary>
    public decimal DiscountPercentage { get; private set; }

    /// <summary>
    /// Gets the calculated net total for the line after applying discounts.
    /// </summary>
    public decimal LineTotal { get; private set; }

    /// <summary>
    /// Gets the calculated VAT/tax amount for the line item.
    /// </summary>
    public decimal TaxAmount { get; private set; }

    /// <summary>
    /// Factory method to construct a validated <see cref="InvoiceItem"/> with computed totals and taxes.
    /// </summary>
    /// <param name="tenantId">The owning tenant identifier.</param>
    /// <param name="productId">The optional catalogue product identifier.</param>
    /// <param name="description">The item description.</param>
    /// <param name="quantity">The item quantity, must be positive.</param>
    /// <param name="unitPrice">The net unit price, must be non-negative.</param>
    /// <param name="taxRate">The VAT percentage rate between 0 and 100.</param>
    /// <param name="discountPercentage">The discount percentage between 0 and 100 (default: 0).</param>
    /// <returns>A new <see cref="InvoiceItem"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when validation constraints are violated.</exception>
    public static InvoiceItem Create(
        Guid tenantId,
        Guid? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate,
        decimal discountPercentage = 0m)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Item description cannot be empty.", nameof(description));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (unitPrice < 0)
        {
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        }

        if (taxRate < 0 || taxRate > 100)
        {
            throw new ArgumentException("Tax rate must be between 0 and 100.", nameof(taxRate));
        }

        if (discountPercentage < 0 || discountPercentage > 100)
        {
            throw new ArgumentException("Discount percentage must be between 0 and 100.", nameof(discountPercentage));
        }

        var subtotal = quantity * unitPrice;
        if (discountPercentage > 0)
        {
            subtotal -= (subtotal * (discountPercentage / 100m));
        }
        var lineTotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero);
        var taxAmount = decimal.Round(lineTotal * (taxRate / 100m), 2, MidpointRounding.AwayFromZero);

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
}
