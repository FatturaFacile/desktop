// <copyright file="ItalianPayrollParameters.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Strategies.Italian;

/// <summary>
/// Italian payroll parameters valid for 2026: IRPEF brackets (Legge di Bilancio 2026), employee work-income
/// deduction (art. 13 TUIR), tax-wedge measures (L. 207/2024) and INPS thresholds (circolare INPS n. 6/2026).
/// </summary>
public static class ItalianPayrollParameters
{
    /// <summary>
    /// The year these parameters apply to.
    /// </summary>
    public const int Year = 2026;

    /// <summary>
    /// INPS IVS employee rate (general regime, standard employees).
    /// </summary>
    public const decimal InpsEmployeeRate = 0.0919m;

    /// <summary>
    /// Additional 1% employee INPS rate on the part of salary above the first pensionable bracket.
    /// </summary>
    public const decimal InpsAdditionalRate = 0.01m;

    /// <summary>
    /// First pensionable bracket (prima fascia di retribuzione pensionabile) 2026.
    /// </summary>
    public const decimal InpsFirstBracket = 56_224m;

    /// <summary>
    /// Indicative employer INPS rate (depends on sector, size and contract: configure per company in production).
    /// </summary>
    public const decimal IndicativeEmployerInpsRate = 0.29m;

    /// <summary>
    /// TFR divisor (annual accrual = annual salary / 13.5).
    /// </summary>
    public const decimal TfrDivisor = 13.5m;

    /// <summary>
    /// Computes the gross annual IRPEF on the taxable income (23% up to 28,000; 33% up to 50,000; 43% above).
    /// </summary>
    /// <param name="taxableIncome">The annual taxable income.</param>
    /// <returns>The gross IRPEF.</returns>
    public static decimal ComputeGrossIrpef(decimal taxableIncome)
    {
        if (taxableIncome <= 0m)
        {
            return 0m;
        }

        var tax = Math.Min(taxableIncome, 28_000m) * 0.23m;

        if (taxableIncome > 28_000m)
        {
            tax += (Math.Min(taxableIncome, 50_000m) - 28_000m) * 0.33m;
        }

        if (taxableIncome > 50_000m)
        {
            tax += (taxableIncome - 50_000m) * 0.43m;
        }

        return tax;
    }

    /// <summary>
    /// Computes the annual employee work-income deduction (detrazione per lavoro dipendente, art. 13 TUIR) for a full year.
    /// </summary>
    /// <param name="income">The annual total income (reddito complessivo).</param>
    /// <returns>The deduction (the 690 / 1,380 minimums apply only to periods shorter than a year and are not modelled).</returns>
    public static decimal ComputeWorkDeduction(decimal income)
    {
        var deduction = income switch
        {
            <= 15_000m => 1_955m,
            <= 28_000m => 1_910m + (1_190m * (28_000m - income) / 13_000m),
            <= 50_000m => 1_910m * (50_000m - income) / 22_000m,
            _ => 0m,
        };

        var increase = income is > 25_000m and <= 35_000m ? 65m : 0m;
        return deduction + increase;
    }

    /// <summary>
    /// Computes the tax-wedge additional deduction (ulteriore detrazione): 1,000 between 20,000 and 32,000,
    /// decreasing to zero at 40,000.
    /// </summary>
    /// <param name="income">The annual total income.</param>
    /// <returns>The additional deduction.</returns>
    public static decimal ComputeTaxWedgeDeduction(decimal income)
    {
        return income switch
        {
            <= 20_000m => 0m,
            <= 32_000m => 1_000m,
            <= 40_000m => 1_000m * (40_000m - income) / 8_000m,
            _ => 0m,
        };
    }

    /// <summary>
    /// Computes the tax-wedge non-taxable bonus (somma non imponibile) for employment income up to 20,000.
    /// </summary>
    /// <param name="employmentIncome">The annual employment income.</param>
    /// <returns>The non-taxable bonus.</returns>
    public static decimal ComputeTaxWedgeBonus(decimal employmentIncome)
    {
        return employmentIncome switch
        {
            <= 0m => 0m,
            <= 8_500m => employmentIncome * 0.071m,
            <= 15_000m => employmentIncome * 0.053m,
            <= 20_000m => employmentIncome * 0.048m,
            _ => 0m,
        };
    }
}
