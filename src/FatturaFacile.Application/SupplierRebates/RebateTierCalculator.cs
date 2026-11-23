// <copyright file="RebateTierCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Pure arithmetic of the rebate scale, the same as the web-api. Retroactive: the rate of the highest tier reached applies to the
/// whole volume. Incremental: every band of volume between a threshold and the next one (or the volume) earns the rate of its own
/// tier. Volume below the first threshold earns nothing. A tier may carry a flat amount, earned once when the tier is reached
/// (retroactive: the flat amount of the highest tier reached, incremental: the flat amounts of all the tiers reached). Amounts are
/// rounded to two decimals, half away from zero, only on the result.
/// </summary>
public static class RebateTierCalculator
{
    private const int Decimals = 2;
    private const decimal Hundred = 100m;

    /// <summary>
    /// Rounds an amount to two decimals, half away from zero.
    /// </summary>
    /// <param name="value">The amount.</param>
    /// <returns>The rounded amount.</returns>
    public static decimal Round(decimal value) => Math.Round(value, Decimals, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Computes the rebate earned on a volume.
    /// </summary>
    /// <param name="tiers">The scale, in any order.</param>
    /// <param name="volume">The qualifying spend.</param>
    /// <param name="mode">How the scale is applied.</param>
    /// <returns>The rebate and the rate of the highest tier reached.</returns>
    public static RebateEarning RebateOn(IReadOnlyList<RebateTier> tiers, decimal volume, RebateTierMode mode)
    {
        ArgumentNullException.ThrowIfNull(tiers);

        if (volume <= 0m)
        {
            return new RebateEarning(0m, RatePercent: null);
        }

        var reached = Sorted(tiers).Where(tier => volume >= tier.ThresholdAmount).ToList();
        if (reached.Count == 0)
        {
            return new RebateEarning(0m, RatePercent: null);
        }

        var top = reached[^1];
        if (mode == RebateTierMode.Retroactive)
        {
            var percent = volume * top.RatePercent / Hundred;

            return new RebateEarning(Round(percent + (top.FlatAmount ?? 0m)), top.RatePercent);
        }

        var total = 0m;
        for (var index = 0; index < reached.Count; index++)
        {
            var upper = index + 1 < reached.Count ? reached[index + 1].ThresholdAmount : volume;
            var band = upper - reached[index].ThresholdAmount;
            total += (band * reached[index].RatePercent / Hundred) + (reached[index].FlatAmount ?? 0m);
        }

        return new RebateEarning(Round(total), top.RatePercent);
    }

    /// <summary>
    /// Computes a fixed target bonus: the whole bonus is earned once the qualifying spend reaches the target.
    /// </summary>
    /// <param name="target">The spend target.</param>
    /// <param name="bonus">The bonus.</param>
    /// <param name="volume">The qualifying spend.</param>
    /// <returns>The bonus when the target is reached, otherwise zero.</returns>
    public static decimal FixedBonusOn(decimal target, decimal bonus, decimal volume) =>
        volume > 0m && volume >= target ? Round(bonus) : 0m;

    /// <summary>
    /// Locates a volume on the scale: the tier it is in, the next tier, the distance to it and the rebate the volume would earn when
    /// it gets there.
    /// </summary>
    /// <param name="tiers">The scale, in any order.</param>
    /// <param name="volume">The qualifying spend.</param>
    /// <param name="mode">How the scale is applied.</param>
    /// <returns>The progress.</returns>
    public static RebateTierProgress Progress(IReadOnlyList<RebateTier> tiers, decimal volume, RebateTierMode mode)
    {
        ArgumentNullException.ThrowIfNull(tiers);

        var sorted = Sorted(tiers);
        RebateTier? current = null;
        RebateTier? next = null;

        foreach (var tier in sorted)
        {
            if (volume > 0m && volume >= tier.ThresholdAmount)
            {
                current = tier;
            }
            else
            {
                next ??= tier;
            }
        }

        var accrued = RebateOn(sorted, volume, mode).Rebate;
        var floor = current?.ThresholdAmount ?? 0m;

        if (next is null)
        {
            return new RebateTierProgress(current, NextTier: null, accrued, DistanceToNext: null, Hundred, ProjectedRebateAtNext: null, ProjectedGain: null);
        }

        var projected = RebateOn(sorted, next.ThresholdAmount, mode).Rebate;
        var span = next.ThresholdAmount - floor;
        var progress = span > 0m ? Round((volume - floor) * Hundred / span) : 0m;

        return new RebateTierProgress(
            current,
            next,
            accrued,
            Round(next.ThresholdAmount - volume),
            Math.Max(0m, progress),
            projected,
            Round(projected - accrued));
    }

    private static List<RebateTier> Sorted(IReadOnlyList<RebateTier> tiers) => [.. tiers.OrderBy(tier => tier.ThresholdAmount)];
}
