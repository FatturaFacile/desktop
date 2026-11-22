// <copyright file="AccountLedgerCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Builds the account ledger (mastrino) of one account: opening balance, lines with running balance, closing balance.
/// </summary>
public static class AccountLedgerCalculator
{
    /// <summary>
    /// Calculates the account ledger.
    /// </summary>
    /// <param name="account">The account.</param>
    /// <param name="movements">The posted lines of the account in the ledger book up to the end of the range.</param>
    /// <param name="from">The first day of the range.</param>
    /// <param name="to">The last day of the range.</param>
    /// <param name="page">The page, from 1.</param>
    /// <param name="perPage">The page size.</param>
    /// <returns>The ledger; the running balance of a page includes every earlier line of the range.</returns>
    public static AccountLedgerResult Calculate(
        LedgerAccountInfo account,
        IEnumerable<AccountMovement> movements,
        DateTime from,
        DateTime to,
        int page,
        int perPage)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(movements);

        var start = from.Date;
        var end = to.Date;
        var size = perPage < 1 ? 100 : perPage;
        var number = page < 1 ? 1 : page;

        var ordered = movements
            .Where(m => m.PostingDate.Date <= end)
            .OrderBy(static m => m.PostingDate.Date)
            .ThenBy(static m => m.EntryNumber, StringComparer.Ordinal)
            .ToList();

        var opening = ordered.Where(m => m.PostingDate.Date < start).Sum(static m => m.Debit - m.Credit) * account.Sign;
        var running = opening;
        List<AccountLedgerLine> all = [];
        var totalDebit = 0m;
        var totalCredit = 0m;

        foreach (var movement in ordered.Where(m => m.PostingDate.Date >= start))
        {
            running += (movement.Debit - movement.Credit) * account.Sign;
            totalDebit += movement.Debit;
            totalCredit += movement.Credit;
            all.Add(new AccountLedgerLine(movement.JournalEntryId, movement.EntryNumber, movement.PostingDate, movement.Description, movement.Debit, movement.Credit, running));
        }

        return new AccountLedgerResult(
            account,
            opening,
            running,
            totalDebit,
            totalCredit,
            [.. all.Skip((number - 1) * size).Take(size)],
            number,
            size,
            all.Count);
    }
}
