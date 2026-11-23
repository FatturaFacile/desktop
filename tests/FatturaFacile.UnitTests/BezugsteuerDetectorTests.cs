// <copyright file="BezugsteuerDetectorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests for <see cref="BezugsteuerDetector"/>.
/// </summary>
public sealed class BezugsteuerDetectorTests
{
    /// <summary>
    /// Verifies foreign cloud/SaaS invoices without domestic CH VAT are flagged for Box 383 with matching deductible input VAT under Effective method.
    /// </summary>
    [Fact]
    public void Evaluate_ForeignCloudService_DetectsBox383Entry()
    {
        var dto = BezugsteuerDetector.Evaluate(
            "AWS EMEA SARL",
            "lu",
            "AWS-2026-001",
            new DateOnly(2026, 2, 15),
            "AWS Cloud Hosting Subscription",
            1000m,
            0m,
            SwissVatMethod.Effective);

        Assert.NotNull(dto);
        Assert.Equal("LU", dto.SupplierCountry);
        Assert.Equal(BezugsteuerCategory.SoftwareCloud, dto.ServiceCategory);
        Assert.Equal("383", dto.AfcBox);
        Assert.Equal(81.00m, dto.TaxAmountChf);
        Assert.Equal(81.00m, dto.DeductibleInputTaxChf);
        Assert.Equal(BezugsteuerStatus.Detected, dto.Status);
    }

    /// <summary>
    /// Verifies domestic CH and Liechtenstein LI suppliers are exempt from Bezugsteuer.
    /// </summary>
    [Theory]
    [InlineData("CH", 0)]
    [InlineData("LI", 0)]
    [InlineData("DE", 81)]
    public void Evaluate_DomesticOrAlreadyTaxed_ReturnsNull(string country, decimal chargedVat)
    {
        var dto = BezugsteuerDetector.Evaluate(
            "Partner AG",
            country,
            "INV-1",
            new DateOnly(2026, 2, 15),
            "IT Consulting",
            1000m,
            chargedVat,
            SwissVatMethod.Effective);

        Assert.Null(dto);
    }

    /// <summary>
    /// Verifies Saldosteuersatz taxpayers owe Bezugsteuer on Box 383 with zero input tax deduction.
    /// </summary>
    [Fact]
    public void Evaluate_SaldosteuersatzMethod_SetsZeroDeductibleInputVat()
    {
        var dto = BezugsteuerDetector.Evaluate(
            "Beratung GmbH",
            "AT",
            "AT-2026-9",
            new DateOnly(2026, 4, 1),
            "Management Beratung",
            2000m,
            0m,
            SwissVatMethod.Saldosteuersatz);

        Assert.NotNull(dto);
        Assert.Equal(BezugsteuerCategory.Consulting, dto.ServiceCategory);
        Assert.Equal(162.00m, dto.TaxAmountChf);
        Assert.Equal(0m, dto.DeductibleInputTaxChf);
    }
}
