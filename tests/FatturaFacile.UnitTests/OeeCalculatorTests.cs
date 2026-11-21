// <copyright file="OeeCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Manufacturing;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the offline OEE formulas (a port of the web-api OeeCalculator).
/// </summary>
public sealed class OeeCalculatorTests
{
    /// <summary>
    /// Availability, performance and quality multiply into the OEE.
    /// </summary>
    [Fact]
    public void Calculate_CombinesTheThreeFactors()
    {
        var metrics = OeeCalculator.Calculate(480d, [Log(planned: 30, unplanned: 60, good: 300, scrap: 10, rework: 10)], idealCycleMinutes: 1d);

        Assert.Equal(450d, metrics.PlannedProductionMinutes);
        Assert.Equal(390d, metrics.OperatingMinutes);
        Assert.Equal(320d, metrics.TotalUnits);
        Assert.Equal(0.866667d, metrics.Availability, 6);
        Assert.Equal(0.820513d, metrics.Performance, 6);
        Assert.Equal(0.9375d, metrics.Quality, 6);
        Assert.Equal(0.666667d, metrics.Oee, 5);
        Assert.Equal(86.67d, metrics.AvailabilityPercent);
        Assert.Equal(82.05d, metrics.PerformancePercent);
        Assert.Equal(93.75d, metrics.QualityPercent);
        Assert.Equal(66.67d, metrics.OeePercent);
    }

    /// <summary>
    /// A performance above the ideal cycle is clamped to 100 percent.
    /// </summary>
    [Fact]
    public void Calculate_ClampsPerformanceAtOne()
    {
        var metrics = OeeCalculator.Calculate(100d, [Log(good: 500)], idealCycleMinutes: 1d);

        Assert.Equal(1d, metrics.Performance);
        Assert.Equal(1d, metrics.Availability);
        Assert.Equal(100d, metrics.OeePercent);
    }

    /// <summary>
    /// Without logs the availability is full but nothing was produced, so performance and OEE are zero and quality stays at 100 percent.
    /// </summary>
    [Fact]
    public void Calculate_NoLogs_HasZeroPerformanceAndFullQuality()
    {
        var metrics = OeeCalculator.Calculate(480d, [], idealCycleMinutes: 1d);

        Assert.Equal(1d, metrics.Availability);
        Assert.Equal(0d, metrics.Performance);
        Assert.Equal(1d, metrics.Quality);
        Assert.Equal(0d, metrics.Oee);
    }

    /// <summary>
    /// Planned downtime that exceeds the schedule leaves no planned production and a zero availability.
    /// </summary>
    [Fact]
    public void Calculate_NoPlannedProduction_HasZeroAvailability()
    {
        var metrics = OeeCalculator.Calculate(60d, [Log(planned: 120, good: 5)], idealCycleMinutes: 1d);

        Assert.Equal(0d, metrics.PlannedProductionMinutes);
        Assert.Equal(0d, metrics.Availability);
        Assert.Equal(0d, metrics.Performance);
        Assert.Equal(0d, metrics.OeePercent);
    }

    /// <summary>
    /// Unplanned downtime goes to the category of its log, scrap and rework are converted with the ideal cycle time.
    /// </summary>
    [Fact]
    public void SixBigLosses_DistributesDowntimeAndDefects()
    {
        var losses = OeeCalculator.SixBigLosses(
            [
                Log(unplanned: 45, scrap: 4, rework: 1, category: "setup_adjustment"),
                Log(unplanned: 15, scrap: 2, category: "startup_rejects"),
                Log(unplanned: 10, category: "unknown_category"),
            ],
            idealCycleMinutes: 2d);

        Assert.Equal(6, losses.Count);
        Assert.Equal(45d, losses["setup_adjustment"]);
        Assert.Equal(10d, losses["equipment_failure"]);
        Assert.Equal(10d, losses["process_defects"]);
        Assert.Equal(19d, losses["startup_rejects"]);
        Assert.Equal(0d, losses["minor_stops"]);
    }

    /// <summary>
    /// Each loss category belongs to one OEE factor.
    /// </summary>
    /// <param name="category">The loss category.</param>
    /// <param name="factor">The expected factor.</param>
    [Theory]
    [InlineData("equipment_failure", "availability")]
    [InlineData("setup_adjustment", "availability")]
    [InlineData("minor_stops", "performance")]
    [InlineData("reduced_speed", "performance")]
    [InlineData("process_defects", "quality")]
    [InlineData("startup_rejects", "quality")]
    public void FactorOf_MapsTheCategoryToItsFactor(string category, string factor) =>
        Assert.Equal(factor, OeeCalculator.FactorOf(category));

    /// <summary>
    /// The ideal cycle time comes from the machine, then from the machines of the work centre, then from the routing operations.
    /// </summary>
    [Fact]
    public void IdealCycleMinutes_FallsBackMachineThenMachinesThenOperations()
    {
        var fast = new MfgMachineCenter { IdealCycleTimeSecondsPerUnit = 30m };
        var slow = new MfgMachineCenter { IdealCycleTimeSecondsPerUnit = 90m };
        var unknown = new MfgMachineCenter { IdealCycleTimeSecondsPerUnit = 0m };

        Assert.Equal(0.5d, OeeCalculator.IdealCycleMinutes(fast, [fast, slow], [4m]));
        Assert.Equal(1d, OeeCalculator.IdealCycleMinutes(machine: null, [fast, slow, unknown], [4m]));
        Assert.Equal(3d, OeeCalculator.IdealCycleMinutes(unknown, [unknown], [2m, 4m, 0m]));
        Assert.Equal(0d, OeeCalculator.IdealCycleMinutes(machine: null, [], []));
    }

    /// <summary>
    /// Telemetry is online within 15 minutes, stale after that and never when the machine has not reported.
    /// </summary>
    [Fact]
    public void TelemetryStatus_ClassifiesByAge()
    {
        var now = new DateTime(2026, 11, 21, 9, 0, 0, DateTimeKind.Utc);

        Assert.Equal("never", OeeCalculator.TelemetryStatus(null, now));
        Assert.Equal("online", OeeCalculator.TelemetryStatus(now.AddMinutes(-15), now));
        Assert.Equal("stale", OeeCalculator.TelemetryStatus(now.AddMinutes(-16), now));
    }

    private static OeeLogEntry Log(double planned = 0, double unplanned = 0, double good = 0, double scrap = 0, double rework = 0, string category = "equipment_failure") =>
        new(planned, unplanned, good, scrap, rework, category);
}
