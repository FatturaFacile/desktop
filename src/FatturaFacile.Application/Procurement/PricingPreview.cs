// <copyright file="PricingPreview.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// The computed pricing of a purchase line.
/// </summary>
/// <param name="DiscountCascade">The normalised cascade.</param>
/// <param name="EffectiveDiscountPercent">The effective discount in percent.</param>
/// <param name="NetUnitPrice">The unit price after the cascade.</param>
/// <param name="LineNetAmount">The net amount.</param>
/// <param name="LineTaxAmount">The tax amount.</param>
/// <param name="LineTotalAmount">The gross amount.</param>
/// <param name="QuantityBase">The quantity in base units.</param>
/// <param name="NetPricePerBaseUnit">The net price per base unit.</param>
/// <param name="LineNetAmountCompanyCurrency">The net amount in company currency.</param>
public sealed record PricingPreview(
    IReadOnlyList<decimal> DiscountCascade,
    decimal EffectiveDiscountPercent,
    decimal NetUnitPrice,
    decimal LineNetAmount,
    decimal LineTaxAmount,
    decimal LineTotalAmount,
    decimal QuantityBase,
    decimal NetPricePerBaseUnit,
    decimal LineNetAmountCompanyCurrency);
