// <copyright file="SwissVatCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests for <see cref="SwissVatCalculator"/> and <see cref="SwissVatRateSchedule"/>.
/// </summary>
public sealed class SwissVatCalculatorTests
{
    /// <summary>
    /// Verifies effective method calculation under 2024+ rates (8.1%, 2.6%, 3.8%), exempt supplies, and input VAT.
    /// </summary>
    [Fact]
    public void Calculate_EffectiveMethod_2024Rates_ComputesExpectedBoxes()
    {
        SwissVatCalculationInput input = new(
            PeriodFrom: new DateOnly(2026, 1, 1),
            PeriodTo: new DateOnly(2026, 3, 31),
            Method: SwissVatMethod.Effective,
            ConsiderationBasis: SwissVatConsiderationBasis.Agreed,
            SalesLines:
            [
                new SwissVatSaleLineInput(10000m, 8.10m, AfcBox: "303"),
                new SwissVatSaleLineInput(2000m, 2.60m, AfcBox: "313"),
                new SwissVatSaleLineInput(1000m, 3.80m, AfcBox: "343"),
                new SwissVatSaleLineInput(1500m, 0m, ExemptionType: SwissVatExemptionType.ExemptWithDeduction, AfcBox: "220"),
                new SwissVatSaleLineInput(500m, 0m, ExemptionType: SwissVatExemptionType.ExemptWithoutDeduction, AfcBox: "230"),
                new SwissVatSaleLineInput(300m, 0m, ExemptionType: SwissVatExemptionType.OutOfScope, AfcBox: "900"),
            ],
            PurchaseLines:
            [
                new SwissVatPurchaseLineInput(2000m, 162.00m, "400", true),
                new SwissVatPurchaseLineInput(1000m, 81.00m, "405", true),
            ],
            BezugsteuerEntries: [],
            AdjustmentsBox410: 50m,
            AdjustmentsBox415: 20m,
            AdjustmentsBox420: 10m);

        var result = SwissVatCalculator.Calculate(input);

        Assert.Equal(15000.00m, result.Box200TotalTurnover);
        Assert.Equal(2000.00m, result.Box289TotalDeductions);
        Assert.Equal(13000.00m, result.Box299TaxableTurnover);
        Assert.Equal(900.00m, result.Box399TotalOutputTax);
        Assert.Equal(263.00m, result.Box479TotalInputTax);
        Assert.Equal(637.00m, result.Box500PayableAmount);
        Assert.Equal(0m, result.Box510CreditAmount);
        Assert.Equal("303", result.Boxes.First(static b => string.Equals(b.Code, "303", StringComparison.Ordinal)).Code);
        Assert.Equal(300.00m, result.Boxes.First(static b => string.Equals(b.Code, "900", StringComparison.Ordinal)).Amount);
    }

    /// <summary>
    /// Verifies historical 2018-2023 rate epoch mapping (7.7%, 2.5%, 3.7% and Boxes 302/312/342/382).
    /// </summary>
    [Fact]
    public void Calculate_HistoricalEpoch2023_UsesLegacyBoxesAndRates()
    {
        SwissVatCalculationInput input = new(
            PeriodFrom: new DateOnly(2023, 10, 1),
            PeriodTo: new DateOnly(2023, 12, 31),
            Method: SwissVatMethod.Effective,
            ConsiderationBasis: SwissVatConsiderationBasis.Agreed,
            SalesLines:
            [
                new SwissVatSaleLineInput(1000m, 7.70m, AfcBox: "302"),
            ],
            PurchaseLines: [],
            BezugsteuerEntries: []);

        var result = SwissVatCalculator.Calculate(input);
        var box302 = result.Boxes.Single(static b => string.Equals(b.Code, "302", StringComparison.Ordinal));

        Assert.Equal(7.70m, box302.RatePercent);
        Assert.Equal(77.00m, box302.TaxAmount);
        Assert.Equal(77.00m, result.Box500PayableAmount);
    }

    /// <summary>
    /// Verifies Saldosteuersatz method applies SSS rate to gross turnover, forfeits standard input VAT,
    /// and includes Bezugsteuer separately.
    /// </summary>
    [Fact]
    public void Calculate_SaldosteuersatzMethod_AppliesRateToGrossTurnoverAndForfeitsInputVat()
    {
        var bez = new BezugsteuerEntryDto(
            Guid.NewGuid(),
            "Cloud AG Germany",
            "DE",
            "INV-1",
            new DateOnly(2026, 2, 10),
            BezugsteuerCategory.SoftwareCloud,
            "Cloud license",
            1000m,
            8.10m,
            81.00m,
            0m,
            "383",
            BezugsteuerStatus.Confirmed);

        SwissVatCalculationInput input = new(
            PeriodFrom: new DateOnly(2026, 1, 1),
            PeriodTo: new DateOnly(2026, 6, 30),
            Method: SwissVatMethod.Saldosteuersatz,
            ConsiderationBasis: SwissVatConsiderationBasis.Agreed,
            SalesLines:
            [
                new SwissVatSaleLineInput(10000m, 8.10m, SssRate: 6.20m),
            ],
            PurchaseLines:
            [
                new SwissVatPurchaseLineInput(1000m, 81.00m, "400", true),
            ],
            BezugsteuerEntries: [bez],
            SssRatePrimary: 6.20m);

        var result = SwissVatCalculator.Calculate(input);

        Assert.Equal(10810.00m, result.Box200TotalTurnover);
        Assert.Equal(0m, result.Box479TotalInputTax);
        Assert.Equal(81.00m, result.ForfeitedInputVat);
        Assert.Equal(670.22m + 81.00m, result.Box399TotalOutputTax);
        Assert.Equal(751.20m, result.Box500PayableAmount);
    }

    /// <summary>
    /// Verifies Swiss UID-MWST Mod-11 check digit validation and commercial 0.05 CHF rounding.
    /// </summary>
    [Fact]
    public void RateSchedule_UidValidationAndCommercialRounding_WorkAsSpecified()
    {
        Assert.True(SwissVatRateSchedule.IsValidUidMwst("CHE-116.281.710 MWST"));
        Assert.False(SwissVatRateSchedule.IsValidUidMwst("CHE-116.281.711 MWST"));
        Assert.False(SwissVatRateSchedule.IsValidUidMwst("CHE-000.000.000 MWST"));
        Assert.Equal(12.35m, SwissVatRateSchedule.RoundSwiss(12.33m, true));
        Assert.Equal(12.30m, SwissVatRateSchedule.RoundSwiss(12.32m, true));
    }
}
