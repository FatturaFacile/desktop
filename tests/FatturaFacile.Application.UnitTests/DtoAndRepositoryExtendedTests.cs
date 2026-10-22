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
        _ = asm.Should().NotBeNull();
        _ = asm.GetName().Name.Should().Be(expected: "FatturaFacile.Application");
    }

    /// <summary>
    /// Creates an in-memory database context instance configured with a tenant mock.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="dbName">The optional database name.</param>
    /// <returns>A new <see cref="FatturaFacileDbContext"/> instance.</returns>
    private static FatturaFacileDbContext CreateInMemoryDbContext(
        Guid    tenantId,
        string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
                     .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                     .Options;

        var tenantMock = new Mock<ITenantProvider>();
        _ = tenantMock.Setup(expression: t => t.CurrentTenantId).Returns(value: tenantId);

        var userMock = new Mock<ICurrentUserService>();
        _ = userMock.Setup(expression: u => u.Username).Returns(value: "TestUser");

        return new FatturaFacileDbContext(
                                          options: options,
                                          tenantProvider: tenantMock.Object,
                                          currentUserService: userMock.Object);
    }

    /// <summary>
    /// Tests all DTO records for accurate instantiation and property mapping.
    /// </summary>
    [Fact]
    public void Application_Dtos_PropertiesAreAssignedCorrectly()
    {
        var id       = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var now      = DateTime.UtcNow;

        var absence = new AbsenceDto(
                                     Id: id,
                                     TenantId: tenantId,
                                     EmployeeId: Guid.NewGuid(),
                                     AbsenceType: AbsenceType.InpsIllnessPuc,
                                     StartDate: now,
                                     EndDate: now.AddDays(value: 2),
                                     TotalWorkingDays: 2m,
                                     IsApproved: true,
                                     CertificateNumber: "MED-123");

        _ = absence.CertificateNumber.Should().Be(expected: "MED-123");
        _ = absence.TotalWorkingDays.Should().Be(expected: 2m);

        var trans = new AppTranslationDto(
                                          Id: id,
                                          TenantId: tenantId,
                                          Locale: "it",
                                          Key: "Key1",
                                          DefaultValue: "DefaultVal",
                                          CustomValue: "CustomVal",
                                          EffectiveValue: "CustomVal",
                                          UpdatedAtUtc: now);

        _ = trans.EffectiveValue.Should().Be(expected: "CustomVal");

        var client = new ClientDto(
                                   Id: id,
                                   TenantId: tenantId,
                                   BusinessName: "BizName",
                                   TradeName: "TradeName",
                                   Jurisdiction: Jurisdiction.Italy,
                                   FiscalCode: "CF123",
                                   VatNumber: "VAT123",
                                   SdiCode: "SDI123",
                                   PecEmail: "pec@test.it",
                                   SwissUid: "CHE-123",
                                   QrIban: "CH123",
                                   CantonalCode: "TI",
                                   AddressFormatted: "Via Roma 1",
                                   Email: "test@test.it",
                                   PhoneNumber: "+3902123",
                                   PaymentTermDays: 30,
                                   CreditLimitAmount: 5000m,
                                   CreditLimitCurrency: "EUR",
                                   IsActive: true);

        _ = client.BusinessName.Should().Be(expected: "BizName");
        _ = client.CreditLimitAmount.Should().Be(expected: 5000m);

        var vehicle = new CustomerVehicleDto(
                                             Id: id,
                                             TenantId: tenantId,
                                             ClientId: Guid.NewGuid(),
                                             LicensePlate: "AA123BB",
                                             VinChassisNumber: "VIN123",
                                             Brand: "Fiat",
                                             Model: "500",
                                             RegistrationYear: 2022,
                                             Jurisdiction: Jurisdiction.Italy,
                                             CurrentKilometers: 10000);

        _ = vehicle.LicensePlate.Should().Be(expected: "AA123BB");

        var kpi = new DashboardKpiDto(
                                      TotalRevenueEur: 1000m,
                                      TotalRevenueChf: 2000m,
                                      PendingReceivablesEur: 300m,
                                      PendingReceivablesChf: 400m,
                                      PendingVatLiabilityEur: 220m,
                                      PendingVatLiabilityChf: 80m,
                                      NextTaxDeadline: now.AddDays(value: 10),
                                      ActiveClientsCount: 5,
                                      PendingInvoicesCount: 2,
                                      StoredTireSetsCount: 8);

        _ = kpi.TotalRevenueEur.Should().Be(expected: 1000m);
        _ = kpi.ActiveClientsCount.Should().Be(expected: 5);

        var line = new InvoiceLineDto(
                                      LineNumber: 1,
                                      Description: "Articolo A",
                                      Quantity: 2m,
                                      UnitPrice: 50m,
                                      VatRate: 22m,
                                      NaturaExemptionCode: null,
                                      LineTotal: 100m);

        _ = line.LineTotal.Should().Be(expected: 100m);

        var inv = new ItalianInvoiceDto(
                                        Id: id,
                                        TenantId: tenantId,
                                        DocumentNumber: "INV-01",
                                        DocumentDate: now,
                                        FormatoTrasmissione: "FPR12",
                                        ProgressivoInvio: "00001",
                                        ClientId: Guid.NewGuid(),
                                        CedenteDenominazione: "Cedente",
                                        CedentePartitaIva: "IT123",
                                        CessionarioDenominazione: "Cessionario",
                                        CessionarioPartitaIva: "IT456",
                                        CessionarioCodiceDestinatario: "0000000",
                                        TotalNet: 100m,
                                        TotalVat: 22m,
                                        TotalGross: 122m,
                                        Currency: "EUR",
                                        PaymentDueDate: now.AddDays(value: 30),
                                        SdiStatus: SdiTransmissionStatus.Draft,
                                        SdiIdentifier: null,
                                        Lines: []);

        _ = inv.DocumentNumber.Should().Be(expected: "INV-01");

        var po = new PurchaseOrderDto(
                                      Id: id,
                                      TenantId: tenantId,
                                      OrderNumber: "PO-01",
                                      SupplierId: Guid.NewGuid(),
                                      SupplierName: "Supplier SpA",
                                      OrderDate: now,
                                      TotalAmount: 500m,
                                      Currency: "EUR",
                                      Status: "Open");

        _ = po.OrderNumber.Should().Be(expected: "PO-01");

        var supplier = new SupplierDto(
                                       Id: id,
                                       TenantId: tenantId,
                                       Name: "Supplier SpA",
                                       Jurisdiction: Jurisdiction.Italy,
                                       VatNumber: "IT123",
                                       SwissUid: null,
                                       SdiCode: "0000000",
                                       AddressFormatted: "Via Milano 2",
                                       Iban: "IT60A123",
                                       PaymentTermsDays: 60,
                                       IsActive: true);

        _ = supplier.Name.Should().Be(expected: "Supplier SpA");

        var qr = new SwissQrBillDto(
                                    Id: id,
                                    TenantId: tenantId,
                                    BillNumber: "QR-01",
                                    BillDate: now,
                                    ClientId: Guid.NewGuid(),
                                    CreditorName: "Creditor AG",
                                    CreditorAddressFormatted: "Via Nassa 1",
                                    CreditorQrIban: "CH4431999123000889012",
                                    DebtorName: "Debtor SA",
                                    DebtorAddressFormatted: "Via Cantonale 2",
                                    Amount: 250m,
                                    Currency: "CHF",
                                    ReferenceNumber: "210000000003139471430009017",
                                    AdditionalMessage: "Msg",
                                    DueDate: now.AddDays(value: 30),
                                    IsPaid: false);

        _ = qr.BillNumber.Should().Be(expected: "QR-01");

        var tenant = new TenantDto(
                                   Id: id,
                                   Code: "TEN-01",
                                   BusinessName: "Tenant Corp",
                                   CountryCode: "IT",
                                   BaseCurrency: Currency.EUR,
                                   VatNumber: "IT123",
                                   FiscalCode: "CF123",
                                   SdiCode: "SDI",
                                   SwissUid: null,
                                   QrIban: null,
                                   ContactEmail: "admin@corp.it",
                                   IsActive: true);

        _ = tenant.Code.Should().Be(expected: "TEN-01");

        var time = new TimeEntryDto(
                                    Id: id,
                                    TenantId: tenantId,
                                    EmployeeId: Guid.NewGuid(),
                                    EmployeeName: "Mario",
                                    ClockInUtc: now,
                                    ClockOutUtc: now.AddHours(value: 8),
                                    BreakDurationMinutes: 60,
                                    TotalNetHours: 7m,
                                    RecordingType: RecordingType.Detailed,
                                    GpsLatitude: 45.46,
                                    GpsLongitude: 9.19);

        _ = time.EmployeeName.Should().Be(expected: "Mario");

        var tire = new TireHotelDto(
                                    Id: id,
                                    TenantId: tenantId,
                                    VehicleId: Guid.NewGuid(),
                                    VehiclePlate: "AA123BB",
                                    Season: TireSeason.Summer,
                                    Brand: "Pirelli",
                                    Dimension: "205/55R16",
                                    DotCode: "DOT2023",
                                    TreadDepthFrontLeftMm: 7m,
                                    TreadDepthFrontRightMm: 7m,
                                    TreadDepthRearLeftMm: 6.5m,
                                    TreadDepthRearRightMm: 6.5m,
                                    WarehouseRackCoordinate: "RACK-1",
                                    Status: "InStorage",
                                    StoredDateUtc: now);

        _ = tire.Brand.Should().Be(expected: "Pirelli");

        var ddt = new TransportDocumentDto(
                                           Id: id,
                                           TenantId: tenantId,
                                           DocumentNumber: "DDT-01",
                                           DocumentDate: now,
                                           ClientId: Guid.NewGuid(),
                                           ClientName: "Client SpA",
                                           DestinationAddressFormatted: "Via Roma",
                                           TransportationReason: "Vendita",
                                           CarrierName: "DHL",
                                           NumberOfPackages: 2,
                                           TotalWeightKg: 15m,
                                           CustomsCmrNotes: null);

        _ = ddt.DocumentNumber.Should().Be(expected: "DDT-01");

        var user = new UserDto(
                               Id: id,
                               TenantId: tenantId,
                               Username: "admin",
                               Email: "admin@corp.it",
                               FullName: "Administrator",
                               Role: UserRole.SuperAdmin,
                               IsActive: true,
                               LastLoginUtc: now);

        _ = user.Username.Should().Be(expected: "admin");
    }

    /// <summary>
    /// Tests Generic Repository CRUD and query operations.
    /// </summary>
    [Fact]
    public async Task Infrastructure_GenericRepository_PerformsCrudOperations()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName   = Guid.NewGuid().ToString();

        await using var context = CreateInMemoryDbContext(
                                                          tenantId: tenantId,
                                                          dbName: dbName);

        var repo = new Repository<Client, Guid>(dbContext: context);

        var address = new Address(
                                  street: "Via Torino 1",
                                  city: "Milano",
                                  postalCode: "20100",
                                  provinceOrCanton: "MI",
                                  countryCode: "IT");

        var client = Client.CreateItalian(
                                          tenantId: tenantId,
                                          businessName: "Cliente Test",
                                          fiscalCode: new FiscalCode(value: "RSSMRA85M01H501Z"),
                                          vatNumber: new VatNumber(
                                                                   value: "12345678901",
                                                                   countryCode: "IT"),
                                          sdiCode: "0000000",
                                          pecEmail: "test@pec.it",
                                          address: address,
                                          email: "test@cliente.it");

        // Act - Add & Save
        await repo.AddAsync(entity: client);
        _ = await context.SaveChangesAsync();

        // Act - GetById
        var fetched = await repo.GetByIdAsync(id: client.Id);
        _ = fetched.Should().NotBeNull();
        _ = fetched!.BusinessName.Should().Be(expected: "Cliente Test");

        // Act - Find
        var found = await repo.FindAsync(predicate: c => c.BusinessName.Contains(
                                                                                 "Test",
                                                                                 StringComparison.Ordinal));

        _ = found.Should().ContainSingle();

        // Act - GetAll
        var all = await repo.GetAllAsync();
        _ = all.Should().ContainSingle();

        // Act - Update
        client.UpdateCommercialTerms(
                                     paymentTermDays: 60,
                                     creditLimit: Money.Euro(amount: 10000m));

        await repo.UpdateAsync(entity: client);
        _ = await context.SaveChangesAsync();

        // Act - Delete
        await repo.DeleteAsync(entity: client);
        _ = await context.SaveChangesAsync();

        var afterDelete = await repo.GetByIdAsync(id: client.Id);
        _ = afterDelete.Should().BeNull();
    }

    /// <summary>
    /// Tests ItalianInvoiceRepository GetAllAsync including lines.
    /// </summary>
    [Fact]
    public async Task Infrastructure_ItalianInvoiceRepository_GetAllIncludesLines()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName   = Guid.NewGuid().ToString();

        await using var context = CreateInMemoryDbContext(
                                                          tenantId: tenantId,
                                                          dbName: dbName);

        var repo = new ItalianInvoiceRepository(dbContext: context);

        var addrCedente = new Address(
                                      street: "Via Garibaldi 1",
                                      city: "Bologna",
                                      postalCode: "40100",
                                      provinceOrCanton: "BO",
                                      countryCode: "IT");

        var addrCessionario = new Address(
                                          street: "Via Dante 10",
                                          city: "Milano",
                                          postalCode: "20121",
                                          provinceOrCanton: "MI",
                                          countryCode: "IT");

        var invoice = ItalianElectronicInvoice.Create(
                                                      tenantId: tenantId,
                                                      documentNumber: "INV-2026-999",
                                                      documentDate: DateTime.UtcNow,
                                                      clientId: Guid.NewGuid(),
                                                      cedenteName: "Cedente Srl",
                                                      cedenteVat: "IT11111111111",
                                                      cedenteCf: "11111111111",
                                                      cedenteAddress: addrCedente,
                                                      cessionarioName: "Cessionario Srl",
                                                      cessionarioVat: "IT22222222222",
                                                      cessionarioCf: "22222222222",
                                                      sdiCode: "0000000",
                                                      pec: null,
                                                      cessionarioAddress: addrCessionario,
                                                      dueDate: DateTime.UtcNow.AddDays(value: 30));

        invoice.AddLine(
                        description: "Consulenza IT",
                        quantity: 1m,
                        unitPrice: 1000m,
                        vatRate: 22m);

        await repo.AddAsync(entity: invoice);
        _ = await context.SaveChangesAsync();

        // Act
        var all = await repo.GetAllAsync();

        // Assert
        _ = all.Should().ContainSingle();
        _ = all[index: 0].Lines.Should().ContainSingle();
        _ = all[index: 0].Lines.ElementAt(index: 0).Description.Should().Be(expected: "Consulenza IT");
    }

    /// <summary>
    /// Tests TranslationRepository and SqliteLocalizationService InitializeCacheAsync.
    /// </summary>
    [Fact]
    public async Task Infrastructure_LocalizationAndTranslationRepo_InitializeAndQuery()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName   = Guid.NewGuid().ToString();

        await using (var context = CreateInMemoryDbContext(
                                                           tenantId: tenantId,
                                                           dbName: dbName))
        {
            var t1 = AppTranslation.Create(
                                           locale: "fr",
                                           key: "Login.Title",
                                           defaultValue: "Connexion",
                                           customValue: null,
                                           tenantId: tenantId);

            var t2 = AppTranslation.Create(
                                           locale: "fr",
                                           key: "Login.Button",
                                           defaultValue: "Se connecter",
                                           customValue: null,
                                           tenantId: tenantId);

            context.AppTranslations.AddRange(
                                             entities: [t1, t2]);

            _ = await context.SaveChangesAsync();
        }

        var services = new ServiceCollection();

        _ = services.AddScoped<IFatturaFacileDbContext>(implementationFactory: _ => CreateInMemoryDbContext(
                                                                                                            tenantId: tenantId,
                                                                                                            dbName: dbName));

        using var provider = services.BuildServiceProvider();

        // Act
        var locService = new SqliteLocalizationService(serviceProvider: provider);
        await locService.InitializeCacheAsync();

        await using var queryContext = CreateInMemoryDbContext(
                                                               tenantId: tenantId,
                                                               dbName: dbName);

        var transRepo = new TranslationRepository(dbContext: queryContext);

        var frTranslations = await transRepo.GetByLocaleAsync(
                                                              locale: "fr",
                                                              tenantId: tenantId);

        // Assert
        _ = frTranslations.Should().HaveCount(expected: 2);
        _ = locService.GetString(key: "Login.Title").Should().NotBeNull();
    }

}
