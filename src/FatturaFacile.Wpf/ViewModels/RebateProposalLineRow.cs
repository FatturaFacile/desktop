// <copyright file="RebateProposalLineRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One row of the analytical breakdown of a settlement proposal, formatted for display.
/// </summary>
/// <param name="Type">The localized row type (invoice, credit note, contribution or bonus).</param>
/// <param name="Reference">The document reference.</param>
/// <param name="Date">The formatted date.</param>
/// <param name="Net">The formatted net amount.</param>
/// <param name="Share">The formatted share of the rebate.</param>
internal sealed record RebateProposalLineRow(string Type, string Reference, string Date, string Net, string Share);
