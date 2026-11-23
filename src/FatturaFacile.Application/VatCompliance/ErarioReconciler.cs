// <copyright file="ErarioReconciler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Reconciles the periodic VAT liquidations with the settlements of the web-api and with the postings to the Erario c/IVA account.
/// </summary>
public static class ErarioReconciler
{
    /// <summary>
    /// The state of a period whose three figures agree.
    /// </summary>
    public const string Reconciled = "reconciled";

    /// <summary>
    /// The state of a period with an amount to settle that nothing was posted for.
    /// </summary>
    public const string NotPosted = "not_posted";

    /// <summary>
    /// The state of a period whose posting differs from the liquidation.
    /// </summary>
    public const string LedgerMismatch = "ledger_mismatch";

    /// <summary>
    /// The state of a period whose settlement on the web-api differs from the local liquidation.
    /// </summary>
    public const string ServerMismatch = "server_mismatch";

    private const decimal Tolerance = 0.01m;

    /// <summary>
    /// Reconciles the periods of a year.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="returns">The Italian liquidations of the year.</param>
    /// <param name="settlements">The settlements pulled from the web-api.</param>
    /// <param name="accountFound">Whether the Erario c/IVA account exists.</param>
    /// <param name="postedCredit">Reads the net credit (credits minus debits) posted to the account in a period.</param>
    /// <returns>The reconciliation.</returns>
    public static ErarioReconciliation Reconcile(
        int year,
        IReadOnlyList<VatReturn> returns,
        IReadOnlyList<VatSettlementSnapshot> settlements,
        bool accountFound,
        Func<DateTime, DateTime, decimal> postedCredit)
    {
        ArgumentNullException.ThrowIfNull(returns);
        ArgumentNullException.ThrowIfNull(settlements);
        ArgumentNullException.ThrowIfNull(postedCredit);

        var rows = returns
            .OrderBy(static r => r.PeriodFrom)
            .Select(r =>
            {
                var from = r.PeriodFrom.Date;
                var to = r.PeriodTo.Date;
                var server = settlements.FirstOrDefault(s => s.PeriodFrom.Date == from && s.PeriodTo.Date == to);
                var posted = accountFound ? postedCredit(from, to) : 0m;
                var balance = r.OutputVat - r.InputVat;
                var difference = balance - posted;

                return new ErarioReconciliationRow(from, to, r.Payable, server?.PayableAmount, balance, posted, difference, StateOf(r.Payable, server?.PayableAmount, balance, posted, accountFound));
            })
            .ToList();

        return new ErarioReconciliation(
            year,
            accountFound,
            rows,
            rows.Sum(static r => r.LocalBalance),
            rows.Sum(static r => r.LedgerPosted),
            rows.Sum(static r => r.Difference),
            rows.Count > 0 && rows.TrueForAll(static r => string.Equals(r.State, Reconciled, StringComparison.Ordinal)));
    }

    private static string StateOf(decimal payable, decimal? server, decimal balance, decimal posted, bool accountFound)
    {
        if (server is { } remote && Math.Abs(remote - payable) > Tolerance)
        {
            return ServerMismatch;
        }

        if (Math.Abs(balance - posted) <= Tolerance)
        {
            return Reconciled;
        }

        var nothingPosted = !accountFound || Math.Abs(posted) <= Tolerance;

        return nothingPosted ? NotPosted : LedgerMismatch;
    }
}
