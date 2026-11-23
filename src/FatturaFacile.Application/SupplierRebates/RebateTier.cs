// <copyright file="RebateTier.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A step of the rebate scale.
/// </summary>
/// <param name="ThresholdAmount">The qualifying spend from which the tier applies.</param>
/// <param name="RatePercent">The rate of the tier, in percent.</param>
/// <param name="FlatAmount">The flat amount earned once the tier is reached, when there is one.</param>
public sealed record RebateTier(decimal ThresholdAmount, decimal RatePercent, decimal? FlatAmount = null);
