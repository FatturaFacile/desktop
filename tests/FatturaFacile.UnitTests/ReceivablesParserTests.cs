// <copyright file="ReceivablesParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the parser of the receivables answers: amounts as invariant decimals, the buckets of the answer, optional members and the offline-friendly fallbacks.
/// </summary>
public sealed class ReceivablesParserTests
{
    private const string ItemsPage = """
        {"data":[{"id":"11111111-1111-1111-1111-111111111111","customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","invoice_id":"22222222-2222-2222-2222-222222222222","sequence":1,"document_type":"invoice","document_number":"INV-1","currency":"EUR","original_amount":"1000.00","open_amount":"400.50","settled_amount":"599.50","issue_date":"2027-05-01","due_date":"2027-05-31","value_date":null,"status":"overdue","stored_status":"partially_paid","days_past_due":30,"dispute_reason":null}],"links":{},"meta":{"current_page":2,"last_page":5,"total":230}}
        """;

    private const string Aging = """
        {"data":{"as_of":"2027-06-30","currency":"CHF","available_currencies":["CHF","EUR"],"buckets":[{"key":"current","from":0,"to":0},{"key":"1_15","from":1,"to":15},{"key":"16_plus","from":16,"to":null}],"totals":{"current":"100.00","1_15":"50.00","16_plus":"25.00"},"total_open":"175.00","total_overdue":"75.00","over_90":"0.00","customers":[{"customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","customer_name":"Rossi SA","buckets":{"current":"100.00","1_15":"50.00"},"total_open":"150.00","total_overdue":"50.00","items":2}]}}
        """;

    private const string Credit = """
        {"data":{"customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","as_of":"2027-06-30","limit":{"id":"33333333-3333-3333-3333-333333333333","customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","currency":"EUR","granted_limit":"1000.00","insured_limit":"500.00","insured_cover":"400.00","insurer_name":"Coface","policy_number":"P-1","coverage_percentage":"80.00","max_overdue_days":30,"overdue_tolerance_amount":"50.00","reaction_policy":"block_order","valid_from":"2027-01-01","valid_to":null,"is_active":true,"notes":null},"exposure":{"open_invoices":"900.00","unbilled_delivery_notes":"100.00","confirmed_orders":"200.00","unreconciled_payments":"50.00","total_exposure":"1150.00","overdue_amount":"300.00","overdue_items":2,"max_days_past_due":45},"utilization_percent":"115.00","available_credit":"-150.00","insured_cover":"400.00","uninsured_exposure":"750.00","is_credit_hold":false,"is_blocked":true,"decision":{"outcome":"block","breaches":["over_limit","overdue_days"],"reason":"x","exposure":"1150.00","limit":"1000.00","overridden":false,"reaction_policy":"block_order"},"overrides":[{"id":"44444444-4444-4444-4444-444444444444","customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","scope":"order","reason":"Approved by the CFO","authorized_by":"cfo","expires_at":"2027-07-31T00:00:00Z","revoked_at":null,"revoke_reason":null,"created_at":"2027-06-01T10:00:00Z"}]}}
        """;

    /// <summary>
    /// A page of items is read from the resource pagination, amounts as decimals with the invariant culture.
    /// </summary>
    [Fact]
    public void ParseItemPage_ResourcePagination_ReadsTheItemsAndTheMeta()
    {
        var page = ReceivablesParser.ParseItemPage(ItemsPage);

        var item = Assert.Single(page.Items);
        Assert.Equal(new Guid(0x11111111, 0x1111, 0x1111, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11), item.Id);
        Assert.Equal("INV-1", item.DocumentNumber);
        Assert.Equal(1000.00m, item.OriginalAmount);
        Assert.Equal(400.50m, item.OpenAmount);
        Assert.Equal(599.50m, item.SettledAmount);
        Assert.Equal(ReceivableStatus.Overdue, item.Status);
        Assert.Equal(30, item.DaysPastDue);
        Assert.Equal(new DateOnly(2027, 5, 31), item.DueDate);
        Assert.Null(item.ValueDate);
        Assert.Null(item.DisputeReason);
        Assert.Empty(item.Allocations);
        Assert.Equal(2, page.CurrentPage);
        Assert.Equal(5, page.LastPage);
        Assert.Equal(230, page.Total);
    }

    /// <summary>
    /// An item with its allocations is read, including a reversed one.
    /// </summary>
    [Fact]
    public void ParseItem_WithAllocations_ReadsTheAllocationsOfTheItem()
    {
        const string json = """
            {"data":{"id":"11111111-1111-1111-1111-111111111111","customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","sequence":1,"document_type":"invoice","document_number":"INV-1","currency":"EUR","original_amount":"100.00","open_amount":"60.00","issue_date":"2027-05-01","due_date":"2027-05-31","status":"disputed","days_past_due":0,"dispute_reason":"Wrong VAT","allocations":[{"id":"55555555-5555-5555-5555-555555555555","receivable_item_id":"11111111-1111-1111-1111-111111111111","source_type":"credit_note","origin":"manual","source_id":"abc","source_reference":"CN-7","amount":"40.00","value_date":"2027-06-02","notes":null,"reversed_at":"2027-06-03T08:00:00Z"}]}}
            """;

        var item = ReceivablesParser.ParseItem(json);

        Assert.Equal(ReceivableStatus.Disputed, item.Status);
        Assert.Equal("Wrong VAT", item.DisputeReason);
        var allocation = Assert.Single(item.Allocations);
        Assert.Equal(AllocationType.CreditNote, allocation.Type);
        Assert.Equal(40.00m, allocation.Amount);
        Assert.Equal("CN-7", allocation.SourceReference);
        Assert.True(allocation.IsReversed);
    }

    /// <summary>
    /// The aging is rendered from the buckets of the answer, not from the defaults, and every customer row carries every bucket.
    /// </summary>
    [Fact]
    public void ParseAging_UsesTheBucketsOfTheAnswer_AndCompletesTheCustomerRows()
    {
        var report = ParseAgingReport();

        Assert.Equal(["current", "1_15", "16_plus"], report.Buckets.Select(bucket => bucket.Key), StringComparer.Ordinal);
        Assert.Null(report.Buckets[2].To);
        Assert.Equal("CHF", report.Currency);
        Assert.Equal(["CHF", "EUR"], report.AvailableCurrencies);
        Assert.Equal(25.00m, report.Totals["16_plus"]);
        Assert.Equal(175.00m, report.TotalOpen);
        Assert.Equal(75.00m, report.TotalOverdue);
        var row = Assert.Single(report.Customers);
        Assert.Equal("Rossi SA", row.CustomerName);
        Assert.Equal(50.00m, row.Buckets["1_15"]);
        Assert.Equal(0m, row.Buckets["16_plus"]);
        Assert.Equal(2, row.Items);
    }

    /// <summary>
    /// An answer without buckets still renders with the default six.
    /// </summary>
    [Fact]
    public void ParseAging_WithoutBuckets_FallsBackToTheDefaultBuckets()
    {
        var report = ReceivablesParser.ParseAging("""{"data":{"as_of":"2027-06-30","currency":"EUR","totals":{},"customers":[]}}""");

        Assert.Equal(6, report.Buckets.Count);
        Assert.Equal(0m, report.Totals["121_plus"]);
    }

    /// <summary>
    /// When the server sends no running balance the ledger computes it in date order.
    /// </summary>
    [Fact]
    public void ParseLedger_WithoutBalances_ComputesTheRunningBalance()
    {
        const string json = """
            {"data":{"movements":[{"date":"2027-06-10","kind":"payment","document_type":"payment","reference":"PAY-1","currency":"EUR","debit":"0.00","credit":"40.00"},{"date":"2027-06-01","kind":"invoice","document_type":"invoice","reference":"INV-1","currency":"EUR","debit":"100.00","credit":"0.00"}],"closing_balance":"60.00"}}
            """;

        var ledger = ReceivablesParser.ParseLedger(json);

        Assert.Equal([100m, 60m], ledger.Movements.Select(row => row.Balance));
        Assert.Equal("INV-1", ledger.Movements[0].Reference);
        Assert.Equal(60m, ledger.ClosingBalance);
    }

    /// <summary>
    /// A ledger that carries its balances is kept as sent, with the closing balance.
    /// </summary>
    [Fact]
    public void ParseLedger_WithBalances_KeepsTheServerBalances()
    {
        const string json = """
            {"data":{"movements":[{"date":"2027-06-01","value_date":"2027-06-02","kind":"invoice","document_type":"invoice","reference":"INV-1","receivable_item_id":"11111111-1111-1111-1111-111111111111","allocation_id":null,"currency":"EUR","debit":"100.00","credit":"0.00","due_date":"2027-07-01","status":"open","balance":"123.45"}],"closing_balance":"123.45"}}
            """;

        var ledger = ReceivablesParser.ParseLedger(json);

        var row = Assert.Single(ledger.Movements);
        Assert.Equal(123.45m, row.Balance);
        Assert.Equal(new DateOnly(2027, 7, 1), row.DueDate);
        Assert.Equal("open", row.Status);
        Assert.Equal(123.45m, ledger.ClosingBalance);
    }

    /// <summary>
    /// The unreconciled payments of a customer are read with what is still unallocated.
    /// </summary>
    [Fact]
    public void ParseUnreconciledPayments_ReadsTheAvailablePayments()
    {
        const string json = """
            {"data":[{"ledger_entry_id":"66666666-6666-6666-6666-666666666666","reference":"BANK-9","date":"2027-06-05","currency":"EUR","amount":"500.00","unallocated":"120.00"}]}
            """;

        var payments = ReceivablesParser.ParseUnreconciledPayments(json);

        var payment = Assert.Single(payments);
        Assert.Equal("BANK-9", payment.Reference);
        Assert.Equal(500.00m, payment.Amount);
        Assert.Equal(120.00m, payment.Unallocated);
    }

    /// <summary>
    /// The credit position carries the limit, the exposure, the block state, the decision and the overrides.
    /// </summary>
    [Fact]
    public void ParseCredit_ReadsLimitExposureDecisionAndOverrides()
    {
        var status = ReceivablesParser.ParseCredit(Credit);

        Assert.NotNull(status.Limit);
        Assert.Equal(1000.00m, status.Limit.GrantedLimit);
        Assert.Equal(CreditReactionPolicy.BlockOrder, status.Limit.ReactionPolicy);
        Assert.Equal(30, status.Limit.MaxOverdueDays);
        Assert.Null(status.Limit.ValidTo);
        Assert.True(status.Limit.IsActive);
        Assert.Equal(1150.00m, status.Exposure.TotalExposure);
        Assert.Equal(45, status.Exposure.MaxDaysPastDue);
        Assert.Equal(115.00m, status.UtilizationPercent);
        Assert.Equal(-150.00m, status.AvailableCredit);
        Assert.True(status.IsBlocked);
        Assert.False(status.IsCreditHold);
        Assert.NotNull(status.Decision);
        Assert.Equal(CreditOutcome.Block, status.Decision.Outcome);
        Assert.Equal(["over_limit", "overdue_days"], status.Decision.Breaches);
        var granted = Assert.Single(status.Overrides);
        Assert.Equal(CreditScope.Order, granted.Scope);
        Assert.Equal(new DateTime(2027, 7, 31, 0, 0, 0, DateTimeKind.Utc), granted.ExpiresAt);
        Assert.Null(granted.RevokedAt);
    }

    /// <summary>
    /// A customer without a limit has no limit, no utilisation and no available credit.
    /// </summary>
    [Fact]
    public void ParseCredit_NoLimit_HasNullLimitAndUtilization()
    {
        const string json = """
            {"data":{"customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","as_of":"2027-06-30","limit":null,"exposure":{"open_invoices":"0.00","unbilled_delivery_notes":"0.00","confirmed_orders":"0.00","unreconciled_payments":"0.00","total_exposure":"0.00","overdue_amount":"0.00","overdue_items":0,"max_days_past_due":0},"utilization_percent":null,"available_credit":null,"insured_cover":"0.00","uninsured_exposure":"0.00","is_credit_hold":true,"is_blocked":false,"decision":null,"overrides":[]}}
            """;

        var status = ReceivablesParser.ParseCredit(json);

        Assert.Null(status.Limit);
        Assert.Null(status.UtilizationPercent);
        Assert.Null(status.AvailableCredit);
        Assert.Null(status.Decision);
        Assert.True(status.IsCreditHold);
        Assert.Empty(status.Overrides);
    }

    /// <summary>
    /// The risk score is read with its rating and its factors, values as text.
    /// </summary>
    [Fact]
    public void ParseRisk_ReadsScoreRatingAndFactors()
    {
        const string json = """
            {"data":{"customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","score":63,"rating":"high","factors":[{"key":"credit_utilization","value":"100.00","points":20},{"key":"max_days_past_due","value":60,"points":12.5}],"blocked":true}}
            """;

        var risk = ReceivablesParser.ParseRisk(json);

        Assert.Equal(63, risk.Score);
        Assert.Equal(RiskRating.High, risk.Rating);
        Assert.True(risk.Blocked);
        Assert.Equal(2, risk.Factors.Count);
        Assert.Equal("100.00", risk.Factors[0].Value);
        Assert.Equal("60", risk.Factors[1].Value);
        Assert.Equal(12.5m, risk.Factors[1].Points);
    }

    /// <summary>
    /// A null dso stays undefined; a missing dso is computed from the receivables and the sales.
    /// </summary>
    [Fact]
    public void ParseDso_NullMeansUndefined_AbsentMeansComputed()
    {
        var undefined = ReceivablesParser.ParseDso("""{"data":{"as_of":"2027-06-30","period_days":90,"currency":"EUR","receivables":"500.00","credit_sales":"0.00","dso":null}}""");
        var given = ReceivablesParser.ParseDso("""{"data":{"as_of":"2027-06-30","period_days":90,"currency":"EUR","receivables":"1000.00","credit_sales":"3000.00","dso":"31.50"}}""");
        var computed = ReceivablesParser.ParseDso("""{"data":{"as_of":"2027-06-30","period_days":60,"currency":"EUR","receivables":"1000.00","credit_sales":"3000.00"}}""");

        Assert.Null(undefined.Dso);
        Assert.Equal(31.50m, given.Dso);
        Assert.Equal(20.00m, computed.Dso);
        Assert.Equal(60, computed.PeriodDays);
    }

    /// <summary>
    /// The provision lines and the totals are read with the default-rate flag.
    /// </summary>
    [Fact]
    public void ParseProvision_ReadsLinesAndTotals()
    {
        const string json = """
            {"data":{"as_of":"2027-06-30","currency":"EUR","lines":[{"key":"current","from":0,"to":0,"open_amount":"1000.00","rate_percent":"0.50","provision":"5.00","is_default_rate":true},{"key":"1_30","from":1,"to":30,"open_amount":"200.00","rate_percent":"2.00","provision":"4.00","is_default_rate":false}],"total_open":"1200.00","total_provision":"9.00","net_realizable_value":"1191.00"}}
            """;

        var summary = ReceivablesParser.ParseProvision(json);

        Assert.Equal(2, summary.Lines.Count);
        Assert.True(summary.Lines[0].IsDefaultRate);
        Assert.False(summary.Lines[1].IsDefaultRate);
        Assert.Equal(2.00m, summary.Lines[1].RatePercent);
        Assert.Equal(1200.00m, summary.TotalOpen);
        Assert.Equal(9.00m, summary.TotalProvision);
        Assert.Equal(1191.00m, summary.NetRealizableValue);
    }

    /// <summary>
    /// The dashboard KPIs and the blocked customers are read; a null DSO stays undefined.
    /// </summary>
    [Fact]
    public void ParseDashboard_ReadsKpisAndBlockedCustomers()
    {
        const string json = """
            {"data":{"as_of":"2027-06-30","currency":"EUR","kpis":{"total_exposure":"5000.00","total_open":"4000.00","overdue_amount":"900.00","over_90":"300.00","dso":null,"bad_debt_provision":"70.00","net_realizable_value":"3930.00","blocked_customers":1,"active_overrides":2},"aging":{},"blocked":[{"customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","reason":"over_limit","exposure":"1200.00","limit":"1000.00"}]}}
            """;

        var dashboard = ReceivablesParser.ParseDashboard(json);

        Assert.Equal(5000.00m, dashboard.Kpis.TotalExposure);
        Assert.Null(dashboard.Kpis.Dso);
        Assert.Equal(2, dashboard.Kpis.ActiveOverrides);
        var blocked = Assert.Single(dashboard.Blocked);
        Assert.Equal(1200.00m, blocked.Exposure);
        Assert.Equal("over_limit", blocked.Reason);
    }

    /// <summary>
    /// The statement is read: the names of the parties, the period, the balances, the open items and the aging.
    /// </summary>
    [Fact]
    public void ParseStatement_ReadsPartiesPeriodAndBalances()
    {
        const string json = """
            {"data":{"tenant":{"name":"Fattura Facile SA"},"customer":{"name":"Rossi SA"},"currency":"CHF","period":{"from":"2027-04-01","to":"2027-06-30"},"as_of":"2027-06-30","opening_balance":"100.00","movements":[{"date":"2027-05-01","kind":"invoice","document_type":"invoice","reference":"INV-1","currency":"CHF","debit":"400.00","credit":"0.00","balance":"500.00"}],"closing_balance":"500.00","open_items":[{"id":"11111111-1111-1111-1111-111111111111","customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","document_number":"INV-1","currency":"CHF","original_amount":"400.00","open_amount":"400.00","issue_date":"2027-05-01","due_date":"2027-05-31","status":"overdue","days_past_due":30}],"total_open":"400.00","total_overdue":"400.00","aging":{"current":"0.00","1_30":"400.00"},"credit":null}}
            """;

        var statement = ReceivablesParser.ParseStatement(json);

        Assert.Equal("Rossi SA", statement.CustomerName);
        Assert.Equal("Fattura Facile SA", statement.CompanyName);
        Assert.Equal(new DateOnly(2027, 4, 1), statement.From);
        Assert.Equal(new DateOnly(2027, 6, 30), statement.To);
        Assert.Equal(100.00m, statement.OpeningBalance);
        Assert.Equal(500.00m, statement.ClosingBalance);
        _ = Assert.Single(statement.Movements);
        _ = Assert.Single(statement.OpenItems);
        Assert.Equal(400.00m, statement.TotalOverdue);
        Assert.Equal(400.00m, statement.Aging["1_30"]);
    }

    /// <summary>
    /// The credit limit history keeps the order of the server.
    /// </summary>
    [Fact]
    public void ParseLimits_ReadsTheHistory()
    {
        const string json = """
            {"data":[{"id":"33333333-3333-3333-3333-333333333333","customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","currency":"EUR","granted_limit":"2000.00","valid_from":"2027-07-01","reaction_policy":"warning","is_active":true},{"id":"77777777-7777-7777-7777-777777777777","customer_id":"c0c0c0c0-c0c0-c0c0-c0c0-c0c0c0c0c0c0","currency":"EUR","granted_limit":"1000.00","valid_from":"2027-01-01","valid_to":"2027-06-30","reaction_policy":"credit_hold","is_active":false}]}
            """;

        var limits = ReceivablesParser.ParseLimits(json);

        Assert.Equal([2000.00m, 1000.00m], limits.Select(limit => limit.GrantedLimit));
        Assert.Equal(CreditReactionPolicy.CreditHold, limits[1].ReactionPolicy);
        Assert.False(limits[1].IsActive);
        Assert.Equal(new DateOnly(2027, 6, 30), limits[1].ValidTo);
    }

    /// <summary>
    /// A credit check answer is read with its outcome, breaches and override flag.
    /// </summary>
    [Fact]
    public void ParseDecision_ReadsTheCreditCheck()
    {
        const string json = """
            {"data":{"outcome":"ok","breaches":["over_limit"],"reason":"covered","exposure":"1200.00","limit":"1000.00","overridden":true,"reaction_policy":"credit_hold"}}
            """;

        var decision = ReceivablesParser.ParseDecision(json);

        Assert.Equal(CreditOutcome.Ok, decision.Outcome);
        Assert.True(decision.Overridden);
        Assert.Equal(["over_limit"], decision.Breaches);
        Assert.Equal(CreditReactionPolicy.CreditHold, decision.ReactionPolicy);
    }

    /// <summary>
    /// A blank answer is refused and a broken one raises a JSON error that the service turns into the cache.
    /// </summary>
    [Fact]
    public void Parse_BlankOrBrokenJson_Throws()
    {
        _ = Assert.Throws<ArgumentException>(() => ReceivablesParser.ParseAging(" "));
        _ = Assert.ThrowsAny<System.Text.Json.JsonException>(() => ReceivablesParser.ParseAging("{not json"));
    }

    private static AgingReport ParseAgingReport() => ReceivablesParser.ParseAging(Aging);
}
