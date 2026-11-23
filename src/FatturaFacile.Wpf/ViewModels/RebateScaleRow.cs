// <copyright file="RebateScaleRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One tier of the rebate scale, formatted for display.
/// </summary>
/// <param name="From">The lower bound.</param>
/// <param name="To">The upper bound, or the localized "no upper limit".</param>
/// <param name="Rate">The rate in percent.</param>
/// <param name="Flat">The flat amount, or a dash.</param>
/// <param name="IsCurrent">True for the tier the spend stands in.</param>
internal sealed record RebateScaleRow(string From, string To, string Rate, string Flat, bool IsCurrent);
