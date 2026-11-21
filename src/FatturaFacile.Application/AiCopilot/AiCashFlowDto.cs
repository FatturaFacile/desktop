// <copyright file="AiCashFlowDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Probabilistic cash flow forecast (percentile bands per day).
/// </summary>
/// <param name="Currency">The currency code.</param>
/// <param name="HorizonDays">The forecast horizon in days.</param>
/// <param name="Dates">The forecast dates.</param>
/// <param name="P10">The pessimistic band (10th percentile).</param>
/// <param name="P50">The median.</param>
/// <param name="P90">The optimistic band (90th percentile).</param>
public sealed record AiCashFlowDto(
    string? Currency,
    int HorizonDays,
    IReadOnlyList<string> Dates,
    IReadOnlyList<decimal> P10,
    IReadOnlyList<decimal> P50,
    IReadOnlyList<decimal> P90);
