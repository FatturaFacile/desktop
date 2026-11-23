// <copyright file="ErarioLedger.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The movements of a year on the Erario c/IVA accounts of the general ledger.
/// </summary>
/// <param name="AccountFound">Whether an Erario c/IVA account exists in the chart of accounts.</param>
/// <param name="Movements">The net credit (credits minus debits) of each posting, with its posting date.</param>
public sealed record ErarioLedger(bool AccountFound, IReadOnlyList<(DateTime Date, decimal NetCredit)> Movements)
{
    /// <summary>
    /// Sums the net credit posted between two dates, both included.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <returns>The net credit.</returns>
    public decimal Between(DateTime from, DateTime to) => Movements.Where(m => m.Date.Date >= from.Date && m.Date.Date <= to.Date).Sum(static m => m.NetCredit);

    /// <summary>
    /// Sums the net credit posted over the whole year read.
    /// </summary>
    /// <returns>The net credit.</returns>
    public decimal Total() => Movements.Sum(static m => m.NetCredit);
}
