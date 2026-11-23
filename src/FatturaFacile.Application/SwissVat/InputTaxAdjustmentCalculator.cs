// <copyright file="InputTaxAdjustmentCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Computes Swiss LIVA Art. 30-33 input tax adjustments (Autoconsumo, Degrassazione, Mixed-use, Subsidies).
/// </summary>
public static class InputTaxAdjustmentCalculator
{
    /// <summary>
    /// Useful life in years for movable assets under Art. 31/32 LIVA (20% annual depreciation).
    /// </summary>
    public const int MovableUsefulLifeYears = 5;

    /// <summary>
    /// Useful life in years for real estate under Art. 31/32 LIVA (5% annual depreciation).
    /// </summary>
    public const int RealEstateUsefulLifeYears = 20;

    /// <summary>
    /// Maps an <see cref="InputTaxAdjustmentType"/> to its official AFC box (410, 415, or 420).
    /// </summary>
    /// <param name="type">The adjustment type.</param>
    /// <returns>The AFC box code.</returns>
    public static string TargetAfcBox(InputTaxAdjustmentType type) => type switch
    {
        InputTaxAdjustmentType.Degrassazione => "410",
        InputTaxAdjustmentType.Autoconsumo or InputTaxAdjustmentType.MixedUseReduction => "415",
        InputTaxAdjustmentType.SubsidyReduction => "420",
        _ => "415",
    };

    /// <summary>
    /// Computes the statutory legal basis citation for <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The adjustment type.</param>
    /// <returns>The LIVA / MWSTG article citation.</returns>
    public static string LegalBasis(InputTaxAdjustmentType type) => type switch
    {
        InputTaxAdjustmentType.Autoconsumo => "Art. 31 LIVA / MWSTG",
        InputTaxAdjustmentType.Degrassazione => "Art. 32 LIVA / MWSTG",
        InputTaxAdjustmentType.MixedUseReduction => "Art. 30 LIVA / MWSTG",
        InputTaxAdjustmentType.SubsidyReduction => "Art. 33(2) LIVA / MWSTG",
        _ => "LIVA / MWSTG",
    };

    /// <summary>
    /// Calculates the CHF adjustment amount from residual asset value or proportional usage ratio.
    /// </summary>
    /// <param name="type">Adjustment type.</param>
    /// <param name="originalCostChf">Original asset cost or input tax base.</param>
    /// <param name="originalVatRate">Original VAT rate percentage.</param>
    /// <param name="yearsElapsed">Completed years since acquisition.</param>
    /// <param name="isRealEstate">True for real estate (20-year horizon); false for movable assets (5 years).</param>
    /// <param name="ratioPercent">Private or exempt usage percentage (0-100).</param>
    /// <returns>The adjustment amount in CHF.</returns>
    public static decimal Compute(
        InputTaxAdjustmentType type,
        decimal originalCostChf,
        decimal originalVatRate,
        int yearsElapsed,
        bool isRealEstate,
        decimal ratioPercent = 100m)
    {
        if (originalCostChf <= 0m || originalVatRate <= 0m)
        {
            return 0m;
        }

        var originalVat = originalCostChf * originalVatRate / 100m;
        var clampedRatio = Math.Clamp(ratioPercent, 0m, 100m) / 100m;

        if (type is InputTaxAdjustmentType.MixedUseReduction or InputTaxAdjustmentType.SubsidyReduction)
        {
            return Math.Round(originalVat * clampedRatio, 2, MidpointRounding.AwayFromZero);
        }

        var usefulLife = isRealEstate ? RealEstateUsefulLifeYears : MovableUsefulLifeYears;
        var remainingYears = Math.Max(0, usefulLife - Math.Max(0, yearsElapsed));
        if (remainingYears == 0)
        {
            return 0m;
        }

        var fraction = (decimal)remainingYears / usefulLife;
        var factor = ratioPercent > 0m ? clampedRatio : 1m;
        return Math.Round(originalVat * fraction * factor, 2, MidpointRounding.AwayFromZero);
    }
}
