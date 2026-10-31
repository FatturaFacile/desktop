// <copyright file="DatabaseSeeder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Initializes database storage with seed data, creating initial tenants (Switzerland and Italy),
/// mapping the active Windows user, and populating sample products, contacts, and invoices.
/// </summary>
public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">The logger instance.</param>
    public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _ = await _context.Database.EnsureCreatedAsync(cancellationToken);

        var currentWindowsUser = Environment.UserName;
        var currentDomain = Environment.UserDomainName;

        _logger.LogInformation("Checking database initialization for Windows user '{User}\\{Domain}'...", currentDomain, currentWindowsUser);

        // Check if tenants exist
        var existingTenants = await _context.Tenants.IgnoreQueryFilters().ToListAsync(cancellationToken);
        if (existingTenants.Count == 0)
        {
            _logger.LogInformation("Database empty. Seeding initial tenants and Windows user mapping...");

            // 1. Swiss Tenant (Default)
            var swissTenant = new Tenant
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Fattura Facile Schweiz AG",
                LegalName = "Fattura Facile Schweiz AG",
                Country = "CH",
                DefaultCurrency = "CHF",
                UidNumber = "CHE-123.456.789",
                Iban = "CH9300762011623852957",
                QrIban = "CH4431999123000889012",
                CreatedAt = DateTime.UtcNow,
            };

            // 2. Italian Tenant
            var italianTenant = new Tenant
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Fattura Facile Italia S.r.l.",
                LegalName = "Fattura Facile Italia S.r.l.",
                Country = "IT",
                DefaultCurrency = "EUR",
                VatNumber = "IT12345678901",
                FiscalCode = "12345678901",
                SdiCode = "M5UXCR1",
                PecEmail = "fatturafacile@pec.it",
                Iban = "IT60X0542811101000000123456",
                CreatedAt = DateTime.UtcNow,
            };

            _context.Tenants.AddRange(swissTenant, italianTenant);

            // 3. User linked to Windows Account
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = !string.IsNullOrWhiteSpace(currentWindowsUser) ? currentWindowsUser : "Administrator",
                WindowsUsername = currentWindowsUser,
                Email = $"{currentWindowsUser.ToLowerInvariant()}@local",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            _ = _context.Users.Add(user);

            // 4. Map User to both Tenants as Admin
            _context.TenantUsers.AddRange(
                new TenantUser
                {
                    TenantId = swissTenant.Id,
                    UserId = user.Id,
                    Role = "Admin",
                    Permissions = "all",
                },
                new TenantUser
                {
                    TenantId = italianTenant.Id,
                    UserId = user.Id,
                    Role = "Admin",
                    Permissions = "all",
                });

            // 5. VAT Codes for Switzerland
            var swissVatCodes = new List<VatCode>
            {
                new() { TenantId = swissTenant.Id, Code = "CH_MWST_81", Description = "Swiss Standard MWST 8.1% (Normalsatz)", Rate = 8.1m, CountryIso = "CH" },
                new() { TenantId = swissTenant.Id, Code = "CH_MWST_26", Description = "Swiss Reduced MWST 2.6% (Reduzierter Satz)", Rate = 2.6m, CountryIso = "CH" },
                new() { TenantId = swissTenant.Id, Code = "CH_EXEMPT", Description = "Swiss VAT Exempt (Steuerbefreit)", Rate = 0.0m, CountryIso = "CH" },
            };

            // VAT Codes for Italy
            var italianVatCodes = new List<VatCode>
            {
                new() { TenantId = italianTenant.Id, Code = "IVA22", Description = "IVA Ordinaria 22%", Rate = 22.0m, CountryIso = "IT" },
                new() { TenantId = italianTenant.Id, Code = "IVA10", Description = "IVA Ridotta 10%", Rate = 10.0m, CountryIso = "IT" },
                new() { TenantId = italianTenant.Id, Code = "IVA04", Description = "IVA Minima 4%", Rate = 4.0m, CountryIso = "IT" },
                new() { TenantId = italianTenant.Id, Code = "REVERSE_CHARGE", Description = "Reverse Charge Art. 17 c. 6", Rate = 0.0m, CountryIso = "IT", IsReverseCharge = true },
                new() { TenantId = italianTenant.Id, Code = "SPLIT_PAYMENT", Description = "Split Payment PA 22%", Rate = 22.0m, CountryIso = "IT", IsSplitPayment = true },
            };

            _context.VatCodes.AddRange(swissVatCodes);
            _context.VatCodes.AddRange(italianVatCodes);

            // 6. Products & Services for Swiss Tenant
            var chProd1 = new Product { TenantId = swissTenant.Id, Sku = "SRV-IT-CONS", Name = "IT Enterprise Consulting", Description = "High-level architecture and system design", SalePrice = 180.00m, UnitMeasure = "H", TaxRate = 8.1m };
            var chProd2 = new Product { TenantId = swissTenant.Id, Sku = "LIC-ENTERPRISE", Name = "Fattura Facile Desktop License", Description = "Perpetual standalone license per workstation", SalePrice = 1250.00m, UnitMeasure = "PZ", TaxRate = 8.1m };
            var chProd3 = new Product { TenantId = swissTenant.Id, Sku = "SRV-MAINT-ANN", Name = "Annual Maintenance & Updates", Description = "Software updates and technical support", SalePrice = 450.00m, UnitMeasure = "ANNO", TaxRate = 8.1m };
            _context.Products.AddRange(chProd1, chProd2, chProd3);

            // Products & Services for Italian Tenant
            var itProd1 = new Product { TenantId = italianTenant.Id, Sku = "CONS-SW-IT", Name = "Consulenza Informatica Avanzata", Description = "Supporto specialistico su architettura .NET", SalePrice = 95.00m, UnitMeasure = "H", TaxRate = 22.0m };
            var itProd2 = new Product { TenantId = italianTenant.Id, Sku = "MOD-SDI-XML", Name = "Modulo Fattura Elettronica SDI", Description = "Generazione e firma XML FatturaPA", SalePrice = 390.00m, UnitMeasure = "PZ", TaxRate = 22.0m };
            _context.Products.AddRange(itProd1, itProd2);

            // 7. Contacts for Swiss Tenant
            var chContact1 = new Contact
            {
                TenantId = swissTenant.Id,
                CompanyName = "Novartis Pharma Services AG",
                Country = "CH",
                UidNumber = "CHE-105.867.234",
                Email = "billing@novartis.ch",
                Phone = "+41 61 324 11 11",
                City = "Basel",
                AddressLine = "Lichtstrasse 35",
                PostalCode = "4056",
            };
            var chContact2 = new Contact
            {
                TenantId = swissTenant.Id,
                CompanyName = "Zürcher Kantonalbank",
                Country = "CH",
                UidNumber = "CHE-108.954.120",
                Email = "invoicing@zkb.ch",
                Phone = "+41 44 292 11 11",
                City = "Zürich",
                AddressLine = "Bahnhofstrasse 9",
                PostalCode = "8001",
            };
            _context.Contacts.AddRange(chContact1, chContact2);

            // Contacts for Italian Tenant
            var itContact1 = new Contact
            {
                TenantId = italianTenant.Id,
                CompanyName = "Rossi Meccanica S.p.A.",
                Country = "IT",
                VatNumber = "IT98765432109",
                FiscalCode = "98765432109",
                SdiCode = "SUBM70N",
                Pec = "rossimeccanica@pec.it",
                Email = "amministrazione@rossimeccanica.it",
                City = "Milano",
                AddressLine = "Via Montenapoleone 8",
                PostalCode = "20121",
            };
            _ = _context.Contacts.Add(itContact1);

            // 8. Document Sequences
            _context.DocumentSequences.AddRange(
                new DocumentSequence { TenantId = swissTenant.Id, DocumentType = "invoice", FiscalYear = DateTime.UtcNow.Year, Prefix = "INV-", LastNumber = 2, Padding = 4 },
                new DocumentSequence { TenantId = italianTenant.Id, DocumentType = "invoice", FiscalYear = DateTime.UtcNow.Year, Prefix = "INV-", LastNumber = 1, Padding = 4 });

            // 9. Initial Invoices showcasing CHF Swiss 0.05 rounding
            var calculator = new DocumentTotalsCalculator();

            // Swiss Invoice 1
            var chItems1 = new List<DocumentLineInput>
            {
                new(chProd1.Id, "IT Enterprise Consulting - Architecture Sprint", 16.5m, 180.00m, 8.1m, 5.0m),
                new(chProd2.Id, "Fattura Facile Desktop License", 2.0m, 1250.00m, 8.1m, 0.0m),
            };
            var calcResult1 = calculator.Calculate(chItems1, "CHF");

            var swissInvoice1 = new Invoice
            {
                TenantId = swissTenant.Id,
                Contact = chContact1,
                InvoiceType = InvoiceType.Invoice,
                InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-0001",
                FiscalYear = DateTime.UtcNow.Year,
                SequentialNumber = 1,
                Date = DateTime.Today.AddDays(-15),
                DueDate = DateTime.Today.AddDays(15),
                Currency = "CHF",
                ExchangeRate = 1.0m,
                TotalNet = calcResult1.TotalNet,
                TotalTax = calcResult1.TotalTax,
                TotalGross = calcResult1.TotalGross,
                RoundingAmount = calcResult1.RoundingAmount,
                PaymentStatus = PaymentStatus.Paid,
                TotalPaid = calcResult1.TotalGross,
                BankName = swissTenant.Name,
                BankIban = swissTenant.Iban,
                QrIban = swissTenant.QrIban,
                Notes = "Payment received with thanks.",
            };
            foreach (var item in calcResult1.Items)
            {
                swissInvoice1.Items.Add(new InvoiceItem
                {
                    TenantId = swissTenant.Id,
                    ProductId = item.ProductId,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TaxRate = item.TaxRate,
                    DiscountPercentage = item.DiscountPercentage,
                    LineTotal = item.LineTotal,
                });
            }

            // Swiss Invoice 2
            var chItems2 = new List<DocumentLineInput>
            {
                new(chProd3.Id, "Annual Maintenance & SLA 24/7", 1.0m, 450.00m, 8.1m, 0.0m),
                new(chProd1.Id, "On-site Deployment & Security Audit", 8.0m, 180.00m, 8.1m, 10.0m),
            };
            var calcResult2 = calculator.Calculate(chItems2, "CHF");

            var swissInvoice2 = new Invoice
            {
                TenantId = swissTenant.Id,
                Contact = chContact2,
                InvoiceType = InvoiceType.Invoice,
                InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-0002",
                FiscalYear = DateTime.UtcNow.Year,
                SequentialNumber = 2,
                Date = DateTime.Today.AddDays(-2),
                DueDate = DateTime.Today.AddDays(28),
                Currency = "CHF",
                ExchangeRate = 1.0m,
                TotalNet = calcResult2.TotalNet,
                TotalTax = calcResult2.TotalTax,
                TotalGross = calcResult2.TotalGross,
                RoundingAmount = calcResult2.RoundingAmount,
                PaymentStatus = PaymentStatus.Unpaid,
                BankName = swissTenant.Name,
                BankIban = swissTenant.Iban,
                QrIban = swissTenant.QrIban,
                Notes = "Swiss QR-Bill payment details attached.",
            };
            foreach (var item in calcResult2.Items)
            {
                swissInvoice2.Items.Add(new InvoiceItem
                {
                    TenantId = swissTenant.Id,
                    ProductId = item.ProductId,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TaxRate = item.TaxRate,
                    DiscountPercentage = item.DiscountPercentage,
                    LineTotal = item.LineTotal,
                });
            }

            _context.Invoices.AddRange(swissInvoice1, swissInvoice2);

            // Italian Invoice 1
            var itItems1 = new List<DocumentLineInput>
            {
                new(itProd1.Id, "Consulenza Informatica Avanzata - Migrazione Cloud a On-Prem", 40.0m, 95.00m, 22.0m, 0.0m),
                new(itProd2.Id, "Modulo Fattura Elettronica SDI Integrato", 1.0m, 390.00m, 22.0m, 0.0m),
            };
            var itCalcResult = calculator.Calculate(itItems1, "EUR");

            var italianInvoice1 = new Invoice
            {
                TenantId = italianTenant.Id,
                Contact = itContact1,
                InvoiceType = InvoiceType.Invoice,
                InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-0001",
                FiscalYear = DateTime.UtcNow.Year,
                SequentialNumber = 1,
                Date = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(25),
                Currency = "EUR",
                ExchangeRate = 1.0m,
                TotalNet = itCalcResult.TotalNet,
                TotalTax = itCalcResult.TotalTax,
                TotalGross = itCalcResult.TotalGross,
                RoundingAmount = 0m,
                PaymentStatus = PaymentStatus.Unpaid,
                BankName = italianTenant.Name,
                BankIban = italianTenant.Iban,
                Notes = "Fattura soggetta a trasmissione SDI codice M5UXCR1.",
            };
            foreach (var item in itCalcResult.Items)
            {
                italianInvoice1.Items.Add(new InvoiceItem
                {
                    TenantId = italianTenant.Id,
                    ProductId = item.ProductId,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TaxRate = item.TaxRate,
                    DiscountPercentage = item.DiscountPercentage,
                    LineTotal = item.LineTotal,
                });
            }

            _ = _context.Invoices.Add(italianInvoice1);

            _ = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Database seed completed successfully with {Count} tenants.", 2);
        }
        else
        {
            // Ensure current Windows user is mapped to existing tenants
            var user = await _context.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.WindowsUsername.ToLower() == currentWindowsUser.ToLower(), cancellationToken);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = currentWindowsUser,
                    WindowsUsername = currentWindowsUser,
                    Email = $"{currentWindowsUser.ToLowerInvariant()}@local",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                };
                _ = _context.Users.Add(user);

                foreach (var tenant in existingTenants)
                {
                    _ = _context.TenantUsers.Add(new TenantUser
                    {
                        TenantId = tenant.Id,
                        UserId = user.Id,
                        Role = "Admin",
                        Permissions = "all",
                    });
                }

                _ = await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Mapped current Windows user '{User}' to {Count} existing tenants.", currentWindowsUser, existingTenants.Count);
            }
        }
    }
}
