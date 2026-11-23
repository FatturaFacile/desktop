// <copyright file="ReceivablesHandlersTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the receivables handlers: they forward to the service, the export saves the file, and every request is authorized and handled.
/// </summary>
public sealed class ReceivablesHandlersTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<IReceivablesService> service = new();

    /// <summary>
    /// The items query hands its filter to the service and returns the answer.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetReceivableItems_ForwardsTheFilter()
    {
        var filter = ReceivableItemFilter.Default with { Bucket = "31_60", CustomerId = ReceivablesFixtures.CustomerId };
        var page = ReceivablesFixtures.Page();
        _ = service.Setup(s => s.LoadItemsAsync(filter, Any)).ReturnsAsync(page);

        var result = await new GetReceivableItemsQueryHandler(service.Object).Handle(new GetReceivableItemsQuery(filter), CancellationToken.None);

        Assert.Same(page, result);
    }

    /// <summary>
    /// The ledger query forwards the customer, the period and the currency.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetCustomerLedger_ForwardsCustomerPeriodAndCurrency()
    {
        var ledger = ReceivablesFixtures.Load(new CustomerLedger([], 0m));
        var from = new DateOnly(2027, 1, 1);
        var to = new DateOnly(2027, 3, 31);
        _ = service.Setup(s => s.LoadLedgerAsync(ReceivablesFixtures.CustomerId, from, to, "CHF", Any)).ReturnsAsync(ledger);

        var result = await new GetCustomerLedgerQueryHandler(service.Object).Handle(new GetCustomerLedgerQuery(ReceivablesFixtures.CustomerId, from, to, "CHF"), CancellationToken.None);

        Assert.Same(ledger, result);
    }

    /// <summary>
    /// The credit check query forwards the customer, the scope and the amount.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CheckCustomerCredit_ForwardsScopeAndAmount()
    {
        var load = ReceivablesFixtures.Load(new CreditDecision(CreditOutcome.Ok, [], null, 0m, 0m, Overridden: false, null));
        _ = service.Setup(s => s.CheckCreditAsync(ReceivablesFixtures.CustomerId, CreditScope.DeliveryNote, 99.5m, Any)).ReturnsAsync(load);

        var result = await new CheckCustomerCreditQueryHandler(service.Object).Handle(new CheckCustomerCreditQuery(ReceivablesFixtures.CustomerId, CreditScope.DeliveryNote, 99.5m), CancellationToken.None);

        Assert.Same(load, result);
    }

    /// <summary>
    /// The allocation command hands the request to the service.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApplyReceivableAllocations_ForwardsTheRequest()
    {
        var request = new ApplyAllocationsRequest(Guid.NewGuid(), ReceivablesFixtures.CustomerId, "INV-1", [], DateTime.UtcNow);
        _ = service.Setup(s => s.ApplyAllocationsAsync(request, Any)).ReturnsAsync(ReceivablesFixtures.Queued());

        var result = await new ApplyReceivableAllocationsCommandHandler(service.Object).Handle(new ApplyReceivableAllocationsCommand(request), CancellationToken.None);

        Assert.Equal(ReceivablesActionStatus.Queued, result.Status);
        service.Verify(s => s.ApplyAllocationsAsync(request, Any), Times.Once);
    }

    /// <summary>
    /// The dispute, credit limit and override commands hand their requests to the service.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SetDisputeAndCreditLimitCommands_ForwardTheirRequests()
    {
        var now = DateTime.UtcNow;
        var dispute = new DisputeRequest(Guid.NewGuid(), ReceivablesFixtures.CustomerId, "INV-1", Guid.NewGuid(), true, "Wrong VAT", now);
        var delete = new DeleteCreditLimitRequest(Guid.NewGuid(), ReceivablesFixtures.CustomerId, "Rossi", Guid.NewGuid(), now);
        var grant = new GrantOverrideRequest(Guid.NewGuid(), ReceivablesFixtures.CustomerId, "Rossi", CreditScope.Order, "Agreed with the CFO", null, now);
        _ = service.Setup(s => s.SetDisputeAsync(dispute, Any)).ReturnsAsync(ReceivablesFixtures.Sent());
        _ = service.Setup(s => s.DeleteCreditLimitAsync(delete, Any)).ReturnsAsync(ReceivablesFixtures.Sent());
        _ = service.Setup(s => s.GrantOverrideAsync(grant, Any)).ReturnsAsync(ReceivablesFixtures.Queued());

        Assert.Equal(ReceivablesActionStatus.Sent, (await new SetReceivableDisputeCommandHandler(service.Object).Handle(new SetReceivableDisputeCommand(dispute), CancellationToken.None)).Status);
        Assert.Equal(ReceivablesActionStatus.Sent, (await new DeleteCreditLimitCommandHandler(service.Object).Handle(new DeleteCreditLimitCommand(delete), CancellationToken.None)).Status);
        Assert.Equal(ReceivablesActionStatus.Queued, (await new GrantCreditOverrideCommandHandler(service.Object).Handle(new GrantCreditOverrideCommand(grant), CancellationToken.None)).Status);
    }

    /// <summary>
    /// The flush command sends the queue and the dismiss command acknowledges one conflict.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task FlushAndDismiss_ForwardToTheService()
    {
        var entryId = Guid.NewGuid();
        _ = service.Setup(s => s.FlushAsync(Any)).ReturnsAsync(new ReceivablesFlushResult(2, 0, 0, Offline: false));
        _ = service.Setup(s => s.DismissConflictAsync(entryId, Any)).Returns(Task.CompletedTask);

        var flushed = await new FlushReceivablesOutboxCommandHandler(service.Object).Handle(new FlushReceivablesOutboxCommand(), CancellationToken.None);
        await new DismissReceivablesConflictCommandHandler(service.Object).Handle(new DismissReceivablesConflictCommand(entryId), CancellationToken.None);

        Assert.Equal(2, flushed.Sent);
        service.Verify(s => s.DismissConflictAsync(entryId, Any), Times.Once);
    }

    /// <summary>
    /// The export command saves the downloaded statement and returns its path.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ExportCustomerStatement_SavesTheDownloadedFile()
    {
        var saver = new Mock<IReceivablesFileSaver>();
        var file = new ReceivablesFile("statement.pdf", new byte[] { 1 });
        var from = new DateOnly(2027, 4, 1);
        var to = new DateOnly(2027, 6, 30);
        _ = service.Setup(s => s.ExportStatementAsync(ReceivablesFixtures.CustomerId, "pdf", from, to, null, Any)).ReturnsAsync(file);
        _ = saver.Setup(s => s.SaveAsync(file, Any)).ReturnsAsync("C:\\Statements\\statement.pdf");

        var result = await new ExportCustomerStatementCommandHandler(service.Object, saver.Object).Handle(new ExportCustomerStatementCommand(ReceivablesFixtures.CustomerId, "pdf", from, to, null), CancellationToken.None);

        Assert.False(result.Offline);
        Assert.Equal("C:\\Statements\\statement.pdf", result.Path);
    }

    /// <summary>
    /// When the server cannot be reached the export reports it and saves nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ExportCustomerStatement_Offline_SavesNothing()
    {
        var saver = new Mock<IReceivablesFileSaver>();
        _ = service.Setup(s => s.ExportStatementAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<string?>(), Any)).ReturnsAsync((ReceivablesFile?)null);

        var result = await new ExportCustomerStatementCommandHandler(service.Object, saver.Object).Handle(new ExportCustomerStatementCommand(ReceivablesFixtures.CustomerId, "xlsx", new DateOnly(2027, 4, 1), new DateOnly(2027, 6, 30), null), CancellationToken.None);

        Assert.True(result.Offline);
        Assert.Null(result.Path);
        saver.Verify(s => s.SaveAsync(It.IsAny<ReceivablesFile>(), Any), Times.Never);
    }

    /// <summary>
    /// Reads need the commercial read permission, allocations the invoice write one and credit limits, overrides and provision rates the accounting write one.
    /// </summary>
    [Fact]
    public void Requests_RequireTheExpectedPermissions()
    {
        Assert.Equal(Permission.CommercialRead, new GetReceivablesAgingQuery(null, null).RequiredPermission);
        Assert.Equal(Permission.CommercialRead, new GetCustomerCreditStatusQuery(ReceivablesFixtures.CustomerId).RequiredPermission);
        Assert.Equal(Permission.CommercialRead, new ExportCustomerStatementCommand(ReceivablesFixtures.CustomerId, "pdf", default, default, null).RequiredPermission);
        Assert.Equal(Permission.InvoicesWrite, new FlushReceivablesOutboxCommand().RequiredPermission);
        Assert.Equal(Permission.InvoicesWrite, new DismissReceivablesConflictCommand(Guid.NewGuid()).RequiredPermission);
        Assert.Equal(Permission.AccountingWrite, new DeleteCreditLimitCommand(new DeleteCreditLimitRequest(Guid.NewGuid(), Guid.NewGuid(), string.Empty, Guid.NewGuid(), DateTime.UtcNow)).RequiredPermission);
        Assert.Equal(Permission.AccountingWrite, new UpdateProvisionRatesCommand(new ProvisionRatesRequest(Guid.NewGuid(), new Dictionary<string, decimal>(), DateTime.UtcNow)).RequiredPermission);
    }

    /// <summary>
    /// Every request of the receivables feature requires a permission and has exactly one handler.
    /// </summary>
    [Fact]
    public void EveryReceivablesRequest_IsAuthorizedAndHasAHandler()
    {
        var assembly = typeof(GetReceivablesAgingQuery).Assembly;
        var requests = assembly.GetTypes()
            .Where(type => string.Equals(type.Namespace, "FatturaFacile.Application.Features.Receivables", StringComparison.Ordinal) && typeof(IBaseRequest).IsAssignableFrom(type))
            .ToList();

        Assert.NotEmpty(requests);
        Assert.All(requests, request => Assert.True(typeof(IAuthorizedRequest).IsAssignableFrom(request), request.Name));
        Assert.All(requests, request =>
        {
            var handlers = assembly.GetTypes().Where(type => type.GetInterfaces().Any(contract => contract.IsGenericType && contract.GetGenericArguments().Contains(request) && contract.Name.StartsWith("IRequestHandler", StringComparison.Ordinal))).ToList();
            _ = Assert.Single(handlers);
        });
    }
}
