// <copyright file="ItalianPayrollTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Features.Privacy;
using FatturaFacile.Application.Payroll.Italian;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Italian monthly payroll.
/// </summary>
public sealed class ItalianPayrollTests
{
    /// <summary>
    /// A normal month withholds INPS 9.19% on the rounded basis and keeps the net consistent.
    /// </summary>
    [Fact]
    public void NormalMonth_WithholdsInpsAndIrpef()
    {
        var result = ItalianPayrollEngine.Calculate(Context(month: 3));

        Assert.NotNull(result);
        var payslip = result.Payslip;
        Assert.Equal(2_307.69m, payslip.GrossSalary);
        Assert.Equal(2_308m, payslip.AhvBasis);
        Assert.Equal(-212.11m, Line(result, ItalianWageCodes.InpsEmployee).Amount);
        Assert.True(Line(result, ItalianWageCodes.Irpef).Amount < 0m);
        Assert.Equal(payslip.GrossSalary - payslip.EmployeeDeductions, payslip.NetSalary);
        Assert.DoesNotContain(result.Lines, static l => string.Equals(l.WageTypeCode, ItalianWageCodes.Thirteenth, StringComparison.Ordinal));
        Assert.Contains(result.Lines, static l => string.Equals(l.WageTypeCode, ItalianWageCodes.Tfr, StringComparison.Ordinal));
    }

    /// <summary>
    /// December pays the thirteenth installment and applies the year-end adjustment.
    /// </summary>
    [Fact]
    public void December_PaysThirteenthAndAdjusts()
    {
        var result = ItalianPayrollEngine.Calculate(Context(month: 12));

        Assert.NotNull(result);
        Assert.Equal(2_307.69m, Line(result, ItalianWageCodes.Thirteenth).Amount);
        Assert.Contains("Conguaglio", result.Payslip.Warnings, StringComparison.Ordinal);
    }

    /// <summary>
    /// Pay below the minimum of the agreement level is flagged.
    /// </summary>
    [Fact]
    public void PayBelowMinimum_IsFlagged()
    {
        var context = Context(month: 3, minimum: 2_500m);
        var result = ItalianPayrollEngine.Calculate(context);

        Assert.NotNull(result);
        Assert.Contains("minimo", result.Payslip.Warnings, StringComparison.Ordinal);
    }

    /// <summary>
    /// Employees hired after the month get no payslip.
    /// </summary>
    [Fact]
    public void NotYetHired_GetsNoPayslip()
    {
        var context = Context(month: 3);
        context.Employee.HireDate = new DateTime(2026, 5, 1);

        Assert.Null(ItalianPayrollEngine.Calculate(context));
    }

    /// <summary>
    /// The default register of processing activities covers the main processing activities.
    /// </summary>
    [Fact]
    public void DefaultRegister_CoversTheMainProcessing()
    {
        var activities = DefaultProcessingActivities.Create();

        Assert.Equal(4, activities.Count);
        Assert.All(activities, static a => Assert.False(string.IsNullOrWhiteSpace(a.RetentionPeriod)));
    }

    private static PayslipLine Line(ItalianPayrollResult result, string code) =>
        Assert.Single(result.Lines, l => string.Equals(l.WageTypeCode, code, StringComparison.Ordinal));

    private static ItalianPayrollContext Context(int month, decimal minimum = 1_800m)
    {
        CollectiveAgreement agreement = new() { Code = "H011", Name = "Commercio", Installments = 13 };

        return new ItalianPayrollContext
        {
            Employee = new HrEmployee
            {
                EmployeeNumber = "1",
                FirstName = "Mario",
                LastName = "Rossi",
                BaseAnnualSalary = 30_000m,
                FteRatio = 1m,
                SalaryInstallmentsPerYear = 13,
                HireDate = new DateTime(2020, 1, 1),
                Currency = "EUR",
                CollectiveAgreementCode = "H011",
                ContractLevel = "4",
            },
            Year = 2026,
            Month = month,
            Agreement = agreement,
            Level = new CollectiveAgreementLevel { AgreementId = agreement.Id, Level = "4", MonthlyMinimum = minimum },
            Settings = new ItalianPayrollSettings(),
        };
    }
}
