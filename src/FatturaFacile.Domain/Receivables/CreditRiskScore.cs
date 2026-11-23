// <copyright file="CreditRiskScore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The composite credit risk of a customer, from 0 (safe) to 100 (critical).
/// </summary>
/// <param name="Score">The score.</param>
/// <param name="Rating">The rating.</param>
/// <param name="Factors">The factors with their points.</param>
/// <param name="Blocked">Whether the customer is blocked.</param>
public sealed record CreditRiskScore(
    int Score,
    RiskRating Rating,
    IReadOnlyList<RiskFactor> Factors,
    bool Blocked);
