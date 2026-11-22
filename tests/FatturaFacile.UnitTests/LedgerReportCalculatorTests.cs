// <copyright file="LedgerReportCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Accounting;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// The trial balance (2 and 4 columns) and the account ledger with running balance.
/// </summary>
public sealed class LedgerReportCalculatorTests
{
    private static readonly LedgerAccountInfo Cash = new(new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), "1000", "Cash", "asset", "debit");
    private static readonly LedgerAccountInfo Sales = new(new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), "3000", "Sales", "revenue", "credit");

    /// <summary>
    /// The four column layout gives opening, movement and closing on the normal side of each account.
    /// </summary>
    [Fact]
    public void TrialBalance_FourColumns_OpeningMovementClosing()
    {
        var facts = new[]
        {
            new LedgerFact(Cash.Id, new DateTime(2025, 12, 31), 500m, 0m),
            new LedgerFact(Sales.Id, new DateTime(2025, 12, 31), 0m, 500m),
            new LedgerFact(Cash.Id, new DateTime(2026, 2, 1), 100.50m, 0m),
            new LedgerFact(Sales.Id, new DateTime(2026, 2, 1), 0m, 100.50m),
            new LedgerFact(Cash.Id, new DateTime(2026, 2, 5), 0m, 20m),
            new LedgerFact(Sales.Id, new DateTime(2026, 2, 5), 20m, 0m),
            new LedgerFact(Cash.Id, new DateTime(2026, 9, 1), 999m, 0m),
        };

        var result = TrialBalanceCalculator.Calculate([Cash, Sales], facts, new DateTime(2026, 1, 1), new DateTime(2026, 6, 30), 4);

        var cash = result.Rows.Single(r => r.AccountId == Cash.Id);
        var sales = result.Rows.Single(r => r.AccountId == Sales.Id);
        Assert.Equal(4, result.Columns);
        Assert.Equal((500m, 100.50m, 20m, 580.50m), (cash.OpeningBalance, cash.Debit, cash.Credit, cash.ClosingBalance));
        Assert.Equal((500m, 20m, 100.50m, 580.50m), (sales.OpeningBalance, sales.Debit, sales.Credit, sales.ClosingBalance));
        Assert.Equal(120.50m, result.TotalDebit);
        Assert.Equal(120.50m, result.TotalCredit);
        Assert.True(result.IsBalanced);
        Assert.Equal<string>(["1000", "3000"], result.Rows.Select(r => r.AccountCode), StringComparer.Ordinal);
    }

    /// <summary>
    /// The two column layout splits the closing position into a debit and a credit balance.
    /// </summary>
    [Fact]
    public void TrialBalance_TwoColumns_SplitsClosingPosition()
    {
        var facts = new[]
        {
            new LedgerFact(Cash.Id, new DateTime(2026, 2, 1), 300m, 0m),
            new LedgerFact(Sales.Id, new DateTime(2026, 2, 1), 0m, 300m),
        };

        var result = TrialBalanceCalculator.Calculate([Cash, Sales], facts, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 2);

        Assert.Equal(2, result.Columns);
        Assert.Equal(300m, result.Rows.Single(r => r.AccountId == Cash.Id).DebitBalance);
        Assert.Equal(0m, result.Rows.Single(r => r.AccountId == Cash.Id).CreditBalance);
        Assert.Equal(300m, result.Rows.Single(r => r.AccountId == Sales.Id).CreditBalance);
        Assert.Equal(300m, result.TotalDebitBalance);
        Assert.Equal(300m, result.TotalCreditBalance);
        Assert.True(result.IsBalanced);
    }

    /// <summary>
    /// One sided data is reported as not balanced.
    /// </summary>
    [Fact]
    public void TrialBalance_OneSidedFacts_AreNotBalanced()
    {
        var result = TrialBalanceCalculator.Calculate([Cash, Sales], [new LedgerFact(Cash.Id, new DateTime(2026, 2, 1), 10m, 0m)], new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 4);

        Assert.False(result.IsBalanced);
    }

    /// <summary>
    /// The mastrino starts from the balance before the range and carries the running balance on the normal side.
    /// </summary>
    [Fact]
    public void AccountLedger_RunningBalance_OnTheNormalSide()
    {
        var entryA = Guid.NewGuid();
        var entryB = Guid.NewGuid();
        var movements = new[]
        {
            new AccountMovement(Guid.NewGuid(), "GLE-2025-0001", new DateTime(2025, 12, 1), "Opening", 0m, 200m),
            new AccountMovement(entryA, "GLE-2026-0001", new DateTime(2026, 1, 10), "Sale", 0m, 50m),
            new AccountMovement(entryB, "GLE-2026-0002", new DateTime(2026, 1, 20), "Refund", 30m, 0m),
        };

        var result = AccountLedgerCalculator.Calculate(Sales, movements, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), 1, 100);

        Assert.Equal(200m, result.OpeningBalance);
        Assert.Equal([250m, 220m], result.Lines.Select(l => l.RunningBalance));
        Assert.Equal(220m, result.ClosingBalance);
        Assert.Equal(30m, result.TotalDebit);
        Assert.Equal(50m, result.TotalCredit);
        Assert.Equal(entryA, result.Lines[0].JournalEntryId);
    }

    /// <summary>
    /// A later page keeps the running balance of every earlier line of the range.
    /// </summary>
    [Fact]
    public void AccountLedger_SecondPage_ContinuesTheRunningBalance()
    {
        var movements = Enumerable.Range(1, 5)
            .Select(i => new AccountMovement(Guid.NewGuid(), "GLE-2026-000" + i.ToString(CultureInfo.InvariantCulture), new DateTime(2026, 1, i), "Line", 10m, 0m))
            .ToList();

        var page2 = AccountLedgerCalculator.Calculate(Cash, movements, new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), 2, 2);

        Assert.Equal(5, page2.TotalLines);
        Assert.Equal([30m, 40m], page2.Lines.Select(l => l.RunningBalance));
        Assert.Equal(50m, page2.ClosingBalance);
    }
}
