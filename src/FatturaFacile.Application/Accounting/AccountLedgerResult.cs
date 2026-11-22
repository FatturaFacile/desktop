// <copyright file="AccountLedgerResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// The account ledger (mastrino) of one account for a range of posting dates.
/// </summary>
/// <param name="Account">The account.</param>
/// <param name="OpeningBalance">The balance before the range, on the normal side.</param>
/// <param name="ClosingBalance">The balance at the end of the range, on the normal side.</param>
/// <param name="TotalDebit">The debit movement of the whole range.</param>
/// <param name="TotalCredit">The credit movement of the whole range.</param>
/// <param name="Lines">The lines of the requested page, oldest first.</param>
/// <param name="Page">The page number, from 1.</param>
/// <param name="PerPage">The page size.</param>
/// <param name="TotalLines">The number of lines of the whole range.</param>
public sealed record AccountLedgerResult(
    LedgerAccountInfo Account,
    decimal OpeningBalance,
    decimal ClosingBalance,
    decimal TotalDebit,
    decimal TotalCredit,
    IReadOnlyList<AccountLedgerLine> Lines,
    int Page,
    int PerPage,
    int TotalLines);
