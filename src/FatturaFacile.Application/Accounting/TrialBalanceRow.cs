// <copyright file="TrialBalanceRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// One account of the trial balance. Opening and closing balances are on the normal side of the account; in the two column layout
/// the closing position is also split into <see cref="DebitBalance"/> and <see cref="CreditBalance"/>.
/// </summary>
/// <param name="AccountId">The account identifier.</param>
/// <param name="AccountCode">The account code.</param>
/// <param name="AccountName">The account name.</param>
/// <param name="AccountType">The account type.</param>
/// <param name="NormalBalance">The normal balance side.</param>
/// <param name="OpeningBalance">The balance before the period.</param>
/// <param name="Debit">The debit movement of the period.</param>
/// <param name="Credit">The credit movement of the period.</param>
/// <param name="ClosingBalance">The balance at the end of the period.</param>
/// <param name="DebitBalance">The closing position when it is a debit one, else zero.</param>
/// <param name="CreditBalance">The closing position when it is a credit one, else zero.</param>
public sealed record TrialBalanceRow(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    string AccountType,
    string NormalBalance,
    decimal OpeningBalance,
    decimal Debit,
    decimal Credit,
    decimal ClosingBalance,
    decimal DebitBalance,
    decimal CreditBalance);
