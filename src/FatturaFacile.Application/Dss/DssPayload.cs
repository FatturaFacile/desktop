// <copyright file="DssPayload.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// The answers of the web-api as received (the `data` objects), the unit of storage and of the atomic save.
/// </summary>
/// <param name="OverviewJson">The overview.</param>
/// <param name="ProfitabilityJson">The profitability matrix.</param>
/// <param name="VarianceBridgeJson">The variance bridge.</param>
/// <param name="SimulationInputsJson">The what-if inputs, or null.</param>
/// <param name="LegalEntitiesJson">The legal entities, or null.</param>
public sealed record DssPayload(
    string OverviewJson,
    string ProfitabilityJson,
    string VarianceBridgeJson,
    string? SimulationInputsJson,
    string? LegalEntitiesJson);
