// <copyright file="MultiTenancyAndIdentityIntegrationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Extensions;
using FatturaFacile.Identity.Application.Commands;
using FatturaFacile.Identity.Domain.Repositories;
using FatturaFacile.Identity.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Common.Configuration;
using FatturaFacile.Infrastructure.Common.Tenancy;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Enums;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using FluentAssertions;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FatturaFacile.IntegrationTests;

/// <summary>
/// Integration tests verifying multi-tenant isolation, automatic tenant filtering, and identity workflows in SQLite.
/// </summary>
public sealed class MultiTenancyAndIdentityIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTenancyAndIdentityIntegrationTests"/> class with an open SQLite in-memory database.
    /// </summary>
    public MultiTenancyAndIdentityIntegrationTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        _ = services.AddLogging();

        // Remove existing DbContextOptions / DbContext before registering with open connection
        _ = services.AddFatturaFacile(opts =>
        {
            opts.Provider = DatabaseProvider.Sqlite;
            opts.ConnectionString = _connection.ConnectionString;
        });

        // Ensure InvoicingDbContext and IdentityDbContext explicitly use the shared open in-memory connection
        _ = services.Remove(services.First(d => d.ServiceType == typeof(DbContextOptions<InvoicingDbContext>)));
        _ = services.Remove(services.First(d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>)));

        _ = services.AddDbContext<InvoicingDbContext>((sp, dbOpts) =>
        {
            _ = dbOpts.UseSqlite(_connection);
            _ = dbOpts.AddInterceptors(
                sp.GetRequiredService<Infrastructure.Common.Interceptors.TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<Infrastructure.Common.Interceptors.AuditAndSoftDeleteInterceptor>());
        });

        _ = services.AddDbContext<IdentityDbContext>((sp, dbOpts) =>
        {
            _ = dbOpts.UseSqlite(_connection);
            _ = dbOpts.AddInterceptors(
                sp.GetRequiredService<Infrastructure.Common.Interceptors.TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<Infrastructure.Common.Interceptors.AuditAndSoftDeleteInterceptor>());
        });

        _serviceProvider = services.BuildServiceProvider();

        // Initialize schema
        using var scope = _serviceProvider.CreateScope();
        var invoicingDb = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var creator = (RelationalDatabaseCreator)invoicingDb.Database.GetService<IRelationalDatabaseCreator>();
        creator.CreateTables();

        var identityCreator = (RelationalDatabaseCreator)identityDb.Database.GetService<IRelationalDatabaseCreator>();
        identityCreator.CreateTables();
    }

    /// <summary>
    /// Verifies that global query filters enforce strict data isolation between two distinct tenants.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultiTenancy_QueryFilter_EnforcesStrictTenantIsolation()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var seedScope = _serviceProvider.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
            var tenantContext = seedScope.ServiceProvider.GetRequiredService<ITenantContext>();

            // Seed invoice for Tenant A
            tenantContext.SetTenant(tenantA);
            var invoiceA = Invoice.Create(new InvoiceCreateRequest(tenantA, Guid.NewGuid(), InvoiceType.Standard, "INV-A-001", DateOnly.FromDateTime(DateTime.UtcNow), DueDate: null, Currency: "EUR", Jurisdiction: Jurisdiction.IT));
            invoiceA.AddItem(productId: null, "Item A", 1, 100, 22);
            _ = await db.Invoices.AddAsync(invoiceA);
            _ = await db.SaveChangesAsync();

            // Seed invoice for Tenant B
            tenantContext.SetTenant(tenantB);
            var invoiceB = Invoice.Create(new InvoiceCreateRequest(tenantB, Guid.NewGuid(), InvoiceType.Standard, "INV-B-001", DateOnly.FromDateTime(DateTime.UtcNow), DueDate: null, Currency: "EUR", Jurisdiction: Jurisdiction.IT));
            invoiceB.AddItem(productId: null, "Item B", 1, 200, 22);
            _ = await db.Invoices.AddAsync(invoiceB);
            _ = await db.SaveChangesAsync();
        }

        // Query as Tenant A: only Tenant A's records must be visible
        await using (var scopeA = _serviceProvider.CreateAsyncScope())
        {
            var db = scopeA.ServiceProvider.GetRequiredService<InvoicingDbContext>();
            var tenantContext = scopeA.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.SetTenant(tenantA);

            var invoices = await db.Invoices.ToListAsync();
            _ = invoices.Should().HaveCount(1);
            _ = invoices[0].InvoiceNumber.Should().Be("INV-A-001");
            _ = invoices[0].TenantId.Should().Be(tenantA);
        }

        // Query as Tenant B: only Tenant B's records must be visible
        await using (var scopeB = _serviceProvider.CreateAsyncScope())
        {
            var db = scopeB.ServiceProvider.GetRequiredService<InvoicingDbContext>();
            var tenantContext = scopeB.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.SetTenant(tenantB);

            var invoices = await db.Invoices.ToListAsync();
            _ = invoices.Should().HaveCount(1);
            _ = invoices[0].InvoiceNumber.Should().Be("INV-B-001");
            _ = invoices[0].TenantId.Should().Be(tenantB);
        }
    }

    /// <summary>
    /// Verifies that IgnoreTenantFilters allows administrative cross-tenant queries.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task MultiTenancy_IgnoreTenantFilters_AllowsCrossTenantAdministrativeQuery()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var seedScope = _serviceProvider.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
            var tenantContext = seedScope.ServiceProvider.GetRequiredService<ITenantContext>();

            tenantContext.SetTenant(tenantA);
            var invoiceA = Invoice.Create(new InvoiceCreateRequest(tenantA, Guid.NewGuid(), InvoiceType.Standard, "INV-A-100", DateOnly.FromDateTime(DateTime.UtcNow), DueDate: null, Currency: "EUR", Jurisdiction: Jurisdiction.IT));
            invoiceA.AddItem(productId: null, "Item", 1, 100, 22);
            _ = await db.Invoices.AddAsync(invoiceA);
            _ = await db.SaveChangesAsync();

            tenantContext.SetTenant(tenantB);
            var invoiceB = Invoice.Create(new InvoiceCreateRequest(tenantB, Guid.NewGuid(), InvoiceType.Standard, "INV-B-100", DateOnly.FromDateTime(DateTime.UtcNow), DueDate: null, Currency: "EUR", Jurisdiction: Jurisdiction.IT));
            invoiceB.AddItem(productId: null, "Item", 1, 100, 22);
            _ = await db.Invoices.AddAsync(invoiceB);
            _ = await db.SaveChangesAsync();
        }

        await using (var adminScope = _serviceProvider.CreateAsyncScope())
        {
            var db = adminScope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
            var tenantContext = adminScope.ServiceProvider.GetRequiredService<ITenantContext>();

            // Bypassing tenant filters
            using (tenantContext.IgnoreTenantFilters())
            {
                var allInvoices = await db.Invoices.ToListAsync();
                _ = allInvoices.Should().HaveCount(2);
            }
        }
    }

    /// <summary>
    /// Verifies end-to-end user registration and authentication workflows with Argon2id and encrypted PII.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Identity_UserRegistrationAndLogin_SucceedsWithArgon2idAndEncryptedPii()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // 1. Register a new tenant with user
        var registerCommand = new RegisterTenantUserCommand(
            "Swiss Alpine Tech AG",
            "swiss-alpine-tech",
            "founder@alpine-tech.ch",
            "SuperSecretPassword2026!",
            "de");

        var regResult = await mediator.Send(registerCommand);
        _ = regResult.IsSuccess.Should().BeTrue();
        _ = regResult.Value.Email.Should().Be("founder@alpine-tech.ch");

        // 2. Authenticate
        var loginCommand = new LoginUserCommand(
            "swiss-alpine-tech",
            "founder@alpine-tech.ch",
            "SuperSecretPassword2026!");

        var loginResult = await mediator.Send(loginCommand);
        _ = loginResult.IsSuccess.Should().BeTrue();
        _ = loginResult.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
        _ = loginResult.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();
        _ = loginResult.Value.PreferredLanguage.Should().Be("de");

        // 3. Verify blind index and AES encrypted email at rest
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = await userRepo.GetByIdAsync(loginResult.Value.UserId);
        _ = user.Should().NotBeNull();
        _ = user.EncryptedEmail.Should().NotContain("founder@alpine-tech.ch", "Email must be encrypted at rest.");
        _ = user.PasswordHash.Should().StartWith("$argon2id$v=19$m=65536,t=4,p=2$", "Password must be hashed with Argon2id.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _serviceProvider.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
