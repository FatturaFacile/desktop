// <copyright file="SalesDocumentsTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Invoicing.Commands;
using FatturaFacile.Application.Features.Sales.Commands;
using FatturaFacile.Application.Features.Sales.Queries;
using FatturaFacile.Application.Sales;
using FatturaFacile.Application.Strategies;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Repositories;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the quote and sales order workflow, totals, numbering and conversions.
/// </summary>
public sealed class SalesDocumentsTests : IDisposable
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly ServiceProvider provider;
    private readonly IMapper mapper;
    private readonly UnitOfWork unitOfWork;
    private readonly CountryStrategyFactory strategies = new(new Mock<IQrCodeEncoder>().Object);
    private readonly Guid contactId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="SalesDocumentsTests"/> class.
    /// </summary>
    public SalesDocumentsTests()
    {
        tenant.SetCurrentTenant(Guid.NewGuid(), "Muster AG", "CH", "CHF");
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        _ = db.Contacts.Add(new Contact { Id = contactId, TenantId = tenant.TenantId, CompanyName = "Kunde AG", Country = "CH" });
        _ = db.SaveChanges();

        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddApplicationServices();
        provider = services.BuildServiceProvider();
        mapper = provider.GetRequiredService<IMapper>();
        unitOfWork = new UnitOfWork(db);
    }

    /// <summary>
    /// Allowed and forbidden quote transitions follow the web-api table.
    /// </summary>
    /// <param name="from">The current status.</param>
    /// <param name="to">The target status.</param>
    /// <param name="allowed">Whether the transition is allowed.</param>
    [Theory]
    [InlineData("draft", "pending_approval", true)]
    [InlineData("draft", "accepted", true)]
    [InlineData("pending_approval", "accepted", false)]
    [InlineData("pending_approval", "sent", false)]
    [InlineData("approved", "sent", true)]
    [InlineData("sent", "draft", false)]
    [InlineData("accepted", "rejected", false)]
    [InlineData("rejected", "approved", false)]
    public void QuoteWorkflow_FollowsTransitionTable(string from, string to, bool allowed)
        => Assert.Equal(allowed, SalesDocumentWorkflow.CanTransitionQuote(from, to));

    /// <summary>
    /// Order transitions allow progress and forbid leaving final statuses.
    /// </summary>
    /// <param name="from">The current status.</param>
    /// <param name="to">The target status.</param>
    /// <param name="allowed">Whether the transition is allowed.</param>
    [Theory]
    [InlineData("draft", "confirmed", true)]
    [InlineData("confirmed", "approved", true)]
    [InlineData("on_hold", "shipped", false)]
    [InlineData("shipped", "completed", true)]
    [InlineData("shipped", "cancelled", false)]
    [InlineData("cancelled", "draft", false)]
    public void OrderWorkflow_FollowsTransitionTable(string from, string to, bool allowed)
        => Assert.Equal(allowed, SalesDocumentWorkflow.CanTransitionOrder(from, to));

    /// <summary>
    /// A forbidden transition raises an explicit error.
    /// </summary>
    [Fact]
    public void AssertQuoteTransition_Forbidden_Throws()
        => _ = Assert.Throws<InvalidOperationException>(() => SalesDocumentWorkflow.AssertQuoteTransition("accepted", "draft"));

    /// <summary>
    /// Totals compute VAT per rate on the group total and round with the country rule.
    /// </summary>
    [Fact]
    public void Calculator_SwissTotals_RoundToFiveRappen()
    {
        List<SalesLineDto> lines =
        [
            new() { Description = "A", Quantity = 3m, UnitPrice = 10.10m, TaxRate = 8.1m },
            new() { Description = "B", Quantity = 1m, UnitPrice = 100m, TaxRate = 8.1m, DiscountPercentage = 10m },
        ];

        var totals = SalesDocumentCalculator.Calculate(lines, strategies.GetTaxAndRoundingStrategy("CH"));

        Assert.Equal(130.30m, totals.Subtotal);
        Assert.Equal(10m, totals.DiscountAmount);
        Assert.Equal(120.30m, totals.TotalNet);
        Assert.Equal([30.30m, 90m], totals.LineTotals);
        Assert.Equal(9.74m, totals.TotalTax);
        Assert.Equal(130.05m, totals.TotalGross);
    }

    /// <summary>
    /// The numbering continues the yearly series and starts a new one each year.
    /// </summary>
    [Fact]
    public void Numbering_ContinuesSeriesPerYear()
    {
        Assert.Equal("QT-2026-0001", SalesDocumentNumbering.Next([], "QT", 2026));
        Assert.Equal("QT-2026-0008", SalesDocumentNumbering.Next(["QT-2026-0007", "QT-2026-0003", "QT-2025-0099", "garbage"], "QT", 2026));
        Assert.Equal("QT-2027-0001", SalesDocumentNumbering.Next(["QT-2026-0007"], "QT", 2027));
    }

    /// <summary>
    /// A created quote is a numbered draft with computed totals and line totals.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CreateQuote_PersistsNumberedDraftWithTotals()
    {
        var first = await CreateQuoteAsync();
        var second = await CreateQuoteAsync();

        Assert.Equal("draft", first.Status);
        Assert.Equal("QT-" + DateTime.Today.Year.ToString(System.Globalization.CultureInfo.InvariantCulture) + "-0001", first.QuoteNumber);
        Assert.EndsWith("-0002", second.QuoteNumber, StringComparison.Ordinal);
        Assert.Equal("Kunde AG", first.ContactName);
        Assert.Equal(200m, first.TotalNet);
        Assert.Equal(216.20m, first.TotalGross);
        Assert.Equal(200m, Assert.Single(first.Items).LineTotal);
        Assert.Equal(2, (await new GetQuotesQueryHandler(unitOfWork.Repository<Quote>(), mapper).Handle(new GetQuotesQuery(), CancellationToken.None)).Count);
    }

    /// <summary>
    /// An unknown customer is rejected.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CreateQuote_UnknownContact_Throws()
    {
        var handler = new CreateQuoteCommandHandler(unitOfWork, tenant, strategies, mapper);

        _ = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(
            new CreateQuoteCommand { ContactId = Guid.NewGuid(), Items = [new SalesLineDto { Description = "x" }] },
            CancellationToken.None));
    }

    /// <summary>
    /// The validator requires lines and a sensible validity date.
    /// </summary>
    [Fact]
    public void CreateQuoteValidator_RejectsEmptyAndBackdatedQuotes()
    {
        CreateQuoteCommandValidator validator = new();

        Assert.False(validator.Validate(new CreateQuoteCommand { ContactId = contactId }).IsValid);
        Assert.False(validator.Validate(new CreateQuoteCommand
        {
            ContactId = contactId,
            Date = new DateTime(2026, 5, 1),
            ValidUntil = new DateTime(2026, 4, 1),
            Items = [new SalesLineDto { Description = "x", Quantity = 1m }],
        }).IsValid);
        Assert.True(validator.Validate(new CreateQuoteCommand { ContactId = contactId, Items = [new SalesLineDto { Description = "x", Quantity = 1m }] }).IsValid);
    }

    /// <summary>
    /// A quote cannot be accepted while it waits for approval, and approval needs the issue permission.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ChangeQuoteStatus_FollowsWorkflowAndApprovalPermission()
    {
        var quote = await CreateQuoteAsync();
        var handler = NewStatusHandler();

        _ = await handler.Handle(new ChangeQuoteStatusCommand(quote.Id, QuoteStatuses.PendingApproval), CancellationToken.None);
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new ChangeQuoteStatusCommand(quote.Id, QuoteStatuses.Accepted), CancellationToken.None));
        var approved = await handler.Handle(new ChangeQuoteStatusCommand(quote.Id, QuoteStatuses.Approved), CancellationToken.None);

        Assert.Equal("approved", approved.Status);
        _ = Assert.NotNull((await db.Quotes.SingleAsync(q => q.Id == quote.Id)).ApprovedAt);
        Assert.Equal(FatturaFacile.Domain.Enums.Permission.InvoicesIssue, new ChangeQuoteStatusCommand(quote.Id, QuoteStatuses.Approved).RequiredPermission);
        Assert.Equal(FatturaFacile.Domain.Enums.Permission.InvoicesWrite, new ChangeQuoteStatusCommand(quote.Id, QuoteStatuses.Sent).RequiredPermission);
    }

    /// <summary>
    /// An expired quote cannot be accepted any more.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ChangeQuoteStatus_AcceptAfterValidity_Throws()
    {
        var quote = await CreateQuoteAsync(validUntil: DateTime.Today.AddDays(-1), date: DateTime.Today.AddDays(-10));

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => NewStatusHandler().Handle(
            new ChangeQuoteStatusCommand(quote.Id, QuoteStatuses.Accepted),
            CancellationToken.None));
    }

    /// <summary>
    /// Converting an accepted quote creates a numbered draft order with the same lines, once only.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConvertQuoteToOrder_CopiesLinesAndLinksOnce()
    {
        var quote = await CreateQuoteAsync();
        var convert = new ConvertQuoteToOrderCommandHandler(unitOfWork, mapper, TimeProvider.System);

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => convert.Handle(new ConvertQuoteToOrderCommand(quote.Id), CancellationToken.None));
        _ = await NewStatusHandler().Handle(new ChangeQuoteStatusCommand(quote.Id, QuoteStatuses.Accepted), CancellationToken.None);

        var order = await convert.Handle(new ConvertQuoteToOrderCommand(quote.Id), CancellationToken.None);

        Assert.Equal("draft", order.Status);
        Assert.StartsWith("SO-", order.OrderNumber, StringComparison.Ordinal);
        Assert.Equal(quote.Id, order.QuoteId);
        Assert.Equal(quote.TotalGross, order.TotalGross);
        _ = Assert.Single(order.Items);
        Assert.Equal(order.Id, (await db.Quotes.SingleAsync(q => q.Id == quote.Id)).ConvertedToOrderId);
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => convert.Handle(new ConvertQuoteToOrderCommand(quote.Id), CancellationToken.None));
        _ = Assert.Single(await new GetSalesOrdersQueryHandler(unitOfWork.Repository<SalesOrder>(), mapper).Handle(new GetSalesOrdersQuery(), CancellationToken.None));
    }

    /// <summary>
    /// An order can only be invoiced once it is confirmed, and only once.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CreateInvoiceFromOrder_RequiresConfirmationAndRunsOnce()
    {
        var order = await CreateOrderAsync();
        Mock<ISender> sender = new();
        var invoiceId = Guid.NewGuid();
        CreateInvoiceCommand? captured = null;
        _ = sender.Setup(s => s.Send(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<InvoiceDto>, CancellationToken>((c, _) => captured = (CreateInvoiceCommand)c)
            .ReturnsAsync(new InvoiceDto { Id = invoiceId });
        CreateInvoiceFromOrderCommandHandler handler = new(unitOfWork, sender.Object, TimeProvider.System);

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new CreateInvoiceFromOrderCommand(order.Id), CancellationToken.None));
        _ = await new ChangeSalesOrderStatusCommandHandler(unitOfWork, mapper, TimeProvider.System)
            .Handle(new ChangeSalesOrderStatusCommand(order.Id, SalesOrderStatuses.Confirmed), CancellationToken.None);

        var invoice = await handler.Handle(new CreateInvoiceFromOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(invoiceId, invoice.Id);
        Assert.NotNull(captured);
        Assert.Equal(contactId, captured.ContactId);
        Assert.StartsWith("INV-", captured.InvoiceNumber, StringComparison.Ordinal);
        Assert.Equal(100m, Assert.Single(captured.Items).UnitPrice);
        Assert.Equal(invoiceId, (await db.SalesOrders.SingleAsync(o => o.Id == order.Id)).InvoiceId);
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new CreateInvoiceFromOrderCommand(order.Id), CancellationToken.None));
    }

    /// <summary>
    /// Releases the database and the service provider.
    /// </summary>
    public void Dispose()
    {
        unitOfWork.Dispose();
        db.Dispose();
        provider.Dispose();
        connection.Dispose();
    }

    private ChangeQuoteStatusCommandHandler NewStatusHandler() => new(unitOfWork, mapper, TimeProvider.System);

    private Task<QuoteDto> CreateQuoteAsync(DateTime? validUntil = null, DateTime? date = null)
    {
        var handler = new CreateQuoteCommandHandler(unitOfWork, tenant, strategies, mapper);
        return handler.Handle(
            new CreateQuoteCommand
            {
                ContactId = contactId,
                Date = date ?? DateTime.Today,
                ValidUntil = validUntil,
                Items = [new SalesLineDto { Description = "Beratung", Quantity = 2m, UnitPrice = 100m, TaxRate = 8.1m }],
            },
            CancellationToken.None);
    }

    private async Task<SalesOrderDto> CreateOrderAsync()
    {
        var quote = await CreateQuoteAsync();
        _ = await NewStatusHandler().Handle(new ChangeQuoteStatusCommand(quote.Id, QuoteStatuses.Accepted), CancellationToken.None);
        return await new ConvertQuoteToOrderCommandHandler(unitOfWork, mapper, TimeProvider.System)
            .Handle(new ConvertQuoteToOrderCommand(quote.Id), CancellationToken.None);
    }
}
