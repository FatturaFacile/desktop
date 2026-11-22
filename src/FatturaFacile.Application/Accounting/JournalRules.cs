// <copyright file="JournalRules.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// The double-entry invariants: balance, positivity, immutability once posted and the storno (reversal) construction.
/// </summary>
public static class JournalRules
{
    /// <summary>
    /// The source document type of a reversal entry.
    /// </summary>
    public const string ReversalSource = "reversal";

    /// <summary>
    /// Sums the debit and credit of the lines.
    /// </summary>
    /// <param name="lines">The lines.</param>
    /// <returns>The totals.</returns>
    public static JournalTotals Totals(IEnumerable<JournalLineInput> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var debit = 0m;
        var credit = 0m;
        foreach (var line in lines)
        {
            debit += line.Debit;
            credit += line.Credit;
        }

        return new JournalTotals(debit, credit);
    }

    /// <summary>
    /// Checks the lines against every double-entry rule.
    /// </summary>
    /// <param name="lines">The lines.</param>
    /// <returns>The broken rules; empty when the lines are a valid, balanced entry.</returns>
    public static IReadOnlyList<JournalViolation> Validate(IReadOnlyCollection<JournalLineInput> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        List<JournalViolation> violations = [];

        var amounts = 0;
        foreach (var line in lines)
        {
            if (line.Debit != 0m || line.Credit != 0m)
            {
                amounts++;
            }
        }

        if (amounts < 2)
        {
            violations.Add(JournalViolation.TooFewLines);
        }

        if (lines.Any(static l => l.Debit < 0m || l.Credit < 0m))
        {
            violations.Add(JournalViolation.NegativeAmount);
        }

        if (lines.Any(static l => l.Debit > 0m && l.Credit > 0m))
        {
            violations.Add(JournalViolation.BothSides);
        }

        if (lines.Any(static l => l.Debit == 0m && l.Credit == 0m))
        {
            violations.Add(JournalViolation.EmptyLine);
        }

        if (lines.Any(static l => l.AccountId == Guid.Empty))
        {
            violations.Add(JournalViolation.MissingAccount);
        }

        var totals = Totals(lines);
        if (totals.Debit != totals.Credit)
        {
            violations.Add(JournalViolation.Unbalanced);
        }

        if (totals.Debit <= 0m && totals.Credit <= 0m)
        {
            violations.Add(JournalViolation.NonPositiveTotal);
        }

        return violations;
    }

    /// <summary>
    /// Converts the lines of an entity to line inputs.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <returns>The line inputs.</returns>
    public static IReadOnlyList<JournalLineInput> ToInputs(GeneralLedgerEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return [.. entry.Lines.OrderBy(static l => l.LineNumber).Select(static l => new JournalLineInput(l.AccountId, l.Debit, l.Credit, l.Description, l.CostCenterId))];
    }

    /// <summary>
    /// Throws when the entry is posted: a posted entry is immutable.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <exception cref="JournalValidationException">The entry is posted.</exception>
    public static void EnsureMutable(GeneralLedgerEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.IsLocked)
        {
            throw new JournalValidationException(JournalViolation.AlreadyPosted);
        }
    }

    /// <summary>
    /// Posts a draft entry: validates it, then locks it.
    /// </summary>
    /// <param name="entry">The draft entry.</param>
    /// <param name="postedAt">The posting instant.</param>
    /// <exception cref="JournalValidationException">The entry is posted already or unbalanced.</exception>
    public static void Post(GeneralLedgerEntry entry, DateTime postedAt)
    {
        EnsureMutable(entry);

        var violations = Validate([.. ToInputs(entry)]);
        if (violations.Count > 0)
        {
            throw new JournalValidationException(violations);
        }

        entry.IsLocked = true;
        entry.PostedAt = postedAt;
    }

    /// <summary>
    /// Builds the storno of a posted entry: a new, posted counter-entry with swapped sides linked to the original, which is left untouched.
    /// </summary>
    /// <param name="original">The posted entry to reverse.</param>
    /// <param name="alreadyReversed">Whether a reversal of the original exists already.</param>
    /// <param name="reversalDate">The posting date of the counter-entry.</param>
    /// <param name="reason">The reason of the storno.</param>
    /// <param name="entryNumber">The number of the counter-entry.</param>
    /// <param name="postedAt">The posting instant.</param>
    /// <returns>The counter-entry, not yet persisted.</returns>
    /// <exception cref="JournalValidationException">The original is a draft, a storno itself or already reversed.</exception>
    public static GeneralLedgerEntry CreateReversal(
        GeneralLedgerEntry original,
        bool alreadyReversed,
        DateTime reversalDate,
        string? reason,
        string entryNumber,
        DateTime postedAt)
    {
        ArgumentNullException.ThrowIfNull(original);

        if (!original.IsLocked)
        {
            throw new JournalValidationException(JournalViolation.NotPosted);
        }

        if (alreadyReversed || original.ReversalOfEntryId is not null)
        {
            throw new JournalValidationException(JournalViolation.CannotReverse);
        }

        var cleanReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        GeneralLedgerEntry reversal = new()
        {
            TenantId = original.TenantId,
            EntryNumber = entryNumber,
            PostingDate = reversalDate,
            DocumentDate = reversalDate,
            Description = "Storno " + original.EntryNumber + (cleanReason is null ? string.Empty : ": " + cleanReason),
            SourceDocumentType = ReversalSource,
            ReferenceId = original.Id,
            LedgerType = original.LedgerType,
            ReversalOfEntryId = original.Id,
            ReversalReason = cleanReason,
            IsLocked = true,
            PostedAt = postedAt,
        };

        var number = 1;
        foreach (var line in original.Lines.OrderBy(static l => l.LineNumber))
        {
            reversal.Lines.Add(new GeneralLedgerLine
            {
                GeneralLedgerEntryId = reversal.Id,
                AccountId = line.AccountId,
                LineNumber = number++,
                Debit = line.Credit,
                Credit = line.Debit,
                Description = line.Description,
                CostCenterId = line.CostCenterId,
            });
        }

        return reversal;
    }
}
