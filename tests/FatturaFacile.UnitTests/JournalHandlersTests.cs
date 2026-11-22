// <copyright file="JournalHandlersTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Accounting.Commands;
using FatturaFacile.Application.Features.Accounting.Queries;
using FatturaFacile.Domain.Accounting;
using FatturaFacile.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// The Prima Nota handlers and the persistence guards over a real SQLite database: create, post, storno, immutability and reports.
/// </summary>
public sealed class JournalHandlersTests : IDisposable
{
    private static readonly DateTime Day = new(2026, 3, 10);

    private readonly GeneralLedgerTestDatabase database = new();

    /// <inheritdoc/>
    public void Dispose() => database.Dispose();

    /// <summary>
    /// A balanced entry is numbered, stored with its lines and locked when posted at once.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_BalancedAndPosted_IsStoredLocked()
    {
        var dto = await Create(100m, 100m, post: true);

        Assert.Equal("GLE-2026-0001", dto.EntryNumber);
        Assert.True(dto.IsLocked);
        _ = Assert.NotNull(dto.PostedAt);
        Assert.Equal(100m, dto.TotalDebit);
        Assert.Equal(2, dto.Lines.Count);

        var stored = await database.Context.Set<GeneralLedgerEntry>().Include(e => e.Lines).AsNoTracking().SingleAsync();
        Assert.True(stored.IsLocked);
        Assert.Equal(LedgerBook.Statutory, stored.LedgerType);
        Assert.Equal(2, stored.Lines.Count);
    }

    /// <summary>
    /// A second entry of the year gets the next number.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_NumbersFollowOneAnotherInTheYear()
    {
        _ = await Create(10m, 10m, post: true);
        var second = await Create(20m, 20m, post: true);

        Assert.Equal("GLE-2026-0002", second.EntryNumber);
    }

    /// <summary>
    /// An unbalanced entry cannot be posted and nothing reaches the store.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_UnbalancedAndPosted_IsRejectedAndNothingIsStored()
    {
        var ex = await Assert.ThrowsAsync<JournalValidationException>(() => Create(100m, 99m, post: true));

        Assert.Contains(JournalViolation.Unbalanced, ex.Violations);
        Assert.Empty(await database.Context.Set<GeneralLedgerEntry>().AsNoTracking().ToListAsync());
    }

    /// <summary>
    /// A draft can be saved unbalanced, but posting it later is refused until it balances.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Draft_CanBeSavedUnbalanced_ButNotPosted()
    {
        var draft = await Create(100m, 40m, post: false);
        Assert.False(draft.IsLocked);

        var post = new PostJournalEntryCommandHandler(database.UnitOfWork, database.Mapper, TimeProvider.System);
        _ = await Assert.ThrowsAsync<JournalValidationException>(() => post.Handle(new PostJournalEntryCommand(draft.Id), CancellationToken.None));

        var balanced = await Create(100m, 100m, post: false);
        var posted = await post.Handle(new PostJournalEntryCommand(balanced.Id), CancellationToken.None);
        Assert.True(posted.IsLocked);
    }

    /// <summary>
    /// A posted entry cannot be changed or deleted through the context: the repository guard throws.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task PostedEntry_CannotBeModifiedOrDeleted()
    {
        var dto = await Create(100m, 100m, post: true);
        var entry = await database.Context.Set<GeneralLedgerEntry>().SingleAsync(e => e.Id == dto.Id);
        var line = await database.Context.Set<GeneralLedgerLine>().FirstAsync(l => l.GeneralLedgerEntryId == dto.Id);

        entry.Description = "tampered";
        _ = await Assert.ThrowsAsync<JournalValidationException>(() => database.Context.SaveChangesAsync());
        database.Context.ChangeTracker.Clear();

        var again = await database.Context.Set<GeneralLedgerLine>().FirstAsync(l => l.Id == line.Id);
        again.Debit = 5m;
        _ = await Assert.ThrowsAsync<JournalValidationException>(() => database.Context.SaveChangesAsync());
        database.Context.ChangeTracker.Clear();

        var toDelete = await database.Context.Set<GeneralLedgerEntry>().SingleAsync(e => e.Id == dto.Id);
        _ = database.Context.Remove(toDelete);
        _ = await Assert.ThrowsAsync<JournalValidationException>(() => database.Context.SaveChangesAsync());
    }

    /// <summary>
    /// The storno is a new posted entry with swapped sides linked to the original, which stays exactly as it was.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reverse_CreatesTheLinkedCounterEntry_AndNeverTouchesTheOriginal()
    {
        var original = await Create(100m, 100m, post: true);
        var handler = new ReverseJournalEntryCommandHandler(database.UnitOfWork, database.Mapper, TimeProvider.System);

        var reversal = await handler.Handle(new ReverseJournalEntryCommand(original.Id, Day.AddDays(1), "wrong customer"), CancellationToken.None);

        Assert.Equal(original.Id, reversal.ReversalOfEntryId);
        Assert.Equal("wrong customer", reversal.ReversalReason);
        Assert.Equal(100m, reversal.Lines.Single(l => l.AccountId == database.Cash.Id).Credit);
        Assert.Equal(100m, reversal.Lines.Single(l => l.AccountId == database.Revenue.Id).Debit);

        database.Context.ChangeTracker.Clear();
        var stored = await database.Context.Set<GeneralLedgerEntry>().Include(e => e.Lines).AsNoTracking().SingleAsync(e => e.Id == original.Id);
        Assert.Null(stored.ReversalOfEntryId);
        Assert.Null(stored.UpdatedAt);
        Assert.Equal(100m, stored.Lines.Single(l => l.AccountId == database.Cash.Id).Debit);
    }

    /// <summary>
    /// An entry is reversed at most once, and a storno cannot be reversed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reverse_Twice_OrAStorno_IsRejected()
    {
        var original = await Create(100m, 100m, post: true);
        var handler = new ReverseJournalEntryCommandHandler(database.UnitOfWork, database.Mapper, TimeProvider.System);
        var reversal = await handler.Handle(new ReverseJournalEntryCommand(original.Id, Day, Reason: null), CancellationToken.None);

        var twice = await Assert.ThrowsAsync<JournalValidationException>(() => handler.Handle(new ReverseJournalEntryCommand(original.Id, Day, Reason: null), CancellationToken.None));
        var storno = await Assert.ThrowsAsync<JournalValidationException>(() => handler.Handle(new ReverseJournalEntryCommand(reversal.Id, Day, Reason: null), CancellationToken.None));

        Assert.Contains(JournalViolation.CannotReverse, twice.Violations);
        Assert.Contains(JournalViolation.CannotReverse, storno.Violations);
    }

    /// <summary>
    /// A draft cannot be reversed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Reverse_Draft_IsRejected()
    {
        var draft = await Create(100m, 100m, post: false);
        var handler = new ReverseJournalEntryCommandHandler(database.UnitOfWork, database.Mapper, TimeProvider.System);

        var ex = await Assert.ThrowsAsync<JournalValidationException>(() => handler.Handle(new ReverseJournalEntryCommand(draft.Id, Day, Reason: null), CancellationToken.None));

        Assert.Contains(JournalViolation.NotPosted, ex.Violations);
    }

    /// <summary>
    /// The trial balance counts posted entries of the ledger book only, and an entry cancelled by its storno nets to zero.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task TrialBalance_CountsPostedEntriesOfTheBook_AndStornoNetsToZero()
    {
        _ = await Create(100m, 100m, post: true);
        _ = await Create(40m, 40m, post: false);
        var tax = await Create(70m, 70m, post: true, LedgerBook.Tax);
        var statutory = new GetTrialBalanceQueryHandler(database.UnitOfWork.Repository<GeneralLedgerEntry>(), database.UnitOfWork.Repository<ChartOfAccount>());

        var before = await statutory.Handle(new GetTrialBalanceQuery(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), LedgerBook.Statutory), CancellationToken.None);
        var taxOnly = await statutory.Handle(new GetTrialBalanceQuery(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), LedgerBook.Tax), CancellationToken.None);

        Assert.Equal(100m, before.TotalDebit);
        Assert.True(before.IsBalanced);
        Assert.Equal(70m, taxOnly.TotalDebit);

        var reverse = new ReverseJournalEntryCommandHandler(database.UnitOfWork, database.Mapper, TimeProvider.System);
        _ = await reverse.Handle(new ReverseJournalEntryCommand(tax.Id, Day, Reason: null), CancellationToken.None);
        var netted = await statutory.Handle(new GetTrialBalanceQuery(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), LedgerBook.Tax, 2), CancellationToken.None);

        Assert.All(netted.Rows, row => Assert.Equal(0m, row.ClosingBalance));
        Assert.Equal(0m, netted.TotalDebitBalance);
        Assert.True(netted.IsBalanced);
    }

    /// <summary>
    /// The account ledger lists the posted lines of the account with running balance.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task AccountLedger_ListsPostedLinesWithRunningBalance()
    {
        _ = await Create(100m, 100m, post: true);
        _ = await Create(25m, 25m, post: true);
        var handler = new GetAccountLedgerQueryHandler(database.UnitOfWork.Repository<GeneralLedgerEntry>(), database.UnitOfWork.Repository<ChartOfAccount>());

        var result = await handler.Handle(new GetAccountLedgerQuery(database.Cash.Id, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), LedgerBook.Statutory), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal([100m, 125m], result.Lines.Select(l => l.RunningBalance));
        Assert.Equal(125m, result.ClosingBalance);
        Assert.Null(await handler.Handle(new GetAccountLedgerQuery(Guid.NewGuid(), Day, Day, LedgerBook.Statutory), CancellationToken.None));
    }

    /// <summary>
    /// The SQLite triggers refuse, below the application, what the guard refuses: changing a posted entry or line, deleting it,
    /// posting an unbalanced entry and a line with both sides.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Triggers_RefuseDirectSqlOnPostedEntries()
    {
        using GeneralLedgerTestDatabase guarded = new(withTriggers: true);
        var entry = new GeneralLedgerEntry { TenantId = guarded.Tenant.TenantId, EntryNumber = "GLE-2026-0001", Description = "Sale" };
        entry.Lines.Add(new GeneralLedgerLine { GeneralLedgerEntryId = entry.Id, AccountId = guarded.Cash.Id, LineNumber = 1, Debit = 100m });
        entry.Lines.Add(new GeneralLedgerLine { GeneralLedgerEntryId = entry.Id, AccountId = guarded.Revenue.Id, LineNumber = 2, Credit = 60m });
        _ = guarded.Context.Add(entry);
        _ = await guarded.Context.SaveChangesAsync();
        var id = entry.Id.ToString().ToUpperInvariant();

        var unbalanced = await Assert.ThrowsAsync<SqliteException>(() => guarded.Context.Database.ExecuteSqlRawAsync("UPDATE general_ledger_entries SET is_locked = 1 WHERE id = {0}", id));
        Assert.Contains("must balance", unbalanced.Message, StringComparison.Ordinal);

        _ = await guarded.Context.Database.ExecuteSqlRawAsync("UPDATE general_ledger_lines SET credit = '100' WHERE general_ledger_entry_id = {0} AND line_number = 2", id);
        _ = await guarded.Context.Database.ExecuteSqlRawAsync("UPDATE general_ledger_entries SET is_locked = 1 WHERE id = {0}", id);

        var update = await Assert.ThrowsAsync<SqliteException>(() => guarded.Context.Database.ExecuteSqlRawAsync("UPDATE general_ledger_entries SET description = 'x' WHERE id = {0}", id));
        var lineUpdate = await Assert.ThrowsAsync<SqliteException>(() => guarded.Context.Database.ExecuteSqlRawAsync("UPDATE general_ledger_lines SET debit = '1' WHERE general_ledger_entry_id = {0}", id));
        var lineDelete = await Assert.ThrowsAsync<SqliteException>(() => guarded.Context.Database.ExecuteSqlRawAsync("DELETE FROM general_ledger_lines WHERE general_ledger_entry_id = {0}", id));
        var delete = await Assert.ThrowsAsync<SqliteException>(() => guarded.Context.Database.ExecuteSqlRawAsync("DELETE FROM general_ledger_entries WHERE id = {0}", id));

        Assert.Contains("immutable", update.Message, StringComparison.Ordinal);
        Assert.Contains("immutable", lineUpdate.Message, StringComparison.Ordinal);
        Assert.Contains("cannot be deleted", lineDelete.Message, StringComparison.Ordinal);
        Assert.Contains("cannot be deleted", delete.Message, StringComparison.Ordinal);

        var bothSides = await Assert.ThrowsAsync<SqliteException>(() => guarded.Context.Database.ExecuteSqlRawAsync(
            "INSERT INTO general_ledger_lines (id, general_ledger_entry_id, account_id, line_number, debit, credit) VALUES ({0}, {1}, {2}, 9, '5', '5')",
            Guid.NewGuid().ToString().ToUpperInvariant(),
            id,
            guarded.Cash.Id.ToString().ToUpperInvariant()));
        Assert.Contains("never both", bothSides.Message, StringComparison.Ordinal);
    }

    private Task<GeneralLedgerEntryDto> Create(decimal debit, decimal credit, bool post, LedgerBook book = LedgerBook.Statutory)
    {
        var handler = new CreateJournalEntryCommandHandler(database.UnitOfWork, database.Tenant, database.Mapper, TimeProvider.System);

        return handler.Handle(
            new CreateJournalEntryCommand(
                Day,
                "Sale",
                book,
                [new JournalLineInput(database.Cash.Id, debit, 0m), new JournalLineInput(database.Revenue.Id, 0m, credit)],
                post),
            CancellationToken.None);
    }
}
