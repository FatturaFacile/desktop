// <copyright file="CustomerLedger.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The customer ledger of a period with its running balance.
/// </summary>
/// <param name="Movements">The movements in date order.</param>
/// <param name="ClosingBalance">The balance after the last movement.</param>
public sealed record CustomerLedger(IReadOnlyList<LedgerMovement> Movements, decimal ClosingBalance)
{
    /// <summary>
    /// Recomputes the running balance of movements, debit minus credit, in date order.
    /// </summary>
    /// <param name="opening">The opening balance.</param>
    /// <param name="movements">The movements; their own balances are ignored.</param>
    /// <returns>The ledger with the running balance.</returns>
    public static CustomerLedger WithRunningBalance(decimal opening, IEnumerable<LedgerMovement> movements)
    {
        ArgumentNullException.ThrowIfNull(movements);

        var balance = opening;
        List<LedgerMovement> rows = [];

        foreach (var movement in movements.OrderBy(candidate => candidate.Date))
        {
            balance = ReceivablesMath.Round(balance + movement.Debit - movement.Credit);
            rows.Add(movement with { Balance = balance });
        }

        return new CustomerLedger(rows, balance);
    }
}
