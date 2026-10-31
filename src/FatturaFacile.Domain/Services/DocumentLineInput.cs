// <copyright file="DocumentLineInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Services;

/// <summary>
/// Represents the raw input data for a single document line item prior to tax and subtotal computation.
/// </summary>
/// <param name="ProductId">The optional unique identifier of the catalog product linked to this line item.</param>
/// <param name="Description">The textual description of the billed good or service.</param>
/// <param name="Quantity">The quantity of units billed for this line item.</param>
/// <param name="UnitPrice">The net unit price before line discounts and tax calculations.</param>
/// <param name="TaxRate">The applicable VAT/tax percentage rate (e.g., 22.0 for 22%).</param>
/// <param name="DiscountPercentage">The percentage discount applied directly to the line item net amount.</param>
public record DocumentLineInput(
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountPercentage);
