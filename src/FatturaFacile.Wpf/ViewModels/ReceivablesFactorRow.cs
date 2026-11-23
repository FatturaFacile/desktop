// <copyright file="ReceivablesFactorRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A factor of the credit risk score.
/// </summary>
/// <param name="Label">The factor.</param>
/// <param name="ValueText">The measured value.</param>
/// <param name="PointsText">The points it adds.</param>
internal sealed record ReceivablesFactorRow(string Label, string ValueText, string PointsText);
