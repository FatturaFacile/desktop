// <copyright file="RebateEarning.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The rebate earned on a volume.
/// </summary>
/// <param name="Rebate">The rebate, rounded to two decimals.</param>
/// <param name="RatePercent">The rate of the highest tier reached, or null when no tier was reached.</param>
public sealed record RebateEarning(decimal Rebate, decimal? RatePercent);
