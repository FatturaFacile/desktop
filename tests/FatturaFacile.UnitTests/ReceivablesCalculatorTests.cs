// <copyright file="ReceivablesCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Domain.Receivables;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the DSO, the bad-debt provision and the composite credit risk score.
/// </summary>
public sealed class ReceivablesCalculatorTests
{
    /// <summary>
    /// DSO is receivables over credit sales times the days of the period.
    /// </summary>
    [Fact]
    public void Dso_ReceivablesAndSales_IsTheRatioTimesThePeriod()
    {
        Assert.Equal(30.00m, DsoCalculator.Calculate(1000m, 3000m, 90));
        Assert.Equal(12.86m, DsoCalculator.Calculate(100m, 700m, 90));
        Assert.Equal(60.00m, DsoCalculator.Calculate(1000m, 1000m, 60));
    }

    /// <summary>
    /// With receivables but no sales the DSO is undefined, not infinite and not zero.
    /// </summary>
    [Fact]
    public void Dso_ReceivablesButNoSales_IsUndefined()
    {
        Assert.Null(DsoCalculator.Calculate(500m, 0m));
        Assert.Null(DsoCalculator.Calculate(500m, -20m, 30));
    }

    /// <summary>
    /// With neither receivables nor sales the DSO is zero.
    /// </summary>
    [Fact]
    public void Dso_NothingAtAll_IsZero() =>
        Assert.Equal(0m, DsoCalculator.Calculate(0m, 0m));

    /// <summary>
    /// The period is clamped between one day and 3660 days.
    /// </summary>
    [Fact]
    public void Dso_PeriodOutOfRange_IsClamped()
    {
        Assert.Equal(1.00m, DsoCalculator.Calculate(100m, 100m, 0));
        Assert.Equal(366000.00m, DsoCalculator.Calculate(100m, 1m, 100000));
        Assert.Equal(1.00m, DsoCalculator.Calculate(100m, 100m, -4));
    }

    /// <summary>
    /// The default rates are 0.5, 1, 5, 10, 25 and 50 percent of the open amount of each bucket.
    /// </summary>
    [Fact]
    public void Provision_DefaultRates_ApplyHalfOneFiveTenTwentyFiveFiftyPercent()
    {
        var engine = new AgingBucketEngine();
        var totals = engine.Buckets.ToDictionary(bucket => bucket.Key, _ => 1000m, StringComparer.Ordinal);

        var summary = BadDebtProvisionCalculator.Calculate(engine.Buckets, totals, null);

        Assert.Equal([5m, 10m, 50m, 100m, 250m, 500m], summary.Lines.Select(line => line.Provision));
        Assert.All(summary.Lines, line => Assert.True(line.IsDefaultRate));
        Assert.Equal(6000m, summary.TotalOpen);
        Assert.Equal(915m, summary.TotalProvision);
        Assert.Equal(5085m, summary.NetRealizableValue);
    }

    /// <summary>
    /// A configured rate is used for its bucket and flagged as not default; an invalid rate is ignored.
    /// </summary>
    [Fact]
    public void Provision_ConfiguredRate_ReplacesTheDefaultOfThatBucket()
    {
        var engine = new AgingBucketEngine();
        var totals = engine.Buckets.ToDictionary(bucket => bucket.Key, _ => 1000m, StringComparer.Ordinal);
        Dictionary<string, decimal> rates = new(StringComparer.Ordinal) { ["61_90"] = 7.5m, ["91_120"] = 150m };

        var summary = BadDebtProvisionCalculator.Calculate(engine.Buckets, totals, rates);

        var custom = summary.Lines.Single(line => string.Equals(line.Key, "61_90", StringComparison.Ordinal));
        Assert.Equal(75.00m, custom.Provision);
        Assert.False(custom.IsDefaultRate);
        var invalid = summary.Lines.Single(line => string.Equals(line.Key, "91_120", StringComparison.Ordinal));
        Assert.Equal(250m, invalid.Provision);
        Assert.True(invalid.IsDefaultRate);
    }

    /// <summary>
    /// A configuration with more buckets than default rates reuses the last default rate.
    /// </summary>
    [Fact]
    public void Provision_MoreBucketsThanDefaults_ReuseTheLastRate()
    {
        var engine = new AgingBucketEngine([10, 20, 30, 40, 50, 60]);
        var totals = engine.Buckets.ToDictionary(bucket => bucket.Key, _ => 100m, StringComparer.Ordinal);

        var summary = BadDebtProvisionCalculator.Calculate(engine.Buckets, totals, null);

        Assert.Equal(8, summary.Lines.Count);
        Assert.Equal(50m, summary.Lines[^1].RatePercent);
        Assert.Equal(50m, summary.Lines[^2].RatePercent);
    }

    /// <summary>
    /// The provision of a line is rounded to two decimals, midpoint away from zero.
    /// </summary>
    [Fact]
    public void Provision_RoundsEveryLineToTwoDecimals()
    {
        AgingBucket[] buckets = [new("current", 0, 0)];

        var summary = BadDebtProvisionCalculator.Calculate(buckets, new Dictionary<string, decimal>(StringComparer.Ordinal) { ["current"] = 33.33m }, new Dictionary<string, decimal>(StringComparer.Ordinal) { ["current"] = 0.5m });

        Assert.Equal(0.17m, summary.Lines[0].Provision);
    }

    /// <summary>
    /// A customer with no exposure problem scores zero and keeps the six factors in order.
    /// </summary>
    [Fact]
    public void Risk_NothingWrong_IsLowWithSixFactors()
    {
        var risk = CreditRiskCalculator.Assess(new RiskInputs(null, 0m, 0, 0m, 0m, Blocked: false));

        Assert.Equal(0, risk.Score);
        Assert.Equal(RiskRating.Low, risk.Rating);
        Assert.Equal(["credit_utilization", "overdue_share", "max_days_past_due", "payment_lateness_days", "uninsured_share", "credit_block"], risk.Factors.Select(factor => factor.Key), StringComparer.Ordinal);
        Assert.Null(risk.Factors[0].Value);
        Assert.All(risk.Factors, factor => Assert.Equal(0m, factor.Points));
    }

    /// <summary>
    /// Each factor contributes its points: utilisation, overdue share, oldest overdue, lateness, uninsured share and the block.
    /// </summary>
    [Fact]
    public void Risk_MixedFactors_SumsThePointsOfEveryFactor()
    {
        var risk = CreditRiskCalculator.Assess(new RiskInputs(100m, 0.5m, 60, 30m, 1m, Blocked: true));

        Assert.Equal(20m, risk.Factors[0].Points);
        Assert.Equal(12.5m, risk.Factors[1].Points);
        Assert.Equal(12.5m, risk.Factors[2].Points);
        Assert.Equal(7.5m, risk.Factors[3].Points);
        Assert.Equal(5m, risk.Factors[4].Points);
        Assert.Equal(5m, risk.Factors[5].Points);
        Assert.Equal(63, risk.Score);
        Assert.Equal(RiskRating.High, risk.Rating);
        Assert.True(risk.Blocked);
    }

    /// <summary>
    /// Factors are capped, so the worst customer scores exactly one hundred.
    /// </summary>
    [Fact]
    public void Risk_EverythingAtTheCap_IsHundredAndCritical()
    {
        var risk = CreditRiskCalculator.Assess(new RiskInputs(250m, 3m, 900, 400m, 1m, Blocked: true));

        Assert.Equal(100, risk.Score);
        Assert.Equal(RiskRating.Critical, risk.Rating);
        Assert.Equal(25m, risk.Factors[0].Points);
    }

    /// <summary>
    /// Utilisation of the limit adds points only above seventy percent, and five more beyond the limit.
    /// </summary>
    [Fact]
    public void Risk_UtilizationBelowSeventyPercent_AddsNothing()
    {
        Assert.Equal(0m, CreditRiskCalculator.Assess(new RiskInputs(70m, 0m, 0, 0m, 0m, false)).Factors[0].Points);
        Assert.Equal(10m, CreditRiskCalculator.Assess(new RiskInputs(85m, 0m, 0, 0m, 0m, false)).Factors[0].Points);
        Assert.Equal(25m, CreditRiskCalculator.Assess(new RiskInputs(101m, 0m, 0, 0m, 0m, false)).Factors[0].Points);
    }

    /// <summary>
    /// The rating thresholds are below 25 low, below 50 medium, below 75 high, else critical.
    /// </summary>
    /// <param name="score">The score.</param>
    /// <param name="expected">The expected rating.</param>
    [Theory]
    [InlineData(0, RiskRating.Low)]
    [InlineData(24, RiskRating.Low)]
    [InlineData(25, RiskRating.Medium)]
    [InlineData(49, RiskRating.Medium)]
    [InlineData(50, RiskRating.High)]
    [InlineData(74, RiskRating.High)]
    [InlineData(75, RiskRating.Critical)]
    [InlineData(100, RiskRating.Critical)]
    public void Rate_Boundaries_FollowTheThresholds(int score, RiskRating expected) =>
        Assert.Equal(expected, CreditRiskCalculator.Rate(score));

    /// <summary>
    /// The overdue share and the uninsured share are ratios of the open invoices and of the total exposure.
    /// </summary>
    [Fact]
    public void BuildInputs_FromAnExposure_ComputesTheShares()
    {
        var exposure = new CreditExposure(1000m, 0m, 200m, 0m, 1200m, 250m, 2, 45);

        var inputs = CreditRiskCalculator.BuildInputs(exposure, 80m, 12m, 600m, blocked: false);

        Assert.Equal(0.25m, inputs.OverdueShare);
        Assert.Equal(0.5m, inputs.UninsuredShare);
        Assert.Equal(45, inputs.MaxDaysPastDue);
        Assert.Equal(80m, inputs.UtilizationPercent);
    }
}
