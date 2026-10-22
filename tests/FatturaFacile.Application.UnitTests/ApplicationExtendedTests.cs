// <copyright file="ApplicationExtendedTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Administration;
using FatturaFacile.Application.Features.Automotive;
using FatturaFacile.Application.Features.Clients;
using FatturaFacile.Application.Features.Invoices;
using FatturaFacile.Application.Features.Localization;
using FatturaFacile.Application.Features.TimeTracking;
using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Repositories;
using FatturaFacile.Domain.ValueObjects;

using Moq;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Contains extended unit tests for application query handlers, command validators, and DTO data models.
/// </summary>
public sealed class ApplicationExtendedTests
{

    /// <summary>
    /// Mock of the tenant provider used across test scenarios.
    /// </summary>
    private readonly Mock<ITenantProvider> tenantProviderMock = new();

    /// <summary>
    /// Tests that the GetTimeEntriesQueryHandler queries the repository and maps entities to DTOs.
    /// </summary>
    [Fact]
    public async Task GetTimeEntriesQueryHandler_WhenHandled_ReturnsMappedDtos()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var empId    = Guid.NewGuid();
        _ = tenantProviderMock.Setup(expression: p => p.CurrentTenantId).Returns(value: tenantId);

        var repoMock = new Mock<ITimeEntryRepository>();

        var entry = TimeEntry.ClockIn(
                                      tenantId: tenantId,
                                      employeeId: empId,
                                      clockInUtc: DateTime.UtcNow.AddHours(value: -8));

        entry.ClockOut(clockOutUtc: DateTime.UtcNow);

        _ = repoMock.Setup(expression: r => r.GetEntriesByEmployeeAndDateRangeAsync(
                                                                                    empId,
                                                                                    It.IsAny<DateTime>(),
                                                                                    It.IsAny<DateTime>(),
                                                                                    It.IsAny<CancellationToken>()))
                    .ReturnsAsync(value: [entry]);

        var mapperMock = new Mock<IMapper>();

        var expectedDtos = new List<TimeEntryDto>
                           {
                               new(
                                   Id: Guid.NewGuid(),
                                   TenantId: tenantId,
                                   EmployeeId: empId,
                                   EmployeeName: "Emp 1",
                                   ClockInUtc: DateTime.UtcNow.AddHours(value: -8),
                                   ClockOutUtc: DateTime.UtcNow,
                                   BreakDurationMinutes: 0,
                                   TotalNetHours: 8m,
                                   RecordingType: RecordingType.Detailed,
                                   GpsLatitude: null,
                                   GpsLongitude: null),
                           };

        _ = mapperMock.Setup(expression: m => m.Map<IReadOnlyList<TimeEntryDto>>(It.IsAny<object>()))
                      .Returns(value: expectedDtos);

        var handler = new GetTimeEntriesQueryHandler(
                                                     repository: repoMock.Object,
                                                     mapper: mapperMock.Object);

        var query = new GetTimeEntriesQuery(EmployeeId: empId);

        // Act
        var result = await handler.Handle(
                                          request: query,
                                          cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().HaveCount(expected: 1);
        _ = result[index: 0].EmployeeId.Should().Be(expected: empId);
        _ = result[index: 0].TotalNetHours.Should().Be(expected: 8m);
    }

    /// <summary>
    /// Tests that the GetTranslationsQueryHandler returns translations from the localization service.
    /// </summary>
    [Fact]
    public async Task GetTranslationsQueryHandler_WhenHandled_ReturnsTranslationsList()
    {
        // Arrange
        var locServiceMock = new Mock<ILocalizationService>();

        var translations = new List<AppTranslationDto>
                           {
                               new(
                                   Id: Guid.NewGuid(),
                                   TenantId: Guid.NewGuid(),
                                   Locale: "it",
                                   Key: "Common.Save",
                                   DefaultValue: "Salva",
                                   CustomValue: null,
                                   EffectiveValue: "Salva",
                                   UpdatedAtUtc: DateTime.UtcNow),
                               new(
                                   Id: Guid.NewGuid(),
                                   TenantId: Guid.NewGuid(),
                                   Locale: "it",
                                   Key: "Common.Cancel",
                                   DefaultValue: "Annulla",
                                   CustomValue: null,
                                   EffectiveValue: "Annulla",
                                   UpdatedAtUtc: DateTime.UtcNow),
                           };

        _ = locServiceMock.Setup(expression: s => s.GetAllTranslationsAsync(
                                                                            "it",
                                                                            It.IsAny<CancellationToken>()))
                          .ReturnsAsync(value: translations);

        var handler = new GetTranslationsQueryHandler(localizationService: locServiceMock.Object);

        // Act
        var result = await handler.Handle(
                                          request: new GetTranslationsQuery(Locale: "it"),
                                          cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().HaveCount(expected: 2);
        _ = result[index: 0].Key.Should().Be(expected: "Common.Save");
    }

    /// <summary>
    /// Tests that GetInvoicesOverviewQueryHandler aggregates Italian invoices and Swiss QR bills.
    /// </summary>
    [Fact]
    public async Task GetInvoicesOverviewQueryHandler_WhenHandled_ReturnsAggregatedInvoices()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _ = tenantProviderMock.Setup(expression: p => p.CurrentTenantId).Returns(value: tenantId);

        var itRepoMock = new Mock<IItalianInvoiceRepository>();

        var itInvoice = ItalianElectronicInvoice.Create(
                                                        tenantId: tenantId,
                                                        documentNumber: "IT-001",
                                                        documentDate: DateTime.UtcNow,
                                                        clientId: Guid.NewGuid(),
                                                        cedenteName: "Cedente Srl",
                                                        cedenteVat: "12345678901",
                                                        cedenteCf: "12345678901",
                                                        cedenteAddress: new Address(
                                                                                    street: "Via Roma 1",
                                                                                    city: "Milano",
                                                                                    postalCode: "20100",
                                                                                    provinceOrCanton: "MI",
                                                                                    countryCode: "IT"),
                                                        cessionarioName: "Cessionario Spa",
                                                        cessionarioVat: "98765432101",
                                                        cessionarioCf: "RSSMRA80A01F205X",
                                                        sdiCode: "SUBM170",
                                                        pec: "dest@pec.it",
                                                        cessionarioAddress: new Address(
                                                                                        street: "Via Milano 2",
                                                                                        city: "Roma",
                                                                                        postalCode: "00100",
                                                                                        provinceOrCanton: "RM",
                                                                                        countryCode: "IT"),
                                                        dueDate: DateTime.UtcNow.AddDays(value: 30));

        itInvoice.AddLine(
                          description: "Consulting",
                          quantity: 1,
                          unitPrice: 1000m,
                          vatRate: 22.0m);

        _ = itRepoMock.Setup(expression: r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(value: [itInvoice]);

        var chRepoMock = new Mock<ISwissQrBillRepository>();

        var chBill = SwissQrBill.Create(
                                        tenantId: tenantId,
                                        billNumber: "QR-001",
                                        billDate: DateTime.UtcNow,
                                        clientId: Guid.NewGuid(),
                                        creditorName: "Gotthard AG",
                                        creditorAddress: new Address(
                                                                     street: "Via San Gottardo",
                                                                     city: "Lugano",
                                                                     postalCode: "6900",
                                                                     provinceOrCanton: "TI",
                                                                     countryCode: "CH"),
                                        creditorQrIban: new QrIban(value: "CH4431999123000889012"),
                                        debtorName: "Client SA",
                                        debtorAddress: new Address(
                                                                   street: "Via Nassa",
                                                                   city: "Lugano",
                                                                   postalCode: "6900",
                                                                   provinceOrCanton: "TI",
                                                                   countryCode: "CH"),
                                        rawAmount: Money.Chf(amount: 1500m),
                                        referenceNumber: "210000000003139471430009017",
                                        dueDate: DateTime.UtcNow.AddDays(value: 30));

        _ = chRepoMock.Setup(expression: r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(value: [chBill]);

        var mapperMock = new Mock<IMapper>();

        _ = mapperMock.Setup(expression: m => m.Map<IReadOnlyList<ItalianInvoiceDto>>(It.IsAny<object>()))
                      .Returns(
                               value:
                               [
                                   new ItalianInvoiceDto(
                                                         Id: Guid.NewGuid(),
                                                         TenantId: tenantId,
                                                         DocumentNumber: "IT-001",
                                                         DocumentDate: DateTime.UtcNow,
                                                         FormatoTrasmissione: "FPA12",
                                                         ProgressivoInvio: "00001",
                                                         ClientId: Guid.NewGuid(),
                                                         CedenteDenominazione: "Cedente",
                                                         CedentePartitaIva: "123",
                                                         CessionarioDenominazione: "Cess",
                                                         CessionarioPartitaIva: null,
                                                         CessionarioCodiceDestinatario: "0000000",
                                                         TotalNet: 1000m,
                                                         TotalVat: 220m,
                                                         TotalGross: 1220m,
                                                         Currency: "EUR",
                                                         PaymentDueDate: DateTime.UtcNow.AddDays(value: 30),
                                                         SdiStatus: SdiTransmissionStatus.Draft,
                                                         SdiIdentifier: null,
                                                         Lines: []),
                               ]);

        _ = mapperMock.Setup(expression: m => m.Map<IReadOnlyList<SwissQrBillDto>>(It.IsAny<object>()))
                      .Returns(
                               value:
                               [
                                   new SwissQrBillDto(
                                                      Id: Guid.NewGuid(),
                                                      TenantId: tenantId,
                                                      BillNumber: "QR-001",
                                                      BillDate: DateTime.UtcNow,
                                                      ClientId: Guid.NewGuid(),
                                                      CreditorName: "Gotthard AG",
                                                      CreditorAddressFormatted: "Lugano",
                                                      CreditorQrIban: "CH44",
                                                      DebtorName: "Client",
                                                      DebtorAddressFormatted: "Lugano",
                                                      Amount: 1500m,
                                                      Currency: "CHF",
                                                      ReferenceNumber: "2100",
                                                      AdditionalMessage: null,
                                                      DueDate: DateTime.UtcNow.AddDays(value: 30),
                                                      IsPaid: false),
                               ]);

        var handler = new GetInvoicesOverviewQueryHandler(
                                                          itRepo: itRepoMock.Object,
                                                          chRepo: chRepoMock.Object,
                                                          mapper: mapperMock.Object);

        // Act
        var result = await handler.Handle(
                                          request: new GetInvoicesOverviewQuery(),
                                          cancellationToken: CancellationToken.None);

        // Assert
        _ = result.ItalianInvoices.Should().HaveCount(expected: 1);
        _ = result.ItalianInvoices[index: 0].DocumentNumber.Should().Be(expected: "IT-001");
        _ = result.SwissQrBills.Should().HaveCount(expected: 1);
        _ = result.SwissQrBills[index: 0].BillNumber.Should().Be(expected: "QR-001");
    }

    /// <summary>
    /// Tests that GetClientsQueryHandler fetches active clients and maps to DTOs.
    /// </summary>
    [Fact]
    public async Task GetClientsQueryHandler_WhenHandled_ReturnsClientDtos()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _ = tenantProviderMock.Setup(expression: p => p.CurrentTenantId).Returns(value: tenantId);

        var clientRepoMock = new Mock<IClientRepository>();

        var client = Client.CreateItalian(
                                          tenantId: tenantId,
                                          businessName: "Alpha Corp Srl",
                                          fiscalCode: new FiscalCode(value: "RSSMRA80A01F205X"),
                                          vatNumber: new VatNumber(
                                                                   value: "12345678901",
                                                                   countryCode: "IT"),
                                          sdiCode: "M5UXCR1",
                                          pecEmail: "alpha@pec.it",
                                          address: new Address(
                                                               street: "Via Lavoratori 1",
                                                               city: "Monza",
                                                               postalCode: "20900",
                                                               provinceOrCanton: "MB",
                                                               countryCode: "IT"),
                                          email: "info@alphacorp.it",
                                          phone: "+39 039 000000");

        _ = clientRepoMock.Setup(expression: r => r.GetActiveClientsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(value: [client]);

        var mapperMock = new Mock<IMapper>();

        _ = mapperMock.Setup(expression: m => m.Map<IReadOnlyList<ClientDto>>(It.IsAny<object>()))
                      .Returns(
                               value:
                               [
                                   new ClientDto(
                                                 Id: Guid.NewGuid(),
                                                 TenantId: tenantId,
                                                 BusinessName: "Alpha Corp Srl",
                                                 TradeName: null,
                                                 Jurisdiction: Jurisdiction.Italy,
                                                 FiscalCode: "RSSMRA80A01F205X",
                                                 VatNumber: "12345678901",
                                                 SdiCode: "M5UXCR1",
                                                 PecEmail: "alpha@pec.it",
                                                 SwissUid: null,
                                                 QrIban: null,
                                                 CantonalCode: null,
                                                 AddressFormatted: "Via Lavoratori 1",
                                                 Email: "info@alphacorp.it",
                                                 PhoneNumber: "+39",
                                                 PaymentTermDays: 30,
                                                 CreditLimitAmount: 1000m,
                                                 CreditLimitCurrency: "EUR",
                                                 IsActive: true),
                               ]);

        var handler = new GetClientsQueryHandler(
                                                 clientRepository: clientRepoMock.Object,
                                                 mapper: mapperMock.Object);

        // Act
        var result = await handler.Handle(
                                          request: new GetClientsQuery(),
                                          cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().HaveCount(expected: 1);
        _ = result[index: 0].BusinessName.Should().Be(expected: "Alpha Corp Srl");
        _ = result[index: 0].SdiCode.Should().Be(expected: "M5UXCR1");
    }

    /// <summary>
    /// Tests that GetTireRecordsQueryHandler returns all in-storage tire hotel records.
    /// </summary>
    [Fact]
    public async Task GetTireRecordsQueryHandler_WhenHandled_ReturnsTireHotelDtos()
    {
        // Arrange
        var tenantId  = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        _ = tenantProviderMock.Setup(expression: p => p.CurrentTenantId).Returns(value: tenantId);

        var tireRepoMock = new Mock<ITireRecordRepository>();

        var record = TireHotelRecord.CheckIn(
                                             tenantId: tenantId,
                                             vehicleId: vehicleId,
                                             season: TireSeason.Summer,
                                             brand: "Pirelli P Zero",
                                             dimension: "245/40 R18",
                                             dotCode: "1224",
                                             flMm: 6.5m,
                                             frMm: 6.5m,
                                             rlMm: 6.2m,
                                             rrMm: 6.2m,
                                             rackCoordinate: "SHELF-3A");

        _ = tireRepoMock.Setup(expression: r => r.GetAllInStorageAsync(It.IsAny<CancellationToken>())).ReturnsAsync(value: [record]);

        var mapperMock = new Mock<IMapper>();

        _ = mapperMock.Setup(expression: m => m.Map<IReadOnlyList<TireHotelDto>>(It.IsAny<object>()))
                      .Returns(
                               value:
                               [
                                   new TireHotelDto(
                                                    Id: Guid.NewGuid(),
                                                    TenantId: tenantId,
                                                    VehicleId: vehicleId,
                                                    VehiclePlate: "AB123CD",
                                                    Season: TireSeason.Summer,
                                                    Brand: "Pirelli P Zero",
                                                    Dimension: "245/40 R18",
                                                    DotCode: "1224",
                                                    TreadDepthFrontLeftMm: 6.5m,
                                                    TreadDepthFrontRightMm: 6.5m,
                                                    TreadDepthRearLeftMm: 6.2m,
                                                    TreadDepthRearRightMm: 6.2m,
                                                    WarehouseRackCoordinate: "SHELF-3A",
                                                    Status: "InStorage",
                                                    StoredDateUtc: DateTime.UtcNow),
                               ]);

        var handler = new GetTireRecordsQueryHandler(
                                                     repository: tireRepoMock.Object,
                                                     mapper: mapperMock.Object);

        // Act
        var result = await handler.Handle(
                                          request: new GetTireRecordsQuery(),
                                          cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().HaveCount(expected: 1);
        _ = result[index: 0].WarehouseRackCoordinate.Should().Be(expected: "SHELF-3A");
        _ = result[index: 0].Brand.Should().Be(expected: "Pirelli P Zero");
    }

    /// <summary>
    /// Tests that GetTenantsQueryHandler returns all active tenants mapped to TenantDto.
    /// </summary>
    [Fact]
    public async Task GetTenantsQueryHandler_WhenHandled_ReturnsTenantDtos()
    {
        // Arrange
        var tenantRepoMock = new Mock<ITenantRepository>();

        var tenant = Tenant.Create(
                                   code: "TENANT-01",
                                   businessName: "Main Tenant",
                                   countryCode: "IT",
                                   baseCurrency: Currency.EUR,
                                   contactEmail: "admin@main.it",
                                   vatNumber: "12345678901");

        _ = tenantRepoMock.Setup(expression: r => r.GetActiveTenantsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(value: [tenant]);

        var mapperMock = new Mock<IMapper>();

        _ = mapperMock.Setup(expression: m => m.Map<IReadOnlyList<TenantDto>>(It.IsAny<object>()))
                      .Returns(
                               value:
                               [
                                   new TenantDto(
                                                 Id: Guid.NewGuid(),
                                                 Code: "TENANT-01",
                                                 BusinessName: "Main Tenant",
                                                 CountryCode: "IT",
                                                 BaseCurrency: Currency.EUR,
                                                 VatNumber: "12345678901",
                                                 FiscalCode: null,
                                                 SdiCode: null,
                                                 SwissUid: null,
                                                 QrIban: null,
                                                 ContactEmail: "admin@main.it",
                                                 IsActive: true),
                               ]);

        var handler = new GetTenantsQueryHandler(
                                                 tenantRepository: tenantRepoMock.Object,
                                                 mapper: mapperMock.Object);

        // Act
        var result = await handler.Handle(
                                          request: new GetTenantsQuery(),
                                          cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().HaveCount(expected: 1);
        _ = result[index: 0].Code.Should().Be(expected: "TENANT-01");
        _ = result[index: 0].BusinessName.Should().Be(expected: "Main Tenant");
    }

    /// <summary>
    /// Tests validation rules for CreateClientCommandValidator across Italian and Swiss formats.
    /// </summary>
    [Fact]
    public async Task CreateClientCommandValidator_ValidatesItalianAndSwissRules()
    {
        // Arrange
        var validator = new CreateClientCommandValidator();

        var invalidCommand = new CreateClientCommand(
                                                     BusinessName: string.Empty,
                                                     Jurisdiction: Jurisdiction.Italy,
                                                     FiscalCode: null,
                                                     VatNumber: null,
                                                     SdiCode: null,
                                                     PecEmail: null,
                                                     SwissUid: null,
                                                     QrIban: null,
                                                     CantonalCode: null,
                                                     Street: string.Empty,
                                                     City: string.Empty,
                                                     PostalCode: string.Empty,
                                                     ProvinceOrCanton: string.Empty,
                                                     CountryCode: "IT",
                                                     Email: "invalid-email",
                                                     PhoneNumber: null);

        var validItCommand = new CreateClientCommand(
                                                     BusinessName: "Valid Srl",
                                                     Jurisdiction: Jurisdiction.Italy,
                                                     FiscalCode: "RSSMRA80A01F205X",
                                                     VatNumber: "12345678901",
                                                     SdiCode: "0000000",
                                                     PecEmail: "valid@pec.it",
                                                     SwissUid: null,
                                                     QrIban: null,
                                                     CantonalCode: null,
                                                     Street: "Via Roma 1",
                                                     City: "Milano",
                                                     PostalCode: "20100",
                                                     ProvinceOrCanton: "MI",
                                                     CountryCode: "IT",
                                                     Email: "info@valid.it",
                                                     PhoneNumber: "+39 02 1234567");

        var validChCommand = new CreateClientCommand(
                                                     BusinessName: "Valid AG",
                                                     Jurisdiction: Jurisdiction.Switzerland,
                                                     FiscalCode: null,
                                                     VatNumber: null,
                                                     SdiCode: null,
                                                     PecEmail: null,
                                                     SwissUid: "CHE-123.456.789",
                                                     QrIban: "CH4431999123000889012",
                                                     CantonalCode: "TI",
                                                     Street: "Via Nassa 1",
                                                     City: "Lugano",
                                                     PostalCode: "6900",
                                                     ProvinceOrCanton: "TI",
                                                     CountryCode: "CH",
                                                     Email: "info@valid.ch",
                                                     PhoneNumber: "+41 91 123 45 67");

        // Act
        var invalidResult = await validator.ValidateAsync(instance: invalidCommand);
        var validItResult = await validator.ValidateAsync(instance: validItCommand);
        var validChResult = await validator.ValidateAsync(instance: validChCommand);

        // Assert
        _ = invalidResult.IsValid.Should().BeFalse();
        _ = invalidResult.Errors.Should().Contain(predicate: e => e.PropertyName == nameof(CreateClientCommand.BusinessName));
        _ = invalidResult.Errors.Should().Contain(predicate: e => e.PropertyName == nameof(CreateClientCommand.Email));

        _ = validItResult.IsValid.Should().BeTrue();
        _ = validChResult.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that all application DTO records preserve values and equality contracts.
    /// </summary>
    [Fact]
    public void ApplicationDtos_ConstructAndExposeProperties()
    {
        // AbsenceDto
        var absence = new AbsenceDto(
                                     Id: Guid.NewGuid(),
                                     TenantId: Guid.NewGuid(),
                                     EmployeeId: Guid.NewGuid(),
                                     AbsenceType: AbsenceType.Ferie,
                                     StartDate: DateTime.UtcNow,
                                     EndDate: DateTime.UtcNow.AddDays(value: 2),
                                     TotalWorkingDays: 2m,
                                     IsApproved: true,
                                     CertificateNumber: "MED-1");

        _ = absence.TotalWorkingDays.Should().Be(expected: 2m);

        // CustomerVehicleDto
        var vehicle = new CustomerVehicleDto(
                                             Id: Guid.NewGuid(),
                                             TenantId: Guid.NewGuid(),
                                             ClientId: Guid.NewGuid(),
                                             LicensePlate: "TI-99999",
                                             VinChassisNumber: "VIN999",
                                             Brand: "Fiat",
                                             Model: "500",
                                             RegistrationYear: 2021,
                                             Jurisdiction: Jurisdiction.Switzerland,
                                             CurrentKilometers: 30000);

        _ = vehicle.LicensePlate.Should().Be(expected: "TI-99999");

        // DashboardKpiDto
        var kpi = new DashboardKpiDto(
                                      TotalRevenueEur: 1000m,
                                      TotalRevenueChf: 2000m,
                                      PendingReceivablesEur: 300m,
                                      PendingReceivablesChf: 400m,
                                      PendingVatLiabilityEur: 50m,
                                      PendingVatLiabilityChf: 60m,
                                      NextTaxDeadline: DateTime.UtcNow.AddDays(value: 15),
                                      ActiveClientsCount: 5,
                                      PendingInvoicesCount: 2,
                                      StoredTireSetsCount: 8);

        _ = kpi.TotalRevenueEur.Should().Be(expected: 1000m);

        // PurchaseOrderDto
        var po = new PurchaseOrderDto(
                                      Id: Guid.NewGuid(),
                                      TenantId: Guid.NewGuid(),
                                      OrderNumber: "PO-100",
                                      SupplierId: Guid.NewGuid(),
                                      SupplierName: "Supplier A",
                                      OrderDate: DateTime.UtcNow,
                                      TotalAmount: 500m,
                                      Currency: "EUR",
                                      Status: "Placed");

        _ = po.OrderNumber.Should().Be(expected: "PO-100");

        // SupplierDto
        var sup = new SupplierDto(
                                  Id: Guid.NewGuid(),
                                  TenantId: Guid.NewGuid(),
                                  Name: "Supplier A",
                                  Jurisdiction: Jurisdiction.Italy,
                                  VatNumber: "12345678901",
                                  SwissUid: null,
                                  SdiCode: "0000000",
                                  AddressFormatted: "Via Roma 1",
                                  Iban: null,
                                  PaymentTermsDays: 30,
                                  IsActive: true);

        _ = sup.Name.Should().Be(expected: "Supplier A");

        // TransportDocumentDto
        var ddt = new TransportDocumentDto(
                                           Id: Guid.NewGuid(),
                                           TenantId: Guid.NewGuid(),
                                           DocumentNumber: "DDT-1",
                                           DocumentDate: DateTime.UtcNow,
                                           ClientId: Guid.NewGuid(),
                                           ClientName: "Client B",
                                           DestinationAddressFormatted: "Via 2",
                                           TransportationReason: "Vendita",
                                           CarrierName: "Vettore",
                                           NumberOfPackages: 1,
                                           TotalWeightKg: 10m,
                                           CustomsCmrNotes: null);

        _ = ddt.DocumentNumber.Should().Be(expected: "DDT-1");
    }

}
