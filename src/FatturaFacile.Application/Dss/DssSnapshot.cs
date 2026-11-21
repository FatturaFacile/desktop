// <copyright file="DssSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Dss;

namespace FatturaFacile.Application.Dss;

/// <summary>
/// Everything the cockpit shows for one filter.
/// </summary>
/// <param name="Filter">The filter.</param>
/// <param name="Overview">The overview.</param>
/// <param name="Profitability">The profitability matrix.</param>
/// <param name="VarianceBridge">The variance bridge.</param>
/// <param name="Baseline">The actuals the offline simulator runs on.</param>
/// <param name="LegalEntities">The legal entities for the filter.</param>
/// <param name="SyncedAtUtc">When the data was pulled from the server.</param>
public sealed record DssSnapshot(
    DssFilter Filter,
    DssOverview Overview,
    DssProfitability Profitability,
    DssVarianceBridge VarianceBridge,
    WhatIfBaseline Baseline,
    IReadOnlyList<DssLegalEntity> LegalEntities,
    DateTime SyncedAtUtc);
