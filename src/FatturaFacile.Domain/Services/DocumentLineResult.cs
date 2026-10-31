// <copyright file="DocumentLineResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Services;

/// <summary>
/// Represents the calculated financial totals and applied taxes for an individual document line item.
/// </summary>
/// <param name="ProductId">The optional unique identifier of the catalog product linked to this line item.</param>
/// <param name="Description">The textual description of the billed good or service.</param>
/// <param name="Quantity">The quantity of units billed for this line item.</param>
/// <param name="UnitPrice">The net unit price before discounts and tax.</param>
/// <param name="TaxRate">The applied VAT/tax percentage rate.</param>
/// <param name="DiscountPercentage">The applied percentage discount.</param>
/// <param name="LineTotal">The computed net subtotal for the line after discounts and standard rounding.</param>
/// <param name="LineTax">The computed tax amount for this specific line item.</param>
public record DocumentLineResult(
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountPercentage,
    decimal LineTotal,
    decimal LineTax);
