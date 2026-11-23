// <copyright file="RebateTierProgress.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Where a qualifying volume stands on the rebate scale.
/// </summary>
/// <param name="CurrentTier">The highest tier reached, or null below the first threshold.</param>
/// <param name="NextTier">The first tier not reached yet, or null on the top tier.</param>
/// <param name="AccruedRebate">The rebate the volume earns today.</param>
/// <param name="DistanceToNext">The spend missing to the next threshold, or null on the top tier.</param>
/// <param name="ProgressPercent">The progress from the floor of the current tier to the next threshold (100 on the top tier).</param>
/// <param name="ProjectedRebateAtNext">The rebate earned exactly at the next threshold, or null on the top tier.</param>
/// <param name="ProjectedGain">The extra rebate earned once the next threshold is reached, or null on the top tier.</param>
public sealed record RebateTierProgress(
    RebateTier? CurrentTier,
    RebateTier? NextTier,
    decimal AccruedRebate,
    decimal? DistanceToNext,
    decimal ProgressPercent,
    decimal? ProjectedRebateAtNext,
    decimal? ProjectedGain);
