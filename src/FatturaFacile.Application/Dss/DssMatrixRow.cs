// <copyright file="DssMatrixRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// A row of the profitability matrix.
/// </summary>
/// <param name="Key">The entity key.</param>
/// <param name="Label">The entity name.</param>
/// <param name="Rank">The rank by net operating profit.</param>
/// <param name="NetRevenue">Net revenue.</param>
/// <param name="GrossMargin">Gross margin.</param>
/// <param name="GrossMarginPct">Gross margin in percent.</param>
/// <param name="ContributionMargin1">Contribution margin I.</param>
/// <param name="ContributionMargin2">Contribution margin II.</param>
/// <param name="Ebitda">EBITDA.</param>
/// <param name="PaymentDelayCost">Financing cost of late payments.</param>
/// <param name="NetOperatingProfit">Net operating profit.</param>
/// <param name="NetMarginPct">Net margin in percent.</param>
/// <param name="InParetoCore">Whether the entity is in the 80/20 core.</param>
/// <param name="IsValueDestroying">Whether the entity loses money.</param>
public sealed record DssMatrixRow(
    string Key,
    string Label,
    int Rank,
    double NetRevenue,
    double GrossMargin,
    double GrossMarginPct,
    double ContributionMargin1,
    double ContributionMargin2,
    double Ebitda,
    double PaymentDelayCost,
    double NetOperatingProfit,
    double NetMarginPct,
    bool InParetoCore,
    bool IsValueDestroying);
