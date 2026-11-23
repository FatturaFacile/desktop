// <copyright file="FulfilmentConfirmationRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A supplier confirmation of the selected order.
/// </summary>
/// <param name="Number">The confirmation number.</param>
/// <param name="DateText">The date.</param>
/// <param name="VarianceText">The variance summary.</param>
/// <param name="ReviewText">Whether it needs review.</param>
/// <param name="LinesText">The lines with a variance, on one line.</param>
internal sealed record FulfilmentConfirmationRow(
    string Number,
    string DateText,
    string VarianceText,
    string ReviewText,
    string LinesText);
