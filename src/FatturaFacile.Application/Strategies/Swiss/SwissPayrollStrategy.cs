// <copyright file="SwissPayrollStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using P = FatturaFacile.Application.Strategies.Swiss.SwissSocialInsuranceParameters;

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// Swiss monthly payroll preview: AVS/AI/IPG, AD (with ceiling), LPP/BVG on the coordinated salary with statutory age
/// credits (or the pension fund rate), LAINF NBU, IGM and withholding tax from the employee's cantonal tariff rate.
/// </summary>
/// <remarks>
/// This is a calculation engine, not a Swissdec-certified payroll: ELM transmission, salary certificates,
/// cumulative period corrections and cantonal withholding-tax tables are not part of it.
/// </remarks>
public sealed class SwissPayrollStrategy : ICountryPayrollStrategy
{
    /// <inheritdoc/>
    public string CountryCode => "CH";

    /// <summary>
    /// Computes the annual LPP/BVG coordinated salary.
    /// </summary>
    /// <param name="annualSalary">The annual AHV salary.</param>
    /// <returns>The coordinated salary (0 below the entry threshold).</returns>
    public static decimal ComputeCoordinatedSalary(decimal annualSalary)
    {
        if (annualSalary < P.BvgEntryThreshold)
        {
            return 0m;
        }

        var coordinated = Math.Min(annualSalary, P.BvgMaxAnnualSalary) - P.BvgCoordinationDeduction;
        return Math.Max(coordinated, P.BvgMinCoordinatedSalary);
    }

    /// <inheritdoc/>
    public PayrollCalculationResultDto CalculatePayroll(HrEmployee employee, DateTime targetMonth)
    {
        ArgumentNullException.ThrowIfNull(employee);

        var fte = employee.FteRatio > 0 ? employee.FteRatio : 1.0m;
        var installments = employee.SalaryInstallmentsPerYear > 0 ? employee.SalaryInstallmentsPerYear : 12;
        var annualGross = employee.BaseAnnualSalary * fte;
        var monthlyGross = Round(annualGross / installments);

        var age = employee.DateOfBirth is { } birth ? targetMonth.Year - birth.Year : (int?)null;
        var belowContributionAge = age is <= 17;
        var retired = age is > P.ReferenceAge;

        List<string> notes = [];

        // AVS/AI/IPG: no ceiling; contributions start the year after the 17th birthday; retirees have a monthly allowance.
        var ahvBase = belowContributionAge ? 0m : monthlyGross;

        if (retired)
        {
            ahvBase = Math.Max(0m, monthlyGross - P.AhvRetireeMonthlyAllowance);
        }

        var ahv = Round(ahvBase * P.AhvEmployeeRate);

        // AD/ALV: up to the annual ceiling, not due before the contribution age or after the reference age.
        var alvBase = belowContributionAge || retired ? 0m : Math.Min(annualGross, P.AlvMaxAnnualSalary) / installments;
        var alv = Round(alvBase * P.AlvEmployeeRate);

        // LPP/BVG: coordinated salary with statutory age credits split 50/50, unless a pension fund rate is configured.
        var bvg = ComputeBvgEmployee(employee, annualGross, age, notes);

        // LAINF NBU and IGM: insurer-specific rates on the salary up to the UVG ceiling.
        var uvgBase = Math.Min(annualGross, P.AlvMaxAnnualSalary) / installments;
        var nbu = Round(uvgBase * employee.NonOccupationalAccidentRatePercent / 100m);
        var ktg = Round(monthlyGross * employee.DailySicknessRatePercent / 100m);

        // Withholding tax: rate from the official cantonal tariff for the employee (no national flat rate exists).
        var qst = 0m;

        if (employee.IsWithholdingTaxSubject)
        {
            if (employee.WithholdingTaxRatePercent is { } qstRate)
            {
                qst = Round(monthlyGross * qstRate / 100m);
            }
            else
            {
                notes.Add("Imposta alla fonte: aliquota della tariffa cantonale mancante, ritenuta non calcolata.");
            }
        }

        if (age is null)
        {
            notes.Add("Data di nascita mancante: regole di età AVS/AD e accrediti LPP non applicabili con precisione.");
        }

        var totalDeductions = ahv + alv + bvg + nbu + ktg + qst;

        var employerAhv = ahv;
        var employerAlv = alv;
        var employerBvg = bvg;
        var employerBu = Round(uvgBase * employee.OccupationalAccidentRatePercent / 100m);
        var employerFak = Round(ahvBase * employee.FamilyAllowanceFundRatePercent / 100m);
        var employerTotalCost = monthlyGross + employerAhv + employerAlv + employerBvg + employerBu + employerFak;

        Dictionary<string, decimal> breakdown = new(StringComparer.OrdinalIgnoreCase)
        {
            ["AVS/AI/IPG"] = ahv,
            ["AD"] = alv,
            ["LPP/BVG"] = bvg,
            ["LAINF NBU"] = nbu,
            ["IGM/KTG"] = ktg,
            ["Imposta alla fonte"] = qst,
        };

        notes.Insert(0, string.Create(
            CultureInfo.InvariantCulture,
            $"Anteprima paga {targetMonth:MM/yyyy} con parametri federali {P.Year}. Non certificata Swissdec; trasmissione ELM non implementata."));

        return new PayrollCalculationResultDto
        {
            CountryCode = "CH",
            Currency = "CHF",
            GrossMonthlySalary = monthlyGross,
            TotalDeductions = totalDeductions,
            NetMonthlySalary = monthlyGross - totalDeductions,
            EmployerTotalCost = Round(employerTotalCost),
            DeductionBreakdown = breakdown,
            ComplianceNotes = string.Join(' ', notes),
        };
    }

    private static decimal ComputeBvgEmployee(HrEmployee employee, decimal annualGross, int? age, List<string> notes)
    {
        var coordinated = ComputeCoordinatedSalary(annualGross);

        if (coordinated == 0m)
        {
            return 0m;
        }

        if (employee.PensionFundEmployeeRatePercent is { } fundRate)
        {
            return Round(coordinated * fundRate / 100m / 12m);
        }

        if (age is null)
        {
            return 0m;
        }

        var totalCredit = SwissSocialInsuranceParameters.GetBvgAgeCreditRate(age.Value);

        if (totalCredit == 0m && age < 25)
        {
            notes.Add("LPP: sotto i 25 anni solo contributi di rischio, definiti dal regolamento della cassa pensione.");
        }

        return Round(coordinated * totalCredit / 2m / 12m);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
