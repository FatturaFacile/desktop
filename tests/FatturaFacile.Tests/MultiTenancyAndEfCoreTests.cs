// <copyright file="MultiTenancyAndEfCoreTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.Tests;

/// <summary>
/// Unit and integration tests verifying multi-tenant EF Core global query filters and automated tenant ID injection.
/// </summary>
public class MultiTenancyAndEfCoreTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CurrentTenantService _tenantService;
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTenancyAndEfCoreTests"/> class with an in-memory SQLite database.
    /// </summary>
    public MultiTenancyAndEfCoreTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _tenantService = new CurrentTenantService();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options, _tenantService);
        _ = _context.Database.EnsureCreated();
    }

    /// <summary>
    /// Verifies that global query filters automatically isolate entities belonging to the active tenant.
    /// </summary>
    [Fact]
    public async Task GlobalQueryFilter_FiltersEntitiesByCurrentTenant()
    {
        var tenant1 = new Tenant { Id = Guid.NewGuid(), Name = "Tenant 1" };
        var tenant2 = new Tenant { Id = Guid.NewGuid(), Name = "Tenant 2" };

        _context.Tenants.AddRange(tenant1, tenant2);
        _ = await _context.SaveChangesAsync();

        // Seed contacts
        var contact1 = new Contact { Id = Guid.NewGuid(), TenantId = tenant1.Id, CompanyName = "Tenant 1 Client" };
        var contact2 = new Contact { Id = Guid.NewGuid(), TenantId = tenant2.Id, CompanyName = "Tenant 2 Client" };

        _context.Contacts.AddRange(contact1, contact2);
        _ = await _context.SaveChangesAsync();

        // Query with tenant 1 context
        _tenantService.TenantId = tenant1.Id;
        var t1Contacts = await _context.Contacts.ToListAsync();

        Assert.Single(t1Contacts);
        Assert.Equal("Tenant 1 Client", t1Contacts[0].CompanyName);

        // Switch to tenant 2 context
        _tenantService.TenantId = tenant2.Id;
        var t2Contacts = await _context.Contacts.ToListAsync();

        Assert.Single(t2Contacts);
        Assert.Equal("Tenant 2 Client", t2Contacts[0].CompanyName);
    }

    /// <summary>
    /// Verifies that SaveChangesAsync automatically injects the active tenant ID into newly added entities.
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_AutomaticallySetsCurrentTenantId()
    {
        var activeTenant = new Tenant { Id = Guid.NewGuid(), Name = "Active Tenant" };
        _ = _context.Tenants.Add(activeTenant);
        _ = await _context.SaveChangesAsync();

        _tenantService.TenantId = activeTenant.Id;

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            CompanyName = "Auto Injected Tenant Company",
            // TenantId left empty (Guid.Empty)
        };

        _ = _context.Contacts.Add(contact);
        _ = await _context.SaveChangesAsync();

        Assert.Equal(activeTenant.Id, contact.TenantId);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
