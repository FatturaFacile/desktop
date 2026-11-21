// <copyright file="DssWhatIfCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Dss;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Parity tests of the offline what-if calculator: every case below is the same case, with the same expected figures, as the
/// unit and feature tests of the server side `WhatIfScenarioSimulator` (web-api, Mod 093).
/// </summary>
public sealed class DssWhatIfCalculatorTests
{
    private const double Tolerance = 0.0001;

    /// <summary>
    /// The documented formulas give the server figures for price, COGS, DSO and FTE changes.
    /// </summary>
    [Fact]
    public void Simulate_PriceCogsDsoAndFte_MatchesServer()
    {
        var baseline = new WhatIfBaseline(1000, 400, 300, 36.5, 100, 365, FteAnnualCost: 50, RevenueFxShare: 0.5, CostFxShare: 0);

        var result = WhatIfScenarioCalculator.Simulate(baseline, new WhatIfScenario(PricePct: 10, CogsPct: 5, DsoDaysDelta: 10, FteDelta: 2));

        Assert.Equal(300, result.Baseline.Ebitda);
        Assert.Equal(1100, result.Simulated.Revenue);
        Assert.Equal(420, result.Simulated.Cogs);
        Assert.Equal(400, result.Simulated.Opex);
        Assert.Equal(280, result.Simulated.Ebitda);
        Assert.Equal(39.863, result.Simulated.NetCashFlow, Tolerance);
        Assert.Equal(647.0588, result.Simulated.BreakEvenRevenue!.Value, Tolerance);
        Assert.Equal(-20, result.Delta.Ebitda);
    }

    /// <summary>
    /// An FX shift moves only the exposed share of revenue and costs.
    /// </summary>
    [Fact]
    public void Simulate_FxShift_MovesOnlyTheExposedShare()
    {
        var baseline = new WhatIfBaseline(1000, 400, 300, 30, 0, 365, RevenueFxShare: 0.5, CostFxShare: 0.2);

        var result = WhatIfScenarioCalculator.Simulate(baseline, new WhatIfScenario(FxShiftPct: 10));

        Assert.Equal(1050, result.Simulated.Revenue);
        Assert.Equal(408, result.Simulated.Cogs);
        Assert.Equal(306, result.Simulated.Opex);
    }

    /// <summary>
    /// A neutral scenario changes nothing.
    /// </summary>
    [Fact]
    public void Simulate_NeutralScenario_ChangesNothing()
    {
        var baseline = new WhatIfBaseline(1000, 400, 300, 30, 0, 365, RevenueFxShare: 0.5, CostFxShare: 0.2);

        var result = WhatIfScenarioCalculator.Simulate(baseline, new WhatIfScenario());

        Assert.Equal(0, result.Delta.Ebitda);
        Assert.Equal(0, result.Delta.NetCashFlow);
        Assert.Equal(0, result.Delta.Revenue);
        Assert.Equal(0, result.Delta.BreakEvenRevenue);
    }

    /// <summary>
    /// The server feature test: +10% price on 1000 of revenue, 400 of COGS and 200 of opex over 181 days.
    /// </summary>
    [Fact]
    public void Simulate_ServerFeatureCase_MatchesServer()
    {
        var baseline = new WhatIfBaseline(1000, 400, 200, 54.3, 500, 181);

        var result = WhatIfScenarioCalculator.Simulate(baseline, new WhatIfScenario(PricePct: 10));

        Assert.Equal(400, result.Baseline.Ebitda);
        Assert.Equal(500, result.Simulated.Ebitda);
        Assert.Equal(100, result.Delta.Ebitda);

        // 100 more EBITDA minus the receivables that finance the extra 100 of sales.
        Assert.Equal(500 + 100 - (100 * 54.3 / 181), result.Simulated.NetCashFlow, 0.01);
        Assert.Equal(Math.Round(200 / (700.0 / 1100), 4, MidpointRounding.AwayFromZero), result.Simulated.BreakEvenRevenue!.Value, Tolerance);
    }

    /// <summary>
    /// The break-even is undefined while the contribution margin is not positive.
    /// </summary>
    [Fact]
    public void Simulate_NegativeContributionMargin_HasNoBreakEven()
    {
        var baseline = new WhatIfBaseline(1000, 900, 50, 0, 0, 365);

        var result = WhatIfScenarioCalculator.Simulate(baseline, new WhatIfScenario(CogsPct: 20));

        Assert.Null(result.Simulated.BreakEvenRevenue);
        Assert.Null(result.Simulated.BreakEvenHeadroomPct);
        Assert.Null(result.Delta.BreakEvenRevenue);
        _ = Assert.NotNull(result.Baseline.BreakEvenRevenue);
    }

    /// <summary>
    /// A DSO reduction cannot make the receivables negative.
    /// </summary>
    [Fact]
    public void Simulate_DsoDeltaBelowZeroDays_ClampsReceivablesAtZero()
    {
        var baseline = new WhatIfBaseline(1000, 400, 200, 10, 0, 100);

        var result = WhatIfScenarioCalculator.Simulate(baseline, new WhatIfScenario(DsoDaysDelta: -50));

        // Receivables fall from 100 to 0: the whole amount is released as cash.
        Assert.Equal(100, result.Simulated.NetCashFlow, Tolerance);
    }

    /// <summary>
    /// Margins and headroom are percentages rounded to two decimals, half away from zero like PHP.
    /// </summary>
    [Fact]
    public void Simulate_Percentages_AreRoundedLikeTheServer()
    {
        var baseline = new WhatIfBaseline(1000, 400, 300, 0, 0, 365);

        var result = WhatIfScenarioCalculator.Simulate(baseline, new WhatIfScenario());

        Assert.Equal(30, result.Baseline.EbitdaMarginPct);

        // Break-even 300 / 0.6 = 500: half of the revenue is headroom.
        Assert.Equal(500, result.Baseline.BreakEvenRevenue);
        Assert.Equal(50, result.Baseline.BreakEvenHeadroomPct);

        var odd = WhatIfScenarioCalculator.Simulate(new WhatIfBaseline(3, 1, 1, 0, 0, 365), new WhatIfScenario());

        // 1 / 3 x 100 = 33.333...
        Assert.Equal(33.33, odd.Baseline.EbitdaMarginPct);
    }

    /// <summary>
    /// A period shorter than one day counts as one day.
    /// </summary>
    [Fact]
    public void Simulate_ZeroPeriodDays_DoesNotDivideByZero()
    {
        var baseline = new WhatIfBaseline(1000, 400, 200, 30, 0, 0);

        var result = WhatIfScenarioCalculator.Simulate(baseline, new WhatIfScenario(PricePct: 5));

        Assert.True(double.IsFinite(result.Simulated.NetCashFlow));
    }

    /// <summary>
    /// The scenario knows when it changes nothing.
    /// </summary>
    [Fact]
    public void Scenario_IsNeutral_OnlyWhenEverySliderIsZero()
    {
        Assert.True(new WhatIfScenario().IsNeutral);
        Assert.False(new WhatIfScenario(PricePct: 0.5).IsNeutral);
        Assert.False(new WhatIfScenario(FteDelta: -1).IsNeutral);
    }
}
