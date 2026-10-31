// <copyright file="DocumentCalculationResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Services;

/// <summary>
/// Represents the comprehensive calculation result for an entire invoice or billing document,
/// containing itemized line results, aggregate net subtotal, aggregate tax, gross total, and currency rounding adjustments.
/// </summary>
/// <param name="Items">The collection of computed document line item results.</param>
/// <param name="TotalNet">The sum of net amounts across all line items after line discounts.</param>
/// <param name="TotalTax">The total aggregated tax amount across all line items.</param>
/// <param name="TotalGross">The final invoice gross total payable, including rounding adjustments if applicable.</param>
/// <param name="RoundingAmount">The currency-specific rounding differential applied (e.g., Swiss QR-Bill 0.05 CHF rounding).</param>
public record DocumentCalculationResult(
    IReadOnlyList<DocumentLineResult> Items,
    decimal TotalNet,
    decimal TotalTax,
    decimal TotalGross,
    decimal RoundingAmount);
