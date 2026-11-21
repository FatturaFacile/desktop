// <copyright file="WhatIfScenarioCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Dss;

/// <summary>
/// Offline what-if engine. It reproduces, formula by formula, the server side `WhatIfScenarioSimulator` of the web-api, so the
/// cockpit gives the same answer with or without a connection:
/// <code language="text">
/// revenue'  = revenue x (1 + price%) x (1 + fx% x revenueFxShare)
/// cogs'     = cogs    x (1 + cogs%)  x (1 + fx% x costFxShare)
/// opex'     = (opex x (1 + opex%) + fteDelta x fteAnnualCost x periodDays / 365) x (1 + fx% x costFxShare)
/// EBITDA'   = revenue' - cogs' - opex'
/// netCash'  = netCash + (EBITDA' - EBITDA) - (receivables' - receivables), receivables = revenue x DSO / periodDays
/// breakEven = opex' / ((revenue' - cogs') / revenue')   (null while the contribution margin is not positive)
/// </code>
/// </summary>
public static class WhatIfScenarioCalculator
{
    private const double DaysPerYear = 365.0;
    private const double Percent = 100.0;
    private const int AmountDecimals = 4;
    private const int PercentDecimals = 2;

    /// <summary>
    /// Simulates a scenario on the actuals.
    /// </summary>
    /// <param name="baseline">The actuals of the period.</param>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The baseline and simulated figures and their difference.</returns>
    public static WhatIfResult Simulate(WhatIfBaseline baseline, WhatIfScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(scenario);

        var days = Math.Max(baseline.PeriodDays, 1);
        var price = scenario.PricePct / Percent;
        var cogsPct = scenario.CogsPct / Percent;
        var opexPct = scenario.OpexPct / Percent;
        var fx = scenario.FxShiftPct / Percent;

        var revenueSim = baseline.Revenue * (1 + price) * (1 + (fx * baseline.RevenueFxShare));
        var cogsSim = baseline.Cogs * (1 + cogsPct) * (1 + (fx * baseline.CostFxShare));
        var opexSim = ((baseline.Opex * (1 + opexPct)) + (scenario.FteDelta * baseline.FteAnnualCost * days / DaysPerYear)) * (1 + (fx * baseline.CostFxShare));

        var ebitda = baseline.Revenue - baseline.Cogs - baseline.Opex;
        var ebitdaSim = revenueSim - cogsSim - opexSim;

        var receivables = baseline.Revenue * baseline.DsoDays / days;
        var receivablesSim = revenueSim * Math.Max(baseline.DsoDays + scenario.DsoDaysDelta, 0) / days;
        var netCashSim = baseline.NetCashFlow + (ebitdaSim - ebitda) - (receivablesSim - receivables);

        var before = Figures(baseline.Revenue, baseline.Cogs, baseline.Opex, ebitda, baseline.NetCashFlow);
        var after = Figures(revenueSim, cogsSim, opexSim, ebitdaSim, netCashSim);

        double? breakEvenDelta = after.BreakEvenRevenue is { } afterBreakEven && before.BreakEvenRevenue is { } beforeBreakEven
            ? Round(afterBreakEven - beforeBreakEven, AmountDecimals)
            : null;
        var delta = new WhatIfDelta(
            Round(after.Revenue - before.Revenue, AmountDecimals),
            Round(after.Ebitda - before.Ebitda, AmountDecimals),
            Round(after.EbitdaMarginPct - before.EbitdaMarginPct, PercentDecimals),
            Round(after.NetCashFlow - before.NetCashFlow, AmountDecimals),
            breakEvenDelta);

        return new WhatIfResult(before, after, delta);
    }

    private static WhatIfFigures Figures(double revenue, double cogs, double opex, double ebitda, double netCash)
    {
        var contributionRatio = revenue > 0 ? (revenue - cogs) / revenue : 0;
        double? breakEven = contributionRatio > 0 ? opex / contributionRatio : null;

        return new WhatIfFigures(
            Round(revenue, AmountDecimals),
            Round(cogs, AmountDecimals),
            Round(opex, AmountDecimals),
            Round(ebitda, AmountDecimals),
            revenue > 0 ? Round(ebitda / revenue * Percent, PercentDecimals) : 0,
            Round(netCash, AmountDecimals),
            breakEven is { } value ? Round(value, AmountDecimals) : null,
            breakEven is { } point && revenue > 0 ? Round((revenue - point) / revenue * Percent, PercentDecimals) : null);
    }

    /// <summary>
    /// Rounds half away from zero, like PHP `round()` on the server.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="decimals">The decimals to keep.</param>
    /// <returns>The rounded value.</returns>
    private static double Round(double value, int decimals) => Math.Round(value, decimals, MidpointRounding.AwayFromZero);
}
