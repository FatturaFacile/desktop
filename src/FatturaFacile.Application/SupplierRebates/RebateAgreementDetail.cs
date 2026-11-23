// <copyright file="RebateAgreementDetail.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The cockpit of one rebate agreement as last pulled: the agreement, its live progress, the accrual runs and the settlement proposals.
/// </summary>
/// <param name="Agreement">The agreement with its scale and contributions.</param>
/// <param name="Progress">The live progress, or null when it was not pulled.</param>
/// <param name="Runs">The accrual runs, latest first.</param>
/// <param name="Proposals">The settlement proposals, latest first.</param>
/// <param name="SyncedAtUtc">When the agreement was pulled.</param>
public sealed record RebateAgreementDetail(
    RebateAgreement Agreement,
    RebateProgress? Progress,
    IReadOnlyList<RebateAccrualRun> Runs,
    IReadOnlyList<RebateSettlementProposal> Proposals,
    DateTime SyncedAtUtc);
