// <copyright file="ProRataCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The pro-rata of VAT deduction (art. 19 c. 5 DPR 633/1972) and the split of purchase VAT into deductible and non-deductible.
/// </summary>
public static class ProRataCalculator
{
    /// <summary>
    /// The percentage of a fully deductible VAT.
    /// </summary>
    public const decimal FullDeduction = 100m;

    /// <summary>
    /// Computes the pro-rata percentage of a year: the share of taxable operations over taxable and exempt operations, rounded up to
    /// the next whole percent as the law requires. Without exempt operations the VAT is fully deductible.
    /// </summary>
    /// <param name="taxableOperations">The taxable and assimilated operations of the year.</param>
    /// <param name="exemptOperations">The exempt operations of the year.</param>
    /// <returns>The deductible percentage from 0 to 100.</returns>
    public static decimal DeductiblePercent(decimal taxableOperations, decimal exemptOperations)
    {
        var total = taxableOperations + exemptOperations;
        return exemptOperations <= 0m || total <= 0m
            ? FullDeduction
            : Math.Clamp(Math.Ceiling(taxableOperations / total * FullDeduction), 0m, FullDeduction);
    }

    /// <summary>
    /// Computes the pro-rata percentage from the sales register of the previous year: taxable and assimilated operations (rated, or
    /// non-taxable under natures N3) against exempt operations (natures N4). Operations outside the scope of VAT (N1, N2) and
    /// reverse-charge integrations are left out.
    /// </summary>
    /// <param name="previousYearSales">The lines of the sales register of the previous year.</param>
    /// <returns>The deductible percentage from 0 to 100.</returns>
    public static decimal FromSales(IEnumerable<VatRegisterEntry> previousYearSales)
    {
        ArgumentNullException.ThrowIfNull(previousYearSales);

        var sales = previousYearSales.Where(static e => e.Kind == VatRegisterKind.Sales && !e.IsReverseCharge).ToList();
        var exempt = sales.Where(static e => e.VatRate == 0m && e.VatNature?.StartsWith("N4", StringComparison.Ordinal) == true).Sum(static e => e.TaxableAmount);
        var taxable = sales.Where(static e => e.VatRate > 0m || e.VatNature?.StartsWith("N3", StringComparison.Ordinal) == true).Sum(static e => e.TaxableAmount);

        return DeductiblePercent(taxable, exempt);
    }

    /// <summary>
    /// Splits a VAT amount into its deductible and non-deductible parts.
    /// </summary>
    /// <param name="vat">The VAT amount.</param>
    /// <param name="percent">The deductible percentage from 0 to 100.</param>
    /// <returns>The deductible and the non-deductible VAT; together they equal <paramref name="vat"/>.</returns>
    public static (decimal Deductible, decimal NonDeductible) Split(decimal vat, decimal percent)
    {
        var deductible = Math.Round(vat * Math.Clamp(percent, 0m, FullDeduction) / FullDeduction, 2, MidpointRounding.AwayFromZero);

        return (deductible, vat - deductible);
    }
}
