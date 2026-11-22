// <copyright file="JournalPersistenceGuard.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// The repository level guard of the ledger: no write may change or delete a posted entry or its lines, and nothing unbalanced
/// reaches the store as posted. The database triggers (<see cref="LedgerGuardSql"/>) enforce the same below the application.
/// </summary>
internal static class JournalPersistenceGuard
{
    /// <summary>
    /// Checks one tracked change before it is saved.
    /// </summary>
    /// <param name="context">The context that owns the entry.</param>
    /// <param name="entry">The tracked change.</param>
    /// <exception cref="JournalValidationException">The change would break a ledger invariant.</exception>
    public static void Check(DbContext context, EntityEntry entry)
    {
        if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            return;
        }

        if (entry.Entity is GeneralLedgerEntry journal)
        {
            CheckEntry(entry, journal);
        }
        else if (entry.Entity is GeneralLedgerLine line)
        {
            CheckLine(context, entry, line);
        }
    }

    private static void CheckEntry(EntityEntry entry, GeneralLedgerEntry journal)
    {
        if (entry.State == EntityState.Added)
        {
            Throw(journal.IsLocked ? JournalRules.Validate([.. JournalRules.ToInputs(journal)]) : SignViolations(journal));
            return;
        }

        if (entry.Property(nameof(GeneralLedgerEntry.IsLocked)).OriginalValue is true)
        {
            throw new JournalValidationException(JournalViolation.AlreadyPosted);
        }

        if (entry.State == EntityState.Modified && journal.IsLocked)
        {
            Throw(JournalRules.Validate([.. JournalRules.ToInputs(journal)]));
        }
    }

    private static List<JournalViolation> SignViolations(GeneralLedgerEntry journal) =>
        [.. JournalRules.Validate([.. JournalRules.ToInputs(journal)]).Where(static v => v is JournalViolation.NegativeAmount or JournalViolation.BothSides)];

    private static void Throw(IReadOnlyList<JournalViolation> violations)
    {
        if (violations.Count > 0)
        {
            throw new JournalValidationException(violations);
        }
    }

    private static bool ParentWasPosted(DbContext context, GeneralLedgerLine line)
    {
        var parent = line.GeneralLedgerEntry ?? context.Set<GeneralLedgerEntry>().Local.FirstOrDefault(e => e.Id == line.GeneralLedgerEntryId);

        if (parent is null)
        {
            // The parent is not tracked: ask the store whether it is posted.
            var id = line.GeneralLedgerEntryId;
            return context.Set<GeneralLedgerEntry>().IgnoreQueryFilters().Where(e => e.Id == id).Select(e => e.IsLocked).FirstOrDefault();
        }

        var parentEntry = context.Entry(parent);
        return parentEntry.State != EntityState.Added && parentEntry.Property(nameof(GeneralLedgerEntry.IsLocked)).OriginalValue is true;
    }

    private static void CheckLine(DbContext context, EntityEntry entry, GeneralLedgerLine line)
    {
        if (ParentWasPosted(context, line))
        {
            throw new JournalValidationException(JournalViolation.AlreadyPosted);
        }

        if (entry.State != EntityState.Deleted)
        {
            Throw(JournalRules.Validate([new JournalLineInput(line.AccountId, line.Debit, line.Credit)]).Where(static v => v is JournalViolation.NegativeAmount or JournalViolation.BothSides).ToList());
        }
    }
}
