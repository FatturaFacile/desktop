// <copyright file="ProjectBillingSalCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// Arithmetic of a SAL progress certificate, the same as the web-api (Mod 081), on decimals rounded half away from zero to cents.
/// The cumulative gross earned value is the contract value times the cumulative completion; the value of the period is that less
/// the cumulative gross of the previous certificates; the advance is recouped in proportion (the whole remaining advance on the
/// final certificate, never more than what is left nor than the gross of the certificate); the retention is withheld on the gross.
/// The invoice amount is the gross less the advance, and the net amount the customer pays now is the invoice less the retention.
/// </summary>
public static class ProjectBillingSalCalculator
{
    private const int Cents = 2;
    private const decimal Hundred = 100m;

    /// <summary>
    /// Previews a SAL certificate.
    /// </summary>
    /// <param name="contractValue">The contract value.</param>
    /// <param name="cumulativePercent">The cumulative completion of the certificate, from 0 to 100.</param>
    /// <param name="previousPercent">The cumulative completion of the previous certificate.</param>
    /// <param name="previousCumulativeGross">The cumulative gross earned value of the previous certificate.</param>
    /// <param name="advanceTotal">The total advance to recoup.</param>
    /// <param name="advanceRecouped">The advance already recouped.</param>
    /// <param name="retentionPercent">The guarantee retention percentage.</param>
    /// <returns>The figures, or null while the progress does not advance within 0-100 or the contract has no value.</returns>
    public static ProjectBillingSalPreview? Preview(
        decimal contractValue,
        decimal cumulativePercent,
        decimal previousPercent,
        decimal previousCumulativeGross,
        decimal advanceTotal,
        decimal advanceRecouped,
        decimal retentionPercent)
    {
        if (contractValue <= 0 || cumulativePercent > Hundred || cumulativePercent <= previousPercent)
        {
            return null;
        }

        var isFinal = cumulativePercent == Hundred;
        var cumulativeGross = Round(contractValue * cumulativePercent / Hundred);
        var currentGross = cumulativeGross - previousCumulativeGross;
        var remaining = Math.Max(advanceTotal - advanceRecouped, 0m);
        var proportional = Round(advanceTotal * currentGross / contractValue);
        var deduction = Math.Max(Math.Min(isFinal ? remaining : proportional, Math.Min(remaining, currentGross)), 0m);
        var retention = Round(currentGross * retentionPercent / Hundred);
        var invoice = currentGross - deduction;

        return new ProjectBillingSalPreview(cumulativeGross, currentGross, deduction, retention, invoice, invoice - retention, isFinal);
    }

    private static decimal Round(decimal value) => Math.Round(value, Cents, MidpointRounding.AwayFromZero);
}
