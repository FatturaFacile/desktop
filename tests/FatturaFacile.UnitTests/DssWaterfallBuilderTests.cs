// <copyright file="DssWaterfallBuilderTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Dss;
using FatturaFacile.Wpf.ViewModels;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the stacked-column bars behind the variance waterfall chart (the same cases as the web frontend chart).
/// </summary>
public sealed class DssWaterfallBuilderTests
{
    /// <summary>
    /// Each change floats between the running totals before and after it, totals start at zero.
    /// </summary>
    [Fact]
    public void Build_PositiveBridge_FloatsEachChange()
    {
        var bars = DssWaterfallBuilder.Build(DssSnapshotParser.ParseVarianceBridge(DssTestData.VarianceBridge).EbitdaSteps, key => key);

        Assert.Equal(Zero("baseline") with { TotalPositive = 200 }, bars[0]);
        Assert.Equal(Zero("volume") with { Base = 200, IncreasePositive = 80 }, bars[1]);

        // A decrease from 400 to 340 floats on 340 and is 60 high.
        Assert.Equal(Zero("cogs") with { Base = 340, DecreasePositive = 60 }, bars[3]);
        Assert.Equal(Zero("actual") with { TotalPositive = 320 }, bars[6]);
    }

    /// <summary>
    /// A change that crosses the zero line is split into its positive and negative part, one below zero hangs under its upper end.
    /// </summary>
    [Fact]
    public void Build_BridgeAcrossZero_SplitsTheBars()
    {
        IReadOnlyList<DssBridgeStep> steps =
        [
            new("baseline", IsTotal: true, -50, -50),
            new("volume", IsTotal: false, 120, 70),
            new("price", IsTotal: false, -150, -80),
            new("cogs", IsTotal: false, -20, -100),
            new("actual", IsTotal: true, -100, -100),
        ];

        var bars = DssWaterfallBuilder.Build(steps, key => key);

        Assert.Equal(Zero("baseline") with { TotalNegative = -50 }, bars[0]);

        // -50 to 70: 70 above and 50 below zero.
        Assert.Equal(Zero("volume") with { IncreasePositive = 70, IncreaseNegative = -50 }, bars[1]);

        // 70 to -80 crosses zero too.
        Assert.Equal(Zero("price") with { DecreasePositive = 70, DecreaseNegative = -80 }, bars[2]);

        // -80 to -100 stays below: the invisible base is the upper end and the bar hangs 20 below it.
        Assert.Equal(Zero("cogs") with { Base = -80, DecreaseNegative = -20 }, bars[3]);
    }

    /// <summary>
    /// The category labels come from the caller, so they are localized.
    /// </summary>
    [Fact]
    public void Build_UsesTheLabelFunction()
    {
        var bars = DssWaterfallBuilder.Build(DssSnapshotParser.ParseVarianceBridge(DssTestData.VarianceBridge).RevenueSteps, key => "L:" + key);

        DssTestData.SameStrings(["L:baseline", "L:volume", "L:price", "L:fx", "L:actual"], bars.Select(bar => bar.Label));
    }

    /// <summary>
    /// A bridge without steps has no bars.
    /// </summary>
    [Fact]
    public void Build_NoSteps_NoBars() =>
        Assert.Empty(DssWaterfallBuilder.Build([], key => key));

    private static DssWaterfallBar Zero(string label) => new(label, 0, 0, 0, 0, 0, 0, 0);
}
