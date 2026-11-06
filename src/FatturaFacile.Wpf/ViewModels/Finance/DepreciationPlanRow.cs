// <copyright file="DepreciationPlanRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.Finance;

/// <summary>
/// A row of a depreciation plan.
/// </summary>
/// <param name="Year">The year.</param>
/// <param name="Amount">The depreciation.</param>
/// <param name="BookValue">The book value at year end.</param>
internal sealed record DepreciationPlanRow(int Year, decimal Amount, decimal BookValue);
