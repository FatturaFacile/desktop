using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using FatturaFacile.Extensions;
using FatturaFacile.Identity.Application.Commands;
using FatturaFacile.Identity.Application.Security;
using FatturaFacile.Identity.Domain.Aggregates;
using FatturaFacile.Identity.Domain.Repositories;
using FatturaFacile.Identity.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Common.Configuration;
using FatturaFacile.Infrastructure.Common.Tenancy;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Enums;
using FatturaFacile.Invoicing.Infrastructure.Persistence;
using MediatR;
using Xunit;

namespace FatturaFacile.IntegrationTests;

public class MultiTenancyAndIdentityIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;

    public MultiTenancyAndIdentityIntegrationTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();

        // Remove existing DbContextOptions / DbContext before registering with open connection
        services.AddFatturaFacile(opts =>
        {
            opts.Provider = DatabaseProvider.Sqlite;
            opts.ConnectionString = _connection.ConnectionString;
        });

        // Ensure InvoicingDbContext and IdentityDbContext explicitly use the shared open in-memory connection
        services.Remove(services.First(d => d.ServiceType == typeof(DbContextOptions<InvoicingDbContext>)));
        services.Remove(services.First(d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>)));

        services.AddDbContext<InvoicingDbContext>((sp, dbOpts) =>
        {
            dbOpts.UseSqlite(_connection);
            dbOpts.AddInterceptors(
                sp.GetRequiredService<FatturaFacile.Infrastructure.Common.Interceptors.TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<FatturaFacile.Infrastructure.Common.Interceptors.AuditAndSoftDeleteInterceptor>());
        });

        services.AddDbContext<IdentityDbContext>((sp, dbOpts) =>
        {
            dbOpts.UseSqlite(_connection);
            dbOpts.AddInterceptors(
                sp.GetRequiredService<FatturaFacile.Infrastructure.Common.Interceptors.TenantSaveChangesInterceptor>(),
                sp.GetRequiredService<FatturaFacile.Infrastructure.Common.Interceptors.AuditAndSoftDeleteInterceptor>());
        });

        _serviceProvider = services.BuildServiceProvider();

        // Create tables on the shared connection
        using (var scope = _serviceProvider.CreateScope())
        {
            var invDb = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
            invDb.Database.EnsureCreated();

            var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            // Since InvoicingDbContext already created the database on this connection, EnsureCreated() on second context returns false without creating new tables.
            // Using IRelationalDatabaseCreator.CreateTables() creates the tables for IdentityDbContext.
            var creator = (Microsoft.EntityFrameworkCore.Storage.RelationalDatabaseCreator)identityDb.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IDatabaseCreator>();
            try
            {
                creator.CreateTables();
            }
            catch (SqliteException)
            {
                // Ignore if already created
            }
        }
    }

    [Fact]
    public async Task MultiTenancy_TenantA_CannotReadTenantB_Invoices()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // 1. Seed invoice for Tenant A
        tenantContext.SetTenant(tenantA);
        var invA = Invoice.Create(tenantA, Guid.NewGuid(), InvoiceType.Standard, "INV-A-1", DateOnly.FromDateTime(DateTime.UtcNow), null, "EUR", Jurisdiction.IT);
        invA.AddItem(null, "Item A", 1m, 100m, 22m);
        db.Invoices.Add(invA);
        await db.SaveChangesAsync();

        // 2. Seed invoice for Tenant B
        tenantContext.SetTenant(tenantB);
        var invB = Invoice.Create(tenantB, Guid.NewGuid(), InvoiceType.Standard, "INV-B-1", DateOnly.FromDateTime(DateTime.UtcNow), null, "EUR", Jurisdiction.IT);
        invB.AddItem(null, "Item B", 1m, 200m, 22m);
        db.Invoices.Add(invB);
        await db.SaveChangesAsync();

        // 3. Query as Tenant A: must only see Tenant A's invoice
        tenantContext.SetTenant(tenantA);
        var invoicesA = await db.Invoices.AsNoTracking().ToListAsync();
        invoicesA.Should().ContainSingle();
        invoicesA[0].InvoiceNumber.Should().Be("INV-A-1");

        // 4. Query as Tenant B: must only see Tenant B's invoice
        tenantContext.SetTenant(tenantB);
        var invoicesB = await db.Invoices.AsNoTracking().ToListAsync();
        invoicesB.Should().ContainSingle();
        invoicesB[0].InvoiceNumber.Should().Be("INV-B-1");

        // 5. Bypass filter scope (administrative worker): sees both
        using (tenantContext.IgnoreTenantFilters())
        {
            var allInvoices = await db.Invoices.AsNoTracking().ToListAsync();
            allInvoices.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task TenantSaveChangesInterceptor_BlocksSavingEntity_WithoutValidTenantId()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        // Ensure no ambient tenant context is set
        tenantContext.SetTenant(Guid.Empty);

        var tenantId = Guid.NewGuid();
        var invoice = Invoice.Create(tenantId, Guid.NewGuid(), InvoiceType.Standard, "MISMATCH-TENANT", DateOnly.FromDateTime(DateTime.UtcNow), null, "EUR", Jurisdiction.IT);

        // Force set an invalid/empty TenantId via reflection/property to trigger the interceptor guard
        var prop = typeof(Invoice).GetProperty("TenantId");
        prop?.SetValue(invoice, Guid.Empty);

        var action = async () =>
        {
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();
        };

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no TenantId was supplied and no active TenantContext is set*");
    }

    [Fact]
    public void Argon2id_HashesAndVerifiesPasswordCorrectly()
    {
        var hasher = _serviceProvider.GetRequiredService<IPasswordHasher>();
        const string password = "SwissSecurePassword2026!#";

        var hash = hasher.HashPassword(password);
        hash.Should().StartWith("$argon2id$v=19$m=65536,t=4,p=2$");

        hasher.VerifyPassword(password, hash).Should().BeTrue();
        hasher.VerifyPassword("WrongPassword", hash).Should().BeFalse();
    }

    [Fact]
    public async Task Identity_LockoutThreshold_TriggeredAfter5FailedAttempts()
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Register initial tenant and user
        var registerCmd = new RegisterTenantUserCommand("Acme Corp", "acme-ch", "admin@acme.ch", "StrongPassword123!");
        var registerResult = await mediator.Send(registerCmd);
        registerResult.IsSuccess.Should().BeTrue();

        // Attempt 5 invalid logins
        for (int i = 0; i < 4; i++)
        {
            var failCmd = new LoginUserCommand("acme-ch", "admin@acme.ch", "WrongPassword");
            var failResult = await mediator.Send(failCmd);
            failResult.IsFailure.Should().BeTrue();
            failResult.ErrorCode.Should().Be("Identity.InvalidCredentials");
        }

        // 5th attempt locks account
        var fifthAttempt = new LoginUserCommand("acme-ch", "admin@acme.ch", "WrongPassword");
        var fifthResult = await mediator.Send(fifthAttempt);
        fifthResult.IsFailure.Should().BeTrue();
        fifthResult.ErrorCode.Should().Be("Identity.UserLockedOut");

        // Even with the correct password, login is blocked while locked out
        var lockedAttempt = new LoginUserCommand("acme-ch", "admin@acme.ch", "StrongPassword123!");
        var lockedResult = await mediator.Send(lockedAttempt);
        lockedResult.IsFailure.Should().BeTrue();
        lockedResult.ErrorCode.Should().Be("Identity.UserLockedOut");
    }

    [Fact]
    public async Task Identity_SuccessfulLogin_IssuesJwtAndRotatesRefreshToken()
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // 1. Register tenant & user
        await mediator.Send(new RegisterTenantUserCommand("Swiss Finance AG", "swiss-finance", "officer@swiss.ch", "TopSecret2026!"));

        // 2. Login
        var loginResult = await mediator.Send(new LoginUserCommand("swiss-finance", "officer@swiss.ch", "TopSecret2026!"));
        loginResult.IsSuccess.Should().BeTrue();
        loginResult.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();

        // 3. Refresh token rotation
        var refreshResult = await mediator.Send(new RefreshTokenCommand(loginResult.Value.RefreshToken));
        refreshResult.IsSuccess.Should().BeTrue();
        refreshResult.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshResult.Value.RefreshToken.Should().NotBe(loginResult.Value.RefreshToken);

        // 4. Old refresh token cannot be reused
        var reuseResult = await mediator.Send(new RefreshTokenCommand(loginResult.Value.RefreshToken));
        reuseResult.IsFailure.Should().BeTrue();
        reuseResult.ErrorCode.Should().Be("Identity.InvalidRefreshToken");
    }

    public void Dispose()
    {
        _connection.Dispose();
        _serviceProvider.Dispose();
    }
}
