// <copyright file="AccountLedgerLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// A line of the account ledger (mastrino) with the running balance after it.
/// </summary>
/// <param name="JournalEntryId">The entry identifier, for drill-down.</param>
/// <param name="EntryNumber">The entry number.</param>
/// <param name="PostingDate">The posting date.</param>
/// <param name="Description">The description.</param>
/// <param name="Debit">The debit amount.</param>
/// <param name="Credit">The credit amount.</param>
/// <param name="RunningBalance">The balance on the normal side after this line, including every earlier line of the range.</param>
public sealed record AccountLedgerLine(Guid JournalEntryId, string EntryNumber, DateTime PostingDate, string Description, decimal Debit, decimal Credit, decimal RunningBalance);
