// <copyright file="ProjectEvmStepRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One step of the profitability waterfall, formatted for display.
/// </summary>
/// <param name="Label">The localized name of the step.</param>
/// <param name="AmountText">The formatted amount.</param>
/// <param name="Share">The size of the step against the largest one, from 0 to 100, for the bar.</param>
/// <param name="IsTotal">True for the revenue and the gross margin bars.</param>
internal sealed record ProjectEvmStepRow(string Label, string AmountText, double Share, bool IsTotal);
