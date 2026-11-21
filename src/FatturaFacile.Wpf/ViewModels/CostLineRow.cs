// <copyright file="CostLineRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A row of the WIP costing grid (one cost element).
/// </summary>
/// <param name="Element">The localized cost element.</param>
/// <param name="Standard">The standard cost of the whole order.</param>
/// <param name="StandardForOutput">The standard cost allowed for the output so far.</param>
/// <param name="Actual">The actual cost so far.</param>
/// <param name="Variance">The variance (positive = unfavourable).</param>
internal sealed record CostLineRow(string Element, decimal Standard, decimal StandardForOutput, decimal Actual, decimal Variance);
