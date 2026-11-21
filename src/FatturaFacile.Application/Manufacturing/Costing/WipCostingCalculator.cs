// <copyright file="WipCostingCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Application.Manufacturing.Costing;

/// <summary>
/// Work-in-progress costing: labour, overhead and material, standard versus actual. Pure decimal arithmetic, no I/O.
/// </summary>
/// <remarks>
/// Standard labour = (labour minutes per unit x quantity + setup minutes) / 60 x labour rate (the run minutes stand in when no labour time is routed).
/// Standard overhead = (run minutes per unit x quantity + setup minutes) / 60 x overhead rate (machine hours).
/// Actual labour = posted labour minutes / 60 x labour rate (setup plus run when none were posted); actual overhead = (setup + run) / 60 x overhead rate.
/// Variance = actual minus the standard allowed for the output completed so far; WIP = actual cost minus the standard value received into stock.
/// </remarks>
public static class WipCostingCalculator
{
    /// <summary>
    /// The decimals of a cost amount.
    /// </summary>
    public const int AmountDecimals = 2;

    /// <summary>
    /// The decimals of a unit cost.
    /// </summary>
    public const int UnitDecimals = 6;

    private const decimal MinutesPerHour = 60m;

    /// <summary>
    /// Computes the standard cost of a production order.
    /// </summary>
    /// <param name="quantity">The target quantity.</param>
    /// <param name="materialCostPerUnit">The standard material cost per unit.</param>
    /// <param name="operations">The routing operations.</param>
    /// <returns>The unrounded standard cost.</returns>
    public static CostBreakdown CalculateStandard(decimal quantity, decimal materialCostPerUnit, IEnumerable<RoutingCostInput> operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(materialCostPerUnit);

        var labor = 0m;
        var overhead = 0m;
        foreach (var operation in operations)
        {
            var laborPerUnit = operation.LaborMinutesPerUnit > 0m ? operation.LaborMinutesPerUnit : operation.RunMinutesPerUnit;
            var laborMinutes = (laborPerUnit * quantity) + operation.SetupMinutes;
            var machineMinutes = (operation.RunMinutesPerUnit * quantity) + operation.SetupMinutes;
            labor += Cost(laborMinutes, operation.HourlyLaborRate);
            overhead += Cost(machineMinutes, operation.HourlyOverheadRate);
        }

        return new CostBreakdown(materialCostPerUnit * quantity, labor, overhead);
    }

    /// <summary>
    /// Computes the actual cost accumulated from material issues and shop-floor postings.
    /// </summary>
    /// <param name="materialIssued">The material cost issued to the order.</param>
    /// <param name="postings">The confirmed time.</param>
    /// <returns>The unrounded actual cost.</returns>
    public static CostBreakdown CalculateActual(decimal materialIssued, IEnumerable<PostedTimeInput> postings)
    {
        ArgumentNullException.ThrowIfNull(postings);
        ArgumentOutOfRangeException.ThrowIfNegative(materialIssued);

        var labor = 0m;
        var overhead = 0m;
        foreach (var posting in postings)
        {
            var attended = posting.LaborMinutes > 0m ? posting.LaborMinutes : posting.SetupMinutes + posting.RunMinutes;
            labor += Cost(attended, posting.HourlyLaborRate);
            overhead += Cost(posting.SetupMinutes + posting.RunMinutes, posting.HourlyOverheadRate);
        }

        return new CostBreakdown(materialIssued, labor, overhead);
    }

    /// <summary>
    /// Compares standard and actual cost and derives the WIP balance.
    /// </summary>
    /// <param name="targetQuantity">The quantity to produce (greater than zero).</param>
    /// <param name="completedQuantity">The good quantity completed so far.</param>
    /// <param name="standard">The standard cost of the whole order.</param>
    /// <param name="actual">The actual cost so far.</param>
    /// <param name="status">The order status.</param>
    /// <returns>The rounded result.</returns>
    public static WipCostResult Evaluate(decimal targetQuantity, decimal completedQuantity, CostBreakdown standard, CostBreakdown actual, ProductionOrderStatus status)
    {
        ArgumentNullException.ThrowIfNull(standard);
        ArgumentNullException.ThrowIfNull(actual);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetQuantity);
        ArgumentOutOfRangeException.ThrowIfNegative(completedQuantity);

        var ratio = completedQuantity / targetQuantity;
        var allowed = standard.Scale(ratio);
        var received = Math.Round(standard.Total * ratio, AmountDecimals, MidpointRounding.AwayFromZero);
        var closed = status is ProductionOrderStatus.Closed or ProductionOrderStatus.Cancelled;
        var wip = closed ? 0m : Math.Max(0m, Math.Round(actual.Total, AmountDecimals, MidpointRounding.AwayFromZero) - received);

        return new WipCostResult(
            standard.Round(AmountDecimals),
            allowed.Round(AmountDecimals),
            actual.Round(AmountDecimals),
            actual.Minus(allowed).Round(AmountDecimals),
            Math.Round(standard.Total / targetQuantity, UnitDecimals, MidpointRounding.AwayFromZero),
            received,
            wip,
            status is ProductionOrderStatus.Completed or ProductionOrderStatus.Closed);
    }

    private static decimal Cost(decimal minutes, decimal hourlyRate)
    {
        // Multiply first: dividing the minutes by 60 first would introduce repeating decimals into the money amount.
        var scaled = minutes * hourlyRate;
        return scaled / MinutesPerHour;
    }
}
