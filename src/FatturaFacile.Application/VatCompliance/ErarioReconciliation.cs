// <copyright file="ErarioReconciliation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The reconciliation of the periodic VAT liquidations of a year with the Erario c/IVA account of the general ledger.
/// </summary>
/// <param name="Year">The year.</param>
/// <param name="AccountFound">Whether an Erario c/IVA account exists in the chart of accounts.</param>
/// <param name="Rows">The periods.</param>
/// <param name="TotalLocal">The sum of the local balances.</param>
/// <param name="TotalLedger">The sum posted to the account.</param>
/// <param name="TotalDifference">The sum of the differences.</param>
/// <param name="IsReconciled">Whether every period is reconciled.</param>
public sealed record ErarioReconciliation(int Year, bool AccountFound, IReadOnlyList<ErarioReconciliationRow> Rows, decimal TotalLocal, decimal TotalLedger, decimal TotalDifference, bool IsReconciled);
