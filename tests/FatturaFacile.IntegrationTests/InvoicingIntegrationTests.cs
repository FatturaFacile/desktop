// <copyright file="InvoicingIntegrationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Extensions;
using FatturaFacile.Infrastructure.Common.Configuration;
using FatturaFacile.Infrastructure.Common.Tenancy;
using FatturaFacile.Invoicing.Application.Commands;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Enums;
using FatturaFacile.Invoicing.Domain.Repositories;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using FluentAssertions;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FatturaFacile.IntegrationTests;

/// <summary>
/// Integration tests verifying the end-to-end invoice creation, persistence, and querying pipeline using SQLite in-memory.
/// </summary>
public sealed class InvoicingIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoicingIntegrationTests"/> class with an open SQLite in-memory database.
    /// </summary>
    public InvoicingIntegrationTests()
    {
        // Keep an open SQLite in-memory connection alive for the test fixture
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        _ = services.AddLogging();

        _ = services.AddFatturaFacile(opts =>
        {
            opts.Provider = DatabaseProvider.Sqlite;
            opts.ConnectionString = _connection.ConnectionString;
        });

        // Override DbContext to use the existing open in-memory connection
        _ = services.AddDbContext<InvoicingDbContext>((sp, dbOpts) =>
        {
            _ = dbOpts.UseSqlite(_connection);
            _ = dbOpts.AddInterceptors(
                sp.GetRequiredService<Infrastructure.Common.Interceptors.TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<Infrastructure.Common.Interceptors.AuditAndSoftDeleteInterceptor>());
        });

        _serviceProvider = services.BuildServiceProvider();

        // Create schema on the in-memory database
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        _ = db.Database.EnsureCreated();
    }

    /// <summary>
    /// Verifies that CreateInvoiceCommand correctly creates, calculates, and stores an invoice in SQLite.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CreateInvoiceCommand_ShouldPersistInvoice_WithCalculatedTotals()
    {
        // Arrange
        await using var scope = _serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        var tenantId = Guid.NewGuid();
        tenantContext.SetTenant(tenantId);

        var contactId = Guid.NewGuid();
        var command = new CreateInvoiceCommand(
            tenantId,
            contactId,
            InvoiceType.ChQrBill,
            "INV-CH-TEST-001",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "CHF",
            Jurisdiction.CH,
            [
                new CreateInvoiceItemInput(ProductId: null, "Consulting Item A", 2m, 150.00m, 8.1m),
                new CreateInvoiceItemInput(ProductId: null, "Consulting Item B", 1m, 200.00m, 8.1m),
            ],
            Notes: "Payment terms 30 days net.");

        // Act
        var result = await mediator.Send(command);

        // Assert
        _ = result.IsSuccess.Should().BeTrue();
        _ = result.Value.Should().NotBeNull();
        _ = result.Value.InvoiceNumber.Should().Be("INV-CH-TEST-001");
        _ = result.Value.Currency.Should().Be("CHF");
        _ = result.Value.Items.Should().HaveCount(2);

        // Verify in database
        var repository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var storedInvoice = await repository.GetByIdAsync(result.Value.Id);
        _ = storedInvoice.Should().NotBeNull();
        _ = storedInvoice.TotalNet.Should().Be(500.00m);
        _ = storedInvoice.TotalGross.Should().Be(540.50m);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _serviceProvider.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
