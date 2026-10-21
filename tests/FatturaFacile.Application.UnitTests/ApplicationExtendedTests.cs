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
    private readonly Mock<ITenantProvider> tenantProviderMock = new();

    /// <summary>
    /// Tests that the GetTimeEntriesQueryHandler queries the repository and maps entities to DTOs.
    /// </summary>
    [Fact]
    public async Task GetTimeEntriesQueryHandler_WhenHandled_ReturnsMappedDtos()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var empId = Guid.NewGuid();
        _ = tenantProviderMock.Setup(p => p.CurrentTenantId).Returns(tenantId);

        var repoMock = new Mock<ITimeEntryRepository>();
        var entry = TimeEntry.ClockIn(
            tenantId,
            empId,
            DateTime.UtcNow.AddHours(-8));
        entry.ClockOut(DateTime.UtcNow);

        _ = repoMock.Setup(r => r.GetEntriesByEmployeeAndDateRangeAsync(
                empId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([entry]);

        var mapperMock = new Mock<IMapper>();
        var expectedDtos = new List<TimeEntryDto>
        {
            new(
                Guid.NewGuid(),
                tenantId,
                empId,
                "Emp 1",
                DateTime.UtcNow.AddHours(-8),
                DateTime.UtcNow,
                0,
                8m,
                RecordingType.Detailed,
                null,
                null),
        };
        _ = mapperMock.Setup(m => m.Map<IReadOnlyList<TimeEntryDto>>(It.IsAny<object>()))
            .Returns(expectedDtos);

        var handler = new GetTimeEntriesQueryHandler(
            repoMock.Object,
            mapperMock.Object);
        var query = new GetTimeEntriesQuery(empId);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].EmployeeId.Should().Be(empId);
        result[0].TotalNetHours.Should().Be(8m);
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
                Guid.NewGuid(),
                Guid.NewGuid(),
                "it",
                "Common.Save",
                "Salva",
                null,
                "Salva",
                DateTime.UtcNow),
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "it",
                "Common.Cancel",
                "Annulla",
                null,
                "Annulla",
                DateTime.UtcNow),
        };
        _ = locServiceMock.Setup(s => s.GetAllTranslationsAsync(
                "it",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(translations);

        var handler = new GetTranslationsQueryHandler(locServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new GetTranslationsQuery("it"),
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Key.Should().Be("Common.Save");
    }

    /// <summary>
    /// Tests that GetInvoicesOverviewQueryHandler aggregates Italian invoices and Swiss QR bills.
    /// </summary>
    [Fact]
    public async Task GetInvoicesOverviewQueryHandler_WhenHandled_ReturnsAggregatedInvoices()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _ = tenantProviderMock.Setup(p => p.CurrentTenantId).Returns(tenantId);

        var itRepoMock = new Mock<IItalianInvoiceRepository>();
        var itInvoice = ItalianElectronicInvoice.Create(
            tenantId,
            "IT-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Cedente Srl",
            "12345678901",
            "12345678901",
            new Address(
                "Via Roma 1",
                "Milano",
                "20100",
                "MI",
                "IT"),
            "Cessionario Spa",
            "98765432101",
            "RSSMRA80A01F205X",
            "SUBM170",
            "dest@pec.it",
            new Address(
                "Via Milano 2",
                "Roma",
                "00100",
                "RM",
                "IT"),
            DateTime.UtcNow.AddDays(30));
        itInvoice.AddLine(
            "Consulting",
            1,
            1000m,
            22.0m);
        _ = itRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([itInvoice]);

        var chRepoMock = new Mock<ISwissQrBillRepository>();
        var chBill = SwissQrBill.Create(
            tenantId,
            "QR-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Gotthard AG",
            new Address(
                "Via San Gottardo",
                "Lugano",
                "6900",
                "TI",
                "CH"),
            new QrIban("CH4431999123000889012"),
            "Client SA",
            new Address(
                "Via Nassa",
                "Lugano",
                "6900",
                "TI",
                "CH"),
            Money.Chf(1500m),
            "210000000003139471430009017",
            DateTime.UtcNow.AddDays(30));
        _ = chRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([chBill]);

        var mapperMock = new Mock<IMapper>();
        _ = mapperMock.Setup(m => m.Map<IReadOnlyList<ItalianInvoiceDto>>(It.IsAny<object>()))
            .Returns(
            [
                new ItalianInvoiceDto(
                    Guid.NewGuid(),
                    tenantId,
                    "IT-001",
                    DateTime.UtcNow,
                    "FPA12",
                    "00001",
                    Guid.NewGuid(),
                    "Cedente",
                    "123",
                    "Cess",
                    null,
                    "0000000",
                    1000m,
                    220m,
                    1220m,
                    "EUR",
                    DateTime.UtcNow.AddDays(30),
                    SdiTransmissionStatus.Draft,
                    null,
                    [])
            ]);
        _ = mapperMock.Setup(m => m.Map<IReadOnlyList<SwissQrBillDto>>(It.IsAny<object>()))
            .Returns(
            [
                new SwissQrBillDto(
                    Guid.NewGuid(),
                    tenantId,
                    "QR-001",
                    DateTime.UtcNow,
                    Guid.NewGuid(),
                    "Gotthard AG",
                    "Lugano",
                    "CH44",
                    "Client",
                    "Lugano",
                    1500m,
                    "CHF",
                    "2100",
                    null,
                    DateTime.UtcNow.AddDays(30),
                    false)
            ]);

        var handler = new GetInvoicesOverviewQueryHandler(
            itRepoMock.Object,
            chRepoMock.Object,
            mapperMock.Object);

        // Act
        var result = await handler.Handle(
            new GetInvoicesOverviewQuery(),
            CancellationToken.None);

        // Assert
        result.ItalianInvoices.Should().HaveCount(1);
        result.ItalianInvoices[0].DocumentNumber.Should().Be("IT-001");
        result.SwissQrBills.Should().HaveCount(1);
        result.SwissQrBills[0].BillNumber.Should().Be("QR-001");
    }

    /// <summary>
    /// Tests that GetClientsQueryHandler fetches active clients and maps to DTOs.
    /// </summary>
    [Fact]
    public async Task GetClientsQueryHandler_WhenHandled_ReturnsClientDtos()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _ = tenantProviderMock.Setup(p => p.CurrentTenantId).Returns(tenantId);

        var clientRepoMock = new Mock<IClientRepository>();
        var client = Client.CreateItalian(
            tenantId,
            "Alpha Corp Srl",
            new FiscalCode("RSSMRA80A01F205X"),
            new VatNumber(
                "12345678901",
                "IT"),
            "M5UXCR1",
            "alpha@pec.it",
            new Address(
                "Via Lavoratori 1",
                "Monza",
                "20900",
                "MB",
                "IT"),
            "info@alphacorp.it",
            "+39 039 000000");
        _ = clientRepoMock.Setup(r => r.GetActiveClientsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([client]);

        var mapperMock = new Mock<IMapper>();
        _ = mapperMock.Setup(m => m.Map<IReadOnlyList<ClientDto>>(It.IsAny<object>()))
            .Returns(
            [
                new ClientDto(
                    Guid.NewGuid(),
                    tenantId,
                    "Alpha Corp Srl",
                    null,
                    Jurisdiction.Italy,
                    "RSSMRA80A01F205X",
                    "12345678901",
                    "M5UXCR1",
                    "alpha@pec.it",
                    null,
                    null,
                    null,
                    "Via Lavoratori 1",
                    "info@alphacorp.it",
                    "+39",
                    30,
                    1000m,
                    "EUR",
                    true)
            ]);

        var handler = new GetClientsQueryHandler(
            clientRepoMock.Object,
            mapperMock.Object);

        // Act
        var result = await handler.Handle(
            new GetClientsQuery(),
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].BusinessName.Should().Be("Alpha Corp Srl");
        result[0].SdiCode.Should().Be("M5UXCR1");
    }

    /// <summary>
    /// Tests that GetTireRecordsQueryHandler returns all in-storage tire hotel records.
    /// </summary>
    [Fact]
    public async Task GetTireRecordsQueryHandler_WhenHandled_ReturnsTireHotelDtos()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        _ = tenantProviderMock.Setup(p => p.CurrentTenantId).Returns(tenantId);

        var tireRepoMock = new Mock<ITireRecordRepository>();
        var record = TireHotelRecord.CheckIn(
            tenantId,
            vehicleId,
            TireSeason.Summer,
            "Pirelli P Zero",
            "245/40 R18",
            "1224",
            6.5m,
            6.5m,
            6.2m,
            6.2m,
            "SHELF-3A");
        _ = tireRepoMock.Setup(r => r.GetAllInStorageAsync(It.IsAny<CancellationToken>())).ReturnsAsync([record]);

        var mapperMock = new Mock<IMapper>();
        _ = mapperMock.Setup(m => m.Map<IReadOnlyList<TireHotelDto>>(It.IsAny<object>()))
            .Returns(
            [
                new TireHotelDto(
                    Guid.NewGuid(),
                    tenantId,
                    vehicleId,
                    "AB123CD",
                    TireSeason.Summer,
                    "Pirelli P Zero",
                    "245/40 R18",
                    "1224",
                    6.5m,
                    6.5m,
                    6.2m,
                    6.2m,
                    "SHELF-3A",
                    "InStorage",
                    DateTime.UtcNow)
            ]);

        var handler = new GetTireRecordsQueryHandler(
            tireRepoMock.Object,
            mapperMock.Object);

        // Act
        var result = await handler.Handle(
            new GetTireRecordsQuery(),
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].WarehouseRackCoordinate.Should().Be("SHELF-3A");
        result[0].Brand.Should().Be("Pirelli P Zero");
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
            "TENANT-01",
            "Main Tenant",
            "IT",
            Currency.EUR,
            "admin@main.it",
            vatNumber: "12345678901");
        _ = tenantRepoMock.Setup(r => r.GetActiveTenantsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([tenant]);

        var mapperMock = new Mock<IMapper>();
        _ = mapperMock.Setup(m => m.Map<IReadOnlyList<TenantDto>>(It.IsAny<object>()))
            .Returns(
            [
                new TenantDto(
                    Guid.NewGuid(),
                    "TENANT-01",
                    "Main Tenant",
                    "IT",
                    Currency.EUR,
                    "12345678901",
                    null,
                    null,
                    null,
                    null,
                    "admin@main.it",
                    true)
            ]);

        var handler = new GetTenantsQueryHandler(
            tenantRepoMock.Object,
            mapperMock.Object);

        // Act
        var result = await handler.Handle(
            new GetTenantsQuery(),
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Code.Should().Be("TENANT-01");
        result[0].BusinessName.Should().Be("Main Tenant");
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
        var invalidResult = await validator.ValidateAsync(invalidCommand);
        var validItResult = await validator.ValidateAsync(validItCommand);
        var validChResult = await validator.ValidateAsync(validChCommand);

        // Assert
        invalidResult.IsValid.Should().BeFalse();
        invalidResult.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientCommand.BusinessName));
        invalidResult.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClientCommand.Email));

        validItResult.IsValid.Should().BeTrue();
        validChResult.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that all application DTO records preserve values and equality contracts.
    /// </summary>
    [Fact]
    public void ApplicationDtos_ConstructAndExposeProperties()
    {
        // AbsenceDto
        var absence = new AbsenceDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            AbsenceType.Ferie,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2),
            2m,
            true,
            "MED-1");
        absence.TotalWorkingDays.Should().Be(2m);

        // CustomerVehicleDto
        var vehicle = new CustomerVehicleDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TI-99999",
            "VIN999",
            "Fiat",
            "500",
            2021,
            Jurisdiction.Switzerland,
            30000);
        vehicle.LicensePlate.Should().Be("TI-99999");

        // DashboardKpiDto
        var kpi = new DashboardKpiDto(
            1000m,
            2000m,
            300m,
            400m,
            50m,
            60m,
            DateTime.UtcNow.AddDays(15),
            5,
            2,
            8);
        kpi.TotalRevenueEur.Should().Be(1000m);

        // PurchaseOrderDto
        var po = new PurchaseOrderDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "PO-100",
            Guid.NewGuid(),
            "Supplier A",
            DateTime.UtcNow,
            500m,
            "EUR",
            "Placed");
        po.OrderNumber.Should().Be("PO-100");

        // SupplierDto
        var sup = new SupplierDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Supplier A",
            Jurisdiction.Italy,
            "12345678901",
            null,
            "0000000",
            "Via Roma 1",
            null,
            30,
            true);
        sup.Name.Should().Be("Supplier A");

        // TransportDocumentDto
        var ddt = new TransportDocumentDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "DDT-1",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Client B",
            "Via 2",
            "Vendita",
            "Vettore",
            1,
            10m,
            null);
        ddt.DocumentNumber.Should().Be("DDT-1");
    }
}
