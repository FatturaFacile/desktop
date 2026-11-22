// <copyright file="UtilizationRules.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The rules of the billable utilization report: actual = hard billable hours over net hours, forecast = all billable hours over net
/// hours, gap = actual minus target, on target within 5 points.
/// </summary>
public static class UtilizationRules
{
    /// <summary>
    /// The tolerance around the target, in points.
    /// </summary>
    public const double ToleranceMargin = 5d;

    private const double Percent = 100d;
    private const int Decimals = 2;

    /// <summary>
    /// Computes a utilization share.
    /// </summary>
    /// <param name="billableHours">The billable hours.</param>
    /// <param name="netAvailableHours">The net availability.</param>
    /// <returns>The share in percent, 0 when there is no net capacity.</returns>
    public static double SharePercent(decimal billableHours, decimal netAvailableHours) =>
        netAvailableHours <= 0m ? 0d : Math.Round((double)(billableHours / netAvailableHours) * Percent, Decimals, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Computes the gap between the actual utilization and its target.
    /// </summary>
    /// <param name="actualPercent">The actual utilization.</param>
    /// <param name="targetPercent">The target.</param>
    /// <returns>Actual minus target, in points.</returns>
    public static double Gap(double actualPercent, double targetPercent) => Math.Round(actualPercent - targetPercent, Decimals, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Classifies a gap.
    /// </summary>
    /// <param name="gapPercent">The gap in points.</param>
    /// <returns>On target within 5 points either way, below or above otherwise.</returns>
    public static UtilizationStatus Classify(double gapPercent)
    {
        if (gapPercent < -ToleranceMargin)
        {
            return UtilizationStatus.BelowTarget;
        }

        return gapPercent > ToleranceMargin ? UtilizationStatus.AboveTarget : UtilizationStatus.OnTarget;
    }
}
