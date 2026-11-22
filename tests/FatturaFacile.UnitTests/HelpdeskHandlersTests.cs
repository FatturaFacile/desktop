// <copyright file="HelpdeskHandlersTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Features.Helpdesk;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests that the helpdesk CQRS handlers delegate to the service and that the requests carry the permission they need.
/// </summary>
public sealed class HelpdeskHandlersTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid TicketId = Guid.NewGuid();
    private static readonly DateTime Now = new(2027, 1, 10, 9, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IHelpdeskService> service = new();

    /// <summary>
    /// The workbench query asks the service for the risk threshold it carries.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Workbench_DelegatesWithTheThreshold()
    {
        HelpdeskWorkbench expected = new([], FromCache: false, Now, [], []);
        _ = service.Setup(s => s.LoadWorkbenchAsync(60, Any)).ReturnsAsync(expected);

        var result = await new GetHelpdeskWorkbenchQueryHandler(service.Object).Handle(new GetHelpdeskWorkbenchQuery(60), CancellationToken.None);

        Assert.Same(expected, result);
    }

    /// <summary>
    /// The metrics, customer context, escalation and knowledge queries delegate with their arguments.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Queries_DelegateWithTheirArguments()
    {
        var from = new DateOnly(2027, 1, 1);
        var to = new DateOnly(2027, 1, 31);
        HelpdeskLoad<HelpdeskMetrics> metrics = new(new HelpdeskMetrics("queue", []), FromCache: false, Now);
        _ = service.Setup(s => s.LoadMetricsAsync(from, to, "queue", Any)).ReturnsAsync(metrics);
        _ = service.Setup(s => s.LoadCustomerContextAsync(TicketId, Any)).ReturnsAsync((HelpdeskLoad<HelpdeskCustomerContext>?)null);
        _ = service.Setup(s => s.LoadEscalationsAsync(TicketId, Any)).ReturnsAsync((HelpdeskLoad<IReadOnlyList<HelpdeskEscalation>>?)null);
        _ = service.Setup(s => s.SearchKnowledgeAsync("vpn", "fr-CH", Any)).ReturnsAsync((HelpdeskLoad<IReadOnlyList<HelpdeskKbArticle>>?)null);

        var sla = await new GetSlaMetricsQueryHandler(service.Object).Handle(new GetSlaMetricsQuery(from, to, "queue"), CancellationToken.None);
        var context = await new GetHelpdeskCustomerContextQueryHandler(service.Object).Handle(new GetHelpdeskCustomerContextQuery(TicketId), CancellationToken.None);
        var escalations = await new GetHelpdeskEscalationsQueryHandler(service.Object).Handle(new GetHelpdeskEscalationsQuery(TicketId), CancellationToken.None);
        var articles = await new SearchHelpdeskKnowledgeQueryHandler(service.Object).Handle(new SearchHelpdeskKnowledgeQuery("vpn", "fr-CH"), CancellationToken.None);

        Assert.Same(metrics, sla);
        Assert.Null(context);
        Assert.Null(escalations);
        Assert.Null(articles);
    }

    /// <summary>
    /// The time log and the two escalation commands hand their request to the service and return its outcome.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Commands_DelegateTheRequestAndReturnTheOutcome()
    {
        HelpdeskTimeLogRequest time = new(Guid.NewGuid(), TicketId, "TCK-1", 30, null, null, IsBillable: true, Now);
        HelpdeskFieldServiceRequest field = new(Guid.NewGuid(), TicketId, "TCK-1", null, null, null, null, Now);
        HelpdeskRmaRequest rma = new(Guid.NewGuid(), TicketId, "TCK-1", "SN-1", null, Goodwill: false, null, Now);
        HelpdeskActionResult sent = new(HelpdeskActionStatus.Sent);
        HelpdeskActionResult queued = new(HelpdeskActionStatus.Queued);
        HelpdeskActionResult conflict = new(HelpdeskActionStatus.Conflict, Detail: "no");
        _ = service.Setup(s => s.LogTimeAsync(time, Any)).ReturnsAsync(sent);
        _ = service.Setup(s => s.EscalateToFieldServiceAsync(field, Any)).ReturnsAsync(queued);
        _ = service.Setup(s => s.EscalateToRmaAsync(rma, Any)).ReturnsAsync(conflict);

        var logged = await new LogTicketTimeCommandHandler(service.Object).Handle(new LogTicketTimeCommand(time), CancellationToken.None);
        var workOrder = await new EscalateTicketToFieldServiceCommandHandler(service.Object).Handle(new EscalateTicketToFieldServiceCommand(field), CancellationToken.None);
        var returned = await new EscalateTicketToRmaCommandHandler(service.Object).Handle(new EscalateTicketToRmaCommand(rma), CancellationToken.None);

        Assert.Same(sent, logged);
        Assert.Same(queued, workOrder);
        Assert.Same(conflict, returned);
    }

    /// <summary>
    /// The flush and dismiss commands delegate to the service.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task FlushAndDismiss_Delegate()
    {
        var entryId = Guid.NewGuid();
        HelpdeskFlushResult flushed = new(2, 0, 0, Offline: false);
        _ = service.Setup(s => s.FlushAsync(Any)).ReturnsAsync(flushed);
        _ = service.Setup(s => s.DismissConflictAsync(entryId, Any)).Returns(Task.CompletedTask);

        var result = await new FlushHelpdeskOutboxCommandHandler(service.Object).Handle(new FlushHelpdeskOutboxCommand(), CancellationToken.None);
        await new DismissHelpdeskConflictCommandHandler(service.Object).Handle(new DismissHelpdeskConflictCommand(entryId), CancellationToken.None);

        Assert.Same(flushed, result);
        service.Verify(s => s.DismissConflictAsync(entryId, Any), Times.Once);
    }

    /// <summary>
    /// Reading needs the commercial read permission and every write (time log, escalation, flush, dismiss) the master data write one.
    /// </summary>
    [Fact]
    public void Requests_CarryTheirPermission()
    {
        Assert.Equal(Permission.CommercialRead, new GetHelpdeskWorkbenchQuery(75).RequiredPermission);
        Assert.Equal(Permission.CommercialRead, new GetSlaMetricsQuery(default, default, "queue").RequiredPermission);
        Assert.Equal(Permission.CommercialRead, new GetHelpdeskCustomerContextQuery(TicketId).RequiredPermission);
        Assert.Equal(Permission.CommercialRead, new GetHelpdeskEscalationsQuery(TicketId).RequiredPermission);
        Assert.Equal(Permission.CommercialRead, new SearchHelpdeskKnowledgeQuery("x", "en").RequiredPermission);
        Assert.Equal(Permission.MasterDataWrite, new FlushHelpdeskOutboxCommand().RequiredPermission);
        Assert.Equal(Permission.MasterDataWrite, new DismissHelpdeskConflictCommand(TicketId).RequiredPermission);
    }
}
