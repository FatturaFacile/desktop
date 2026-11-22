// <copyright file="ProjectBillingRetainer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// The prepaid retainer / hour bank of a project.
/// </summary>
/// <param name="ByHours">Whether the bank is counted in hours (else in amount).</param>
/// <param name="RemainingHours">The hours left.</param>
/// <param name="RemainingAmount">The amount left.</param>
/// <param name="RemainingPercent">The share of the bank left, as a percentage.</param>
/// <param name="ConsumedHours">The hours burned.</param>
/// <param name="ThresholdPercent">The low-balance alert threshold, as a percentage.</param>
/// <param name="IsLow">Whether the balance is at or under the threshold.</param>
public sealed record ProjectBillingRetainer(bool ByHours, decimal RemainingHours, decimal RemainingAmount, decimal RemainingPercent, decimal ConsumedHours, decimal ThresholdPercent, bool IsLow)
{
    /// <summary>
    /// Gets the share of the bank already burned, from 0 to 100, for the burn-down bar.
    /// </summary>
    public decimal BurnedPercent => Math.Clamp(100m - RemainingPercent, 0m, 100m);
}
