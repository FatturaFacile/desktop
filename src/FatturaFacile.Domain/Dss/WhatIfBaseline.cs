// <copyright file="WhatIfBaseline.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Dss;

/// <summary>
/// The actuals a scenario is simulated on, in the reporting currency of the period.
/// </summary>
/// <param name="Revenue">Revenue of the period.</param>
/// <param name="Cogs">Cost of goods sold of the period.</param>
/// <param name="Opex">Operating expenses of the period.</param>
/// <param name="DsoDays">Days sales outstanding.</param>
/// <param name="NetCashFlow">Net cash flow of the period.</param>
/// <param name="PeriodDays">Length of the period in days.</param>
/// <param name="FteAnnualCost">Annual cost of one full-time equivalent.</param>
/// <param name="RevenueFxShare">Share (0..1) of revenue exposed to an EUR/CHF shift.</param>
/// <param name="CostFxShare">Share (0..1) of costs exposed to an EUR/CHF shift.</param>
public sealed record WhatIfBaseline(
    double Revenue,
    double Cogs,
    double Opex,
    double DsoDays,
    double NetCashFlow,
    int PeriodDays,
    double FteAnnualCost = 0,
    double RevenueFxShare = 0,
    double CostFxShare = 0);
