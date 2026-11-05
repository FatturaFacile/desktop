// <copyright file="MultiTenantSecurityTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests verifying multi-tenant isolation, global query filters, and read-only protection of shared records.
/// </summary>
public sealed class MultiTenantSecurityTests
{
    /// <summary>
    /// Verifies that global query filters prevent cross-tenant data leakage.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GlobalQueryFilter_IsolatesTenantData()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantService = new MockTenantService { TenantId = tenantA };
        var interceptor = new TenantSaveChangesInterceptor(tenantService);

        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        await using (var seedContext = new FatturaFacileDbContext(options, tenantService))
        {
            // Seed global VAT code
            seedContext.VatCodes.Add(new VatCode
            {
                Code = "VAT-GLOBAL",
                Description = "Shared Standard VAT",
                Rate = 22m,
                TenantId = null, // Global
            });

            // Seed Invoice for Tenant A
            tenantService.SetTenant(tenantA);
            seedContext.Invoices.Add(new Invoice
            {
                TenantId = tenantA,
                InvoiceNumber = "INV-TENANT-A",
                Date = DateTime.UtcNow,
                Currency = "EUR",
            });

            // Seed Invoice for Tenant B
            tenantService.SetTenant(tenantB);
            seedContext.Invoices.Add(new Invoice
            {
                TenantId = tenantB,
                InvoiceNumber = "INV-TENANT-B",
                Date = DateTime.UtcNow,
                Currency = "CHF",
            });

            await seedContext.SaveChangesAsync();
        }

        // Query as Tenant A
        tenantService.SetTenant(tenantA);
        await using (var queryContextA = new FatturaFacileDbContext(options, tenantService))
        {
            var invoices = await queryContextA.Invoices.ToListAsync();
            Assert.Single(invoices);
            Assert.Equal("INV-TENANT-A", invoices[0].InvoiceNumber);

            // Global VAT code must be visible
            var vatCodes = await queryContextA.VatCodes.ToListAsync();
            Assert.Contains(vatCodes, v => string.Equals(v.Code, "VAT-GLOBAL", StringComparison.Ordinal));
        }

        // Query as Tenant B
        tenantService.SetTenant(tenantB);
        await using (var queryContextB = new FatturaFacileDbContext(options, tenantService))
        {
            var invoices = await queryContextB.Invoices.ToListAsync();
            Assert.Single(invoices);
            Assert.Equal("INV-TENANT-B", invoices[0].InvoiceNumber);
        }
    }

    /// <summary>
    /// Verifies that modifying or deleting a shared global record (TenantId == null) throws InvalidOperationException.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task TenantInterceptor_BlocksModificationOfGlobalRecords()
    {
        var tenantId = Guid.NewGuid();
        var tenantService = new MockTenantService { TenantId = tenantId };
        var interceptor = new TenantSaveChangesInterceptor(tenantService);

        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        var globalVat = new VatCode
        {
            Code = "VAT-22",
            Description = "IVA Ordinaria",
            Rate = 22m,
            TenantId = null,
        };

        await using (var context = new FatturaFacileDbContext(options, tenantService))
        {
            context.VatCodes.Add(globalVat);
            await context.SaveChangesAsync();
        }

        // Attempt to modify the global VAT code
        await using (var context = new FatturaFacileDbContext(options, tenantService))
        {
            var vat = await context.VatCodes.FirstAsync(v => string.Equals(v.Code, "VAT-22", StringComparison.Ordinal));
            vat.Rate = 25m; // Modification

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        }

        // Attempt to delete the global VAT code
        await using (var context = new FatturaFacileDbContext(options, tenantService))
        {
            var vat = await context.VatCodes.FirstAsync(v => string.Equals(v.Code, "VAT-22", StringComparison.Ordinal));
            context.VatCodes.Remove(vat); // Deletion

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        }
    }

    private sealed class MockTenantService : ICurrentTenantService
    {
        public event EventHandler? CurrentTenantChanged;

        public Guid TenantId { get; set; }

        public string CountryCode { get; set; } = "IT";

        public string TenantName { get; set; } = "Mock Tenant";

        public string DefaultCurrency { get; set; } = "EUR";

        public void SetTenant(Guid tenantId)
        {
            TenantId = tenantId;
            CurrentTenantChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetCurrentTenant(Guid tenantId, string tenantName, string countryCode, string defaultCurrency)
        {
            TenantId = tenantId;
            TenantName = tenantName;
            CountryCode = countryCode;
            DefaultCurrency = defaultCurrency;
            CurrentTenantChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
