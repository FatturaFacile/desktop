// <copyright file="CustomerCreditStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The credit position of a customer: limit, exposure, utilisation, block state and overrides.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="AsOf">The day of the figures.</param>
/// <param name="Limit">The limit in force, when any.</param>
/// <param name="Exposure">The exposure.</param>
/// <param name="UtilizationPercent">The share of the limit used, null without a limit.</param>
/// <param name="AvailableCredit">The credit still available, null without a limit.</param>
/// <param name="InsuredCover">The cover of the credit insurance.</param>
/// <param name="UninsuredExposure">The exposure the insurance does not cover.</param>
/// <param name="IsCreditHold">Whether the customer is on credit hold.</param>
/// <param name="IsBlocked">Whether the customer is blocked.</param>
/// <param name="Decision">The credit decision the server reports, when any.</param>
/// <param name="Overrides">The overrides of the customer.</param>
public sealed record CustomerCreditStatus(
    Guid CustomerId,
    DateOnly AsOf,
    CustomerCreditLimit? Limit,
    CreditExposure Exposure,
    decimal? UtilizationPercent,
    decimal? AvailableCredit,
    decimal InsuredCover,
    decimal UninsuredExposure,
    bool IsCreditHold,
    bool IsBlocked,
    CreditDecision? Decision,
    IReadOnlyList<CreditOverride> Overrides);
