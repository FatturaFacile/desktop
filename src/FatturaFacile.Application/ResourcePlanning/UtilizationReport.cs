// <copyright file="UtilizationReport.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The billable utilization report of a period.
/// </summary>
/// <param name="From">The from.</param>
/// <param name="To">The to.</param>
/// <param name="GroupBy">The group by.</param>
/// <param name="DefaultTargetPercent">The default target percent.</param>
/// <param name="Rows">The rows.</param>
/// <param name="Totals">The totals row.</param>
public sealed record UtilizationReport(
    DateOnly From,
    DateOnly To,
    UtilizationGroupBy GroupBy,
    double DefaultTargetPercent,
    IReadOnlyList<UtilizationRow> Rows,
    UtilizationRow Totals);
