// <copyright file="CustomerLifecycleHealth.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.CustomerLifecycle;

/// <summary>
/// The health of a customer.
/// </summary>
/// <param name="HealthIndex">The composite health index (0-100).</param>
/// <param name="ChurnRisk">low, medium, high or critical.</param>
public sealed record CustomerLifecycleHealth(
    [property: JsonPropertyName("health_index")] double HealthIndex,
    [property: JsonPropertyName("churn_risk")] string? ChurnRisk);
