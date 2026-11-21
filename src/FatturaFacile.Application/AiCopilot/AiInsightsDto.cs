// <copyright file="AiInsightsDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Predictive insights of the web-api.
/// </summary>
/// <param name="CashFlow">The cash flow forecast, or null when none is available.</param>
/// <param name="PaymentRiskCount">The number of customers with a payment risk score.</param>
/// <param name="Recommendations">The pending recommendations (reorder, churn risk...).</param>
/// <param name="Anomalies">The anomaly alerts.</param>
public sealed record AiInsightsDto(
    AiCashFlowDto? CashFlow,
    int PaymentRiskCount,
    IReadOnlyList<AiSuggestionDto> Recommendations,
    IReadOnlyList<AiSuggestionDto> Anomalies);
