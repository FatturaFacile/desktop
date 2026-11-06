// <copyright file="ItalianPayrollStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using P = FatturaFacile.Application.Strategies.Italian.ItalianPayrollParameters;

namespace FatturaFacile.Application.Strategies.Italian;

/// <summary>
/// Italian monthly payroll preview: INPS employee contributions (including the additional 1%), progressive IRPEF
/// with art. 13 TUIR deductions and 2026 tax-wedge measures, regional/municipal surcharges, TFR accrual.
/// </summary>
/// <remarks>
/// The withholding is the annual tax spread evenly over the installments; monthly progressive withholding with
/// year-end adjustment (conguaglio), family deductions, trattamento integrativo, CCNL tables, LUL, UniEmens,
/// F24 and CU are not part of this engine.
/// </remarks>
public sealed class ItalianPayrollStrategy : ICountryPayrollStrategy
{
    /// <inheritdoc/>
    public string CountryCode => "IT";

    /// <inheritdoc/>
    public PayrollCalculationResultDto CalculatePayroll(HrEmployee employee, DateTime targetMonth)
    {
        ArgumentNullException.ThrowIfNull(employee);

        var fte = employee.FteRatio > 0 ? employee.FteRatio : 1.0m;
        var installments = employee.SalaryInstallmentsPerYear > 0 ? employee.SalaryInstallmentsPerYear : 12;
        var annualGross = employee.BaseAnnualSalary * fte;
        var monthlyGross = Round(annualGross / installments);

        // INPS: 9.19% plus 1% on the annual part above the first pensionable bracket.
        var annualInps = (annualGross * P.InpsEmployeeRate) +
                         (Math.Max(0m, annualGross - P.InpsFirstBracket) * P.InpsAdditionalRate);
        var monthlyInps = Round(annualInps / installments);

        // IRPEF on the annual taxable income, net of deductions, spread over the installments.
        var taxableIncome = annualGross - annualInps;
        var grossIrpef = P.ComputeGrossIrpef(taxableIncome);
        var deductions = P.ComputeWorkDeduction(taxableIncome) +
                         P.ComputeTaxWedgeDeduction(taxableIncome);
        var netIrpef = Math.Max(0m, grossIrpef - deductions);
        var monthlyIrpef = Round(netIrpef / installments);

        var surchargeRate = (employee.RegionalSurchargeRatePercent + employee.MunicipalSurchargeRatePercent) / 100m;
        var monthlySurcharges = Round(taxableIncome * surchargeRate / installments);

        var monthlyBonus = Round(P.ComputeTaxWedgeBonus(taxableIncome) / installments);

        var totalDeductions = monthlyInps + monthlyIrpef + monthlySurcharges;
        var netMonthly = monthlyGross - totalDeductions + monthlyBonus;

        var employerInps = Round(monthlyGross * P.IndicativeEmployerInpsRate);
        var tfrMonthly = Round(annualGross / P.TfrDivisor / 12m);
        var employerTotalCost = monthlyGross + employerInps + tfrMonthly;

        Dictionary<string, decimal> breakdown = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Contributi INPS dipendente"] = monthlyInps,
            ["IRPEF netta"] = monthlyIrpef,
            ["Addizionali regionale e comunale"] = monthlySurcharges,
            ["Somma non imponibile cuneo fiscale"] = monthlyBonus,
            ["TFR maturato (costo azienda)"] = tfrMonthly,
        };

        return new PayrollCalculationResultDto
        {
            CountryCode = "IT",
            Currency = "EUR",
            GrossMonthlySalary = monthlyGross,
            TotalDeductions = totalDeductions,
            NetMonthlySalary = netMonthly,
            EmployerTotalCost = Round(employerTotalCost),
            DeductionBreakdown = breakdown,
            ComplianceNotes = string.Create(
                CultureInfo.InvariantCulture,
                $"Anteprima paga {targetMonth:MM/yyyy} con parametri {P.Year} (IRPEF 23/33/43%, detrazioni art. 13 TUIR, cuneo fiscale). Contributi datore INPS indicativi; esclusi CCNL, detrazioni familiari, conguaglio, LUL, UniEmens, F24, CU."),
        };
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
