// <copyright file="DtoAndRepositoryExtendedTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;
using FatturaFacile.Infrastructure.Localization;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Contains automated unit tests verifying Application DTOs and Infrastructure repository query/command methods.
/// </summary>
public sealed class DtoAndRepositoryExtendedTests
{
    /// <summary>
    /// Tests AssemblyReference accessibility.
    /// </summary>
    [Fact]
    public void Application_AssemblyReference_ReturnsExecutingAssembly()
    {
        var asm = typeof(AssemblyReference).Assembly;
        asm.Should().NotBeNull();
        asm.GetName().Name.Should().Be("FatturaFacile.Application");
    }

    private static FatturaFacileDbContext CreateInMemoryDbContext(
        Guid tenantId,
        string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var tenantMock = new Mock<ITenantProvider>();
        _ = tenantMock.Setup(t => t.CurrentTenantId).Returns(tenantId);

        var userMock = new Mock<ICurrentUserService>();
        _ = userMock.Setup(u => u.Username).Returns("TestUser");

        return new FatturaFacileDbContext(
            options,
            tenantMock.Object,
            userMock.Object);
    }

    /// <summary>
    /// Tests all DTO records for accurate instantiation and property mapping.
    /// </summary>
    [Fact]
    public void Application_Dtos_PropertiesAreAssignedCorrectly()
    {
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var absence = new AbsenceDto(
            id,
            tenantId,
            Guid.NewGuid(),
            AbsenceType.InpsIllnessPuc,
            now,
            now.AddDays(2),
            2m,
            true,
            "MED-123");
        absence.CertificateNumber.Should().Be("MED-123");
        absence.TotalWorkingDays.Should().Be(2m);

        var trans = new AppTranslationDto(
            id,
            tenantId,
            "it",
            "Key1",
            "DefaultVal",
            "CustomVal",
            "CustomVal",
            now);
        trans.EffectiveValue.Should().Be("CustomVal");

        var client = new ClientDto(
            id,
            tenantId,
            "BizName",
            "TradeName",
            Jurisdiction.Italy,
            "CF123",
            "VAT123",
            "SDI123",
            "pec@test.it",
            "CHE-123",
            "CH123",
            "TI",
            "Via Roma 1",
            "test@test.it",
            "+3902123",
            30,
            5000m,
            "EUR",
            true);
        client.BusinessName.Should().Be("BizName");
        client.CreditLimitAmount.Should().Be(5000m);

        var vehicle = new CustomerVehicleDto(
            id,
            tenantId,
            Guid.NewGuid(),
            "AA123BB",
            "VIN123",
            "Fiat",
            "500",
            2022,
            Jurisdiction.Italy,
            10000);
        vehicle.LicensePlate.Should().Be("AA123BB");

        var kpi = new DashboardKpiDto(
            1000m,
            2000m,
            300m,
            400m,
            220m,
            80m,
            now.AddDays(10),
            5,
            2,
            8);
        kpi.TotalRevenueEur.Should().Be(1000m);
        kpi.ActiveClientsCount.Should().Be(5);

        var line = new InvoiceLineDto(
            1,
            "Articolo A",
            2m,
            50m,
            22m,
            null,
            100m);
        line.LineTotal.Should().Be(100m);

        var inv = new ItalianInvoiceDto(
            id,
            tenantId,
            "INV-01",
            now,
            "FPR12",
            "00001",
            Guid.NewGuid(),
            "Cedente",
            "IT123",
            "Cessionario",
            "IT456",
            "0000000",
            100m,
            22m,
            122m,
            "EUR",
            now.AddDays(30),
            SdiTransmissionStatus.Draft,
            null,
            []);
        inv.DocumentNumber.Should().Be("INV-01");

        var po = new PurchaseOrderDto(
            id,
            tenantId,
            "PO-01",
            Guid.NewGuid(),
            "Supplier SpA",
            now,
            500m,
            "EUR",
            "Open");
        po.OrderNumber.Should().Be("PO-01");

        var supplier = new SupplierDto(
            id,
            tenantId,
            "Supplier SpA",
            Jurisdiction.Italy,
            "IT123",
            null,
            "0000000",
            "Via Milano 2",
            "IT60A123",
            60,
            true);
        supplier.Name.Should().Be("Supplier SpA");

        var qr = new SwissQrBillDto(
            id,
            tenantId,
            "QR-01",
            now,
            Guid.NewGuid(),
            "Creditor AG",
            "Via Nassa 1",
            "CH4431999123000889012",
            "Debtor SA",
            "Via Cantonale 2",
            250m,
            "CHF",
            "210000000003139471430009017",
            "Msg",
            now.AddDays(30),
            false);
        qr.BillNumber.Should().Be("QR-01");

        var tenant = new TenantDto(
            id,
            "TEN-01",
            "Tenant Corp",
            "IT",
            Currency.EUR,
            "IT123",
            "CF123",
            "SDI",
            null,
            null,
            "admin@corp.it",
            true);
        tenant.Code.Should().Be("TEN-01");

        var time = new TimeEntryDto(
            id,
            tenantId,
            Guid.NewGuid(),
            "Mario",
            now,
            now.AddHours(8),
            60,
            7m,
            RecordingType.Detailed,
            45.46,
            9.19);
        time.EmployeeName.Should().Be("Mario");

        var tire = new TireHotelDto(
            id,
            tenantId,
            Guid.NewGuid(),
            "AA123BB",
            TireSeason.Summer,
            "Pirelli",
            "205/55R16",
            "DOT2023",
            7m,
            7m,
            6.5m,
            6.5m,
            "RACK-1",
            "InStorage",
            now);
        tire.Brand.Should().Be("Pirelli");

        var ddt = new TransportDocumentDto(
            id,
            tenantId,
            "DDT-01",
            now,
            Guid.NewGuid(),
            "Client SpA",
            "Via Roma",
            "Vendita",
            "DHL",
            2,
            15m,
            null);
        ddt.DocumentNumber.Should().Be("DDT-01");

        var user = new UserDto(
            id,
            tenantId,
            "admin",
            "admin@corp.it",
            "Administrator",
            UserRole.SuperAdmin,
            true,
            now);
        user.Username.Should().Be("admin");
    }

    /// <summary>
    /// Tests Generic Repository CRUD and query operations.
    /// </summary>
    [Fact]
    public async Task Infrastructure_GenericRepository_PerformsCrudOperations()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateInMemoryDbContext(
            tenantId,
            dbName);
        var repo = new Repository<Client, Guid>(context);

        var address = new Address(
            "Via Torino 1",
            "Milano",
            "20100",
            "MI",
            "IT");
        var client = Client.CreateItalian(
            tenantId,
            "Cliente Test",
            new FiscalCode("RSSMRA85M01H501Z"),
            new VatNumber(
                "12345678901",
                "IT"),
            "0000000",
            "test@pec.it",
            address,
            "test@cliente.it");

        // Act - Add & Save
        await repo.AddAsync(client);
        _ = await context.SaveChangesAsync();

        // Act - GetById
        var fetched = await repo.GetByIdAsync(client.Id);
        fetched.Should().NotBeNull();
        fetched!.BusinessName.Should().Be("Cliente Test");

        // Act - Find
        var found = await repo.FindAsync(c => c.BusinessName.Contains(
            "Test",
            StringComparison.Ordinal));
        found.Should().ContainSingle();

        // Act - GetAll
        var all = await repo.GetAllAsync();
        all.Should().ContainSingle();

        // Act - Update
        client.UpdateCommercialTerms(
            60,
            Money.Euro(10000m));
        await repo.UpdateAsync(client);
        _ = await context.SaveChangesAsync();

        // Act - Delete
        await repo.DeleteAsync(client);
        _ = await context.SaveChangesAsync();

        var afterDelete = await repo.GetByIdAsync(client.Id);
        afterDelete.Should().BeNull();
    }

    /// <summary>
    /// Tests ItalianInvoiceRepository GetAllAsync including lines.
    /// </summary>
    [Fact]
    public async Task Infrastructure_ItalianInvoiceRepository_GetAllIncludesLines()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateInMemoryDbContext(
            tenantId,
            dbName);
        var repo = new ItalianInvoiceRepository(context);

        var addrCedente = new Address(
            "Via Garibaldi 1",
            "Bologna",
            "40100",
            "BO",
            "IT");
        var addrCessionario = new Address(
            "Via Dante 10",
            "Milano",
            "20121",
            "MI",
            "IT");
        var invoice = ItalianElectronicInvoice.Create(
            tenantId,
            "INV-2026-999",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Cedente Srl",
            "IT11111111111",
            "11111111111",
            addrCedente,
            "Cessionario Srl",
            "IT22222222222",
            "22222222222",
            "0000000",
            null,
            addrCessionario,
            DateTime.UtcNow.AddDays(30));
        invoice.AddLine(
            "Consulenza IT",
            1m,
            1000m,
            22m);

        await repo.AddAsync(invoice);
        _ = await context.SaveChangesAsync();

        // Act
        var all = await repo.GetAllAsync();

        // Assert
        all.Should().ContainSingle();
        all[0].Lines.Should().ContainSingle();
        all[0].Lines.ElementAt(0).Description.Should().Be("Consulenza IT");
    }

    /// <summary>
    /// Tests TranslationRepository and SqliteLocalizationService InitializeCacheAsync.
    /// </summary>
    [Fact]
    public async Task Infrastructure_LocalizationAndTranslationRepo_InitializeAndQuery()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();
        await using (var context = CreateInMemoryDbContext(
                         tenantId,
                         dbName))
        {
            var t1 = AppTranslation.Create(
                "fr",
                "Login.Title",
                "Connexion",
                null,
                tenantId);
            var t2 = AppTranslation.Create(
                "fr",
                "Login.Button",
                "Se connecter",
                null,
                tenantId);
            context.AppTranslations.AddRange(
                t1,
                t2);
            _ = await context.SaveChangesAsync();
        }

        var services = new ServiceCollection();
        services.AddScoped<IFatturaFacileDbContext>(_ => CreateInMemoryDbContext(
            tenantId,
            dbName));
        using var provider = services.BuildServiceProvider();

        // Act
        var locService = new SqliteLocalizationService(provider);
        await locService.InitializeCacheAsync();

        await using var queryContext = CreateInMemoryDbContext(
            tenantId,
            dbName);
        var transRepo = new TranslationRepository(queryContext);
        var frTranslations = await transRepo.GetByLocaleAsync(
            "fr",
            tenantId);

        // Assert
        frTranslations.Should().HaveCount(2);
        locService.GetString("Login.Title").Should().NotBeNull();
    }
}
