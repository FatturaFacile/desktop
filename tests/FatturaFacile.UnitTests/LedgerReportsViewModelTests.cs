// <copyright file="LedgerReportsViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Features.Accounting.Queries;
using FatturaFacile.Domain.Accounting;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// The trial balance and account ledger (mastrino) inspection screens.
/// </summary>
public sealed class LedgerReportsViewModelTests
{
    private readonly LedgerFakeSender sender = new();
    private readonly LedgerReportsViewModel viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="LedgerReportsViewModelTests"/> class.
    /// </summary>
    public LedgerReportsViewModelTests()
    {
        viewModel = new LedgerReportsViewModel(sender, LocalizationService.Instance);
    }

    /// <summary>
    /// The trial balance is requested for the chosen book, period and layout and its rows and totals are shown.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CalculateTrialBalance_AsksTheChosenBookAndLayout()
    {
        var row = new TrialBalanceRow(LedgerFakeSender.Cash.Id, "1000", "Cash", "asset", "debit", 0m, 100m, 0m, 100m, 100m, 0m);
        sender.TrialBalance = new TrialBalanceResult(2, [row], 100m, 100m, 100m, 100m, IsBalanced: true);
        viewModel.SelectedLedger = viewModel.LedgerBooks.Single(b => b.Book == LedgerBook.Ifrs);
        viewModel.TwoColumns = true;
        viewModel.From = new DateTime(2026, 1, 1);
        viewModel.To = new DateTime(2026, 6, 30);

        await viewModel.CalculateTrialBalanceCommand.ExecuteAsync(parameter: null);

        var query = Assert.Single(sender.Requests.OfType<GetTrialBalanceQuery>());
        Assert.Equal(LedgerBook.Ifrs, query.Ledger);
        Assert.Equal(2, query.Columns);
        Assert.Equal(new DateTime(2026, 6, 30), query.To);
        _ = Assert.Single(viewModel.TrialBalanceRows);
        Assert.Equal(100m, viewModel.TotalDebit);
        Assert.Equal(LocalizationService.Instance["Gl_TrialBalanceBalanced"], viewModel.BalanceMessage);
    }

    /// <summary>
    /// Selecting a trial balance row drills down into the account ledger with its running balance.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectingARow_DrillsDownIntoTheAccountLedger()
    {
        var row = new TrialBalanceRow(LedgerFakeSender.Cash.Id, "1000", "Cash", "asset", "debit", 0m, 100m, 0m, 100m, 100m, 0m);
        sender.TrialBalance = new TrialBalanceResult(4, [row], 100m, 100m, 100m, 100m, IsBalanced: true);
        sender.AccountLedger = new AccountLedgerResult(
            new LedgerAccountInfo(LedgerFakeSender.Cash.Id, "1000", "Cash", "asset", "debit"),
            0m,
            100m,
            100m,
            0m,
            [new AccountLedgerLine(Guid.NewGuid(), "GLE-2026-0001", new DateTime(2026, 2, 1), "Sale", 100m, 0m, 100m)],
            1,
            500,
            1);
        await viewModel.LoadAsync();

        viewModel.SelectedRow = viewModel.TrialBalanceRows[0];

        Assert.Equal(LedgerFakeSender.Cash.Id, viewModel.SelectedAccount?.Id);
        var line = Assert.Single(viewModel.AccountLedgerLines);
        Assert.Equal(100m, line.RunningBalance);
        Assert.Equal(100m, viewModel.ClosingBalance);
        Assert.Equal(LedgerBook.Statutory, Assert.Single(sender.Requests.OfType<GetAccountLedgerQuery>()).Ledger);
    }

    /// <summary>
    /// An unbalanced trial balance is flagged.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task UnbalancedTrialBalance_IsFlagged()
    {
        sender.TrialBalance = new TrialBalanceResult(4, [], 10m, 5m, 0m, 0m, IsBalanced: false);

        await viewModel.CalculateTrialBalanceCommand.ExecuteAsync(parameter: null);

        Assert.Equal(LocalizationService.Instance["Gl_TrialBalanceUnbalanced"], viewModel.BalanceMessage);
    }
}
