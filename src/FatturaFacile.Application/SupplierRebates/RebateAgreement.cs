// <copyright file="RebateAgreement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A supplier rebate agreement (premio di fine anno) with its scale and contributions.
/// </summary>
/// <param name="Id">The agreement identifier.</param>
/// <param name="Code">The agreement code.</param>
/// <param name="Name">The agreement name.</param>
/// <param name="Direction">The direction (purchase for supplier rebates).</param>
/// <param name="ContactId">The supplier identifier.</param>
/// <param name="ContactName">The supplier name, when the server sent it.</param>
/// <param name="TierMode">How the scale is applied.</param>
/// <param name="StartsOn">The first day of the agreement.</param>
/// <param name="EndsOn">The last day of the agreement.</param>
/// <param name="Currency">The currency.</param>
/// <param name="Status">The status: draft, active, closed or cancelled.</param>
/// <param name="Notes">The notes.</param>
/// <param name="RebateType">The rebate type: tiered_volume, fixed_bonus or marketing_contribution.</param>
/// <param name="SupplierContractId">The blanket agreement the spend is limited to, or null for all the purchases.</param>
/// <param name="FixedBonusTarget">The spend target of a fixed bonus.</param>
/// <param name="FixedBonusAmount">The amount of a fixed bonus.</param>
/// <param name="AccrualFrequency">The accrual frequency: monthly, quarterly or custom.</param>
/// <param name="AccrualAccountId">The accrued income account.</param>
/// <param name="IncomeAccountId">The supplier bonus income account.</param>
/// <param name="Version">The optimistic concurrency version.</param>
/// <param name="OpenRebate">The rebate accrued and not settled yet, when the server computed it.</param>
/// <param name="Tiers">The rebate scale.</param>
/// <param name="Contributions">The planned contributions.</param>
public sealed record RebateAgreement(
    string Id,
    string Code,
    string Name,
    string Direction,
    string ContactId,
    string? ContactName,
    RebateTierMode TierMode,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string Currency,
    string Status,
    string? Notes,
    string RebateType,
    string? SupplierContractId,
    decimal? FixedBonusTarget,
    decimal? FixedBonusAmount,
    string AccrualFrequency,
    string? AccrualAccountId,
    string? IncomeAccountId,
    int Version,
    decimal? OpenRebate,
    IReadOnlyList<RebateTier> Tiers,
    IReadOnlyList<RebateContribution> Contributions)
{
    /// <summary>
    /// Gets a value indicating whether the agreement can be deleted or edited freely (it is a draft).
    /// </summary>
    public bool IsDraft => string.Equals(Status, RebateCodes.StatusDraft, StringComparison.Ordinal);

    /// <summary>
    /// Gets a value indicating whether the agreement can be activated (it is a draft).
    /// </summary>
    public bool CanActivate => IsDraft;

    /// <summary>
    /// Gets a value indicating whether the agreement can be closed (it is active).
    /// </summary>
    public bool CanClose => string.Equals(Status, RebateCodes.StatusActive, StringComparison.Ordinal);

    /// <summary>
    /// Gets a value indicating whether accruals and settlements can be run (the agreement is active).
    /// </summary>
    public bool CanAccrue => CanClose;

    /// <summary>
    /// Gets a value indicating whether the accruals book a journal entry (both accounts are set).
    /// </summary>
    public bool HasLedgerAccounts => !string.IsNullOrEmpty(AccrualAccountId) && !string.IsNullOrEmpty(IncomeAccountId);
}
