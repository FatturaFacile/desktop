// <copyright file="InvoiceItemDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an individual invoice line item.
/// </summary>
/// <param name="Id">The unique invoice line identifier.</param>
/// <param name="ProductId">The optional product catalog foreign key reference.</param>
/// <param name="Description">The line item commercial description rendered on the printed invoice.</param>
/// <param name="Quantity">The billed product or service quantity.</param>
/// <param name="UnitPrice">The net unit price excluding discounts and tax.</param>
/// <param name="TaxRate">The applicable VAT or tax rate percentage.</param>
/// <param name="DiscountPercentage">The line discount percentage applied against the unit price.</param>
/// <param name="LineTotal">The computed line net total amount after discount.</param>
public record InvoiceItemDto(
    Guid Id,
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountPercentage,
    decimal LineTotal)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceItemDto"/> record with default empty values.
    /// </summary>
    public InvoiceItemDto()
        : this(default, null, string.Empty, 0, 0, 0, 0, 0)
    {
    }
}
