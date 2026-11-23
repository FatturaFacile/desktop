// <copyright file="PurchasePricingCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Globalization;
using FatturaFacile.Application.Procurement;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the local pricing estimate of a purchase line (Mod 046): cascading discounts, dual unit of measure and company currency.
/// </summary>
public sealed class PurchasePricingCalculatorTests
{
    /// <summary>
    /// Two cascading discounts multiply instead of adding, and the dual unit of measure and the currency follow from the net amounts.
    /// </summary>
    [Fact]
    public void Compute_CascadeFactorAndRate_AreAppliedInOrder()
    {
        var preview = PurchasePricingCalculator.Compute(new PricingPreviewRequest(10m, 100m, [10m, 5m], 12m, 1.1m, 22m));

        Assert.Equal(14.5m, preview.EffectiveDiscountPercent);
        Assert.Equal(85.5m, preview.NetUnitPrice);
        Assert.Equal(855m, preview.LineNetAmount);
        Assert.Equal(188.10m, preview.LineTaxAmount);
        Assert.Equal(1043.10m, preview.LineTotalAmount);
        Assert.Equal(120m, preview.QuantityBase);
        Assert.Equal(7.125m, preview.NetPricePerBaseUnit);
        Assert.Equal(940.5m, preview.LineNetAmountCompanyCurrency);
        Assert.Equal([10m, 5m], preview.DiscountCascade);
    }

    /// <summary>
    /// Without a cascade, a factor, a rate or a tax rate the amounts are the plain quantity times price.
    /// </summary>
    [Fact]
    public void Compute_NoOptions_UsesPlainAmounts()
    {
        var preview = PurchasePricingCalculator.Compute(new PricingPreviewRequest(3m, 20m, [], null, null, null));

        Assert.Equal(0m, preview.EffectiveDiscountPercent);
        Assert.Equal(60m, preview.LineNetAmount);
        Assert.Equal(0m, preview.LineTaxAmount);
        Assert.Equal(60m, preview.LineTotalAmount);
        Assert.Equal(3m, preview.QuantityBase);
        Assert.Equal(20m, preview.NetPricePerBaseUnit);
        Assert.Equal(60m, preview.LineNetAmountCompanyCurrency);
    }

    /// <summary>
    /// A step outside 0 to 100 percent is dropped from the cascade.
    /// </summary>
    [Fact]
    public void NormalizeCascade_DropsInvalidSteps()
    {
        Assert.Equal([10m, 0m, 100m], PurchasePricingCalculator.NormalizeCascade([10m, -1m, 0m, 100m, 101m]));
        Assert.Empty(PurchasePricingCalculator.NormalizeCascade(null));
    }

    /// <summary>
    /// A typed cascade accepts several separators and both decimal marks.
    /// </summary>
    /// <param name="text">The typed text.</param>
    /// <param name="expected">The expected steps, as a comma separated invariant list.</param>
    [Theory]
    [InlineData("10 + 5", "10,5")]
    [InlineData("10;5,5", "10,5.5")]
    [InlineData("12.5/2", "12.5,2")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void TryParseCascade_ValidText_ReturnsTheSteps(string? text, string expected)
    {
        var ok = PurchasePricingCalculator.TryParseCascade(text, out var steps);

        Assert.True(ok);
        Assert.Equal(expected, string.Join(',', steps.Select(step => step.ToString(CultureInfo.InvariantCulture))));
    }

    /// <summary>
    /// A step that is not a number or not a percentage makes the whole text invalid.
    /// </summary>
    /// <param name="text">The typed text.</param>
    [Theory]
    [InlineData("abc")]
    [InlineData("10 + 120")]
    [InlineData("-5")]
    public void TryParseCascade_InvalidText_ReturnsFalse(string text) =>
        Assert.False(PurchasePricingCalculator.TryParseCascade(text, out _));

    /// <summary>
    /// The cascade is shown step by step with a percent sign.
    /// </summary>
    [Fact]
    public void FormatCascade_ListsTheSteps()
    {
        Assert.Equal("10% + 5.5%", PurchasePricingCalculator.FormatCascade([10m, 5.5m], CultureInfo.InvariantCulture));
        Assert.Equal(string.Empty, PurchasePricingCalculator.FormatCascade([], CultureInfo.InvariantCulture));
    }
}
