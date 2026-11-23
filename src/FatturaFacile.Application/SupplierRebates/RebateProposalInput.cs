// <copyright file="RebateProposalInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The data of a settlement proposal.
/// </summary>
/// <param name="Mode">How the proposal is executed.</param>
/// <param name="TaxRate">The VAT rate in percent (required unless the proposal sets off against a payable).</param>
/// <param name="PassiveInvoiceId">The payable to set off against (required for a set-off).</param>
/// <param name="Notes">The notes.</param>
public sealed record RebateProposalInput(RebateExecutionMode Mode, decimal? TaxRate, string? PassiveInvoiceId, string? Notes);
