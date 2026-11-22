// <copyright file="CreditLimitRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Creates (no limit id) or updates a credit limit.
/// </summary>
/// <param name="CommandId">The identifier of the command, which is also its idempotency key.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Reference">A label for the queue list.</param>
/// <param name="LimitId">The limit to update, null to create.</param>
/// <param name="Currency">The ISO currency.</param>
/// <param name="GrantedLimit">The granted limit.</param>
/// <param name="InsuredLimit">The insured limit.</param>
/// <param name="InsurerName">The insurer.</param>
/// <param name="PolicyNumber">The policy number.</param>
/// <param name="CoveragePercentage">The coverage of the insurance.</param>
/// <param name="MaxOverdueDays">The overdue days tolerated.</param>
/// <param name="OverdueToleranceAmount">The overdue amount tolerated.</param>
/// <param name="ReactionPolicy">The reaction to a breach.</param>
/// <param name="ValidFrom">The first day in force.</param>
/// <param name="ValidTo">The last day in force.</param>
/// <param name="IsActive">Whether the limit is switched on.</param>
/// <param name="Notes">Free notes.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record CreditLimitRequest(
    Guid CommandId,
    Guid CustomerId,
    string Reference,
    Guid? LimitId,
    string Currency,
    decimal GrantedLimit,
    decimal? InsuredLimit,
    string? InsurerName,
    string? PolicyNumber,
    decimal? CoveragePercentage,
    int? MaxOverdueDays,
    decimal? OverdueToleranceAmount,
    CreditReactionPolicy ReactionPolicy,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool IsActive,
    string? Notes,
    DateTime OccurredAtUtc);
