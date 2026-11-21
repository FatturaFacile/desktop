// <copyright file="DssOverview.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// The overview of the cockpit: scorecard, alerts and working capital.
/// </summary>
/// <param name="Currency">The reporting currency.</param>
/// <param name="PeriodFrom">The first day of the period.</param>
/// <param name="PeriodTo">The last day of the period.</param>
/// <param name="Kpis">The indicators.</param>
/// <param name="Alerts">The alerts, most severe first.</param>
/// <param name="WorkingCapital">The working capital metrics.</param>
public sealed record DssOverview(
    string Currency,
    string PeriodFrom,
    string PeriodTo,
    IReadOnlyList<DssKpi> Kpis,
    IReadOnlyList<DssAlert> Alerts,
    DssWorkingCapital WorkingCapital);
