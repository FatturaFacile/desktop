// <copyright file="ReceivablesFixtures.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable SA1600 // Elements should be documented: internal test fixtures.
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.UnitTests;

internal static class ReceivablesFixtures
{
    public static readonly Guid CustomerId = new(0xC0C0C0C0, 0xC0C0, 0xC0C0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0);
    public static readonly Guid OtherCustomerId = new(0xD0D0D0D0, 0xD0D0, 0xD0D0, 0xD0, 0xD0, 0xD0, 0xD0, 0xD0, 0xD0, 0xD0, 0xD0);
    public static readonly DateTime Synced = new(2027, 6, 30, 9, 0, 0, DateTimeKind.Utc);
    public static readonly DateOnly Today = new(2027, 6, 30);

    public static ReceivablesLoad<T> Load<T>(T data, bool fromCache = false) => new(data, fromCache, Synced);

    public static ReceivableItem Item(string number, decimal open, int daysPastDue, ReceivableStatus status = ReceivableStatus.Open, decimal? original = null, Guid? id = null, IReadOnlyList<ReceivableAllocation>? allocations = null) =>
        new(id ?? Guid.NewGuid(), CustomerId, null, 1, "invoice", number, "EUR", original ?? open, open, Today.AddDays(-daysPastDue - 30), Today.AddDays(-daysPastDue), null, status, Math.Max(0, daysPastDue), status == ReceivableStatus.Disputed ? "Wrong VAT" : null, allocations ?? []);

    public static ReceivableAllocation Allocation(Guid itemId, decimal amount, bool reversed = false) =>
        new(Guid.NewGuid(), itemId, AllocationType.Payment, "manual", null, "BANK-1", amount, Today, null, reversed ? Synced : null);

    public static ReceivablesLoad<ReceivableItemPage> Page(params ReceivableItem[] items) => Load(new ReceivableItemPage(items, 1, 3, 120));

    public static AgingReport Aging(Guid? customerId = null)
    {
        var buckets = new AgingBucketEngine().Buckets;
        var totals = buckets.ToDictionary(bucket => bucket.Key, _ => 0m, StringComparer.Ordinal);
        totals["current"] = 600m;
        totals["31_60"] = 300m;
        totals["121_plus"] = 100m;
        var customerBuckets = buckets.ToDictionary(bucket => bucket.Key, _ => 0m, StringComparer.Ordinal);
        customerBuckets["31_60"] = 300m;

        return new AgingReport(
            Today,
            "EUR",
            ["EUR"],
            buckets,
            totals,
            1000m,
            400m,
            100m,
            [new AgingCustomerRow(customerId ?? CustomerId, "Rossi SA", customerBuckets, 300m, 300m, 2)]);
    }

    public static ReceivablesDashboard Dashboard(int blocked = 1) =>
        new(
            Today,
            "EUR",
            new ReceivablesKpis(5000m, 4000m, 900m, 300m, 41.5m, 70m, 3930m, blocked, 2),
            blocked == 0 ? Array.Empty<BlockedCustomer>() : [new BlockedCustomer(CustomerId, "over_limit", 1200m, 1000m)]);

    public static CustomerCreditLimit Limit(Guid? id = null, decimal granted = 1000m, CreditReactionPolicy policy = CreditReactionPolicy.BlockOrder) =>
        new(id ?? Guid.NewGuid(), CustomerId, "EUR", granted, 0m, 0m, null, null, 0m, 30, 50m, policy, Today.AddDays(-100), null, true, null);

    public static CustomerCreditStatus Credit(bool blocked = false, bool hold = false, CreditOutcome? outcome = null, IReadOnlyList<CreditOverride>? overrides = null)
    {
        var decision = outcome is { } value ? new CreditDecision(value, value == CreditOutcome.Ok ? Array.Empty<string>() : ["over_limit"], null, 1150m, 1000m, Overridden: false, CreditReactionPolicy.BlockOrder) : null;

        return new CustomerCreditStatus(
            CustomerId,
            Today,
            Limit(),
            new CreditExposure(900m, 100m, 200m, 50m, 1150m, 300m, 2, 45),
            115m,
            -150m,
            0m,
            1150m,
            hold,
            blocked,
            decision,
            overrides ?? []);
    }

    public static CreditOverride Override(CreditScope scope = CreditScope.Order, bool revoked = false) =>
        new(Guid.NewGuid(), CustomerId, scope, "Approved by the CFO", "cfo", Synced.AddDays(30), revoked ? Synced : null, null, Synced);

    public static CreditRiskScore Risk(RiskRating rating = RiskRating.High) =>
        new(63, rating, [new RiskFactor("credit_utilization", "100.00", 20m), new RiskFactor("credit_block", "1", 5m)], Blocked: true);

    public static ProvisionSummary Provision()
    {
        var engine = new AgingBucketEngine();
        var totals = engine.Buckets.ToDictionary(bucket => bucket.Key, _ => 1000m, StringComparer.Ordinal);

        return BadDebtProvisionCalculator.Calculate(engine.Buckets, totals, null);
    }

    public static ReceivablesActionResult Sent() => new(ReceivablesActionStatus.Sent);

    public static ReceivablesActionResult Queued() => new(ReceivablesActionStatus.Queued);
}
