// <copyright file="RebateProposalRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One settlement proposal of an agreement, formatted for display.
/// </summary>
/// <param name="Id">The proposal identifier.</param>
/// <param name="Mode">The localized execution mode.</param>
/// <param name="Status">The localized status.</param>
/// <param name="Net">The formatted net amount.</param>
/// <param name="Tax">The formatted VAT.</param>
/// <param name="Gross">The formatted gross total.</param>
/// <param name="Proposal">The proposal behind the row.</param>
internal sealed record RebateProposalRow(string Id, string Mode, string Status, string Net, string Tax, string Gross, RebateSettlementProposal Proposal);
