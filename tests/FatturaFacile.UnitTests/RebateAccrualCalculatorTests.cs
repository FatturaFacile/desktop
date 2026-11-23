// <copyright file="RebateAccrualCalculatorTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the accrual arithmetic: the signed qualifying spend (credit notes count negative), the accrual of a period as the
/// difference between the rebate earned to date and what was already accrued, and the journal entry preview.
/// </summary>
public sealed class RebateAccrualCalculatorTests
{
    /// <summary>
    /// A credit note (TD04) counts negative, whatever the case of the code; every other document counts positive.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <param name="expected">The expected signed amount.</param>
    [Theory]
    [InlineData("TD01", 200)]
    [InlineData("TD04", -200)]
    [InlineData("td04", -200)]
    [InlineData("TD24", 200)]
    public void SignedNet_CreditNoteCountsNegative(string documentType, int expected) =>
        Assert.Equal(expected, RebateAccrualCalculator.SignedNet(documentType, 200m));

    /// <summary>
    /// The qualifying spend subtracts the credit notes.
    /// </summary>
    [Fact]
    public void NetVolume_SubtractsCreditNotes()
    {
        RebateInvoiceAmount[] documents =
        [
            new RebateInvoiceAmount("TD01", 1000m),
            new RebateInvoiceAmount("TD01", 500m),
            new RebateInvoiceAmount(RebateCodes.CreditNoteDocumentType, 300m),
        ];

        Assert.Equal(1200m, RebateAccrualCalculator.NetVolume(documents));
        Assert.Equal(0m, RebateAccrualCalculator.NetVolume([]));
    }

    /// <summary>
    /// The qualifying spend is rounded half away from zero.
    /// </summary>
    [Fact]
    public void NetVolume_RoundsToTwoDecimals() =>
        Assert.Equal(100.01m, RebateAccrualCalculator.NetVolume([new RebateInvoiceAmount("TD01", 100.005m)]));

    /// <summary>
    /// The accrual of a period is the rebate earned to date minus what was already accrued.
    /// </summary>
    /// <param name="earned">The rebate earned to date.</param>
    /// <param name="accrued">The rebate already accrued.</param>
    /// <param name="expected">The expected accrual.</param>
    [Theory]
    [InlineData(1300, 1000, 300)]
    [InlineData(1000, 1000, 0)]
    [InlineData(1000, 1300, -300)]
    [InlineData(200, 0, 200)]
    public void Delta_IsEarnedMinusAccrued(int earned, int accrued, int expected) =>
        Assert.Equal(expected, RebateAccrualCalculator.Delta(earned, accrued));

    /// <summary>
    /// A drop of the earned rebate (a credit note after a tier was reached) is a negative accrual, and the delta is rounded.
    /// </summary>
    [Fact]
    public void Delta_AfterACreditNote_CanBeNegativeAndIsRounded()
    {
        var earnedBefore = RebateTierCalculator.RebateOn([new RebateTier(0m, 5m)], 1000m, RebateTierMode.Retroactive).Rebate;
        var volumeAfter = RebateAccrualCalculator.NetVolume([new RebateInvoiceAmount("TD01", 1000m), new RebateInvoiceAmount("TD04", 100.01m)]);
        var earnedAfter = RebateTierCalculator.RebateOn([new RebateTier(0m, 5m)], volumeAfter, RebateTierMode.Retroactive).Rebate;

        Assert.Equal(50m, earnedBefore);
        Assert.Equal(899.99m, volumeAfter);
        Assert.Equal(-5.00m, RebateAccrualCalculator.Delta(earnedAfter, earnedBefore));
    }

    /// <summary>
    /// A positive accrual debits accrued income and credits the supplier bonus income; a negative one swaps the sides.
    /// </summary>
    [Fact]
    public void Preview_DebitsOrCreditsAccruedIncomeBySign()
    {
        var positive = RebateAccrualCalculator.Preview(300m, hasAccounts: true);
        var negative = RebateAccrualCalculator.Preview(-50m, hasAccounts: true);

        Assert.Equal(new RebateJournalPreview(IsPosted: true, 300m, DebitsAccruedIncome: true), positive);
        Assert.Equal(new RebateJournalPreview(IsPosted: true, 50m, DebitsAccruedIncome: false), negative);
    }

    /// <summary>
    /// Nothing is booked without both accounts or without an amount.
    /// </summary>
    [Fact]
    public void Preview_WithoutAccountsOrAmount_IsNotPosted()
    {
        Assert.False(RebateAccrualCalculator.Preview(300m, hasAccounts: false).IsPosted);
        Assert.False(RebateAccrualCalculator.Preview(0m, hasAccounts: true).IsPosted);
    }

    /// <summary>
    /// The VAT and the gross total of a settlement are rounded to two decimals and the set-off checks the open balance.
    /// </summary>
    [Fact]
    public void SettlementAmounts_AreRoundedAndTheSetOffChecksTheOpenBalance()
    {
        Assert.Equal(81m, RebateSettlementCalculator.Tax(1000m, 8.1m));
        Assert.Equal(1081m, RebateSettlementCalculator.Gross(1000m, 8.1m));
        Assert.Equal(16.2m, RebateSettlementCalculator.Tax(200m, 8.1m));
        Assert.Equal(0m, RebateSettlementCalculator.Tax(0.05m, 8.1m));

        RebatePayableInvoice invoice = new("p1", "F-1", new DateOnly(2027, 2, 1), 1000m, 400m, "CHF");
        Assert.Equal(600m, invoice.OpenBalance);
        Assert.True(RebateSettlementCalculator.CanSetOff(invoice, 600m));
        Assert.False(RebateSettlementCalculator.CanSetOff(invoice, 600.01m));
    }
}
