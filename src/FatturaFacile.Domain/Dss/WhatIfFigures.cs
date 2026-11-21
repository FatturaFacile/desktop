// <copyright file="WhatIfFigures.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Dss;

/// <summary>
/// The figures of one side of a simulation.
/// </summary>
/// <param name="Revenue">Revenue.</param>
/// <param name="Cogs">Cost of goods sold.</param>
/// <param name="Opex">Operating expenses.</param>
/// <param name="Ebitda">EBITDA.</param>
/// <param name="EbitdaMarginPct">EBITDA margin in percent.</param>
/// <param name="NetCashFlow">Net cash flow.</param>
/// <param name="BreakEvenRevenue">Revenue at which EBITDA is zero, or null while the contribution margin is not positive.</param>
/// <param name="BreakEvenHeadroomPct">Distance of the revenue from the break-even in percent, or null without a break-even.</param>
public sealed record WhatIfFigures(
    double Revenue,
    double Cogs,
    double Opex,
    double Ebitda,
    double EbitdaMarginPct,
    double NetCashFlow,
    double? BreakEvenRevenue,
    double? BreakEvenHeadroomPct);
