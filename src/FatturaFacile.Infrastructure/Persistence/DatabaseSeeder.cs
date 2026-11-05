// <copyright file="DatabaseSeeder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Provides baseline initial seed data for offline desktop deployment (Switzerland &amp; Italy).
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the offline database if the tenants table is empty.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the seed operation.</returns>
    public static Task SeedAsync(
        FatturaFacileDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        return SeedInternalAsync(context, cancellationToken);
    }

    private static async Task SeedInternalAsync(
        FatturaFacileDbContext context,
        CancellationToken cancellationToken)
    {
        _ = await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        if (await context.Tenants.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var itTenantId = new Guid(
            0x11111111,
            0x1111,
            0x1111,
            0x11,
            0x11,
            0x11,
            0x11,
            0x11,
            0x11,
            0x11,
            0x11) /* 11111111-1111-1111-1111-111111111111 */;
        var chTenantId = new Guid(
            0x22222222,
            0x2222,
            0x2222,
            0x22,
            0x22,
            0x22,
            0x22,
            0x22,
            0x22,
            0x22,
            0x22) /* 22222222-2222-2222-2222-222222222222 */;

        var itTenant = new Tenant
        {
            Id = itTenantId,
            Name = "FatturaFacile Italia S.r.l.",
            LegalName = "FatturaFacile Italia S.r.l.",
            Country = "IT",
            DefaultCurrency = "EUR",
            VatNumber = "01234567890",
            FiscalCode = "01234567890",
            Iban = "IT60X0542811101000000123456",
            SdiCode = "M5UXCR1",
            PecEmail = "fatturafacile@pec.it",
            CreatedAt = DateTime.UtcNow,
        };

        var chTenant = new Tenant
        {
            Id = chTenantId,
            Name = "FatturaFacile Schweiz AG",
            LegalName = "FatturaFacile Schweiz AG",
            Country = "CH",
            DefaultCurrency = "CHF",
            UidNumber = "CHE-987.654.321",
            Iban = "CH9300762011623852957",
            QrIban = "CH4431999123000889012",
            CreatedAt = DateTime.UtcNow,
        };

        await context.Tenants.AddRangeAsync([itTenant, chTenant], cancellationToken)
           .ConfigureAwait(false);

        // Global Shared VAT codes (TenantId = null)
        var vatCodes = new[]
        {
            new VatCode
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "IT_22",
                Description = "IVA Ordinaria 22%",
                CountryIso = "IT",
                Rate = 22m,
                TaxRatePercent = 22m,
                IsActive = true,
            },
            new VatCode
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "IT_10",
                Description = "IVA Ridotta 10%",
                CountryIso = "IT",
                Rate = 10m,
                TaxRatePercent = 10m,
                IsActive = true,
            },
            new VatCode
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "IT_04",
                Description = "IVA Minima 4%",
                CountryIso = "IT",
                Rate = 4m,
                TaxRatePercent = 4m,
                IsActive = true,
            },
            new VatCode
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "IT_N1",
                Description = "Escluso Art. 15",
                CountryIso = "IT",
                Rate = 0m,
                TaxRatePercent = 0m,
                Nature = "N1",
                IsActive = true,
            },
            new VatCode
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "CH_81",
                Description = "MWST Normalsatz 8.1%",
                CountryIso = "CH",
                Rate = 8.1m,
                TaxRatePercent = 8.1m,
                IsActive = true,
            },
            new VatCode
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "CH_26",
                Description = "MWST Reduzierter Satz 2.6%",
                CountryIso = "CH",
                Rate = 2.6m,
                TaxRatePercent = 2.6m,
                IsActive = true,
            },
            new VatCode
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "CH_38",
                Description = "MWST Beherbergungssatz 3.8%",
                CountryIso = "CH",
                Rate = 3.8m,
                TaxRatePercent = 3.8m,
                IsActive = true,
            },
        };
        await context.VatCodes.AddRangeAsync(vatCodes, cancellationToken).ConfigureAwait(false);

        // Global Shared Chart of Accounts (TenantId = null)
        var accounts = new[]
        {
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "1000",
                Name = "Cassa / Kasse",
                Type = "asset",
                Standard = "shared",
                NormalBalance = "debit",
                Currency = "EUR",
                IsActive = true,
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "1020",
                Name = "Banca c/c / Bankguthaben",
                Type = "asset",
                Standard = "shared",
                NormalBalance = "debit",
                Currency = "EUR",
                IsActive = true,
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "1100",
                Name = "Crediti verso Clienti / Forderungen aus Lieferungen",
                Type = "asset",
                Standard = "shared",
                NormalBalance = "debit",
                Currency = "EUR",
                IsActive = true,
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "2000",
                Name = "Debiti verso Fornitori / Verbindlichkeiten aus Lieferungen",
                Type = "liability",
                Standard = "shared",
                NormalBalance = "credit",
                Currency = "EUR",
                IsActive = true,
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "3000",
                Name = "Ricavi delle Vendite / Produktionserloes",
                Type = "revenue",
                Standard = "shared",
                NormalBalance = "credit",
                Currency = "EUR",
                IsActive = true,
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "4000",
                Name = "Acquisti Merci e Materie / Materialaufwand",
                Type = "expense",
                Standard = "shared",
                NormalBalance = "debit",
                Currency = "EUR",
                IsActive = true,
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "5000",
                Name = "Salari e Stipendi / Personalaufwand",
                Type = "expense",
                Standard = "shared",
                NormalBalance = "debit",
                Currency = "EUR",
                IsActive = true,
            },
        };
        await context.ChartOfAccounts.AddRangeAsync(accounts, cancellationToken).ConfigureAwait(false);

        // Payment Methods
        var payMethods = new[]
        {
            new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "BANK_TRANSFER",
                IsElectronic = true,
                RequiresIban = true,
                IsActive = true,
            },
            new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "SWISS_QR",
                IsElectronic = true,
                RequiresIban = true,
                IsActive = true,
            },
            new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "RIBA",
                IsElectronic = true,
                RequiresIban = true,
                IsActive = true,
            },
            new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Code = "CASH",
                IsElectronic = false,
                RequiresIban = false,
                IsActive = true,
            },
        };
        await context.PaymentMethods.AddRangeAsync(payMethods, cancellationToken).ConfigureAwait(false);

        // Seed Contacts for Italian tenant
        var contactIt1 = new Contact
        {
            Id = Guid.NewGuid(),
            TenantId = itTenantId,
            Type = "client",
            Country = "IT",
            CompanyName = "Acme Consulting S.p.A.",
            VatNumber = "09876543211",
            FiscalCode = "09876543211",
            SdiCode = "SUBM70N",
            Email = "amministrazione@acme.it",
            Phone = "+39 02 1234567",
            PaymentTermsDays = 30,
        };

        var contactCh1 = new Contact
        {
            Id = Guid.NewGuid(),
            TenantId = chTenantId,
            Type = "client",
            Country = "CH",
            CompanyName = "Helvetia Alpine Tech AG",
            UidNumber = "CHE-444.555.666",
            Iban = "CH9300762011623852957",
            QrIban = "CH4431999123000889012",
            Email = "finance@helvetia-alpine.ch",
            Phone = "+41 44 987 65 43",
            PaymentTermsDays = 30,
        };

        await context.Contacts.AddRangeAsync([contactIt1, contactCh1], cancellationToken)
           .ConfigureAwait(false);

        // Seed Products
        var prodIt1 = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = itTenantId,
            Sku = "SRV-CONS-01",
            Name = "Consulenza Direzionale ERP",
            Description = "Giornata di consulenza specialistica ERP e architettura software",
            UnitMeasure = "GG",
            CostPrice = 400m,
            SalePrice = 850m,
            TaxRate = 22m,
            TrackInventory = false,
            IsActive = true,
        };

        var prodCh1 = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = chTenantId,
            Sku = "SRV-CLOUD-CH",
            Name = "Swiss Secure Cloud Hosting",
            Description = "Dedicated Swiss Datacenter hosting container pack",
            UnitMeasure = "MESE",
            CostPrice = 250m,
            SalePrice = 650m,
            TaxRate = 8.1m,
            TrackInventory = false,
            IsActive = true,
        };

        await context.Products.AddRangeAsync([prodIt1, prodCh1], cancellationToken)
           .ConfigureAwait(false);

        // Seed Employees
        var empIt = new HrEmployee
        {
            Id = Guid.NewGuid(),
            TenantId = itTenantId,
            EmployeeNumber = "EMP-IT-001",
            FirstName = "Marco",
            LastName = "Rossi",
            Email = "m.rossi@fatturafacile.it",
            SocialSecurityNumber = "RSSMRC85M01H501Z",
            DepartmentName = "Ingegneria Software",
            JobTitle = "Principal Software Architect",
            EmploymentStatus = "active",
            EmploymentType = "full_time",
            BaseAnnualSalary = 65_000m,
            HourlyCostRate = 45m,
            Currency = "EUR",
            FteRatio = 1.0m,
        };

        var empCh = new HrEmployee
        {
            Id = Guid.NewGuid(),
            TenantId = chTenantId,
            EmployeeNumber = "EMP-CH-001",
            FirstName = "Urs",
            LastName = "Meier",
            Email = "u.meier@fatturafacile.ch",
            SocialSecurityNumber = "756.1234.5678.90",
            DepartmentName = "Enterprise Architecture",
            JobTitle = "Lead Solutions Engineer",
            EmploymentStatus = "active",
            EmploymentType = "full_time",
            BaseAnnualSalary = 135_000m,
            HourlyCostRate = 85m,
            Currency = "CHF",
            FteRatio = 1.0m,
        };

        await context.HrEmployees.AddRangeAsync([empIt, empCh], cancellationToken)
           .ConfigureAwait(false);

        _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
