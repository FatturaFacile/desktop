// <copyright file="ErarioReconciliationRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The reconciliation of one VAT period between the local liquidation, the settlement of the web-api and the Erario c/IVA account.
/// </summary>
/// <param name="From">The first day of the period.</param>
/// <param name="To">The last day of the period.</param>
/// <param name="LocalPayable">The amount payable of the liquidation computed here (negative is a credit).</param>
/// <param name="ServerPayable">The amount payable of the web-api settlement of the same period, when it was pulled.</param>
/// <param name="LocalBalance">The VAT of the period computed here: VAT on sales minus deductible VAT on purchases.</param>
/// <param name="LedgerPosted">The net credit (credits minus debits) posted to the Erario c/IVA account in the period.</param>
/// <param name="Difference">The local balance minus the amount posted to the account.</param>
/// <param name="State">The localization key suffix of the outcome: reconciled, not_posted, ledger_mismatch or server_mismatch.</param>
public sealed record ErarioReconciliationRow(DateTime From, DateTime To, decimal LocalPayable, decimal? ServerPayable, decimal LocalBalance, decimal LedgerPosted, decimal Difference, string State);
