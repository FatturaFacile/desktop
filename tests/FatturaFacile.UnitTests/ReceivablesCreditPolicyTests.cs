// <copyright file="ReceivablesCreditPolicyTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Domain.Receivables;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the exposure rule, the credit decision and reaction policies, the credit limits and the overrides.
/// </summary>
public sealed class ReceivablesCreditPolicyTests
{
    private static readonly DateOnly Today = new(2027, 6, 30);
    private static readonly Guid CustomerId = new(0xC0C0C0C0, 0xC0C0, 0xC0C0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0);

    private static ReceivableItem Item(decimal open, int daysPastDue, ReceivableStatus status = ReceivableStatus.Open, decimal? original = null) =>
        new(Guid.NewGuid(), CustomerId, null, 1, "invoice", "INV-1", "EUR", original ?? open, open, Today.AddDays(-daysPastDue - 30), Today.AddDays(-daysPastDue), null, status, Math.Max(0, daysPastDue), null, []);

    private static CustomerCreditLimit Limit(decimal granted, CreditReactionPolicy policy, int? maxOverdueDays = null, decimal tolerance = 0m, decimal insured = 0m, decimal coverage = 0m) =>
        new(Guid.NewGuid(), CustomerId, "EUR", granted, insured, 0m, null, null, coverage, maxOverdueDays, tolerance, policy, Today.AddDays(-30), null, true, null);

    private static CreditExposure Exposure(decimal total, decimal overdue = 0m, int maxDays = 0) =>
        new(total, 0m, 0m, 0m, total, overdue, overdue > 0m ? 1 : 0, maxDays);

    private static CreditOverride Override(CreditScope scope, DateTime? expires = null, DateTime? revoked = null) =>
        new(Guid.NewGuid(), CustomerId, scope, "Agreed with the sales manager", "admin", expires, revoked, null, null);

    /// <summary>
    /// The exposure rule adds open invoices, unbilled delivery notes and confirmed orders and subtracts the unreconciled payments.
    /// </summary>
    [Fact]
    public void Exposure_Total_IsInvoicesPlusDeliveryNotesPlusOrdersMinusUnreconciledPayments()
    {
        Assert.Equal(950.00m, CreditExposure.Total(800m, 100m, 200m, 150m));
        Assert.Equal(-200.00m, CreditExposure.Total(100m, 0m, 0m, 300m));
    }

    /// <summary>
    /// The exposure of a customer is built from its open items: paid and written-off items count for nothing.
    /// </summary>
    [Fact]
    public void Calculate_FromItems_CountsOnlyOpenItemsAndTheOverdueOnes()
    {
        ReceivableItem[] items =
        [
            Item(500m, 40),
            Item(300m, -5),
            Item(0m, 100, ReceivableStatus.Paid, original: 700m),
            Item(900m, 100, ReceivableStatus.WrittenOff),
        ];

        var exposure = CreditExposure.Calculate(items, Today, 100m, 200m, 150m);

        Assert.Equal(800m, exposure.OpenInvoices);
        Assert.Equal(950m, exposure.TotalExposure);
        Assert.Equal(500m, exposure.OverdueAmount);
        Assert.Equal(1, exposure.OverdueItems);
        Assert.Equal(40, exposure.MaxDaysPastDue);
        Assert.Equal(100m, exposure.UnbilledDeliveryNotes);
        Assert.Equal(200m, exposure.ConfirmedOrders);
        Assert.Equal(150m, exposure.UnreconciledPayments);
    }

    /// <summary>
    /// Without a limit and without a credit hold nothing is checked.
    /// </summary>
    [Fact]
    public void Evaluate_NoLimitAndNoHold_IsOk()
    {
        var decision = CreditDecisionPolicy.Evaluate(Exposure(5000m), null, false, CreditScope.Order, 100m, [], DateTime.UtcNow);

        Assert.Equal(CreditOutcome.Ok, decision.Outcome);
        Assert.Empty(decision.Breaches);
    }

    /// <summary>
    /// Under the limit the outcome is ok and the projected exposure includes what the operation adds.
    /// </summary>
    [Fact]
    public void Evaluate_UnderTheLimit_IsOkWithTheProjectedExposure()
    {
        var decision = CreditDecisionPolicy.Evaluate(Exposure(600m), Limit(1000m, CreditReactionPolicy.CreditHold), false, CreditScope.Order, 300m, [], DateTime.UtcNow);

        Assert.Equal(CreditOutcome.Ok, decision.Outcome);
        Assert.Equal(900m, decision.Exposure);
        Assert.Equal(1000m, decision.Limit);
        Assert.False(decision.Blocks);
    }

    /// <summary>
    /// What the operation adds counts: an order that crosses the limit is an over-limit breach.
    /// </summary>
    [Fact]
    public void Evaluate_AdditionalPushesOverTheLimit_ReportsOverLimit()
    {
        var decision = CreditDecisionPolicy.Evaluate(Exposure(900m), Limit(1000m, CreditReactionPolicy.BlockOrder), false, CreditScope.Order, 200m, [], DateTime.UtcNow);

        Assert.Equal(["over_limit"], decision.Breaches);
        Assert.True(decision.Blocks);
    }

    /// <summary>
    /// The reaction policy decides which operation is blocked and which only warns.
    /// </summary>
    [Fact]
    public void Evaluate_ReactionPolicy_DecidesWhatIsBlocked()
    {
        var exposure = Exposure(1500m);

        var warning = CreditDecisionPolicy.Evaluate(exposure, Limit(1000m, CreditReactionPolicy.Warning), false, CreditScope.Order, 0m, [], DateTime.UtcNow);
        var orderOnOrderPolicy = CreditDecisionPolicy.Evaluate(exposure, Limit(1000m, CreditReactionPolicy.BlockOrder), false, CreditScope.Order, 0m, [], DateTime.UtcNow);
        var noteOnOrderPolicy = CreditDecisionPolicy.Evaluate(exposure, Limit(1000m, CreditReactionPolicy.BlockOrder), false, CreditScope.DeliveryNote, 0m, [], DateTime.UtcNow);
        var noteOnNotePolicy = CreditDecisionPolicy.Evaluate(exposure, Limit(1000m, CreditReactionPolicy.BlockDeliveryNote), false, CreditScope.DeliveryNote, 0m, [], DateTime.UtcNow);
        var orderOnNotePolicy = CreditDecisionPolicy.Evaluate(exposure, Limit(1000m, CreditReactionPolicy.BlockDeliveryNote), false, CreditScope.Order, 0m, [], DateTime.UtcNow);
        var hold = CreditDecisionPolicy.Evaluate(exposure, Limit(1000m, CreditReactionPolicy.CreditHold), false, CreditScope.DeliveryNote, 0m, [], DateTime.UtcNow);

        Assert.Equal(CreditOutcome.Warning, warning.Outcome);
        Assert.Equal(CreditOutcome.Block, orderOnOrderPolicy.Outcome);
        Assert.Equal(CreditOutcome.Warning, noteOnOrderPolicy.Outcome);
        Assert.Equal(CreditOutcome.Block, noteOnNotePolicy.Outcome);
        Assert.Equal(CreditOutcome.Warning, orderOnNotePolicy.Outcome);
        Assert.Equal(CreditOutcome.Block, hold.Outcome);
        Assert.True(hold.Blocks);
        Assert.True(warning.Warns);
    }

    /// <summary>
    /// A breach of the tolerated overdue days needs both the days and an overdue amount over the tolerance.
    /// </summary>
    [Fact]
    public void Evaluate_OverdueBeyondTheTolerance_ReportsOverdueDays()
    {
        var limit = Limit(10000m, CreditReactionPolicy.BlockOrder, maxOverdueDays: 30, tolerance: 100m);

        var breached = CreditDecisionPolicy.Evaluate(Exposure(1000m, overdue: 500m, maxDays: 45), limit, false, CreditScope.Order, 0m, [], DateTime.UtcNow);
        var withinTolerance = CreditDecisionPolicy.Evaluate(Exposure(1000m, overdue: 50m, maxDays: 45), limit, false, CreditScope.Order, 0m, [], DateTime.UtcNow);
        var withinDays = CreditDecisionPolicy.Evaluate(Exposure(1000m, overdue: 500m, maxDays: 20), limit, false, CreditScope.Order, 0m, [], DateTime.UtcNow);

        Assert.Equal(["overdue_days"], breached.Breaches);
        Assert.Equal(CreditOutcome.Block, breached.Outcome);
        Assert.Equal(CreditOutcome.Ok, withinTolerance.Outcome);
        Assert.Equal(CreditOutcome.Ok, withinDays.Outcome);
    }

    /// <summary>
    /// A customer on credit hold with no limit is blocked on orders.
    /// </summary>
    [Fact]
    public void Evaluate_CreditHoldWithoutLimit_BlocksOrders()
    {
        var decision = CreditDecisionPolicy.Evaluate(Exposure(0m), null, true, CreditScope.Order, 0m, [], DateTime.UtcNow);

        Assert.Equal(CreditOutcome.Block, decision.Outcome);
        Assert.Equal(["credit_hold"], decision.Breaches);
        Assert.Equal(CreditReactionPolicy.CreditHold, decision.ReactionPolicy);
    }

    /// <summary>
    /// An active override that covers the scope turns the block into ok and says so, keeping the breaches.
    /// </summary>
    [Fact]
    public void Evaluate_ActiveOverride_LetsTheOperationThrough()
    {
        var now = new DateTime(2027, 6, 30, 12, 0, 0, DateTimeKind.Utc);
        var limit = Limit(1000m, CreditReactionPolicy.CreditHold);

        var overridden = CreditDecisionPolicy.Evaluate(Exposure(1500m), limit, false, CreditScope.Order, 0m, [Override(CreditScope.Order, expires: now.AddDays(1))], now);
        var everything = CreditDecisionPolicy.Evaluate(Exposure(1500m), limit, false, CreditScope.DeliveryNote, 0m, [Override(CreditScope.All)], now);

        Assert.Equal(CreditOutcome.Ok, overridden.Outcome);
        Assert.True(overridden.Overridden);
        Assert.Equal(["over_limit"], overridden.Breaches);
        Assert.Equal(CreditOutcome.Ok, everything.Outcome);
    }

    /// <summary>
    /// A revoked, expired or other-scope override does not let the operation through.
    /// </summary>
    [Fact]
    public void Evaluate_RevokedExpiredOrOtherScopeOverride_DoesNotHelp()
    {
        var now = new DateTime(2027, 6, 30, 12, 0, 0, DateTimeKind.Utc);
        var limit = Limit(1000m, CreditReactionPolicy.CreditHold);

        var revoked = CreditDecisionPolicy.Evaluate(Exposure(1500m), limit, false, CreditScope.Order, 0m, [Override(CreditScope.Order, revoked: now.AddHours(-1))], now);
        var expired = CreditDecisionPolicy.Evaluate(Exposure(1500m), limit, false, CreditScope.Order, 0m, [Override(CreditScope.Order, expires: now.AddHours(-1))], now);
        var otherScope = CreditDecisionPolicy.Evaluate(Exposure(1500m), limit, false, CreditScope.Order, 0m, [Override(CreditScope.DeliveryNote)], now);

        Assert.Equal(CreditOutcome.Block, revoked.Outcome);
        Assert.Equal(CreditOutcome.Block, expired.Outcome);
        Assert.Equal(CreditOutcome.Block, otherScope.Outcome);
        Assert.False(otherScope.Overridden);
    }

    /// <summary>
    /// The reaction policy matrix: which policy blocks which scope.
    /// </summary>
    /// <param name="policy">The policy.</param>
    /// <param name="scope">The operation.</param>
    /// <param name="expected">Whether it blocks.</param>
    [Theory]
    [InlineData(CreditReactionPolicy.Warning, CreditScope.Order, false)]
    [InlineData(CreditReactionPolicy.BlockOrder, CreditScope.Order, true)]
    [InlineData(CreditReactionPolicy.BlockOrder, CreditScope.DeliveryNote, false)]
    [InlineData(CreditReactionPolicy.BlockDeliveryNote, CreditScope.DeliveryNote, true)]
    [InlineData(CreditReactionPolicy.BlockDeliveryNote, CreditScope.Order, false)]
    [InlineData(CreditReactionPolicy.CreditHold, CreditScope.Order, true)]
    [InlineData(CreditReactionPolicy.CreditHold, CreditScope.DeliveryNote, true)]
    public void Blocks_Matrix_MatchesTheServerRules(CreditReactionPolicy policy, CreditScope scope, bool expected)
    {
        Assert.Equal(expected, CreditDecisionPolicy.Blocks(policy, scope));
    }

    /// <summary>
    /// A limit is in force from its first to its last day included, and only while active.
    /// </summary>
    [Fact]
    public void CreditLimit_IsEffectiveOn_IncludesBothEndsAndNeedsActive()
    {
        var limit = Limit(1000m, CreditReactionPolicy.Warning) with { ValidFrom = Today, ValidTo = Today.AddDays(10) };

        Assert.False(limit.IsEffectiveOn(Today.AddDays(-1)));
        Assert.True(limit.IsEffectiveOn(Today));
        Assert.True(limit.IsEffectiveOn(Today.AddDays(10)));
        Assert.False(limit.IsEffectiveOn(Today.AddDays(11)));
        Assert.False((limit with { IsActive = false }).IsEffectiveOn(Today));
        Assert.True((limit with { ValidTo = null }).IsEffectiveOn(Today.AddDays(500)));
    }

    /// <summary>
    /// Utilisation, available credit and the insured cover follow from the limit and the exposure.
    /// </summary>
    [Fact]
    public void CreditLimit_UtilizationAvailableAndCover_AreComputed()
    {
        var limit = Limit(1000m, CreditReactionPolicy.Warning, insured: 100000m, coverage: 80m);

        Assert.Equal(85.00m, limit.UtilizationPercent(850m));
        Assert.Equal(150.00m, limit.AvailableCredit(850m));
        Assert.Equal(-50.00m, limit.AvailableCredit(1050m));
        Assert.Equal(80000.00m, limit.ComputeInsuredCover());
        Assert.Null((limit with { GrantedLimit = 0m }).UtilizationPercent(10m));
    }

    /// <summary>
    /// An override is active until it is revoked or expires, and covers its own scope or everything.
    /// </summary>
    [Fact]
    public void Override_ActiveAndCovers_FollowExpiryRevocationAndScope()
    {
        var now = new DateTime(2027, 6, 30, 12, 0, 0, DateTimeKind.Utc);

        Assert.True(Override(CreditScope.Order).IsActiveAt(now));
        Assert.True(Override(CreditScope.Order, expires: now.AddMinutes(1)).IsActiveAt(now));
        Assert.False(Override(CreditScope.Order, expires: now).IsActiveAt(now));
        Assert.False(Override(CreditScope.Order, revoked: now).IsActiveAt(now));
        Assert.True(Override(CreditScope.All).Covers(CreditScope.DeliveryNote));
        Assert.True(Override(CreditScope.Order).Covers(CreditScope.Order));
        Assert.False(Override(CreditScope.Order).Covers(CreditScope.DeliveryNote));
    }
}
