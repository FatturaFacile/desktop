// <copyright file="UtilizationRulesTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the billable utilization rules: share, gap and the status within 5 points of the target.
/// </summary>
public sealed class UtilizationRulesTests
{
    /// <summary>
    /// The share is the billable hours over the net hours and 0 without capacity.
    /// </summary>
    [Fact]
    public void SharePercent_IsBillableOverNet()
    {
        Assert.Equal(62.5d, UtilizationRules.SharePercent(100m, 160m));
        Assert.Equal(0d, UtilizationRules.SharePercent(10m, 0m));
    }

    /// <summary>
    /// The gap is actual minus target.
    /// </summary>
    [Fact]
    public void Gap_IsActualMinusTarget()
    {
        Assert.Equal(-7.5d, UtilizationRules.Gap(62.5d, 70d));
        Assert.Equal(3d, UtilizationRules.Gap(73d, 70d));
    }

    /// <summary>
    /// A gap within 5 points either way is on target.
    /// </summary>
    /// <param name="gap">The gap.</param>
    /// <param name="expected">The status.</param>
    [Theory]
    [InlineData(0d, UtilizationStatus.OnTarget)]
    [InlineData(5d, UtilizationStatus.OnTarget)]
    [InlineData(-5d, UtilizationStatus.OnTarget)]
    [InlineData(5.01d, UtilizationStatus.AboveTarget)]
    [InlineData(-5.01d, UtilizationStatus.BelowTarget)]
    [InlineData(-40d, UtilizationStatus.BelowTarget)]
    public void Classify_UsesTheFivePointTolerance(double gap, UtilizationStatus expected) =>
        Assert.Equal(expected, UtilizationRules.Classify(gap));
}
