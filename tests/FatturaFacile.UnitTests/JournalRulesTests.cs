// <copyright file="JournalRulesTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Domain.Accounting;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// The double-entry invariants: balance, positivity, immutability once posted and the storno.
/// </summary>
public sealed class JournalRulesTests
{
    private static readonly Guid CashId = new(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    private static readonly Guid SalesId = new(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Equal debit and credit above zero is a valid entry.
    /// </summary>
    [Fact]
    public void Validate_BalancedEntry_HasNoViolations()
    {
        var violations = JournalRules.Validate([Debit(CashId, 100.10m), Credit(SalesId, 100.10m)]);

        Assert.Empty(violations);
    }

    /// <summary>
    /// A difference of a single cent is an imbalance: there is no tolerance.
    /// </summary>
    [Fact]
    public void Validate_OneCentDifference_IsUnbalanced()
    {
        var violations = JournalRules.Validate([Debit(CashId, 100.01m), Credit(SalesId, 100.00m)]);

        Assert.Contains(JournalViolation.Unbalanced, violations);
    }

    /// <summary>
    /// Floating point sums do not leak into the check: ten times 0.1 equals 1.
    /// </summary>
    [Fact]
    public void Totals_UsesDecimalArithmetic()
    {
        var lines = Enumerable.Range(0, 10).Select(_ => Debit(CashId, 0.1m)).Append(Credit(SalesId, 1m)).ToList();

        var totals = JournalRules.Totals(lines);

        Assert.Equal(1m, totals.Debit);
        Assert.Equal(0m, totals.Difference);
        Assert.True(totals.IsBalanced);
    }

    /// <summary>
    /// The shape rules: two lines, no negatives, one side per line, an account on each line and a positive total.
    /// </summary>
    [Fact]
    public void Validate_ReportsEachShapeViolation()
    {
        Assert.Contains(JournalViolation.TooFewLines, JournalRules.Validate([Debit(CashId, 10m)]));
        Assert.Contains(JournalViolation.NegativeAmount, JournalRules.Validate([Debit(CashId, -10m), Credit(SalesId, -10m)]));
        Assert.Contains(JournalViolation.BothSides, JournalRules.Validate([new JournalLineInput(CashId, 10m, 10m), Credit(SalesId, 10m)]));
        Assert.Contains(JournalViolation.EmptyLine, JournalRules.Validate([Debit(CashId, 10m), Credit(SalesId, 10m), new JournalLineInput(CashId, 0m, 0m)]));
        Assert.Contains(JournalViolation.MissingAccount, JournalRules.Validate([Debit(Guid.Empty, 10m), Credit(SalesId, 10m)]));
        Assert.Contains(JournalViolation.NonPositiveTotal, JournalRules.Validate([new JournalLineInput(CashId, 0m, 0m), new JournalLineInput(SalesId, 0m, 0m)]));
    }

    /// <summary>
    /// Posting locks a balanced draft and stamps the posting instant.
    /// </summary>
    [Fact]
    public void Post_BalancedDraft_LocksIt()
    {
        var entry = Entry(100m, 100m);
        var at = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);

        JournalRules.Post(entry, at);

        Assert.True(entry.IsLocked);
        Assert.Equal(at, entry.PostedAt);
    }

    /// <summary>
    /// An unbalanced draft cannot be posted and stays a draft.
    /// </summary>
    [Fact]
    public void Post_UnbalancedDraft_IsRejected()
    {
        var entry = Entry(100m, 99m);

        var ex = Assert.Throws<JournalValidationException>(() => JournalRules.Post(entry, DateTime.UtcNow));

        Assert.Contains(JournalViolation.Unbalanced, ex.Violations);
        Assert.False(entry.IsLocked);
        Assert.Null(entry.PostedAt);
    }

    /// <summary>
    /// A posted entry is immutable: it can be neither posted nor edited again.
    /// </summary>
    [Fact]
    public void PostedEntry_IsImmutable()
    {
        var entry = Entry(100m, 100m);
        JournalRules.Post(entry, DateTime.UtcNow);

        var again = Assert.Throws<JournalValidationException>(() => JournalRules.Post(entry, DateTime.UtcNow));
        var edit = Assert.Throws<JournalValidationException>(() => JournalRules.EnsureMutable(entry));

        Assert.Contains(JournalViolation.AlreadyPosted, again.Violations);
        Assert.Contains(JournalViolation.AlreadyPosted, edit.Violations);
    }

    /// <summary>
    /// The storno swaps the sides, links to the original and carries the reason; the original is not touched.
    /// </summary>
    [Fact]
    public void CreateReversal_SwapsSidesLinksAndLeavesTheOriginalUntouched()
    {
        var original = Entry(100m, 100m);
        original.LedgerType = LedgerBook.Tax;
        JournalRules.Post(original, DateTime.UtcNow);
        var before = JournalRules.ToInputs(original);
        var date = new DateTime(2026, 4, 2);

        var reversal = JournalRules.CreateReversal(original, alreadyReversed: false, date, " wrong customer ", "GLE-2026-0002", DateTime.UtcNow);

        Assert.Equal(original.Id, reversal.ReversalOfEntryId);
        Assert.Equal("wrong customer", reversal.ReversalReason);
        Assert.Equal(LedgerBook.Tax, reversal.LedgerType);
        Assert.Equal(date, reversal.PostingDate);
        Assert.True(reversal.IsLocked);
        Assert.Equal(JournalRules.ReversalSource, reversal.SourceDocumentType);
        var cash = reversal.Lines.Single(l => l.AccountId == CashId);
        var sales = reversal.Lines.Single(l => l.AccountId == SalesId);
        Assert.Equal((0m, 100m), (cash.Debit, cash.Credit));
        Assert.Equal((100m, 0m), (sales.Debit, sales.Credit));
        Assert.True(JournalRules.Totals(JournalRules.ToInputs(reversal)).IsBalanced);

        // The original is exactly as posted: no link, no change of any line.
        Assert.Null(original.ReversalOfEntryId);
        Assert.Equal(before, JournalRules.ToInputs(original));
        Assert.DoesNotContain(original.Lines, reversal.Lines.Contains);
    }

    /// <summary>
    /// Only a posted original that was not reversed yet and is not a storno itself can be reversed.
    /// </summary>
    [Fact]
    public void CreateReversal_RejectsDraftsReversalsAndDoubleStorno()
    {
        var draft = Entry(10m, 10m);
        Assert.Contains(JournalViolation.NotPosted, Assert.Throws<JournalValidationException>(() => Reverse(draft)).Violations);

        var posted = Entry(10m, 10m);
        JournalRules.Post(posted, DateTime.UtcNow);
        Assert.Contains(JournalViolation.CannotReverse, Assert.Throws<JournalValidationException>(() => Reverse(posted, alreadyReversed: true)).Violations);

        var storno = Reverse(posted);
        Assert.Contains(JournalViolation.CannotReverse, Assert.Throws<JournalValidationException>(() => Reverse(storno)).Violations);
    }

    /// <summary>
    /// Entry numbers continue the highest sequence of the year.
    /// </summary>
    [Fact]
    public void JournalNumbering_ContinuesTheYearSequence()
    {
        Assert.Equal("GLE-2026-0001", JournalNumbering.Next(2026, []));
        Assert.Equal("GLE-2026-0008", JournalNumbering.Next(2026, ["GLE-2026-0007", "GLE-2026-0003", "GLE-2025-0099", "INV-2026-0100"]));
    }

    private static GeneralLedgerEntry Reverse(GeneralLedgerEntry original, bool alreadyReversed = false) =>
        JournalRules.CreateReversal(original, alreadyReversed, DateTime.Today, reason: null, entryNumber: "GLE-2026-0099", postedAt: DateTime.UtcNow);

    private static JournalLineInput Debit(Guid account, decimal amount) => new(account, amount, 0m);

    private static JournalLineInput Credit(Guid account, decimal amount) => new(account, 0m, amount);

    private static GeneralLedgerEntry Entry(decimal debit, decimal credit)
    {
        GeneralLedgerEntry entry = new() { EntryNumber = "GLE-2026-0001", Description = "Sale" };
        entry.Lines.Add(new GeneralLedgerLine { GeneralLedgerEntryId = entry.Id, AccountId = CashId, LineNumber = 1, Debit = debit });
        entry.Lines.Add(new GeneralLedgerLine { GeneralLedgerEntryId = entry.Id, AccountId = SalesId, LineNumber = 2, Credit = credit });
        return entry;
    }
}
