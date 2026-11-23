// <copyright file="FulfilmentMovementRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A virtual stock movement of the selected dropshipment.
/// </summary>
/// <param name="DirectionText">The direction.</param>
/// <param name="QuantityText">The quantity in base units.</param>
/// <param name="ValueText">The valued amount.</param>
/// <param name="WhenText">When it moved.</param>
internal sealed record FulfilmentMovementRow(
    string DirectionText,
    string QuantityText,
    string ValueText,
    string WhenText);
