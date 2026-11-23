// <copyright file="InputTaxAdjustmentCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests for <see cref="InputTaxAdjustmentCalculator"/>.
/// </summary>
public sealed class InputTaxAdjustmentCalculatorTests
{
    /// <summary>
    /// Verifies 5-year movable depreciation for Autoconsumo (Art. 31 LIVA -> Box 415).
    /// </summary>
    [Fact]
    public void Compute_AutoconsumoMovableAfter2Years_Returns60PercentOfOriginalVat()
    {
        var amount = InputTaxAdjustmentCalculator.Compute(
            InputTaxAdjustmentType.Autoconsumo,
            originalCostChf: 10000m,
            originalVatRate: 8.10m,
            yearsElapsed: 2,
            isRealEstate: false,
            ratioPercent: 100m);

        Assert.Equal(486.00m, amount);
        Assert.Equal("415", InputTaxAdjustmentCalculator.TargetAfcBox(InputTaxAdjustmentType.Autoconsumo));
    }

    /// <summary>
    /// Verifies 20-year real-estate depreciation for Degrassazione (Art. 32 LIVA -> Box 410).
    /// </summary>
    [Fact]
    public void Compute_DegrassazioneRealEstateAfter5Years_Returns75PercentOfOriginalVat()
    {
        var amount = InputTaxAdjustmentCalculator.Compute(
            InputTaxAdjustmentType.Degrassazione,
            originalCostChf: 100000m,
            originalVatRate: 8.10m,
            yearsElapsed: 5,
            isRealEstate: true,
            ratioPercent: 100m);

        Assert.Equal(6075.00m, amount);
        Assert.Equal("410", InputTaxAdjustmentCalculator.TargetAfcBox(InputTaxAdjustmentType.Degrassazione));
    }

    /// <summary>
    /// Verifies mixed-use and subsidy proportional reductions map to Boxes 415 and 420.
    /// </summary>
    [Fact]
    public void Compute_MixedUseAndSubsidy_ApplyProportionalRatioAndCorrectBoxes()
    {
        var mixed = InputTaxAdjustmentCalculator.Compute(
            InputTaxAdjustmentType.MixedUseReduction,
            originalCostChf: 10000m,
            originalVatRate: 8.10m,
            yearsElapsed: 0,
            isRealEstate: false,
            ratioPercent: 25m);

        var subsidy = InputTaxAdjustmentCalculator.Compute(
            InputTaxAdjustmentType.SubsidyReduction,
            originalCostChf: 10000m,
            originalVatRate: 8.10m,
            yearsElapsed: 0,
            isRealEstate: false,
            ratioPercent: 10m);

        Assert.Equal(202.50m, mixed);
        Assert.Equal(81.00m, subsidy);
        Assert.Equal("420", InputTaxAdjustmentCalculator.TargetAfcBox(InputTaxAdjustmentType.SubsidyReduction));
    }
}
