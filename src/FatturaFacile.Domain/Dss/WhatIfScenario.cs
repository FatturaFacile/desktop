// <copyright file="WhatIfScenario.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Dss;

/// <summary>
/// The six sliders of the what-if simulator. Percentages are whole numbers (5 means +5%).
/// </summary>
/// <param name="PricePct">Change of the selling prices, in percent.</param>
/// <param name="CogsPct">Change of the cost of goods sold, in percent.</param>
/// <param name="OpexPct">Change of the operating expenses, in percent.</param>
/// <param name="DsoDaysDelta">Change of the days sales outstanding.</param>
/// <param name="FxShiftPct">EUR/CHF shift, in percent.</param>
/// <param name="FteDelta">Change of the full-time equivalents.</param>
public sealed record WhatIfScenario(
    double PricePct = 0,
    double CogsPct = 0,
    double OpexPct = 0,
    double DsoDaysDelta = 0,
    double FxShiftPct = 0,
    double FteDelta = 0)
{
    private const double Tolerance = 1e-9;

    /// <summary>
    /// Gets a value indicating whether the scenario changes nothing.
    /// </summary>
    public bool IsNeutral => new[] { PricePct, CogsPct, OpexPct, DsoDaysDelta, FxShiftPct, FteDelta }.All(value => Math.Abs(value) < Tolerance);
}
