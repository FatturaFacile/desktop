// <copyright file="PricingPreviewRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Inputs of the pricing preview of a purchase line.
/// </summary>
/// <param name="Quantity">The quantity in the purchase unit.</param>
/// <param name="UnitPrice">The list unit price.</param>
/// <param name="DiscountCascade">The cascading discount steps in percent.</param>
/// <param name="UomConversionFactor">The base units per purchase unit.</param>
/// <param name="ExchangeRate">The exchange rate to company currency.</param>
/// <param name="TaxRate">The tax rate in percent.</param>
public sealed record PricingPreviewRequest(
    decimal Quantity,
    decimal UnitPrice,
    IReadOnlyList<decimal> DiscountCascade,
    decimal? UomConversionFactor,
    decimal? ExchangeRate,
    decimal? TaxRate);
