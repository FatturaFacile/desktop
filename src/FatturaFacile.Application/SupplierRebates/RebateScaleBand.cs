// <copyright file="RebateScaleBand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A tier of the scale with its upper bound, as the server shows it.
/// </summary>
/// <param name="ThresholdAmount">The lower bound.</param>
/// <param name="MaxThreshold">The upper bound (the threshold of the next tier), or null on the top tier.</param>
/// <param name="RatePercent">The rate in percent.</param>
/// <param name="FlatAmount">The flat amount, when there is one.</param>
public sealed record RebateScaleBand(decimal ThresholdAmount, decimal? MaxThreshold, decimal RatePercent, decimal? FlatAmount);
