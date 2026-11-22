// <copyright file="CustomerLifecycleSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.CustomerLifecycle;

/// <summary>
/// The 360 summary of a customer: health and KPIs.
/// </summary>
/// <param name="Health">The health.</param>
/// <param name="Kpis">The KPIs.</param>
public sealed record CustomerLifecycleSummary(
    [property: JsonPropertyName("health")] CustomerLifecycleHealth Health,
    [property: JsonPropertyName("kpis")] CustomerLifecycleKpis Kpis);
