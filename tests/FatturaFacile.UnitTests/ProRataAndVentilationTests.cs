// <copyright file="ProRataAndVentilationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the pro-rata of VAT deduction and of the scorporo and ventilazione of the register of corrispettivi.
/// </summary>
public sealed class ProRataAndVentilationTests
{
    /// <summary>
    /// The pro-rata is the share of taxable operations rounded up to the next whole percent.
    /// </summary>
    /// <param name="taxable">The taxable operations.</param>
    /// <param name="exempt">The exempt operations.</param>
    /// <param name="expected">The expected percentage.</param>
    [Theory]
    [InlineData(750, 250, 75)]
    [InlineData(333, 667, 34)]
    [InlineData(1000, 0, 100)]
    [InlineData(0, 0, 100)]
    [InlineData(0, 500, 0)]
    public void DeductiblePercent_RoundsUpToTheNextWholePercent(int taxable, int exempt, int expected) =>
        Assert.Equal(expected, ProRataCalculator.DeductiblePercent(taxable, exempt));

    /// <summary>
    /// The split of the VAT always adds up to the VAT.
    /// </summary>
    /// <param name="vat">The VAT.</param>
    /// <param name="percent">The deductible percentage.</param>
    [Theory]
    [InlineData(100.01, 33)]
    [InlineData(22.00, 75)]
    [InlineData(0.05, 50)]
    [InlineData(-44.00, 40)]
    public void Split_AddsUpToTheVat(double vat, int percent)
    {
        var (deductible, nonDeductible) = ProRataCalculator.Split((decimal)vat, percent);

        Assert.Equal((decimal)vat, deductible + nonDeductible);
    }

    /// <summary>
    /// The pro-rata of the next year comes from the exempt and taxable operations of the sales register of the previous year.
    /// </summary>
    [Fact]
    public void FromSales_UsesTheExemptAndTaxableOperationsOfTheYear()
    {
        List<VatRegisterEntry> sales =
        [
            new() { Kind = VatRegisterKind.Sales, VatRate = 22m, TaxableAmount = 600m },
            new() { Kind = VatRegisterKind.Sales, VatRate = 0m, VatNature = "N3.5", TaxableAmount = 150m },
            new() { Kind = VatRegisterKind.Sales, VatRate = 0m, VatNature = "N4", TaxableAmount = 250m },
            new() { Kind = VatRegisterKind.Sales, VatRate = 0m, VatNature = "N2.2", TaxableAmount = 9000m },
            new() { Kind = VatRegisterKind.Sales, VatRate = 22m, TaxableAmount = 5000m, IsReverseCharge = true },
            new() { Kind = VatRegisterKind.Purchases, VatRate = 22m, TaxableAmount = 7000m },
        ];

        Assert.Equal(75m, ProRataCalculator.FromSales(sales));
    }

    /// <summary>
    /// The scorporo splits the gross amount at one rate.
    /// </summary>
    [Fact]
    public void Scorporo_SplitsTheGrossAmount()
    {
        var (taxable, vat) = ReceiptsVentilation.Scorporo(122m, 22m);

        Assert.Equal(100m, taxable);
        Assert.Equal(22m, vat);
    }

    /// <summary>
    /// The ventilazione follows the taxable purchases of each rate and closes to the takings to the cent.
    /// </summary>
    [Fact]
    public void Distribute_FollowsThePurchasesMixAndClosesToTheTakings()
    {
        (decimal, decimal)[] purchases = [(22m, 6000m), (10m, 3000m), (4m, 1000m), (22m, 0m)];

        var lines = ReceiptsVentilation.Distribute(1000.01m, purchases);

        Assert.Equal(3, lines.Count);
        Assert.Equal(1000.01m, lines.Sum(static l => l.Taxable + l.Vat));
        Assert.All(lines, static l => Assert.InRange(Math.Abs((l.Taxable * l.Rate / 100m) - l.Vat), 0m, 0.01m));
        Assert.True(lines.Single(static l => l.Rate == 22m).Taxable > lines.Single(static l => l.Rate == 10m).Taxable);
    }

    /// <summary>
    /// Without purchases there is nothing to ventilate on, so the takings are split at the ordinary rate.
    /// </summary>
    [Fact]
    public void Distribute_WithoutPurchases_UsesTheOrdinaryRate()
    {
        var (rate, taxable, vat) = Assert.Single(ReceiptsVentilation.Distribute(244m, []));

        Assert.Equal(ReceiptsVentilation.OrdinaryRate, rate);
        Assert.Equal(200m, taxable);
        Assert.Equal(44m, vat);
    }
}
