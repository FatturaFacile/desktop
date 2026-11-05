// <copyright file="CountryStrategyTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Strategies.Italian;
using FatturaFacile.Application.Strategies.Swiss;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests validating country-specific strategies for Switzerland (CH) and Italy (IT).
/// </summary>
public sealed class CountryStrategyTests
{
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
        var rawAmount = decimal.Parse(rawAmountStr, System.Globalization.CultureInfo.InvariantCulture);
        var expectedRounded = decimal.Parse(expectedRoundedStr, System.Globalization.CultureInfo.InvariantCulture);

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
    /// Verifies Swiss QR-Bill metadata generation adheres to ISO 20022 and SCOR standards.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SwissInvoicingStrategy_GeneratesQrBillPayload()
    {
        var strategy = new SwissInvoicingStrategy();
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "FatturaFacile Schweiz AG",
            Country = "CH",
            Iban = "CH9300762011623852957",
            VatNumber = "CHE-123.456.789",
        };

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            CompanyName = "Muster AG",
            Country = "CH",
            QrIban = "CH4431999123000889012",
        };

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            InvoiceNumber = "FAT-CH-2026-001",
            Date = DateTime.UtcNow,
            TotalGross = 1250.50m,
            Currency = "CHF",
        };

        await strategy.ProcessInvoiceComplianceAsync(invoice, tenant, contact);

        Assert.NotNull(invoice.QrBillData);
        Assert.StartsWith("CH", invoice.QrBillData.QrIban, StringComparison.Ordinal);
        Assert.NotEmpty(invoice.QrBillData.ScorReference);
    }

    /// <summary>
    /// Verifies Italian FatturaPA SDI electronic invoice compliance generation.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ItalianInvoicingStrategy_GeneratesSdiMetadata()
    {
        var strategy = new ItalianInvoicingStrategy();
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "FatturaFacile Italia S.r.l.",
            Country = "IT",
            VatNumber = "IT01234567890",
        };

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            CompanyName = "Rossi S.r.l.",
            VatNumber = "IT09876543210",
            FiscalCode = "09876543210",
            SdiCode = "M5UXCR1",
            Country = "IT",
        };

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            InvoiceNumber = "FAT-IT-2026-001",
            Date = DateTime.UtcNow,
            TotalGross = 2440.00m,
            Currency = "EUR",
        };

        await strategy.ProcessInvoiceComplianceAsync(invoice, tenant, contact);

        Assert.NotNull(invoice.SdiMetadata);
        Assert.Equal("FPR12", invoice.SdiMetadata.TransmissionFormat);
        Assert.Equal("validated", invoice.SdiMetadata.SdiStatus);
        Assert.StartsWith("IT01234567890_", invoice.SdiMetadata.FileName, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies Swissdec ELM payroll deductions calculation (AVS/AI, AC, LPP, QST).
    /// </summary>
    [Fact]
    public void SwissPayrollStrategy_CalculatesAccurateStatutoryDeductions()
    {
        var strategy = new SwissPayrollStrategy();
        var employee = new HrEmployee
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BaseAnnualSalary = 120_000m,
            Currency = "CHF",
        };

        var result = strategy.CalculatePayroll(employee, DateTime.UtcNow);

        Assert.Equal(10000m, result.GrossMonthlySalary);
        Assert.Equal("CHF", result.Currency);
        Assert.Equal("CH", result.CountryCode);

        Assert.Equal(530m, result.DeductionBreakdown["AVS/AI/APG (5.30%)"]);
        Assert.Equal(110m, result.DeductionBreakdown["AC (1.10%)"]);
        Assert.Equal(700m, result.DeductionBreakdown["LPP / BVG (7.00%)"]);
        Assert.Equal(1000m, result.DeductionBreakdown["QST Imposta alla fonte (10.00%)"]);

        const decimal expectedTotalDeductions = 530m + 110m + 700m + 1000m;
        Assert.Equal(expectedTotalDeductions, result.TotalDeductions);
        Assert.Equal(10000m - expectedTotalDeductions, result.NetMonthlySalary);
    }

    /// <summary>
    /// Verifies Italian statutory payroll deductions (INPS, IRPEF, TFR).
    /// </summary>
    [Fact]
    public void ItalianPayrollStrategy_CalculatesAccurateStatutoryDeductions()
    {
        var strategy = new ItalianPayrollStrategy();
        var employee = new HrEmployee
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BaseAnnualSalary = 36_000m,
            Currency = "EUR",
        };

        var result = strategy.CalculatePayroll(employee, DateTime.UtcNow);

        Assert.Equal(3000m, result.GrossMonthlySalary);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal("IT", result.CountryCode);

        Assert.Equal(275.70m, result.DeductionBreakdown["Contributi Previdenziali INPS (9.19%)"]);
        Assert.Equal(690.00m, result.DeductionBreakdown["Ritenuta Fiscale IRPEF Base (23.00%)"]);

        Assert.True(result.NetMonthlySalary > 0);
        Assert.True(result.NetMonthlySalary < result.GrossMonthlySalary);
    }
}
