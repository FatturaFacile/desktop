// <copyright file="CostBreakdown.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Costing;

/// <summary>
/// A cost split into the three WIP cost elements.
/// </summary>
/// <param name="Material">The material cost.</param>
/// <param name="Labor">The labour cost.</param>
/// <param name="Overhead">The overhead cost.</param>
public sealed record CostBreakdown(decimal Material, decimal Labor, decimal Overhead)
{
    /// <summary>
    /// Gets a breakdown with every element equal to zero.
    /// </summary>
    public static CostBreakdown Zero { get; } = new(0m, 0m, 0m);

    /// <summary>
    /// Gets the sum of the three elements.
    /// </summary>
    public decimal Total => Material + Labor + Overhead;

    /// <summary>
    /// Adds another breakdown element by element.
    /// </summary>
    /// <param name="other">The other breakdown.</param>
    /// <returns>The sum.</returns>
    public CostBreakdown Plus(CostBreakdown other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new CostBreakdown(Material + other.Material, Labor + other.Labor, Overhead + other.Overhead);
    }

    /// <summary>
    /// Subtracts another breakdown element by element.
    /// </summary>
    /// <param name="other">The other breakdown.</param>
    /// <returns>The difference.</returns>
    public CostBreakdown Minus(CostBreakdown other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new CostBreakdown(Material - other.Material, Labor - other.Labor, Overhead - other.Overhead);
    }

    /// <summary>
    /// Multiplies every element by a factor.
    /// </summary>
    /// <param name="factor">The factor.</param>
    /// <returns>The scaled breakdown.</returns>
    public CostBreakdown Scale(decimal factor) => new(Material * factor, Labor * factor, Overhead * factor);

    /// <summary>
    /// Rounds every element to the given number of decimals (away from zero, as the Italian strategy does).
    /// </summary>
    /// <param name="decimals">The decimals.</param>
    /// <returns>The rounded breakdown.</returns>
    public CostBreakdown Round(int decimals) => new(
        Math.Round(Material, decimals, MidpointRounding.AwayFromZero),
        Math.Round(Labor, decimals, MidpointRounding.AwayFromZero),
        Math.Round(Overhead, decimals, MidpointRounding.AwayFromZero));
}
