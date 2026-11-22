// <copyright file="HelpdeskParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Globalization;
using FatturaFacile.Application.Helpdesk;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the helpdesk parsers, the knowledge base locale mapping and the local time billing estimate.
/// </summary>
public sealed class HelpdeskParserTests
{
    private const string BreachRisk = """        [{"ticket_id":"11111111-1111-1111-1111-111111111111","ticket_number":"TCK-1","subject":"Printer down","priority":"high","status":"open","customer_id":"22222222-2222-2222-2222-222222222222","assigned_team_id":null,"assigned_technician_id":null,"risk_percent":"92.5","targets":[{"target":"response","due_at":"2027-01-10T10:00:00Z","elapsed_percent":92.5,"state":"at_risk"},{"target":"resolution","due_at":"2027-01-10T08:00:00Z","elapsed_percent":110,"state":"breached"}]},{"ticket_id":"33333333-3333-3333-3333-333333333333","ticket_number":"TCK-2","subject":"Slow VPN","priority":"low","status":"pending","risk_percent":80,"targets":[{"target":"next_update","due_at":"2027-01-11T10:00:00+01:00","elapsed_percent":80,"state":"at_risk"}]},{"ticket_number":"TCK-3","subject":"No identifier"}]""";

    private const string Metrics = """        {"period":{"from":"2027-01-01","to":"2027-01-31"},"group_by":"queue","totals":{"tickets":5},"groups":[{"key":"L1","tickets":4,"mttfr_minutes":12.5,"mttr_minutes":"185","first_response_compliance_percent":97.5,"resolution_compliance_percent":70,"csat":{"responses":3,"average":4.33,"distribution":{"5":2,"4":1}}},{"key":"L2","tickets":1,"mttfr_minutes":null,"mttr_minutes":null,"first_response_compliance_percent":null,"resolution_compliance_percent":null,"csat":{"responses":0,"average":null,"distribution":{}}}]}""";

    private const string Context = """        {"customer_id":"22222222-2222-2222-2222-222222222222","contracts":[{"id":"44444444-4444-4444-4444-444444444444","name":"Gold","kind":"support","included_hours":"10.00","consumed_hours":"7.50","remaining_hours":"2.50","end_date":"2027-12-31"},{"id":"55555555-5555-5555-5555-555555555555","name":"Silver","kind":"support","included_hours":"5","consumed_hours":"5","remaining_hours":"0","end_date":null},{"name":"Time and material","kind":"t_and_m"}],"assets":[{"id":"66666666-6666-6666-6666-666666666666","serial_number":"SN-100","warranty_expiry_date":"2026-06-30"},{"serial_number":"SN-200"}],"open_invoices":{"count":3,"overdue_count":1,"total_gross":"1250.50"},"recent_tickets":[{"id":"77777777-7777-7777-7777-777777777777","ticket_number":"TCK-0","subject":"Old one","status":"closed"}]}""";

    /// <summary>
    /// The breach-risk list keeps the tickets with an identifier, reads numbers sent as strings and flags the breached targets.
    /// </summary>
    [Fact]
    public void ParseBreachRisk_ReadsTicketsTargetsAndBreaches()
    {
        var tickets = HelpdeskParser.ParseBreachRisk(BreachRisk);

        Assert.Equal(2, tickets.Count);
        var first = tickets[0];
        Assert.Equal("TCK-1", first.TicketNumber);
        Assert.Equal(92.5d, first.RiskPercent);
        Assert.True(first.IsBreached);
        Assert.Equal(new DateTime(2027, 1, 10, 8, 0, 0, DateTimeKind.Utc), first.EarliestDueAt);
        DssTestData.SameStrings(["response", "resolution"], first.Targets.Select(target => target.Target));
        Assert.Null(first.AssignedTeamId);

        var second = tickets[1];
        Assert.False(second.IsBreached);
        Assert.Equal(new DateTime(2027, 1, 11, 9, 0, 0, DateTimeKind.Utc), second.EarliestDueAt?.ToUniversalTime());
    }

    /// <summary>
    /// A page wrapper around the list is accepted too.
    /// </summary>
    [Fact]
    public void ParseBreachRisk_AcceptsAPageWrapper()
    {
        var tickets = HelpdeskParser.ParseBreachRisk("{\"data\":" + BreachRisk + "}");

        Assert.Equal(2, tickets.Count);
    }

    /// <summary>
    /// The metrics keep the groups, the compliance figures and the satisfaction, with null for what the server could not compute.
    /// </summary>
    [Fact]
    public void ParseMetrics_ReadsGroupsComplianceAndCsat()
    {
        var metrics = HelpdeskParser.ParseMetrics(Metrics);

        Assert.Equal("queue", metrics.GroupBy);
        Assert.Equal(5, metrics.TotalTickets);
        var level1 = metrics.Groups[0];
        Assert.Equal("L1", level1.Key);
        Assert.Equal(12.5d, level1.MttfrMinutes);
        Assert.Equal(185d, level1.MttrMinutes);
        Assert.Equal(97.5d, level1.FirstResponseCompliancePercent);
        Assert.Equal(3, level1.Csat.Responses);
        Assert.Equal(4.33d, level1.Csat.Average);
        var level2 = metrics.Groups[1];
        Assert.Null(level2.MttfrMinutes);
        Assert.Null(level2.ResolutionCompliancePercent);
        Assert.Null(level2.Csat.Average);
    }

    /// <summary>
    /// The customer context carries the hour banks, the assets with their warranty, the open invoices and the recent tickets.
    /// </summary>
    [Fact]
    public void ParseCustomerContext_ReadsContractsAssetsInvoicesAndTickets()
    {
        var context = HelpdeskParser.ParseCustomerContext(Context);

        Assert.Equal(3, context.Contracts.Count);
        Assert.Equal(2.5m, context.Contracts[0].RemainingHours);
        Assert.Equal(new DateOnly(2027, 12, 31), context.Contracts[0].EndDate);
        Assert.Null(context.Contracts[2].RemainingHours);
        Assert.Equal(2.5m, context.RemainingContractHours);
        DssTestData.SameStrings(["SN-100", "SN-200"], context.Assets.Select(asset => asset.SerialNumber));
        Assert.Equal(new DateOnly(2026, 6, 30), context.Assets[0].WarrantyExpiryDate);
        Assert.Null(context.Assets[1].WarrantyExpiryDate);
        Assert.Equal(new HelpdeskOpenInvoices(3, 1, 1250.50m), context.OpenInvoices);
        Assert.Equal("TCK-0", Assert.Single(context.RecentTickets).Number);
    }

    /// <summary>
    /// Without any hour bank the remaining hours are unknown, not zero, and a missing invoices block reads as none.
    /// </summary>
    [Fact]
    public void ParseCustomerContext_WithoutBankOrInvoices_HasNoRemainingHours()
    {
        var context = HelpdeskParser.ParseCustomerContext("""{"contracts":[{"name":"T&M"}]}""");

        Assert.Null(context.RemainingContractHours);
        Assert.Equal(new HelpdeskOpenInvoices(0, 0, 0m), context.OpenInvoices);
        Assert.Empty(context.Assets);
    }

    /// <summary>
    /// The escalation history is listed most recent first.
    /// </summary>
    [Fact]
    public void ParseEscalations_AreMostRecentFirst()
    {
        const string Json = """            [{"id":"88888888-8888-8888-8888-888888888888","target":"response","threshold_percent":75,"elapsed_percent":"76.2","action":"notify","outcome":"sent","fired_at":"2027-01-10T09:00:00Z"},{"id":"99999999-9999-9999-9999-999999999999","target":"resolution","threshold_percent":100,"elapsed_percent":101,"action":"reassign","outcome":"done","fired_at":"2027-01-10T11:00:00Z"}]""";

        var steps = HelpdeskParser.ParseEscalations(Json);

        DssTestData.SameStrings(["reassign", "notify"], steps.Select(step => step.Action));
        Assert.Equal(76.2d, steps[1].ElapsedPercent);
    }

    /// <summary>
    /// The knowledge base search accepts a plain list and the page of the server, drops untitled rows and shortens long excerpts.
    /// </summary>
    [Fact]
    public void ParseKnowledge_AcceptsBothShapesAndShortensTheSummary()
    {
        var longSummary = new string('x', 500);
        var page = "{\"data\":[{\"id\":\"a1\",\"title\":\"Reset the router\",\"summary\":\"" + longSummary + "\"},{\"id\":\"a2\",\"summary\":\"no title\"},{\"id\":\"a3\",\"title\":\"VPN\",\"content\":\"Body text\"}],\"meta\":{}}";

        var fromPage = HelpdeskParser.ParseKnowledge(page);
        var fromList = HelpdeskParser.ParseKnowledge("[{\"id\":\"a1\",\"title\":\"Reset the router\"}]");

        DssTestData.SameStrings(["Reset the router", "VPN"], fromPage.Select(article => article.Title));
        Assert.EndsWith("...", fromPage[0].Summary, StringComparison.Ordinal);
        Assert.True(fromPage[0].Summary.Length < longSummary.Length);
        Assert.Equal("Body text", fromPage[1].Summary);
        _ = Assert.Single(fromList);
    }

    /// <summary>
    /// The time entry answer says how the time was settled; an answer without it, or that is not JSON, gives nothing.
    /// </summary>
    [Fact]
    public void ParseTimeEntry_ReadsTheSettlement()
    {
        const string Body = """{"data":{"id":"x","hours":"0.5","minutes":30,"billing_status":"partially_deducted","contract_hours":"0.25","billable_hours":"0.25","service_contract_id":null,"total_charge":"20.00"}}""";

        var outcome = HelpdeskParser.ParseTimeEntry(Body);

        Assert.NotNull(outcome);
        Assert.Equal(HelpdeskTimeOutcome.PartiallyDeducted, outcome.BillingStatus);
        Assert.Equal(0.25m, outcome.ContractHours);
        Assert.Equal(0.25m, outcome.BillableHours);
        Assert.Equal(20.00m, outcome.TotalCharge);
        Assert.False(outcome.IsEstimate);
        Assert.Null(HelpdeskParser.ParseTimeEntry("""{"data":{"id":"x"}}"""));
        Assert.Null(HelpdeskParser.ParseTimeEntry("not json"));
        Assert.Null(HelpdeskParser.ParseTimeEntry(string.Empty));
    }

    /// <summary>
    /// The tracking token of an RMA is read from the meta block.
    /// </summary>
    [Fact]
    public void ParseTrackingToken_ReadsTheMeta()
    {
        Assert.Equal("tok-123", HelpdeskParser.ParseTrackingToken("""{"data":{"id":"x"},"meta":{"tracking_token":"tok-123"}}"""));
        Assert.Null(HelpdeskParser.ParseTrackingToken("""{"data":{"id":"x"}}"""));
        Assert.Null(HelpdeskParser.ParseTrackingToken("oops"));
    }

    /// <summary>
    /// German and French map to the Swiss variants, English to en, Italian keeps the region and anything else falls back to it-IT.
    /// </summary>
    /// <param name="culture">The culture name.</param>
    /// <param name="expected">The knowledge base locale.</param>
    [Theory]
    [InlineData("it-CH", "it-CH")]
    [InlineData("it-IT", "it-IT")]
    [InlineData("it", "it-IT")]
    [InlineData("de-DE", "de-CH")]
    [InlineData("de-CH", "de-CH")]
    [InlineData("fr-FR", "fr-CH")]
    [InlineData("fr-CH", "fr-CH")]
    [InlineData("en-US", "en")]
    [InlineData("en-GB", "en")]
    [InlineData("ja-JP", "it-IT")]
    public void KnowledgeLocale_MapsTheCulture(string culture, string expected)
    {
        var locale = HelpdeskKnowledgeLocale.From(CultureInfo.GetCultureInfo(culture));

        Assert.Equal(expected, locale);
        Assert.Contains(locale, HelpdeskKnowledgeLocale.Supported, StringComparer.Ordinal);
    }

    /// <summary>
    /// The estimate deducts from the hour bank first, bills the rest, and bills everything without a bank.
    /// </summary>
    /// <param name="minutes">The minutes worked.</param>
    /// <param name="billable">Whether the entry is billable.</param>
    /// <param name="bank">The hours left on the bank.</param>
    /// <param name="status">The expected billing status.</param>
    /// <param name="contract">The expected deducted hours.</param>
    /// <param name="charged">The expected billable hours.</param>
    [Theory]
    [InlineData(30, true, 1.0, "contract_deducted", 0.5, 0.0)]
    [InlineData(60, true, 1.0, "contract_deducted", 1.0, 0.0)]
    [InlineData(90, true, 0.5, "partially_deducted", 0.5, 1.0)]
    [InlineData(30, true, null, "billable_pending", 0.0, 0.5)]
    [InlineData(30, true, 0.0, "billable_pending", 0.0, 0.5)]
    [InlineData(30, false, 5.0, "non_billable", 0.0, 0.0)]
    public void TimeBilling_Estimate_FollowsTheContractBank(int minutes, bool billable, double? bank, string status, double contract, double charged)
    {
        var outcome = HelpdeskTimeBilling.Estimate(minutes, billable, bank is { } hours ? (decimal)hours : null);

        Assert.Equal(status, outcome.BillingStatus);
        Assert.Equal((decimal)contract, outcome.ContractHours);
        Assert.Equal((decimal)charged, outcome.BillableHours);
        Assert.True(outcome.IsEstimate);
    }
}
