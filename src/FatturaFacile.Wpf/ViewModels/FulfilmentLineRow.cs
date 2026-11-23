// <copyright file="FulfilmentLineRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A purchase order line with its dual unit of measure and cascading discount.
/// </summary>
/// <param name="Id">The line.</param>
/// <param name="LineNumber">The line number.</param>
/// <param name="Description">The description.</param>
/// <param name="QuantityText">The quantity in the purchase unit.</param>
/// <param name="BaseQuantityText">The quantity in base units with the conversion.</param>
/// <param name="ReceivedText">The received quantity.</param>
/// <param name="UnitPriceText">The list unit price.</param>
/// <param name="CascadeText">The cascading discount steps.</param>
/// <param name="EffectiveDiscountText">The effective discount.</param>
/// <param name="PromisedText">The promised delivery date.</param>
/// <param name="NetText">The net amount.</param>
internal sealed record FulfilmentLineRow(
    Guid Id,
    int LineNumber,
    string Description,
    string QuantityText,
    string BaseQuantityText,
    string ReceivedText,
    string UnitPriceText,
    string CascadeText,
    string EffectiveDiscountText,
    string PromisedText,
    string NetText);
