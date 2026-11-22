// <copyright file="AccountMovement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// One posted line of an account with the entry it belongs to.
/// </summary>
/// <param name="JournalEntryId">The entry identifier.</param>
/// <param name="EntryNumber">The entry number.</param>
/// <param name="PostingDate">The posting date.</param>
/// <param name="Description">The line (or entry) description.</param>
/// <param name="Debit">The debit amount.</param>
/// <param name="Credit">The credit amount.</param>
public sealed record AccountMovement(Guid JournalEntryId, string EntryNumber, DateTime PostingDate, string Description, decimal Debit, decimal Credit);
