// <copyright file="CapacityBandsTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.ResourcePlanning;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the saturation rules: load, band thresholds, the overload highlight and the cap for bookings without capacity.
/// </summary>
public sealed class CapacityBandsTests
{
    /// <summary>
    /// The load is hard plus soft hours over the net hours.
    /// </summary>
    [Fact]
    public void LoadPercent_IsBookedOverNet()
    {
        Assert.Equal(87.5d, CapacityBands.LoadPercent(24m, 11m, 40m));
        Assert.Equal(0d, CapacityBands.LoadPercent(0m, 0m, 40m));
        Assert.Equal(33.33d, CapacityBands.LoadPercent(10m, 3m, 39m));
    }

    /// <summary>
    /// Bookings against no net capacity are capped at 999 and nothing booked against none is 0.
    /// </summary>
    [Fact]
    public void LoadPercent_WithoutNetCapacity_IsCapped()
    {
        Assert.Equal(999d, CapacityBands.LoadPercent(8m, 0m, 0m));
        Assert.Equal(999d, CapacityBands.LoadPercent(0m, 4m, -2m));
        Assert.Equal(0d, CapacityBands.LoadPercent(0m, 0m, 0m));
        Assert.Equal(999d, CapacityBands.LoadPercent(400m, 0m, 1m));
    }

    /// <summary>
    /// Below 80 is green, 80 to 100 included is yellow and above 100 is red.
    /// </summary>
    /// <param name="load">The load.</param>
    /// <param name="expected">The band.</param>
    [Theory]
    [InlineData(0d, CapacityBand.Green)]
    [InlineData(79.99d, CapacityBand.Green)]
    [InlineData(80d, CapacityBand.Yellow)]
    [InlineData(99.5d, CapacityBand.Yellow)]
    [InlineData(100d, CapacityBand.Yellow)]
    [InlineData(100.01d, CapacityBand.Red)]
    [InlineData(999d, CapacityBand.Red)]
    public void Classify_UsesTheDefaultThresholds(double load, CapacityBand expected) =>
        Assert.Equal(expected, CapacityBands.Classify(load, 40m, 10m));

    /// <summary>
    /// No net capacity and nothing booked has no band.
    /// </summary>
    [Fact]
    public void Classify_NoCapacityAndNothingBooked_IsNone()
    {
        Assert.Equal(CapacityBand.None, CapacityBands.Classify(0d, 0m, 0m));
        Assert.Equal(CapacityBand.Red, CapacityBands.Classify(999d, 0m, 8m));
    }

    /// <summary>
    /// The thresholds of the answer replace the defaults.
    /// </summary>
    [Fact]
    public void Classify_HonoursTheThresholdsOfTheAnswer()
    {
        var thresholds = new HeatmapThresholds(60d, 90d);

        Assert.Equal(CapacityBand.Green, CapacityBands.Classify(59d, 40m, 10m, thresholds));
        Assert.Equal(CapacityBand.Yellow, CapacityBands.Classify(60d, 40m, 10m, thresholds));
        Assert.Equal(CapacityBand.Red, CapacityBands.Classify(91d, 40m, 10m, thresholds));
    }

    /// <summary>
    /// A cell is classified from its load and hours.
    /// </summary>
    [Fact]
    public void Classify_Cell_UsesItsLoad()
    {
        var overloaded = ResourcePlanningTestData.Cell(new DateOnly(2026, 11, 16), 32m, 30m, 10m, 125d, CapacityBand.Red);
        var comfortable = ResourcePlanningTestData.Cell(new DateOnly(2026, 11, 23), 40m, 10m, 5m, 37.5d, CapacityBand.Green);
        var empty = ResourcePlanningTestData.Cell(new DateOnly(2026, 11, 30), 0m, 0m, 0m, 0d, CapacityBand.None);

        Assert.Equal(CapacityBand.Red, CapacityBands.Classify(overloaded));
        Assert.Equal(CapacityBand.Green, CapacityBands.Classify(comfortable));
        Assert.Equal(CapacityBand.None, CapacityBands.Classify(empty));
    }

    /// <summary>
    /// Only a load above the red threshold is an overload.
    /// </summary>
    [Fact]
    public void IsOverload_IsStrictlyAboveRed()
    {
        Assert.False(CapacityBands.IsOverload(100d));
        Assert.True(CapacityBands.IsOverload(100.5d));
        Assert.True(CapacityBands.IsOverload(95d, new HeatmapThresholds(70d, 90d)));
    }
}
