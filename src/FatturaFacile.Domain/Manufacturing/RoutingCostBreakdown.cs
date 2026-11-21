// <copyright file="RoutingCostBreakdown.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// The cost of a routing or of one of its steps, split by cost type (material is not known offline and stays zero).
/// </summary>
/// <param name="Setup">The setup cost.</param>
/// <param name="MachineRun">The machine run cost.</param>
/// <param name="DirectLabor">The direct labour cost.</param>
/// <param name="Overhead">The overhead cost.</param>
/// <param name="Subcontracting">The subcontracting cost.</param>
/// <param name="Material">The material cost.</param>
public sealed record RoutingCostBreakdown(decimal Setup, decimal MachineRun, decimal DirectLabor, decimal Overhead, decimal Subcontracting, decimal Material)
{
    /// <summary>
    /// Gets the total cost.
    /// </summary>
    public decimal Total => Setup + MachineRun + DirectLabor + Overhead + Subcontracting + Material;
}
