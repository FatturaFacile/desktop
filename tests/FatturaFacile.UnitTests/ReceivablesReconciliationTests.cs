// <copyright file="ReceivablesReconciliationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Domain.Receivables;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the reconciliation rules, the ledger running balance and the wire names.
/// </summary>
public sealed class ReceivablesReconciliationTests
{
    private static readonly DateOnly Today = new(2027, 6, 30);
    private static readonly Guid CustomerId = new(0xC0C0C0C0, 0xC0C0, 0xC0C0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0);

    private static ReceivableItem Item(decimal open, int daysPastDue, ReceivableStatus status = ReceivableStatus.Open, decimal? original = null) =>
        new(Guid.NewGuid(), CustomerId, null, 1, "invoice", "INV-1", "EUR", original ?? open, open, Today.AddDays(-daysPastDue - 30), Today.AddDays(-daysPastDue), null, status, Math.Max(0, daysPastDue), null, []);

    /// <summary>
    /// An allocation needs a positive amount that does not exceed the open amount, on an item that still has something to collect.
    /// </summary>
    [Fact]
    public void Validate_Amounts_AreCheckedAgainstTheItem()
    {
        var item = Item(100m, 5);

        Assert.Equal(ReconciliationRules.AmountKey, ReconciliationRules.Validate(item, AllocationType.Payment, 0m));
        Assert.Equal(ReconciliationRules.AmountKey, ReconciliationRules.Validate(item, AllocationType.Payment, -1m));
        Assert.Equal(ReconciliationRules.ExceedsKey, ReconciliationRules.Validate(item, AllocationType.Discount, 100.01m));
        Assert.Null(ReconciliationRules.Validate(item, AllocationType.Payment, 100m));
        Assert.Null(ReconciliationRules.Validate(item, AllocationType.Payment, 0.01m));
        Assert.Equal(ReconciliationRules.ClosedKey, ReconciliationRules.Validate(Item(0m, 5, ReceivableStatus.Paid, original: 100m), AllocationType.Payment, 10m));
        Assert.Equal(ReconciliationRules.ClosedKey, ReconciliationRules.Validate(Item(50m, 5, ReceivableStatus.WrittenOff), AllocationType.WriteOff, 10m));
    }

    /// <summary>
    /// A debit adjustment raises the open amount, so it is not capped by it and works on a settled item.
    /// </summary>
    [Fact]
    public void Validate_DebitAdjustment_IsAllowedOnAnyItemAndBeyondTheOpenAmount()
    {
        Assert.Null(ReconciliationRules.Validate(Item(10m, 5), AllocationType.DebitAdjustment, 500m));
        Assert.Null(ReconciliationRules.Validate(Item(0m, 5, ReceivableStatus.Paid, original: 100m), AllocationType.DebitAdjustment, 5m));
    }

    /// <summary>
    /// An amount taken from an unreconciled payment cannot exceed what that payment still has.
    /// </summary>
    [Fact]
    public void Validate_PaymentBeyondTheChosenPayment_IsRefused()
    {
        var payment = new UnreconciledPayment(Guid.NewGuid(), "BANK-1", Today, "EUR", 100m, 40m);
        var item = Item(100m, 5);

        Assert.Equal(ReconciliationRules.PaymentExceedsKey, ReconciliationRules.Validate(item, AllocationType.Payment, 40.01m, payment));
        Assert.Null(ReconciliationRules.Validate(item, AllocationType.Payment, 40m, payment));
        Assert.Null(ReconciliationRules.Validate(item, AllocationType.CreditNote, 60m, payment));
    }

    /// <summary>
    /// A partial payment leaves the item partially paid, a full one closes it, a write-off writes it off.
    /// </summary>
    [Fact]
    public void Preview_PartialAndFullSettlement_ShowsTheNewStateOfTheItem()
    {
        var item = Item(100m, 5, original: 100m);

        var partial = ReconciliationRules.Preview(item, AllocationType.Payment, 40m);
        var full = ReconciliationRules.Preview(item, AllocationType.Payment, 100m);
        var writeOff = ReconciliationRules.Preview(item, AllocationType.WriteOff, 100m);

        Assert.Equal(new ReconciliationPreview(60m, ReceivableStatus.PartiallyPaid), partial);
        Assert.Equal(new ReconciliationPreview(0m, ReceivableStatus.Paid), full);
        Assert.Equal(new ReconciliationPreview(0m, ReceivableStatus.WrittenOff), writeOff);
    }

    /// <summary>
    /// A debit adjustment raises the open amount; a disputed item stays disputed while something is open.
    /// </summary>
    [Fact]
    public void Preview_DebitAdjustmentAndDispute_RaiseTheOpenAmountAndKeepTheDispute()
    {
        var open = Item(100m, 5, original: 100m);
        var disputed = Item(100m, 5, ReceivableStatus.Disputed, original: 100m);

        Assert.Equal(new ReconciliationPreview(150m, ReceivableStatus.Open), ReconciliationRules.Preview(open, AllocationType.DebitAdjustment, 50m));
        Assert.Equal(new ReconciliationPreview(70m, ReceivableStatus.Disputed), ReconciliationRules.Preview(disputed, AllocationType.CreditNote, 30m));
    }

    /// <summary>
    /// The running balance of the ledger accumulates debit minus credit in date order, from the opening balance.
    /// </summary>
    [Fact]
    public void WithRunningBalance_OrdersByDateAndAccumulatesDebitMinusCredit()
    {
        LedgerMovement Movement(int day, decimal debit, decimal credit) =>
            new(Today.AddDays(day), null, "invoice", "invoice", "R" + day, null, null, "EUR", debit, credit, null, null, 0m);

        var ledger = CustomerLedger.WithRunningBalance(100m, [Movement(3, 0m, 50m), Movement(1, 300m, 0m), Movement(2, 0m, 120.5m)]);

        Assert.Equal([400m, 279.5m, 229.5m], ledger.Movements.Select(row => row.Balance));
        Assert.Equal(229.5m, ledger.ClosingBalance);
    }

    /// <summary>
    /// Every enum value survives the trip to its wire name and back, and unknown names read as the default.
    /// </summary>
    [Fact]
    public void Wire_EveryEnumValue_RoundTrips()
    {
        foreach (var status in Enum.GetValues<ReceivableStatus>())
        {
            Assert.Equal(status, ReceivablesWire.ParseStatus(status.ToWire()));
        }

        foreach (var type in Enum.GetValues<AllocationType>())
        {
            Assert.Equal(type, ReceivablesWire.ParseAllocationType(type.ToWire()));
        }

        foreach (var policy in Enum.GetValues<CreditReactionPolicy>())
        {
            Assert.Equal(policy, ReceivablesWire.ParsePolicy(policy.ToWire()));
        }

        foreach (var scope in Enum.GetValues<CreditScope>())
        {
            Assert.Equal(scope, ReceivablesWire.ParseScope(scope.ToWire()));
        }

        foreach (var outcome in Enum.GetValues<CreditOutcome>())
        {
            Assert.Equal(outcome, ReceivablesWire.ParseOutcome(outcome.ToWire()));
        }

        foreach (var rating in Enum.GetValues<RiskRating>())
        {
            Assert.Equal(rating, ReceivablesWire.ParseRating(rating.ToWire()));
        }

        Assert.Equal(ReceivableStatus.Open, ReceivablesWire.ParseStatus("nonsense"));
        Assert.Equal(AllocationType.Payment, ReceivablesWire.ParseAllocationType(null));
        Assert.Equal("partially_paid", ReceivableStatus.PartiallyPaid.ToWire());
        Assert.Equal("block_delivery_note", CreditReactionPolicy.BlockDeliveryNote.ToWire());
    }

    /// <summary>
    /// Amounts round to two decimals with the midpoint away from zero.
    /// </summary>
    [Fact]
    public void ReceivablesMath_Round_IsMidpointAwayFromZero()
    {
        Assert.Equal(1.01m, ReceivablesMath.Round(1.005m));
        Assert.Equal(-1.01m, ReceivablesMath.Round(-1.005m));
        Assert.Equal(0.17m, ReceivablesMath.Percent(33.33m, 0.5m));
    }
}
