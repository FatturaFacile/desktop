// <copyright file="DssDetailLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A label and value of the detail dialog of a profitability row.
/// </summary>
/// <param name="Label">The localized label.</param>
/// <param name="Value">The formatted value.</param>
internal sealed record DssDetailLine(string Label, string Value);
