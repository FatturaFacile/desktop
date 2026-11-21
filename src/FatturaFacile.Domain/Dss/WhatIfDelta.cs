// <copyright file="WhatIfDelta.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Dss;

/// <summary>
/// The difference between the simulated and the baseline figures.
/// </summary>
/// <param name="Revenue">Revenue difference.</param>
/// <param name="Ebitda">EBITDA difference.</param>
/// <param name="EbitdaMarginPct">EBITDA margin difference in percentage points.</param>
/// <param name="NetCashFlow">Net cash flow difference.</param>
/// <param name="BreakEvenRevenue">Break-even difference, or null when either side has none.</param>
public sealed record WhatIfDelta(
    double Revenue,
    double Ebitda,
    double EbitdaMarginPct,
    double NetCashFlow,
    double? BreakEvenRevenue);
