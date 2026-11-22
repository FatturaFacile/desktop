// <copyright file="CustomerLifecycleTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Text;
using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Application.Features.CustomerLifecycle;
using FatturaFacile.Application.Sync;
using FatturaFacile.Infrastructure.CustomerLifecycle;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the customer 360 query and of the HTTP client behind it.
/// </summary>
public sealed class CustomerLifecycleTests
{
    private const string SummaryJson = """
        {"data":{"account":{"id":"a"},"health":{"health_index":70.5,"churn_risk":"medium"},"kpis":{"lifetime_value":"1000.00","open_pipeline_value":"500.00","open_opportunities":1,"overdue_receivable_amount":"0.00","open_tickets":2,"sla_breach_rate":10.0,"mttr_hours":4.5,"active_contracts":1,"contract_coverage":"covered","contract_hours_remaining":"8.00","contracts_expiring_soon":0,"interventions_scheduled":1,"interventions_completed":3}}}
        """;

    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    /// <summary>
    /// A contact without a CRM account has no lifecycle view and triggers no further request.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Handle_ContactWithoutAccount_ReturnsNull()
    {
        Mock<ICustomerLifecycleApiClient> api = new();
        _ = api.Setup(a => a.FindAccountIdAsync(It.IsAny<Guid>(), Any)).ReturnsAsync((Guid?)null);

        var view = await new GetCustomerLifecycleQueryHandler(api.Object).Handle(new GetCustomerLifecycleQuery(Guid.NewGuid(), []), CancellationToken.None);

        Assert.Null(view);
        api.Verify(a => a.GetSummaryAsync(It.IsAny<Guid>(), Any), Times.Never);
    }

    /// <summary>
    /// The view joins the summary and the filtered timeline of the account behind the contact.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Handle_ContactWithAccount_JoinsSummaryAndTimeline()
    {
        var accountId = Guid.NewGuid();
        string[] types = ["ticket"];
        CustomerLifecycleSummary summary = new(new CustomerLifecycleHealth(HealthIndex: 70, ChurnRisk: "low"), new CustomerLifecycleKpis(LifetimeValue: "0", OpenPipelineValue: "0", OpenOpportunities: 0, OverdueReceivableAmount: "0", OpenTickets: 0, SlaBreachRate: 0, MttrHours: null, ActiveContracts: 0, ContractCoverage: "none", ContractHoursRemaining: "0", ContractsExpiringSoon: 0, InterventionsScheduled: 0, InterventionsCompleted: 0));
        List<CustomerTimelineEvent> timeline = [new CustomerTimelineEvent(Type: "ticket", Id: "t1", OccurredAt: DateTimeOffset.UtcNow, Title: "Chiller offline", Status: "open", Amount: null, Reference: "T-1")];
        Mock<ICustomerLifecycleApiClient> api = new();
        _ = api.Setup(a => a.FindAccountIdAsync(It.IsAny<Guid>(), Any)).ReturnsAsync(accountId);
        _ = api.Setup(a => a.GetSummaryAsync(accountId, Any)).ReturnsAsync(summary);
        _ = api.Setup(a => a.GetTimelineAsync(accountId, types, Any)).ReturnsAsync(timeline);

        var view = await new GetCustomerLifecycleQueryHandler(api.Object).Handle(new GetCustomerLifecycleQuery(Guid.NewGuid(), types), CancellationToken.None);

        Assert.NotNull(view);
        Assert.Same(summary, view.Summary);
        _ = Assert.Single(view.Timeline);
    }

    /// <summary>
    /// The client reads the account from a paginated list and parses the summary.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Client_ParsesAccountAndSummary()
    {
        var accountId = Guid.NewGuid();
        Mock<ISyncApiClient> sync = new();
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, It.Is<string>(u => u.StartsWith("crm/accounts?contact_id=", StringComparison.Ordinal)), jsonBody: null, Any))
            .ReturnsAsync(Reply("{\"data\":{\"data\":[{\"id\":\"" + accountId + "\"}]}}"));
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, "crm/customers/" + accountId + "/360", jsonBody: null, Any)).ReturnsAsync(Reply(SummaryJson));
        CustomerLifecycleApiClient client = new(sync.Object);

        var found = await client.FindAccountIdAsync(Guid.NewGuid(), CancellationToken.None);
        var summary = await client.GetSummaryAsync(accountId, CancellationToken.None);

        Assert.Equal(accountId, found);
        Assert.Equal(70.5, summary.Health.HealthIndex);
        Assert.Equal("covered", summary.Kpis.ContractCoverage);
        Assert.Equal(4.5, summary.Kpis.MttrHours);
        Assert.Equal(3, summary.Kpis.InterventionsCompleted);
    }

    /// <summary>
    /// Type filters are sent as repeated array parameters and the events are parsed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Client_Timeline_SendsTypeFilters()
    {
        var accountId = Guid.NewGuid();
        const string json = "{\"data\":[{\"type\":\"ticket\",\"id\":\"t1\",\"occurred_at\":\"2026-03-01T10:00:00+00:00\",\"title\":\"Chiller offline\",\"status\":\"open\",\"amount\":null,\"reference\":\"T-1\"}]}";
        Mock<ISyncApiClient> sync = new();
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, It.Is<string>(u => u.EndsWith("&types%5B%5D=ticket&types%5B%5D=invoice", StringComparison.Ordinal)), jsonBody: null, Any)).ReturnsAsync(Reply(json));

        var events = await new CustomerLifecycleApiClient(sync.Object).GetTimelineAsync(accountId, ["ticket", "invoice"], CancellationToken.None);

        var only = Assert.Single(events);
        Assert.Equal("Chiller offline", only.Title);
        Assert.Equal("T-1", only.Reference);
    }

    /// <summary>
    /// A server failure surfaces as an HTTP exception carrying the status code.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Client_ServerError_Throws()
    {
        Mock<ISyncApiClient> sync = new();
        _ = sync.Setup(s => s.SendAuthorizedAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), jsonBody: null, Any)).ReturnsAsync(new SyncApiReply(HttpStatusCode.NotFound, Encoding.UTF8.GetBytes("{}")));

        var failure = await Assert.ThrowsAsync<HttpRequestException>(() => new CustomerLifecycleApiClient(sync.Object).GetSummaryAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(HttpStatusCode.NotFound, failure.StatusCode);
    }

    private static SyncApiReply Reply(string json) => new(HttpStatusCode.OK, Encoding.UTF8.GetBytes(json));
}
