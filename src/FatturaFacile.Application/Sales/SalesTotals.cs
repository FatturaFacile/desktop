// <copyright file="SalesTotals.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Sales;

/// <summary>
/// Computed totals of a sales document.
/// </summary>
/// <param name="Subtotal">The sum of quantity times price before discounts.</param>
/// <param name="DiscountAmount">The total line discount.</param>
/// <param name="TotalNet">The net total after discounts.</param>
/// <param name="TotalTax">The VAT total, computed per rate on the group total.</param>
/// <param name="TotalGross">The rounded grand total.</param>
/// <param name="LineTotals">The taxable total of every line, in input order.</param>
public sealed record SalesTotals(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TotalNet,
    decimal TotalTax,
    decimal TotalGross,
    IReadOnlyList<decimal> LineTotals);
