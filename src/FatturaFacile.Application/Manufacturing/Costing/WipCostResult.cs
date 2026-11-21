// <copyright file="WipCostResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Costing;

/// <summary>
/// Standard versus actual costing of a production order and its WIP balance.
/// </summary>
/// <param name="Standard">The standard cost of the whole order quantity.</param>
/// <param name="StandardForOutput">The standard cost allowed for the quantity completed so far.</param>
/// <param name="Actual">The actual cost accumulated so far.</param>
/// <param name="Variance">The actual minus standard for the output (positive = unfavourable).</param>
/// <param name="StandardUnitCost">The standard cost per unit.</param>
/// <param name="ReceivedValue">The standard value of the output received.</param>
/// <param name="WipBalance">The work in progress balance (actual cost not yet received into stock).</param>
/// <param name="IsVarianceFinal">A value indicating whether the order is completed, so the variances are final.</param>
public sealed record WipCostResult(CostBreakdown Standard, CostBreakdown StandardForOutput, CostBreakdown Actual, CostBreakdown Variance, decimal StandardUnitCost, decimal ReceivedValue, decimal WipBalance, bool IsVarianceFinal);
