// <copyright file="AiConfidenceBand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Confidence band of a suggestion.
/// </summary>
public enum AiConfidenceBand
{
    /// <summary>
    /// Below 70%: review carefully.
    /// </summary>
    Low = 0,

    /// <summary>
    /// From 70% to below 90%.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// 90% and above.
    /// </summary>
    High = 2,
}
