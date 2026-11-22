// <copyright file="TrialBalanceCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Builds the trial balance (bilancio di verifica) from posted ledger lines, in decimal arithmetic.
/// </summary>
public static class TrialBalanceCalculator
{
    /// <summary>
    /// Calculates the trial balance.
    /// </summary>
    /// <param name="accounts">The chart of accounts.</param>
    /// <param name="facts">The posted lines of the ledger book up to the end of the period.</param>
    /// <param name="from">The first day of the period.</param>
    /// <param name="to">The last day of the period.</param>
    /// <param name="columns">The layout, 2 or 4 columns.</param>
    /// <returns>The trial balance; accounts without balance and movement are omitted.</returns>
    public static TrialBalanceResult Calculate(
        IReadOnlyCollection<LedgerAccountInfo> accounts,
        IEnumerable<LedgerFact> facts,
        DateTime from,
        DateTime to,
        int columns)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(facts);

        var start = from.Date;
        var end = to.Date;
        var byAccount = accounts.ToDictionary(static a => a.Id);
        Dictionary<Guid, (decimal OpeningNet, decimal Debit, decimal Credit)> sums = [];

        foreach (var fact in facts)
        {
            var day = fact.PostingDate.Date;
            if (day > end)
            {
                continue;
            }

            _ = sums.TryGetValue(fact.AccountId, out var current);
            sums[fact.AccountId] = day < start
                ? (current.OpeningNet + fact.Debit - fact.Credit, current.Debit, current.Credit)
                : (current.OpeningNet, current.Debit + fact.Debit, current.Credit + fact.Credit);
        }

        List<TrialBalanceRow> rows = [];
        var openingNetSum = 0m;

        foreach (var (accountId, sum) in sums)
        {
            var account = byAccount.TryGetValue(accountId, out var known)
                ? known
                : new LedgerAccountInfo(accountId, accountId.ToString("N")[..8], string.Empty, "asset", "debit");

            if (sum.OpeningNet == 0m && sum.Debit == 0m && sum.Credit == 0m)
            {
                continue;
            }

            var closingNet = sum.OpeningNet + sum.Debit - sum.Credit;
            openingNetSum += sum.OpeningNet;
            rows.Add(new TrialBalanceRow(
                account.Id,
                account.Code,
                account.Name,
                account.AccountType,
                account.NormalBalance,
                sum.OpeningNet * account.Sign,
                sum.Debit,
                sum.Credit,
                closingNet * account.Sign,
                closingNet > 0m ? closingNet : 0m,
                closingNet < 0m ? -closingNet : 0m));
        }

        var ordered = rows.OrderBy(static r => r.AccountCode, StringComparer.Ordinal).ToList();
        var totalDebit = ordered.Sum(static r => r.Debit);
        var totalCredit = ordered.Sum(static r => r.Credit);
        var totalDebitBalance = ordered.Sum(static r => r.DebitBalance);
        var totalCreditBalance = ordered.Sum(static r => r.CreditBalance);

        return new TrialBalanceResult(
            columns == 2 ? 2 : 4,
            ordered,
            totalDebit,
            totalCredit,
            totalDebitBalance,
            totalCreditBalance,
            totalDebit == totalCredit && openingNetSum == 0m && totalDebitBalance == totalCreditBalance);
    }
}
