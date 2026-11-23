// <copyright file="RebateTierCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the rebate scale arithmetic, which must give the same figures as the web-api: retroactive and incremental scales, flat
/// amounts, the fixed target bonus, the volume below the first tier and the distance and projection to the next tier.
/// </summary>
public sealed class RebateTierCalculatorTests
{
    private static readonly RebateTier[] Scale =
    [
        new RebateTier(10000m, 1m),
        new RebateTier(50000m, 2m, 100m),
        new RebateTier(100_000m, 3m, 500m),
    ];

    /// <summary>
    /// A volume below the first threshold earns nothing, in both modes.
    /// </summary>
    /// <param name="mode">The scale mode.</param>
    [Theory]
    [InlineData(RebateTierMode.Retroactive)]
    [InlineData(RebateTierMode.Incremental)]
    public void RebateOn_BelowFirstTier_EarnsNothing(RebateTierMode mode)
    {
        var earning = RebateTierCalculator.RebateOn(Scale, 9999.99m, mode);

        Assert.Equal(0m, earning.Rebate);
        Assert.Null(earning.RatePercent);
    }

    /// <summary>
    /// A volume that is zero, negative or has no scale earns nothing.
    /// </summary>
    /// <param name="volume">The volume.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void RebateOn_NonPositiveVolume_EarnsNothing(int volume)
    {
        Assert.Equal(0m, RebateTierCalculator.RebateOn(Scale, volume, RebateTierMode.Retroactive).Rebate);
        Assert.Equal(0m, RebateTierCalculator.RebateOn([], 1000m, RebateTierMode.Incremental).Rebate);
    }

    /// <summary>
    /// Retroactive: the rate of the highest tier reached applies to the whole volume, plus the flat amount of that tier only.
    /// </summary>
    /// <param name="volume">The volume.</param>
    /// <param name="expected">The expected rebate.</param>
    /// <param name="rate">The expected rate of the highest tier reached.</param>
    [Theory]
    [InlineData(10000, 100.00, 1)]
    [InlineData(20000, 200.00, 1)]
    [InlineData(50000, 1100.00, 2)]
    [InlineData(60000, 1300.00, 2)]
    [InlineData(120000, 4100.00, 3)]
    public void RebateOn_Retroactive_AppliesTheTopRateToTheWholeVolume(double volume, double expected, double rate)
    {
        var earning = RebateTierCalculator.RebateOn(Scale, (decimal)volume, RebateTierMode.Retroactive);

        Assert.Equal((decimal)expected, earning.Rebate);
        Assert.Equal((decimal)rate, earning.RatePercent);
    }

    /// <summary>
    /// Incremental: every band earns the rate of its own tier and the flat amounts of all the tiers reached are added.
    /// </summary>
    /// <param name="volume">The volume.</param>
    /// <param name="expected">The expected rebate.</param>
    [Theory]
    [InlineData(10000, 0.00)]
    [InlineData(20000, 100.00)]
    [InlineData(50000, 500.00)]
    [InlineData(60000, 700.00)]
    [InlineData(120000, 2600.00)]
    public void RebateOn_Incremental_EarnsEachBandAtItsOwnRate(double volume, double expected) =>
        Assert.Equal((decimal)expected, RebateTierCalculator.RebateOn(Scale, (decimal)volume, RebateTierMode.Incremental).Rebate);

    /// <summary>
    /// The order of the tiers does not matter.
    /// </summary>
    [Fact]
    public void RebateOn_UnorderedScale_GivesTheSameResult()
    {
        RebateTier[] shuffled = [Scale[2], Scale[0], Scale[1]];

        Assert.Equal(
            RebateTierCalculator.RebateOn(Scale, 60000m, RebateTierMode.Incremental),
            RebateTierCalculator.RebateOn(shuffled, 60000m, RebateTierMode.Incremental));
        Assert.Equal(
            RebateTierCalculator.RebateOn(Scale, 60000m, RebateTierMode.Retroactive),
            RebateTierCalculator.RebateOn(shuffled, 60000m, RebateTierMode.Retroactive));
    }

    /// <summary>
    /// The result is rounded to two decimals, half away from zero, and only at the end.
    /// </summary>
    [Fact]
    public void RebateOn_Rounding_IsHalfAwayFromZero()
    {
        RebateTier[] scale = [new RebateTier(0m, 12.5m)];

        // 0.04 x 12.5% = 0.005 exactly: banker's rounding would give 0.00.
        Assert.Equal(0.01m, RebateTierCalculator.RebateOn(scale, 0.04m, RebateTierMode.Retroactive).Rebate);
        Assert.Equal(0.01m, RebateTierCalculator.RebateOn(scale, 0.04m, RebateTierMode.Incremental).Rebate);
        Assert.Equal(3.33m, RebateTierCalculator.Round(3.325m));
        Assert.Equal(-3.33m, RebateTierCalculator.Round(-3.325m));
    }

    /// <summary>
    /// A fixed bonus is earned in full once the spend reaches the target, and not before.
    /// </summary>
    [Fact]
    public void FixedBonusOn_EarnsTheWholeBonusAtTheTarget()
    {
        Assert.Equal(0m, RebateTierCalculator.FixedBonusOn(30000m, 750m, 29999.99m));
        Assert.Equal(750m, RebateTierCalculator.FixedBonusOn(30000m, 750m, 30000m));
        Assert.Equal(750m, RebateTierCalculator.FixedBonusOn(30000m, 750m, 90000m));
        Assert.Equal(0m, RebateTierCalculator.FixedBonusOn(0m, 750m, 0m));
    }

    /// <summary>
    /// Between two tiers the progress shows the current and the next tier, the distance, the projected rebate and the gain.
    /// </summary>
    [Fact]
    public void Progress_Retroactive_ShowsDistanceAndProjection()
    {
        var progress = RebateTierCalculator.Progress(Scale, 20000m, RebateTierMode.Retroactive);

        Assert.Equal(Scale[0], progress.CurrentTier);
        Assert.Equal(Scale[1], progress.NextTier);
        Assert.Equal(200m, progress.AccruedRebate);
        Assert.Equal(30000m, progress.DistanceToNext);
        Assert.Equal(25m, progress.ProgressPercent);
        Assert.Equal(1100m, progress.ProjectedRebateAtNext);
        Assert.Equal(900m, progress.ProjectedGain);
    }

    /// <summary>
    /// The projection of an incremental scale counts the bands up to the next threshold and the flat amount it unlocks.
    /// </summary>
    [Fact]
    public void Progress_Incremental_ProjectsTheBandsUpToTheNextThreshold()
    {
        var progress = RebateTierCalculator.Progress(Scale, 20000m, RebateTierMode.Incremental);

        Assert.Equal(100m, progress.AccruedRebate);
        Assert.Equal(500m, progress.ProjectedRebateAtNext);
        Assert.Equal(400m, progress.ProjectedGain);
    }

    /// <summary>
    /// Below the first tier there is no current tier, the next one is the first and the progress counts from zero.
    /// </summary>
    [Fact]
    public void Progress_BelowFirstTier_PointsAtTheFirstTier()
    {
        var progress = RebateTierCalculator.Progress(Scale, 4000m, RebateTierMode.Retroactive);

        Assert.Null(progress.CurrentTier);
        Assert.Equal(Scale[0], progress.NextTier);
        Assert.Equal(0m, progress.AccruedRebate);
        Assert.Equal(6000m, progress.DistanceToNext);
        Assert.Equal(40m, progress.ProgressPercent);
        Assert.Equal(100m, progress.ProjectedRebateAtNext);
        Assert.Equal(100m, progress.ProjectedGain);
    }

    /// <summary>
    /// A volume of zero has not reached any tier.
    /// </summary>
    [Fact]
    public void Progress_ZeroVolume_StartsFromTheFirstTier()
    {
        var progress = RebateTierCalculator.Progress(Scale, 0m, RebateTierMode.Retroactive);

        Assert.Null(progress.CurrentTier);
        Assert.Equal(10000m, progress.DistanceToNext);
        Assert.Equal(0m, progress.ProgressPercent);
    }

    /// <summary>
    /// A volume exactly on a threshold has reached that tier.
    /// </summary>
    [Fact]
    public void Progress_OnAThreshold_HasReachedTheTier()
    {
        var progress = RebateTierCalculator.Progress(Scale, 50000m, RebateTierMode.Retroactive);

        Assert.Equal(Scale[1], progress.CurrentTier);
        Assert.Equal(Scale[2], progress.NextTier);
        Assert.Equal(50000m, progress.DistanceToNext);
        Assert.Equal(0m, progress.ProgressPercent);
    }

    /// <summary>
    /// On the top tier there is no next tier and the progress is complete.
    /// </summary>
    [Fact]
    public void Progress_TopTier_HasNoNextTier()
    {
        var progress = RebateTierCalculator.Progress(Scale, 120_000m, RebateTierMode.Retroactive);

        Assert.Equal(Scale[2], progress.CurrentTier);
        Assert.Null(progress.NextTier);
        Assert.Null(progress.DistanceToNext);
        Assert.Null(progress.ProjectedRebateAtNext);
        Assert.Null(progress.ProjectedGain);
        Assert.Equal(100m, progress.ProgressPercent);
        Assert.Equal(4100m, progress.AccruedRebate);
    }
}
