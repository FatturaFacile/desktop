// <copyright file="ItalianPayrollStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies.Italian;

/// <summary>
/// Italian CCNL/INPS payroll strategy computing INPS, IRPEF, and TFR deductions.
/// </summary>
public sealed class ItalianPayrollStrategy : ICountryPayrollStrategy
{
    /// <inheritdoc/>
    public string CountryCode => "IT";

    /// <inheritdoc/>
    public PayrollCalculationResultDto CalculatePayroll(HrEmployee employee, DateTime targetMonth)
    {
        ArgumentNullException.ThrowIfNull(employee);

        var fte = employee.FteRatio > 0 ? employee.FteRatio : 1.0m;
        var grossMonthly = employee.BaseAnnualSalary / 12m * fte;

        // Italian employee statutory deductions
        const decimal InpsRate = 0.0919m;      // INPS IVS a carico lavoratore (9.19%)
        const decimal IrpefRate = 0.2300m;    // IRPEF 1° scaglione base (23.00%)
        const decimal AddizionaliRate = 0.0173m; // Addizionale Regionale e Comunale (1.73%)

        var inpsDeduction = Math.Round(grossMonthly * InpsRate, 2, MidpointRounding.AwayFromZero);
        var irpefDeduction = Math.Round(grossMonthly * IrpefRate, 2, MidpointRounding.AwayFromZero);
        var addizionaliDeduction = Math.Round(grossMonthly * AddizionaliRate, 2, MidpointRounding.AwayFromZero);

        var totalEmployeeDeductions = inpsDeduction + irpefDeduction + addizionaliDeduction;
        var netMonthly = grossMonthly - totalEmployeeDeductions;

        // Employer costs
        var inpsAzienda = Math.Round(grossMonthly * 0.2450m, 2, MidpointRounding.AwayFromZero); // INPS c/azienda 24.50%
        var inailAzienda = Math.Round(grossMonthly * 0.0150m, 2, MidpointRounding.AwayFromZero); // INAIL 1.50%
        var tfrAccrual = Math.Round(grossMonthly / 13.5m, 2, MidpointRounding.AwayFromZero);   // Quota TFR mensile
        var employerTotalCost = grossMonthly + inpsAzienda + inailAzienda + tfrAccrual;

        Dictionary<string, decimal> breakdown = new()
        {
            ["Contributi Previdenziali INPS (9.19%)"] = inpsDeduction,
            ["Ritenuta Fiscale IRPEF Base (23.00%)"] = irpefDeduction,
            ["Addizionali Regionale / Comunale (1.73%)"] = addizionaliDeduction,
            ["Trattamento Fine Rapporto (TFR Accantonato)"] = tfrAccrual,
        };

        return new PayrollCalculationResultDto
        {
            CountryCode = "IT",
            Currency = "EUR",
            GrossMonthlySalary = Math.Round(grossMonthly, 2, MidpointRounding.AwayFromZero),
            TotalDeductions = totalEmployeeDeductions,
            NetMonthlySalary = Math.Round(netMonthly, 2, MidpointRounding.AwayFromZero),
            EmployerTotalCost = Math.Round(employerTotalCost, 2, MidpointRounding.AwayFromZero),
            DeductionBreakdown = breakdown,
            ComplianceNotes = $"CCNL Commercio / Terziario. Codice Fiscale: {employee.SocialSecurityNumber ?? "RSSMRA80A01H501U"}. Periodo: {targetMonth:MMMM yyyy}.",
        };
    }
}
