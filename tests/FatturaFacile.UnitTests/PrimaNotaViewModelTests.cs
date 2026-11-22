// <copyright file="PrimaNotaViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Accounting.Commands;
using FatturaFacile.Domain.Accounting;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// The Prima Nota entry screen: live Dare/Avere/Differenza, post blocked until balanced, template hydration and storno.
/// </summary>
public sealed class PrimaNotaViewModelTests
{
    private readonly LedgerFakeSender sender = new();
    private readonly PrimaNotaViewModel viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimaNotaViewModelTests"/> class.
    /// </summary>
    public PrimaNotaViewModelTests()
    {
        viewModel = new PrimaNotaViewModel(sender, LocalizationService.Instance);
    }

    /// <summary>
    /// A fresh entry has two empty lines and cannot be posted.
    /// </summary>
    [Fact]
    public void NewEntry_HasTwoEmptyLines_AndPostIsBlocked()
    {
        Assert.Equal(2, viewModel.Lines.Count);
        Assert.False(viewModel.IsBalanced);
        Assert.False(viewModel.PostCommand.CanExecute(parameter: null));
    }

    /// <summary>
    /// Post stays blocked while Dare and Avere differ, whatever else is filled in, and opens when they match.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Post_IsBlockedUntilTheEntryBalances()
    {
        await viewModel.LoadAsync();
        viewModel.Description = "Sale";
        Fill(viewModel.Lines[0], LedgerFakeSender.Cash, debit: 100m);
        Fill(viewModel.Lines[1], LedgerFakeSender.Revenue, credit: 40m);

        Assert.Equal(100m, viewModel.TotalDebit);
        Assert.Equal(40m, viewModel.TotalCredit);
        Assert.Equal(60m, viewModel.Difference);
        Assert.False(viewModel.IsBalanced);
        Assert.False(viewModel.PostCommand.CanExecute(parameter: null));

        viewModel.Lines[1].Credit = 100m;

        Assert.Equal(0m, viewModel.Difference);
        Assert.True(viewModel.IsBalanced);
        Assert.True(viewModel.PostCommand.CanExecute(parameter: null));

        viewModel.Description = string.Empty;
        Assert.False(viewModel.PostCommand.CanExecute(parameter: null));
    }

    /// <summary>
    /// A line is a debit or a credit: typing one side clears the other.
    /// </summary>
    [Fact]
    public void Line_DebitAndCredit_AreMutuallyExclusive()
    {
        var line = viewModel.Lines[0];

        line.Debit = 10m;
        line.Credit = 5m;

        Assert.Equal(0m, line.Debit);
        Assert.Equal(5m, line.Credit);
    }

    /// <summary>
    /// Posting sends the balanced lines to the selected ledger book and starts a new entry.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Post_SendsTheBalancedEntryToTheSelectedLedgerBook()
    {
        await viewModel.LoadAsync();
        viewModel.Description = "Sale";
        viewModel.SelectedLedger = viewModel.LedgerBooks.Single(b => b.Book == LedgerBook.Tax);
        Fill(viewModel.Lines[0], LedgerFakeSender.Cash, debit: 100m);
        Fill(viewModel.Lines[1], LedgerFakeSender.Revenue, credit: 100m);

        await viewModel.PostCommand.ExecuteAsync(parameter: null);

        var command = Assert.Single(sender.Requests.OfType<CreateJournalEntryCommand>());
        Assert.Equal(LedgerBook.Tax, command.LedgerType);
        Assert.True(command.PostImmediately);
        Assert.Equal(2, command.Lines.Count);
        Assert.Equal(100m, JournalRules.Totals(command.Lines).Credit);
        Assert.Equal(string.Empty, viewModel.Description);
        Assert.False(viewModel.IsBalanced);
    }

    /// <summary>
    /// Saving a draft sends the entry without posting it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveDraft_DoesNotPost()
    {
        await viewModel.LoadAsync();
        viewModel.Description = "Draft";
        Fill(viewModel.Lines[0], LedgerFakeSender.Cash, debit: 100m);

        await viewModel.SaveDraftCommand.ExecuteAsync(parameter: null);

        Assert.False(Assert.Single(sender.Requests.OfType<CreateJournalEntryCommand>()).PostImmediately);
    }

    /// <summary>
    /// Applying a template replaces the lines with the balanced hydrated ones, takes its ledger book and proposes its name.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApplyTemplate_FillsTheLinesFromTheTemplate()
    {
        await viewModel.LoadAsync();
        viewModel.SelectedTemplate = viewModel.Templates.Single(t => string.Equals(t.Code, "SALES_INVOICE", StringComparison.Ordinal));
        viewModel.TemplateAmount = 1000m;
        viewModel.TemplateTaxRate = 22m;
        viewModel.TemplateAccounts.Single(a => string.Equals(a.Key, "receivable_account", StringComparison.Ordinal)).Account = LedgerFakeSender.Receivable;
        viewModel.TemplateAccounts.Single(a => string.Equals(a.Key, "revenue_account", StringComparison.Ordinal)).Account = LedgerFakeSender.Revenue;
        viewModel.TemplateAccounts.Single(a => string.Equals(a.Key, "vat_account", StringComparison.Ordinal)).Account = LedgerFakeSender.OutputVat;

        await viewModel.ApplyTemplateCommand.ExecuteAsync(parameter: null);

        Assert.Equal(3, viewModel.Lines.Count);
        Assert.Equal(1220m, viewModel.TotalDebit);
        Assert.Equal(1220m, viewModel.TotalCredit);
        Assert.True(viewModel.IsBalanced);
        Assert.NotNull(viewModel.SelectedTemplate);
        Assert.Equal(viewModel.SelectedTemplate.Name, viewModel.Description);
        Assert.True(viewModel.PostCommand.CanExecute(parameter: null));
    }

    /// <summary>
    /// A template with a missing account does not touch the lines and tells why.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApplyTemplate_WithMissingAccounts_KeepsTheLines()
    {
        await viewModel.LoadAsync();
        viewModel.SelectedTemplate = viewModel.Templates.Single(t => string.Equals(t.Code, "SALES_INVOICE", StringComparison.Ordinal));
        viewModel.TemplateAmount = 100m;

        await viewModel.ApplyTemplateCommand.ExecuteAsync(parameter: null);

        Assert.Equal(2, viewModel.Lines.Count);
        Assert.False(string.IsNullOrWhiteSpace(viewModel.StatusMessage));
    }

    /// <summary>
    /// A rule violation of the handler is shown in the language of the user, not as a key.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Post_ViolationFromTheHandler_IsShownLocalized()
    {
        await viewModel.LoadAsync();
        sender.CreateFailure = new JournalValidationException(JournalViolation.Unbalanced);
        viewModel.Description = "Sale";
        Fill(viewModel.Lines[0], LedgerFakeSender.Cash, debit: 100m);
        Fill(viewModel.Lines[1], LedgerFakeSender.Revenue, credit: 100m);

        await viewModel.PostCommand.ExecuteAsync(parameter: null);

        Assert.Equal(LocalizationService.Instance["Gl_Violation_Unbalanced"], viewModel.StatusMessage);
        Assert.NotEqual("Gl_Violation_Unbalanced", viewModel.StatusMessage, StringComparer.Ordinal);
    }

    /// <summary>
    /// The storno of a posted entry sends the reversal with the date and reason; a draft or a storno offers none.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ReverseSelected_SendsTheReversalOfAPostedEntry()
    {
        var posted = new GeneralLedgerEntryDto { Id = Guid.NewGuid(), EntryNumber = "GLE-2026-0001", IsLocked = true };
        var draft = new GeneralLedgerEntryDto { Id = Guid.NewGuid(), EntryNumber = "GLE-2026-0002" };
        var storno = new GeneralLedgerEntryDto { Id = Guid.NewGuid(), EntryNumber = "GLE-2026-0003", IsLocked = true, ReversalOfEntryId = posted.Id };
        sender.Entries = [posted, draft, storno];
        await viewModel.LoadAsync();

        Assert.True(viewModel.Entries.Single(e => e.Entry.Id == posted.Id).CanReverse);
        Assert.False(viewModel.Entries.Single(e => e.Entry.Id == draft.Id).CanReverse);
        Assert.False(viewModel.Entries.Single(e => e.Entry.Id == storno.Id).CanReverse);
        Assert.True(viewModel.Entries.Single(e => e.Entry.Id == draft.Id).CanPost);

        viewModel.SelectedEntry = viewModel.Entries.Single(e => e.Entry.Id == posted.Id);
        viewModel.ReversalReason = "wrong customer";
        viewModel.ReversalDate = new DateTime(2026, 4, 2);
        await viewModel.ReverseSelectedCommand.ExecuteAsync(parameter: null);

        var command = Assert.Single(sender.Requests.OfType<ReverseJournalEntryCommand>());
        Assert.Equal(posted.Id, command.EntryId);
        Assert.Equal("wrong customer", command.Reason);
        Assert.Equal(new DateTime(2026, 4, 2), command.ReversalDate);
    }

    private static void Fill(JournalLineRow line, ChartOfAccountDto account, decimal debit = 0m, decimal credit = 0m)
    {
        line.Account = account;
        line.Debit = debit;
        line.Credit = credit;
    }
}
