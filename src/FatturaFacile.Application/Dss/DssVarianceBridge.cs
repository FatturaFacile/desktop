// <copyright file="DssVarianceBridge.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// The EBITDA and revenue variance bridges.
/// </summary>
/// <param name="EbitdaSteps">The EBITDA bridge.</param>
/// <param name="RevenueSteps">The revenue bridge.</param>
/// <param name="VolumeBasis">invoice_items, or unit_fallback when the whole revenue change is a price effect.</param>
public sealed record DssVarianceBridge(
    IReadOnlyList<DssBridgeStep> EbitdaSteps,
    IReadOnlyList<DssBridgeStep> RevenueSteps,
    string VolumeBasis);
