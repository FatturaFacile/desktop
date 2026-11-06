// <copyright file="SwissSocialInsuranceParameters.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// Swiss federal social insurance parameters valid for 2026 (source: UFAS/BSV, "Rendite e contributi 2026" and
/// "Importi limite LPP 2026"). Insurer- and canton-specific rates (LAINF, IGM, CAF, pension fund rules,
/// withholding tax tariffs) are configured per employee.
/// </summary>
public static class SwissSocialInsuranceParameters
{
    /// <summary>
    /// The year these parameters apply to.
    /// </summary>
    public const int Year = 2026;

    /// <summary>
    /// AVS/AI/IPG (AHV/IV/EO) employee rate (employer pays the same).
    /// </summary>
    public const decimal AhvEmployeeRate = 0.053m;

    /// <summary>
    /// AD/ALV employee rate (employer pays the same) up to the maximum insured salary.
    /// </summary>
    public const decimal AlvEmployeeRate = 0.011m;

    /// <summary>
    /// Maximum annual salary insured by AD/ALV and LAINF/UVG.
    /// </summary>
    public const decimal AlvMaxAnnualSalary = 148_200m;

    /// <summary>
    /// Monthly AHV allowance (franchigia) for employees who reached the reference age.
    /// </summary>
    public const decimal AhvRetireeMonthlyAllowance = 1_400m;

    /// <summary>
    /// AHV reference age (65; the transition for women born 1961-1963 under AVS 21 is not modelled).
    /// </summary>
    public const int ReferenceAge = 65;

    /// <summary>
    /// LPP/BVG entry threshold (minimum annual salary).
    /// </summary>
    public const decimal BvgEntryThreshold = 22_680m;

    /// <summary>
    /// LPP/BVG coordination deduction.
    /// </summary>
    public const decimal BvgCoordinationDeduction = 26_460m;

    /// <summary>
    /// LPP/BVG upper salary limit.
    /// </summary>
    public const decimal BvgMaxAnnualSalary = 90_720m;

    /// <summary>
    /// LPP/BVG minimum coordinated salary.
    /// </summary>
    public const decimal BvgMinCoordinatedSalary = 3_780m;

    /// <summary>
    /// Gets the statutory BVG/LPP age credit (total, employee + employer) for the BVG age (calendar year minus birth year).
    /// </summary>
    /// <param name="bvgAge">The BVG age.</param>
    /// <returns>The age credit rate (0 before age 25 or after the reference age).</returns>
    public static decimal GetBvgAgeCreditRate(int bvgAge)
    {
        return bvgAge switch
        {
            < 25 => 0m,
            <= 34 => 0.07m,
            <= 44 => 0.10m,
            <= 54 => 0.15m,
            <= ReferenceAge => 0.18m,
            _ => 0m,
        };
    }
}
