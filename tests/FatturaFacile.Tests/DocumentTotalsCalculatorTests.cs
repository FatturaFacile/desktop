// <copyright file="DocumentTotalsCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Services;
using Xunit;

namespace FatturaFacile.Tests;

/// <summary>
/// Unit tests verifying document line subtotaling, VAT computations, and Swiss QR-Bill 0.05 CHF rounding algorithms.
/// </summary>
public class DocumentTotalsCalculatorTests
{
    private readonly DocumentTotalsCalculator _calculator = new();

    /// <summary>
    /// Verifies standard EUR currency calculation without rounding adjustments.
    /// </summary>
    [Fact]
    public void Calculate_StandardEur_ComputesCorrectTotalsWithoutChfRounding()
    {
        // Arrange
        var lines = new List<DocumentLineInput>
        {
            new(null, "Consulting", 10.0m, 100.00m, 22.0m, 10.0m), // 10 * 100 * 0.9 = 900 net, tax = 198 (22%)
        };

        // Act
        var result = _calculator.Calculate(lines, "EUR");

        // Assert
        Assert.Equal(900.00m, result.TotalNet);
        Assert.Equal(198.00m, result.TotalTax);
        Assert.Equal(1098.00m, result.TotalGross);
        Assert.Equal(0.00m, result.RoundingAmount);
    }

    /// <summary>
    /// Verifies that Swiss 5 centimes rounding accurately rounds fractional amounts to the nearest 0.05 CHF boundary.
    /// </summary>
    /// <param name="inputStr">The unrounded input amount.</param>
    /// <param name="expectedGrossStr">The expected rounded gross amount.</param>
    /// <param name="expectedDiffStr">The expected rounding adjustment difference.</param>
    [Theory]
    [InlineData("100.00", "100.00", "0.00")]
    [InlineData("100.01", "100.00", "-0.01")]
    [InlineData("100.02", "100.00", "-0.02")]
    [InlineData("100.03", "100.05", "0.02")]
    [InlineData("100.04", "100.05", "0.01")]
    [InlineData("100.05", "100.05", "0.00")]
    [InlineData("100.06", "100.05", "-0.01")]
    [InlineData("100.07", "100.05", "-0.02")]
    [InlineData("100.08", "100.10", "0.02")]
    [InlineData("100.09", "100.10", "0.01")]
    [InlineData("100.10", "100.10", "0.00")]
    public void RoundChfAmount_FollowsSwissFiveCentimesRoundingRule(string inputStr, string expectedGrossStr, string expectedDiffStr)
    {
        var input = decimal.Parse(inputStr, CultureInfo.InvariantCulture);
        var expectedGross = decimal.Parse(expectedGrossStr, CultureInfo.InvariantCulture);
        var expectedDiff = decimal.Parse(expectedDiffStr, CultureInfo.InvariantCulture);

        var rounded = DocumentTotalsCalculator.RoundChfAmount(input);
        var diff = Math.Round(rounded - input, 2, MidpointRounding.AwayFromZero);

        Assert.Equal(expectedGross, rounded);
        Assert.Equal(expectedDiff, diff);
    }

    /// <summary>
    /// Verifies that invoice totals calculated in CHF currency apply Swiss 0.05 CHF rounding adjustments.
    /// </summary>
    [Fact]
    public void Calculate_ChfCurrency_AppliesSwissRounding()
    {
        // 1 item with price 100.03 and 0% tax -> unrounded = 100.03, rounded = 100.05, diff = +0.02
        var lines = new List<DocumentLineInput>
        {
            new(null, "Test Item", 1.0m, 100.03m, 0.0m, 0.0m),
        };

        var result = _calculator.Calculate(lines, "CHF");

        Assert.Equal(100.03m, result.TotalNet);
        Assert.Equal(0.00m, result.TotalTax);
        Assert.Equal(100.05m, result.TotalGross);
        Assert.Equal(0.02m, result.RoundingAmount);
    }
}
