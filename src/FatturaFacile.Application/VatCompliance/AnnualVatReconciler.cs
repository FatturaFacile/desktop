// <copyright file="AnnualVatReconciler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Taxation;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Builds the annual VAT reconciliation schedule from the periodic liquidations, the VAT registers and the Erario c/IVA account.
/// </summary>
public static class AnnualVatReconciler
{
    private const decimal Tolerance = 0.05m;
    private const int MonthlyPeriodsExpected = 11;
    private const int QuarterlyPeriodsExpected = 3;

    /// <summary>
    /// Builds the schedule of a year.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="returns">The Italian liquidations of the year.</param>
    /// <param name="register">The lines of the sales and purchases registers of the year.</param>
    /// <param name="ledgerPosted">The net credit posted to the Erario c/IVA account in the year.</param>
    /// <returns>The schedule.</returns>
    public static AnnualVatReconciliation Build(int year, IReadOnlyList<VatReturn> returns, IReadOnlyList<VatRegisterEntry> register, decimal ledgerPosted)
    {
        ArgumentNullException.ThrowIfNull(returns);
        ArgumentNullException.ThrowIfNull(register);

        var periods = returns.OrderBy(static r => r.PeriodFrom).ToList();
        var rows = periods.ConvertAll(static r => new AnnualVatPeriodRow(r.PeriodFrom.Date, r.PeriodTo.Date, r.OutputVat, r.InputVat, r.PreviousCredit, r.Interest, r.Payable, r.Status));
        var output = periods.Sum(static r => r.OutputVat);
        var input = periods.Sum(static r => r.InputVat);
        var interest = periods.Sum(static r => r.Interest);
        var opening = periods.Count > 0 ? periods[0].PreviousCredit : 0m;

        // Split-payment VAT is not collected and integrated reverse-charge VAT is both due and deducted: neither is in the
        // liquidation of an invoice, so the comparison is made on the lines without them.
        var comparable = register.Where(static e => !e.IsReverseCharge && !e.IsSplitPayment).ToList();
        var sales = comparable.Where(static e => e.Kind == VatRegisterKind.Sales).ToList();
        var purchases = comparable.Where(static e => e.Kind == VatRegisterKind.Purchases).ToList();
        var salesVat = sales.Sum(static e => e.VatAmount);
        var purchasesVat = purchases.Sum(static e => e.DeductibleVat);

        List<string> findings = [];
        if (Math.Abs(output - salesVat) > Tolerance)
        {
            findings.Add("sales_vat_mismatch");
        }

        if (Math.Abs(input - purchasesVat) > Tolerance)
        {
            findings.Add("purchases_vat_mismatch");
        }

        if (Math.Abs(output - input - ledgerPosted) > Tolerance)
        {
            findings.Add("ledger_mismatch");
        }

        var expected = periods.Exists(static r => ItalianVatPeriodIsMonthly(r)) ? MonthlyPeriodsExpected : QuarterlyPeriodsExpected;
        if (periods.Count < expected)
        {
            findings.Add("periods_missing");
        }

        if (periods.Exists(static r => !string.Equals(r.Status, "Final", StringComparison.Ordinal)))
        {
            findings.Add("periods_not_final");
        }

        return new AnnualVatReconciliation(
            year,
            rows,
            output,
            input,
            interest,
            opening,
            output - input + interest - opening,
            sales.Sum(static e => e.TaxableAmount),
            salesVat,
            purchases.Sum(static e => e.TaxableAmount),
            purchasesVat,
            ledgerPosted,
            findings);
    }

    private static bool ItalianVatPeriodIsMonthly(VatReturn vatReturn) => ItalianVatPeriod.IsMonthly(vatReturn.PeriodFrom, vatReturn.PeriodTo);
}
