// <copyright file="WipCostingCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Costing;
using FatturaFacile.Domain.Manufacturing;
using Xunit;

namespace FatturaFacile.UnitTests.Manufacturing;

/// <summary>
/// Tests of the WIP costing math: labour, overhead and material, standard versus actual, in decimal arithmetic.
/// </summary>
public sealed class WipCostingCalculatorTests
{
    private static readonly RoutingCostInput[] Routing =
    [
        new(SetupMinutes: 30m, RunMinutesPerUnit: 6m, LaborMinutesPerUnit: 6m, HourlyLaborRate: 60m, HourlyOverheadRate: 120m),
        new(SetupMinutes: 0m, RunMinutesPerUnit: 3m, LaborMinutesPerUnit: 0m, HourlyLaborRate: 40m, HourlyOverheadRate: 80m),
    ];

    private static readonly PostedTimeInput[] Postings =
    [
        new(SetupMinutes: 30m, RunMinutes: 20m, LaborMinutes: 40m, HourlyLaborRate: 60m, HourlyOverheadRate: 120m),
        new(SetupMinutes: 0m, RunMinutes: 12m, LaborMinutes: 0m, HourlyLaborRate: 40m, HourlyOverheadRate: 80m),
    ];

    /// <summary>
    /// Standard cost adds material, labour (setup plus per-unit labour time, run time as fallback) and overhead (machine hours).
    /// </summary>
    [Fact]
    public void CalculateStandard_SplitsMaterialLaborAndOverhead()
    {
        var standard = WipCostingCalculator.CalculateStandard(quantity: 10m, materialCostPerUnit: 5m, Routing);

        Assert.Equal(50m, standard.Material);
        Assert.Equal(110m, standard.Labor);
        Assert.Equal(220m, standard.Overhead);
        Assert.Equal(380m, standard.Total);
    }

    /// <summary>
    /// Actual cost uses the posted labour minutes (setup plus run when none were posted) and the machine minutes for overhead.
    /// </summary>
    [Fact]
    public void CalculateActual_UsesPostedTime()
    {
        var actual = WipCostingCalculator.CalculateActual(materialIssued: 25m, Postings);

        Assert.Equal(25m, actual.Material);
        Assert.Equal(48m, actual.Labor);
        Assert.Equal(116m, actual.Overhead);
        Assert.Equal(189m, actual.Total);
    }

    /// <summary>
    /// The variance compares the actual cost with the standard allowed for the output completed, per element.
    /// </summary>
    [Fact]
    public void Evaluate_HalfCompleted_ComputesVarianceAgainstAllowedStandard()
    {
        var standard = WipCostingCalculator.CalculateStandard(10m, 5m, Routing);
        var actual = WipCostingCalculator.CalculateActual(25m, Postings);

        var result = WipCostingCalculator.Evaluate(10m, 5m, standard, actual, ProductionOrderStatus.InProgress);

        Assert.Equal(25m, result.StandardForOutput.Material);
        Assert.Equal(55m, result.StandardForOutput.Labor);
        Assert.Equal(110m, result.StandardForOutput.Overhead);
        Assert.Equal(0m, result.Variance.Material);
        Assert.Equal(-7m, result.Variance.Labor);
        Assert.Equal(6m, result.Variance.Overhead);
        Assert.Equal(-1m, result.Variance.Total);
        Assert.Equal(38m, result.StandardUnitCost);
        Assert.Equal(190m, result.ReceivedValue);
        Assert.Equal(0m, result.WipBalance);
        Assert.False(result.IsVarianceFinal);
    }

    /// <summary>
    /// WIP is the actual cost that has not yet been received into stock at standard.
    /// </summary>
    [Fact]
    public void Evaluate_PartialOutput_LeavesWipBalance()
    {
        var standard = WipCostingCalculator.CalculateStandard(10m, 5m, Routing);
        var actual = WipCostingCalculator.CalculateActual(25m, Postings);

        var result = WipCostingCalculator.Evaluate(10m, 2m, standard, actual, ProductionOrderStatus.InProgress);

        Assert.Equal(76m, result.ReceivedValue);
        Assert.Equal(113m, result.WipBalance);
    }

    /// <summary>
    /// A closed order carries no WIP and a completed or closed order has final variances.
    /// </summary>
    /// <param name="status">The order status.</param>
    /// <param name="expectedWip">The expected WIP balance.</param>
    /// <param name="expectedFinal">Whether the variances are final.</param>
    [Theory]
    [InlineData(ProductionOrderStatus.Completed, 113, true)]
    [InlineData(ProductionOrderStatus.Closed, 0, true)]
    [InlineData(ProductionOrderStatus.Cancelled, 0, false)]
    public void Evaluate_ClosedOrCompleted_FinalisesVariancesAndClearsWip(ProductionOrderStatus status, int expectedWip, bool expectedFinal)
    {
        var standard = WipCostingCalculator.CalculateStandard(10m, 5m, Routing);
        var actual = WipCostingCalculator.CalculateActual(25m, Postings);

        var result = WipCostingCalculator.Evaluate(10m, 2m, standard, actual, status);

        Assert.Equal(expectedWip, result.WipBalance);
        Assert.Equal(expectedFinal, result.IsVarianceFinal);
    }

    /// <summary>
    /// Amounts are rounded to two decimals away from zero, unit costs to six.
    /// </summary>
    [Fact]
    public void Evaluate_RoundsAwayFromZero()
    {
        var standard = WipCostingCalculator.CalculateStandard(quantity: 3m, materialCostPerUnit: 0.335m, []);

        var result = WipCostingCalculator.Evaluate(3m, 0m, standard, CostBreakdown.Zero, ProductionOrderStatus.Planned);

        Assert.Equal(1.01m, result.Standard.Material);
        Assert.Equal(0.335m, result.StandardUnitCost);
    }

    /// <summary>
    /// A non-positive target quantity or a negative amount is rejected.
    /// </summary>
    [Fact]
    public void Evaluate_InvalidInput_Throws()
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => WipCostingCalculator.Evaluate(0m, 0m, CostBreakdown.Zero, CostBreakdown.Zero, ProductionOrderStatus.Planned));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => WipCostingCalculator.CalculateStandard(-1m, 0m, []));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => WipCostingCalculator.CalculateActual(-1m, []));
    }
}
