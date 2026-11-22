// <copyright file="JournalEntryRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A row of the journal list: an entry with its localized ledger book and status.
/// </summary>
/// <param name="Entry">The entry.</param>
/// <param name="LedgerLabel">The localized ledger book.</param>
/// <param name="StatusText">The localized status: draft, posted, reversed or reversal.</param>
internal sealed record JournalEntryRow(GeneralLedgerEntryDto Entry, string LedgerLabel, string StatusText)
{
    /// <summary>
    /// Gets the entry number.
    /// </summary>
    public string EntryNumber => Entry.EntryNumber;

    /// <summary>
    /// Gets the posting date.
    /// </summary>
    public DateTime PostingDate => Entry.PostingDate;

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description => Entry.Description;

    /// <summary>
    /// Gets the total debit.
    /// </summary>
    public decimal TotalDebit => Entry.TotalDebit;

    /// <summary>
    /// Gets the total credit.
    /// </summary>
    public decimal TotalCredit => Entry.TotalCredit;

    /// <summary>
    /// Gets the lines of the entry.
    /// </summary>
    public IList<GeneralLedgerLineDto> Lines => Entry.Lines;

    /// <summary>
    /// Gets a value indicating whether the entry is a posted original that can still be reversed.
    /// </summary>
    public bool CanReverse => Entry is { IsLocked: true, IsReversed: false, ReversalOfEntryId: null };

    /// <summary>
    /// Gets a value indicating whether the entry is a draft that can be posted.
    /// </summary>
    public bool CanPost => !Entry.IsLocked;
}
