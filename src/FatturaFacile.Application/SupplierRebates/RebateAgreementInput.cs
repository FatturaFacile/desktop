// <copyright file="RebateAgreementInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The data of a rebate agreement to create or update. The scale is replaced as a whole on every write.
/// </summary>
/// <param name="Code">The agreement code (unique in the company).</param>
/// <param name="Name">The agreement name.</param>
/// <param name="ContactId">The supplier identifier.</param>
/// <param name="TierMode">How the scale is applied.</param>
/// <param name="StartsOn">The first day.</param>
/// <param name="EndsOn">The last day.</param>
/// <param name="Currency">The currency (three letters).</param>
/// <param name="RebateType">The rebate type (see <see cref="RebateCodes"/>).</param>
/// <param name="SupplierContractId">The blanket agreement the spend is limited to, or null.</param>
/// <param name="FixedBonusTarget">The spend target of a fixed bonus.</param>
/// <param name="FixedBonusAmount">The amount of a fixed bonus.</param>
/// <param name="AccrualFrequency">The accrual frequency (see <see cref="RebateCodes"/>).</param>
/// <param name="AccrualAccountId">The accrued income account.</param>
/// <param name="IncomeAccountId">The supplier bonus income account.</param>
/// <param name="Notes">The notes.</param>
/// <param name="Tiers">The scale.</param>
/// <param name="Version">The version being edited, for an update.</param>
public sealed record RebateAgreementInput(
    string Code,
    string Name,
    string ContactId,
    RebateTierMode TierMode,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string Currency,
    string RebateType,
    string? SupplierContractId,
    decimal? FixedBonusTarget,
    decimal? FixedBonusAmount,
    string AccrualFrequency,
    string? AccrualAccountId,
    string? IncomeAccountId,
    string? Notes,
    IReadOnlyList<RebateTier> Tiers,
    int? Version = null);
