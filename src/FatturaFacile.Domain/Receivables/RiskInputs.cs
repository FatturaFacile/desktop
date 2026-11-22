// <copyright file="RiskInputs.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The measurements the risk score is made of.
/// </summary>
/// <param name="UtilizationPercent">The share of the credit limit used, null without a limit.</param>
/// <param name="OverdueShare">The overdue share of the open amount, 0 to 1.</param>
/// <param name="MaxDaysPastDue">The days past due of the oldest overdue item.</param>
/// <param name="PaymentLatenessDays">The average lateness of the payments of the last twelve months.</param>
/// <param name="UninsuredShare">The share of the exposure the insurance does not cover, 0 to 1.</param>
/// <param name="Blocked">Whether the customer is blocked.</param>
public sealed record RiskInputs(
    decimal? UtilizationPercent,
    decimal OverdueShare,
    int MaxDaysPastDue,
    decimal PaymentLatenessDays,
    decimal UninsuredShare,
    bool Blocked);
