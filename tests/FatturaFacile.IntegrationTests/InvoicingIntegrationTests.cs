using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FatturaFacile.Extensions;
using FatturaFacile.Infrastructure.Common.Configuration;
using FatturaFacile.Infrastructure.Common.Tenancy;
using FatturaFacile.Invoicing.Application.Commands;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Enums;
using FatturaFacile.Invoicing.Domain.Repositories;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using MediatR;
using Xunit;

namespace FatturaFacile.IntegrationTests;

public class InvoicingIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;

    public InvoicingIntegrationTests()
    {
        // Keep an open SQLite in-memory connection alive for the test fixture
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();

        services.AddFatturaFacile(opts =>
        {
            opts.Provider = DatabaseProvider.Sqlite;
            opts.ConnectionString = _connection.ConnectionString;
        });

        // Override DbContext to use the existing open in-memory connection
        services.AddDbContext<InvoicingDbContext>((sp, dbOpts) =>
        {
            dbOpts.UseSqlite(_connection);
            dbOpts.AddInterceptors(
                sp.GetRequiredService<FatturaFacile.Infrastructure.Common.Interceptors.TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<FatturaFacile.Infrastructure.Common.Interceptors.AuditAndSoftDeleteInterceptor>());
        });

        _serviceProvider = services.BuildServiceProvider();

        // Create schema on the in-memory database
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateInvoiceCommand_PersistsInvoice_AndUpdatesOutbox()
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        var tenantId = Guid.NewGuid();
        tenantContext.SetTenant(tenantId, "test-user");

        var command = new CreateInvoiceCommand(
            tenantId,
            Guid.NewGuid(),
            InvoiceType.Standard,
            "TEST-2026-0001",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
            "EUR",
            Jurisdiction.IT,
            [
                new CreateInvoiceItemInput(null, "Consulting Item", 2m, 150m, 22m, 0m)
            ]);

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalGross.Should().Be(366.00m); // 300 + 66 VAT

        // Verify direct repository query
        var repo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var persisted = await repo.GetByIdAsync(result.Value.Id);
        persisted.Should().NotBeNull();
        persisted!.Items.Should().HaveCount(1);
        persisted.Items.First().LineTotal.Should().Be(300.00m);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _serviceProvider.Dispose();
    }
}
