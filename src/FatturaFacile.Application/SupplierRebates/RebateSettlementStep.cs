// <copyright file="RebateSettlementStep.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The optional data of a settlement workflow step.
/// </summary>
/// <param name="Version">The version of the proposal the user saw (optimistic concurrency token).</param>
/// <param name="ExecutedOn">The execution date, for an execution.</param>
/// <param name="PayablesAccountId">The payables account, for a set-off execution that posts to the ledger.</param>
/// <param name="VatAccountId">The VAT account, for a set-off of a proposal with VAT.</param>
public sealed record RebateSettlementStep(int? Version, DateOnly? ExecutedOn = null, string? PayablesAccountId = null, string? VatAccountId = null);
