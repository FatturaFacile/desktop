// <copyright file="SwissPayrollEngineTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Payroll.Swiss;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Swiss payroll engine with hand-calculated expected values.
/// </summary>
public sealed class SwissPayrollEngineTests
{
    private static readonly DateTime _birth1986 = new(1986, 5, 10, 0, 0, 0, DateTimeKind.Unspecified);

    /// <summary>
    /// A full month of a monthly paid employee: AHV, ALV, NBU, KTG and BVG (age credit 10 %).
    /// </summary>
    [Fact]
    public void MonthlySalary_ComputesSocialContributionsAndBvg()
    {
        var context = Context(Employee(84_500m, installments: 13), 2026, 1);

        var result = SwissPayrollEngine.Calculate(context)!;

        Assert.Equal(6500m, result.Payslip.GrossSalary);
        Assert.Equal(-344.50m, Amount(result, SwissWageTypeCodes.Ahv));
        Assert.Equal(-71.50m, Amount(result, SwissWageTypeCodes.Alv));
        Assert.Equal(-78.00m, Amount(result, SwissWageTypeCodes.Nbu));
        Assert.Equal(-32.50m, Amount(result, SwissWageTypeCodes.Ktg));
        Assert.Equal(58_040m, result.Payslip.BvgInsuredSalary);
        Assert.Equal(-241.85m, Amount(result, SwissWageTypeCodes.Bvg));
        Assert.Equal(241.85m, Amount(result, SwissWageTypeCodes.BvgEmployer));
        Assert.Equal(768.35m, result.Payslip.EmployeeDeductions);
        Assert.Equal(5731.65m, result.Payslip.NetSalary);
        Assert.Equal(5731.65m, result.Payslip.PayoutAmount);
        Assert.Null(result.Payslip.Warnings);
    }

    /// <summary>
    /// The 13th salary is paid in December pro rata of the paid days of the year.
    /// </summary>
    [Fact]
    public void December_PaysThirteenthSalary()
    {
        var context = Context(Employee(84_500m, installments: 13), 2026, 12);
        context.YearToDate = new SwissPayrollYearToDate(330m, 71_500m, 71_500m);

        var result = SwissPayrollEngine.Calculate(context)!;

        Assert.Equal(6500m, Amount(result, SwissWageTypeCodes.ThirteenthSalary));
        Assert.Equal(13_000m, result.Payslip.GrossSalary);
    }

    /// <summary>
    /// An employee hired on the 16th is paid 15 days (30-day convention).
    /// </summary>
    [Fact]
    public void EntryDuringMonth_PaysProRata()
    {
        var employee = Employee(78_000m, installments: 12);
        employee.HireDate = new DateTime(2026, 3, 16, 0, 0, 0, DateTimeKind.Unspecified);

        var result = SwissPayrollEngine.Calculate(Context(employee, 2026, 3))!;

        Assert.Equal(15m, result.Payslip.PaidDays);
        Assert.Equal(3250m, result.Payslip.GrossSalary);
        Assert.Null(SwissPayrollEngine.Calculate(Context(employee, 2026, 2)));
    }

    /// <summary>
    /// The ALV basis is capped at the cumulated annual ceiling (CHF 148'200).
    /// </summary>
    [Fact]
    public void HighSalary_CapsAlvBasis()
    {
        var result = SwissPayrollEngine.Calculate(Context(Employee(180_000m, installments: 12), 2026, 1))!;

        Assert.Equal(15_000m, result.Payslip.AhvBasis);
        Assert.Equal(12_350m, result.Payslip.AlvBasis);
        Assert.Equal(12_350m, result.Payslip.UvgBasis);
    }

    /// <summary>
    /// Past the reference age the AHV allowance of CHF 1'400 applies and no ALV is due.
    /// </summary>
    [Fact]
    public void Pensioner_AppliesAllowanceAndNoAlv()
    {
        var employee = Employee(60_000m, installments: 12);
        employee.Gender = "F";
        employee.DateOfBirth = new DateTime(1958, 3, 1, 0, 0, 0, DateTimeKind.Unspecified);

        var result = SwissPayrollEngine.Calculate(Context(employee, 2026, 1))!;

        Assert.Equal(3600m, result.Payslip.AhvBasis);
        Assert.Equal(0m, result.Payslip.AlvBasis);
        Assert.Equal(0m, Amount(result, SwissWageTypeCodes.Bvg));
    }

    /// <summary>
    /// Child allowance below 16 and education allowance in education are paid with the cantonal amounts.
    /// </summary>
    [Fact]
    public void Children_ReceiveFamilyAllowances()
    {
        var employee = Employee(78_000m, installments: 12);
        var context = Context(employee, 2026, 6);
        context.Children =
        [
            new HrEmployeeChild { EmployeeId = employee.Id, FirstName = "Luca", DateOfBirth = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) },
            new HrEmployeeChild { EmployeeId = employee.Id, FirstName = "Sara", DateOfBirth = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), InEducation = true },
        ];

        var result = SwissPayrollEngine.Calculate(context)!;

        Assert.Equal(215m, Amount(result, SwissWageTypeCodes.ChildAllowance));
        Assert.Equal(268m, Amount(result, SwissWageTypeCodes.EducationAllowance));
        Assert.Equal(6500m, result.Payslip.AhvBasis);
        Assert.Equal(6983m, result.Payslip.GrossSalary);
    }

    /// <summary>
    /// Withholding tax uses the tariff row of the monthly income (monthly model, canton ZH).
    /// </summary>
    [Fact]
    public void WithholdingTax_UsesTariffRow()
    {
        var employee = Employee(78_000m, installments: 12);
        employee.IsWithholdingTaxSubject = true;
        employee.WithholdingTaxCode = "A0N";
        employee.WorkCanton = "ZH";
        employee.ResidenceCanton = "ZH";
        var context = Context(employee, 2026, 1);
        var validFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        context.WithholdingTaxTariffs =
        [
            new WithholdingTaxTariff { Canton = "ZH", TariffCode = "A0N", ValidFrom = validFrom, IncomeFrom = 0m, IncomeStep = 6000m, RatePercent = 5m },
            new WithholdingTaxTariff { Canton = "ZH", TariffCode = "A0N", ValidFrom = validFrom, IncomeFrom = 6000m, IncomeStep = 1000m, RatePercent = 8m },
        ];

        var result = SwissPayrollEngine.Calculate(context)!;

        Assert.Equal(8m, result.Payslip.WithholdingTaxRatePercent);
        Assert.Equal(-520m, Amount(result, SwissWageTypeCodes.WithholdingTax));
    }

    /// <summary>
    /// The AHV 21 reference age of women born in 1962 is 64 years and 6 months.
    /// </summary>
    [Fact]
    public void ReferenceAge_FollowsAhv21Transition()
    {
        var birth = new DateTime(1962, 4, 15, 0, 0, 0, DateTimeKind.Unspecified);

        Assert.Equal(new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Unspecified), SwissPayrollRules.ReferenceAgeDate(birth, "F"));
        Assert.Equal(new DateTime(2027, 4, 15, 0, 0, 0, DateTimeKind.Unspecified), SwissPayrollRules.ReferenceAgeDate(birth, "M"));
        Assert.Equal(8.33m, SwissPayrollRules.HolidayCompensationPercent(4));
        Assert.Equal(10.64m, SwissPayrollRules.HolidayCompensationPercent(5));
    }

    private static decimal Amount(SwissPayrollResult result, string code) =>
        result.Lines.Where(l => string.Equals(l.WageTypeCode, code, StringComparison.Ordinal)).Sum(static l => l.Amount);

    private static HrEmployee Employee(decimal annualSalary, int installments) => new()
    {
        FirstName = "Mario",
        LastName = "Rossi",
        HireDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
        DateOfBirth = _birth1986,
        Gender = "M",
        BaseAnnualSalary = annualSalary,
        SalaryInstallmentsPerYear = installments,
        WorkCanton = "TI",
        Currency = "CHF",
    };

    private static SwissPayrollContext Context(HrEmployee employee, int year, int month) => new()
    {
        Employee = employee,
        Settings = new SwissPayrollSettings
        {
            NbuEmployeeRatePercent = 1.2m,
            BuEmployerRatePercent = 0.1m,
            KtgEmployeeRatePercent = 0.5m,
            KtgEmployerRatePercent = 0.5m,
            FakEmployerRatePercent = 1.5m,
        },
        Year = year,
        Month = month,
        WageTypes = SwissWageTypeCatalog.Create().ToDictionary(static w => w.Code, StringComparer.Ordinal),
        FamilyAllowanceRates = [new FamilyAllowanceRate { Canton = "TI", ValidFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), ChildAllowance = 215m, EducationAllowance = 268m }],
    };
}
