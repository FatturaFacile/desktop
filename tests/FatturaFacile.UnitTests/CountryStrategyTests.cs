// <copyright file="CountryStrategyTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Strategies.Italian;
using FatturaFacile.Application.Strategies.Swiss;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Services;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests validating country-specific strategies for Switzerland (CH) and Italy (IT).
/// </summary>
public sealed class CountryStrategyTests
{
    private static readonly DateTime _january2026 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

    /// <summary>
    /// Verifies Swiss 0.05 cash and invoice rounding logic.
    /// </summary>
    /// <param name="rawAmountStr">The unrounded gross total as string.</param>
    /// <param name="expectedRoundedStr">The expected 0.05 rounded amount as string.</param>
    [Theory]
    [InlineData("10.02", "10.00")]
    [InlineData("10.03", "10.05")]
    [InlineData("10.07", "10.05")]
    [InlineData("10.08", "10.10")]
    [InlineData("100.00", "100.00")]
    public void SwissTaxStrategy_RoundsToNearestFiveRappen(string rawAmountStr, string expectedRoundedStr)
    {
        var rawAmount = decimal.Parse(rawAmountStr, CultureInfo.InvariantCulture);
        var expectedRounded = decimal.Parse(expectedRoundedStr, CultureInfo.InvariantCulture);

        var strategy = new SwissTaxAndRoundingStrategy();
        var rounded = strategy.RoundFinalAmount(rawAmount);

        Assert.Equal(expectedRounded, rounded);
    }

    /// <summary>
    /// Verifies Italian standard cent rounding logic.
    /// </summary>
    [Fact]
    public void ItalianTaxStrategy_RoundsToStandardTwoDecimals()
    {
        var strategy = new ItalianTaxAndRoundingStrategy();
        var rounded = strategy.RoundFinalAmount(10.024m);

        Assert.Equal(10.02m, rounded);
    }

    /// <summary>
    /// Verifies that a regular IBAN produces an ISO 11649 creditor reference (SCOR).
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SwissInvoicingStrategy_UsesScorWithRegularIban()
    {
        var strategy = new SwissInvoicingStrategy(new QrCoderQrCodeEncoder());
        var tenant = TestData.SwissTenant();
        var invoice = TestData.Invoice(tenant.Id, "CHF", TestData.Line("Hosting", 1m, 1250.50m, 0m));

        await strategy.ProcessInvoiceComplianceAsync(invoice, tenant, TestData.SwissContact());

        Assert.NotNull(invoice.QrBillData);
        Assert.Null(invoice.QrBillData.ValidationErrors);
        Assert.Equal("SCOR", invoice.QrBillData.ReferenceType);
        Assert.True(SwissPaymentReference.IsValidCreditorReference(invoice.QrBillData.PaymentReference));
    }

    /// <summary>
    /// Verifies that a creditor without structured address yields validation errors and no payload.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SwissInvoicingStrategy_ReportsMissingCreditorAddress()
    {
        var strategy = new SwissInvoicingStrategy(new QrCoderQrCodeEncoder());
        var tenant = TestData.SwissTenant();
        tenant.PostalCode = null;
        tenant.City = null;
        var invoice = TestData.Invoice(tenant.Id, "CHF", TestData.Line("Hosting", 1m, 100m, 8.1m));

        await strategy.ProcessInvoiceComplianceAsync(invoice, tenant, TestData.SwissContact());

        Assert.NotNull(invoice.QrBillData);
        Assert.NotNull(invoice.QrBillData.ValidationErrors);
        Assert.Null(invoice.QrBillData.QrPayload);
    }

    /// <summary>
    /// Verifies Swiss payroll with 2026 parameters (age 46: LPP age credit 15%, employee half on the max coordinated salary).
    /// </summary>
    [Fact]
    public void SwissPayrollStrategy_CalculatesStatutoryDeductions()
    {
        var employee = new HrEmployee
        {
            BaseAnnualSalary = 120_000m,
            Currency = "CHF",
            DateOfBirth = new DateTime(1980, 5, 1, 0, 0, 0, DateTimeKind.Unspecified),
        };

        var result = new SwissPayrollStrategy().CalculatePayroll(employee, _january2026);

        Assert.Equal(10_000m, result.GrossMonthlySalary);
        Assert.Equal(530.00m, result.DeductionBreakdown["AVS/AI/IPG"]);
        Assert.Equal(110.00m, result.DeductionBreakdown["AD"]);
        Assert.Equal(401.63m, result.DeductionBreakdown["LPP/BVG"]);
        Assert.Equal(1_041.63m, result.TotalDeductions);
        Assert.Equal(8_958.37m, result.NetMonthlySalary);
        Assert.DoesNotContain("Certified", result.ComplianceNotes, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies the AD ceiling and the withholding tax rate taken from the employee tariff.
    /// </summary>
    [Fact]
    public void SwissPayrollStrategy_AppliesAlvCeilingAndWithholdingTax()
    {
        var employee = new HrEmployee
        {
            BaseAnnualSalary = 160_000m,
            DateOfBirth = new DateTime(2000, 2, 1, 0, 0, 0, DateTimeKind.Unspecified),
            IsWithholdingTaxSubject = true,
            WithholdingTaxCode = "A0N",
            WithholdingTaxRatePercent = 12m,
        };

        var result = new SwissPayrollStrategy().CalculatePayroll(employee, _january2026);

        Assert.Equal(706.67m, result.DeductionBreakdown["AVS/AI/IPG"]);
        Assert.Equal(135.85m, result.DeductionBreakdown["AD"]);
        Assert.Equal(187.43m, result.DeductionBreakdown["LPP/BVG"]);
        Assert.Equal(1_600.00m, result.DeductionBreakdown["Imposta alla fonte"]);
    }

    /// <summary>
    /// Verifies the AHV age rules: retiree allowance and no contributions before the contribution age.
    /// </summary>
    /// <param name="birthYear">The birth year.</param>
    /// <param name="expectedAhv">The expected AHV deduction.</param>
    [Theory]
    [InlineData(1958, "190.80")]
    [InlineData(2010, "0.00")]
    public void SwissPayrollStrategy_AppliesAhvAgeRules(int birthYear, string expectedAhv)
    {
        var employee = new HrEmployee
        {
            BaseAnnualSalary = 60_000m,
            DateOfBirth = new DateTime(birthYear, 6, 1, 0, 0, 0, DateTimeKind.Unspecified),
        };

        var result = new SwissPayrollStrategy().CalculatePayroll(employee, _january2026);

        Assert.Equal(decimal.Parse(expectedAhv, CultureInfo.InvariantCulture), result.DeductionBreakdown["AVS/AI/IPG"]);
        Assert.Equal(0m, result.DeductionBreakdown["AD"]);
    }

    /// <summary>
    /// Verifies Italian payroll with 2026 IRPEF brackets, art. 13 TUIR deduction and tax-wedge deduction.
    /// </summary>
    /// <param name="annual">The annual gross salary.</param>
    /// <param name="installments">The number of installments.</param>
    /// <param name="expectedInps">The expected monthly INPS.</param>
    /// <param name="expectedIrpef">The expected monthly net IRPEF.</param>
    /// <param name="expectedNet">The expected monthly net salary.</param>
    [Theory]
    [InlineData("36000", 12, "275.70", "458.92", "2265.38")]
    [InlineData("18000", 12, "137.85", "65.23", "1362.30")]
    public void ItalianPayrollStrategy_CalculatesStatutoryDeductions(
        string annual,
        int installments,
        string expectedInps,
        string expectedIrpef,
        string expectedNet)
    {
        var employee = new HrEmployee
        {
            BaseAnnualSalary = decimal.Parse(annual, CultureInfo.InvariantCulture),
            SalaryInstallmentsPerYear = installments,
            Currency = "EUR",
        };

        var result = new ItalianPayrollStrategy().CalculatePayroll(employee, _january2026);

        Assert.Equal(decimal.Parse(expectedInps, CultureInfo.InvariantCulture), result.DeductionBreakdown["Contributi INPS dipendente"]);
        Assert.Equal(decimal.Parse(expectedIrpef, CultureInfo.InvariantCulture), result.DeductionBreakdown["IRPEF netta"]);
        Assert.Equal(decimal.Parse(expectedNet, CultureInfo.InvariantCulture), result.NetMonthlySalary);
    }

    /// <summary>
    /// Verifies the 2026 IRPEF brackets (23% / 33% / 43%).
    /// </summary>
    [Fact]
    public void ItalianPayrollParameters_ApplyIrpefBrackets()
    {
        Assert.Equal(6_440m, ItalianPayrollParameters.ComputeGrossIrpef(28_000m));
        Assert.Equal(13_700m, ItalianPayrollParameters.ComputeGrossIrpef(50_000m));
        Assert.Equal(18_000m, ItalianPayrollParameters.ComputeGrossIrpef(60_000m));
    }
}
