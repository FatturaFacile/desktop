// <copyright file="RiskFactor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// One factor of the composite credit risk score.
/// </summary>
/// <param name="Key">The factor key: credit_utilization, overdue_share, max_days_past_due, payment_lateness_days, uninsured_share, credit_block.</param>
/// <param name="Value">The measured value as text.</param>
/// <param name="Points">The points the factor adds to the score.</param>
public sealed record RiskFactor(
    string Key,
    string? Value,
    decimal Points);
