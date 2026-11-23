// <copyright file="RebateProgress.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The live position of a rebate agreement.
/// </summary>
/// <param name="AgreementId">The agreement identifier.</param>
/// <param name="Currency">The currency.</param>
/// <param name="AsOf">The date the position refers to.</param>
/// <param name="QualifyingVolume">The qualifying spend to the date.</param>
/// <param name="EarnedRebate">The rebate earned to the date.</param>
/// <param name="AccruedRebate">The rebate accrued by the posted runs.</param>
/// <param name="SettledRebate">The rebate already settled.</param>
/// <param name="Tier">The tier progress of a tiered volume rebate, otherwise null.</param>
/// <param name="Bonus">The bonus progress of a fixed bonus, otherwise null.</param>
/// <param name="Contributions">The contributions by status.</param>
/// <param name="Scale">The scale with the upper bounds.</param>
public sealed record RebateProgress(
    string AgreementId,
    string Currency,
    DateOnly? AsOf,
    decimal QualifyingVolume,
    decimal EarnedRebate,
    decimal AccruedRebate,
    decimal SettledRebate,
    RebateTierProgress? Tier,
    RebateBonusProgress? Bonus,
    RebateContributionSummary Contributions,
    IReadOnlyList<RebateScaleBand> Scale);
