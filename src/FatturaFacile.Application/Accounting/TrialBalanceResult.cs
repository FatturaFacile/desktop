// <copyright file="TrialBalanceResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// A trial balance of one ledger book and period.
/// </summary>
/// <param name="Columns">The layout: 2 (debit and credit balances) or 4 (opening, debit, credit, closing).</param>
/// <param name="Rows">The account rows ordered by code.</param>
/// <param name="TotalDebit">The sum of the period debits.</param>
/// <param name="TotalCredit">The sum of the period credits.</param>
/// <param name="TotalDebitBalance">The sum of the debit closing positions.</param>
/// <param name="TotalCreditBalance">The sum of the credit closing positions.</param>
/// <param name="IsBalanced">Whether debits equal credits and the debit positions equal the credit positions.</param>
public sealed record TrialBalanceResult(
    int Columns,
    IReadOnlyList<TrialBalanceRow> Rows,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal TotalDebitBalance,
    decimal TotalCreditBalance,
    bool IsBalanced);
