// <copyright file="InvoiceItemDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Invoicing.Application.DTOs;

/// <summary>
/// Data transfer object representing a single invoice line item.
/// </summary>
public sealed record InvoiceItemDto
{
    /// <summary>
    /// Gets the line item identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the optional product catalog identifier.
    /// </summary>
    public Guid? ProductId { get; init; }

    /// <summary>
    /// Gets the item description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the quantity supplied.
    /// </summary>
    public decimal Quantity { get; init; }

    /// <summary>
    /// Gets the net unit price.
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// Gets the VAT percentage rate.
    /// </summary>
    public decimal TaxRate { get; init; }

    /// <summary>
    /// Gets the discount percentage applied to this line.
    /// </summary>
    public decimal DiscountPercentage { get; init; }

    /// <summary>
    /// Gets the calculated net total after discounts.
    /// </summary>
    public decimal LineTotal { get; init; }

    /// <summary>
    /// Gets the calculated tax amount.
    /// </summary>
    public decimal TaxAmount { get; init; }
}
