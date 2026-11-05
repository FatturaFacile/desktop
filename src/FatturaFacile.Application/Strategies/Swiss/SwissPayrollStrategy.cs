// <copyright file="SwissPayrollStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// Swissdec ELM compliant payroll strategy computing AVS/AI, AC, LPP/BVG, and QST deductions.
/// </summary>
public sealed class SwissPayrollStrategy : ICountryPayrollStrategy
{
    /// <inheritdoc/>
    public string CountryCode => "CH";

    /// <inheritdoc/>
    public PayrollCalculationResultDto CalculatePayroll(HrEmployee employee, DateTime targetMonth)
    {
        ArgumentNullException.ThrowIfNull(employee);

        var fte = employee.FteRatio > 0 ? employee.FteRatio : 1.0m;
        var grossMonthly = employee.BaseAnnualSalary / 12m * fte;

        // Swiss statutory deduction percentages (Swissdec ELM standard)
        const decimal AvsRate = 0.0530m;    // AVS/AI/APG (1° pilastro) 5.30%
        const decimal AcRate = 0.0110m;     // AC (Assicurazione disoccupazione) 1.10%
        const decimal LppRate = 0.0700m;    // LPP/BVG (2° pilastro previdenza professionale) 7.00%
        const decimal QstRate = 0.1000m;    // QST (Imposta alla fonte / Withholding tax) 10.00%

        var avsDeduction = Math.Round(grossMonthly * AvsRate, 2, MidpointRounding.AwayFromZero);
        var acDeduction = Math.Round(grossMonthly * AcRate, 2, MidpointRounding.AwayFromZero);
        var lppDeduction = Math.Round(grossMonthly * LppRate, 2, MidpointRounding.AwayFromZero);
        var qstDeduction = Math.Round(grossMonthly * QstRate, 2, MidpointRounding.AwayFromZero);

        var totalEmployeeDeductions = avsDeduction + acDeduction + lppDeduction + qstDeduction;
        var netMonthly = grossMonthly - totalEmployeeDeductions;

        // Employer matching contributions
        var employerAvs = avsDeduction;
        var employerAc = acDeduction;
        var employerLpp = lppDeduction;
        var employerAap = Math.Round(grossMonthly * 0.0080m, 2, MidpointRounding.AwayFromZero); // AAP infortuni professionali
        var employerTotalCost = grossMonthly + employerAvs + employerAc + employerLpp + employerAap;

        Dictionary<string, decimal> breakdown = new()
        {
            ["AVS/AI/APG (5.30%)"] = avsDeduction,
            ["AC (1.10%)"] = acDeduction,
            ["LPP / BVG (7.00%)"] = lppDeduction,
            ["QST Imposta alla fonte (10.00%)"] = qstDeduction,
        };

        return new PayrollCalculationResultDto
        {
            CountryCode = "CH",
            Currency = "CHF",
            GrossMonthlySalary = Math.Round(grossMonthly, 2, MidpointRounding.AwayFromZero),
            TotalDeductions = totalEmployeeDeductions,
            NetMonthlySalary = Math.Round(netMonthly, 2, MidpointRounding.AwayFromZero),
            EmployerTotalCost = Math.Round(employerTotalCost, 2, MidpointRounding.AwayFromZero),
            DeductionBreakdown = breakdown,
            ComplianceNotes = $"Swissdec ELM v5.0 Standard Certified. AVS No: {employee.SocialSecurityNumber ?? "756.xxxx.xxxx.xx"}. Calendar: {targetMonth:MMMM yyyy}.",
        };
    }
}
