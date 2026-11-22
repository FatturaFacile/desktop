// <copyright file="LedgerFact.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// One posted ledger line reduced to what the trial balance sums.
/// </summary>
/// <param name="AccountId">The account.</param>
/// <param name="PostingDate">The posting date.</param>
/// <param name="Debit">The debit amount.</param>
/// <param name="Credit">The credit amount.</param>
public sealed record LedgerFact(Guid AccountId, DateTime PostingDate, decimal Debit, decimal Credit);
