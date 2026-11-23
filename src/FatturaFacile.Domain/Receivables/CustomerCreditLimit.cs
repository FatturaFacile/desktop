// <copyright file="CustomerCreditLimit.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// A credit limit granted to a customer, with its insurance cover, overdue tolerance and reaction policy.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Currency">The ISO currency.</param>
/// <param name="GrantedLimit">The granted limit.</param>
/// <param name="InsuredLimit">The limit covered by the credit insurance.</param>
/// <param name="InsuredCover">The part of the exposure the insurance covers.</param>
/// <param name="InsurerName">The insurer.</param>
/// <param name="PolicyNumber">The policy number.</param>
/// <param name="CoveragePercentage">The coverage of the insurance, 0 to 100.</param>
/// <param name="MaxOverdueDays">The overdue days tolerated, when limited.</param>
/// <param name="OverdueToleranceAmount">The overdue amount tolerated beyond the days.</param>
/// <param name="ReactionPolicy">The reaction to a breach.</param>
/// <param name="ValidFrom">The first day in force.</param>
/// <param name="ValidTo">The last day in force, when limited.</param>
/// <param name="IsActive">Whether the limit is switched on.</param>
/// <param name="Notes">Free notes.</param>
public sealed record CustomerCreditLimit(
    Guid Id,
    Guid CustomerId,
    string Currency,
    decimal GrantedLimit,
    decimal InsuredLimit,
    decimal InsuredCover,
    string? InsurerName,
    string? PolicyNumber,
    decimal CoveragePercentage,
    int? MaxOverdueDays,
    decimal OverdueToleranceAmount,
    CreditReactionPolicy ReactionPolicy,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool IsActive,
    string? Notes)
{
    /// <summary>
    /// Gets a value indicating whether the limit is in force on a day.
    /// </summary>
    /// <param name="day">The day.</param>
    /// <returns>True when active and the day is inside the validity.</returns>
    public bool IsEffectiveOn(DateOnly day) => IsActive && ValidFrom <= day && (ValidTo is null || ValidTo >= day);

    /// <summary>
    /// Computes the share of the limit that is used.
    /// </summary>
    /// <param name="totalExposure">The total exposure.</param>
    /// <returns>The percentage, or null when the limit is not positive.</returns>
    public decimal? UtilizationPercent(decimal totalExposure) =>
        GrantedLimit > 0m ? ReceivablesMath.Round(totalExposure / GrantedLimit * 100m) : null;

    /// <summary>
    /// Computes the credit still available.
    /// </summary>
    /// <param name="totalExposure">The total exposure.</param>
    /// <returns>The limit minus the exposure.</returns>
    public decimal AvailableCredit(decimal totalExposure) => ReceivablesMath.Round(GrantedLimit - totalExposure);

    /// <summary>
    /// Computes the cover of the insurance: the insured limit times the coverage.
    /// </summary>
    /// <returns>The cover.</returns>
    public decimal ComputeInsuredCover() => ReceivablesMath.Percent(InsuredLimit, CoveragePercentage);
}
