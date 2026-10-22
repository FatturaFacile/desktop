// <copyright file="ApplicationHandlerTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Administration;
using FatturaFacile.Application.Features.Automotive;
using FatturaFacile.Application.Features.Clients;
using FatturaFacile.Application.Features.Dashboard;
using FatturaFacile.Application.Features.Invoices;
using FatturaFacile.Application.Features.Localization;
using FatturaFacile.Application.Features.TimeTracking;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Repositories;
using FatturaFacile.Domain.ValueObjects;
using FluentValidation.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Comprehensive unit tests for Application layer CQRS command and query handlers.
/// </summary>
public sealed class ApplicationHandlerTests
{
    /// <summary>
    /// The AutoMapper instance configured for application tests.
    /// </summary>
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationHandlerTests"/> class.
    /// </summary>
    public ApplicationHandlerTests()
    {
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddApplication();
        using var sp = services.BuildServiceProvider();
        mapper = sp.GetRequiredService<IMapper>();
    }

    /// <summary>
    /// Tests LoginCommandHandler with valid credentials.
    /// </summary>
    [Fact]
    public async Task LoginCommandHandler_ValidCredentials_ShouldReturnUserDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user = User.Create(
            tenantId: tenantId,
            username: "admin",
            email: "admin@fatturafacile.it",
            fullName: "Administrator",
            passwordHash: "hashedPassword",
            salt: "saltValue",
            role: UserRole.Admin);

        var userRepoMock = new Mock<IUserRepository>();
        _ = userRepoMock.Setup(expression: r => r.GetByUsernameAsync(
                                                                     "admin",
                                                                     It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: user);

        var tenant = Tenant.Create(
            code: "ACME",
            businessName: "Acme Corporation",
            countryCode: "IT",
            baseCurrency: Currency.EUR,
            contactEmail: "info@acme.it");
        var tenantRepoMock = new Mock<ITenantRepository>();
        _ = tenantRepoMock.Setup(expression: r => r.GetByIdAsync(
                                                                 tenantId,
                                                                 It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: tenant);

        var tenantProviderMock = new Mock<ITenantProvider>();
        var currentUserMock = new Mock<ICurrentUserService>();
        var hasherMock = new Mock<IPasswordHasher>();
        _ = hasherMock.Setup(expression: h => h.VerifyHash(
                                                           "Secret123!",
                                                           "hashedPassword",
                                                           "saltValue"))
            .Returns(value: true);

        var handler = new LoginCommandHandler(
            userRepository: userRepoMock.Object,
            tenantRepository: tenantRepoMock.Object,
            tenantProvider: tenantProviderMock.Object,
            currentUserService: currentUserMock.Object,
            passwordHasher: hasherMock.Object,
            mapper: mapper);

        // Act
        var result = await handler.Handle(
            request: new LoginCommand(
                                      Username: "admin",
                                      Password: "Secret123!",
                                      TenantId: tenantId),
            cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Username.Should().Be(expected: "admin");
        _ = result.Role.Should().Be(expected: UserRole.Admin);
        tenantProviderMock.Verify(
            expression: t => t.SetTenant(
                                         tenant.Id,
                                         "ACME"),
            times: Times.Once);
        currentUserMock.Verify(
            expression: c => c.SetUser(
                                       user.Id,
                                       "admin",
                                       "Administrator",
                                       nameof(UserRole.Admin)),
            times: Times.Once);
    }

    /// <summary>
    /// Tests LoginCommandHandler with invalid tenant throws UnauthorizedAccessException.
    /// </summary>
    [Fact]
    public async Task LoginCommandHandler_NonExistentTenant_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var tenantRepoMock = new Mock<ITenantRepository>();
        _ = tenantRepoMock.Setup(expression: r => r.GetByIdAsync(
                                                                 It.IsAny<Guid>(),
                                                                 It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: (Tenant?)null);

        var handler = new LoginCommandHandler(
            userRepository: new Mock<IUserRepository>().Object,
            tenantRepository: tenantRepoMock.Object,
            tenantProvider: new Mock<ITenantProvider>().Object,
            currentUserService: new Mock<ICurrentUserService>().Object,
            passwordHasher: new Mock<IPasswordHasher>().Object,
            mapper: mapper);

        // Act
        var act = async () =>
            await handler.Handle(
                request: new LoginCommand(
                                          Username: "admin",
                                          Password: "pwd",
                                          TenantId: Guid.NewGuid()),
                cancellationToken: CancellationToken.None);

        // Assert
        _ = await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(expectedWildcardPattern: "*Selected tenant does not exist*");
    }

    /// <summary>
    /// Tests LoginCommandHandler with invalid password throws UnauthorizedAccessException.
    /// </summary>
    [Fact]
    public async Task LoginCommandHandler_InvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user = User.Create(
            tenantId: tenantId,
            username: "admin",
            email: "admin@test.it",
            fullName: "Admin",
            passwordHash: "hash",
            salt: "salt",
            role: UserRole.Admin);

        var userRepoMock = new Mock<IUserRepository>();
        _ = userRepoMock.Setup(expression: r => r.GetByUsernameAsync(
                                                                     "admin",
                                                                     It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: user);

        var tenantRepoMock = new Mock<ITenantRepository>();
        _ = tenantRepoMock.Setup(expression: r => r.GetByIdAsync(
                                                                 tenantId,
                                                                 It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                value: Tenant.Create(
                                     code: "CODE",
                                     businessName: "Company",
                                     countryCode: "IT",
                                     baseCurrency: Currency.EUR,
                                     contactEmail: "info@company.it"));

        var hasherMock = new Mock<IPasswordHasher>();
        _ = hasherMock.Setup(expression: h => h.VerifyHash(
                                                           It.IsAny<string>(),
                                                           It.IsAny<string>(),
                                                           It.IsAny<string>()))
            .Returns(value: false);

        var handler = new LoginCommandHandler(
            userRepository: userRepoMock.Object,
            tenantRepository: tenantRepoMock.Object,
            tenantProvider: new Mock<ITenantProvider>().Object,
            currentUserService: new Mock<ICurrentUserService>().Object,
            passwordHasher: hasherMock.Object,
            mapper: mapper);

        // Act
        var act = async () =>
            await handler.Handle(
                request: new LoginCommand(
                                          Username: "admin",
                                          Password: "wrong_password",
                                          TenantId: tenantId),
                cancellationToken: CancellationToken.None);

        // Assert
        _ = await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(expectedWildcardPattern: "*Invalid credentials*");
    }

    /// <summary>
    /// Tests LoginCommandValidator validation rules.
    /// </summary>
    [Fact]
    public void LoginCommandValidator_Rules_ShouldValidateProperly()
    {
        // Arrange
        var validator = new LoginCommandValidator();

        // Act & Assert
        var resultValid = validator.TestValidate(
            objectToTest: new LoginCommand(
                                           Username: "admin",
                                           Password: "pwd",
                                           TenantId: Guid.NewGuid()));
        resultValid.ShouldNotHaveAnyValidationErrors();

        var resultInvalid = validator.TestValidate(
            objectToTest: new LoginCommand(
                                           Username: "",
                                           Password: "",
                                           TenantId: Guid.Empty));
        _ = resultInvalid.ShouldHaveValidationErrorFor(memberAccessor: c => c.Username);
        _ = resultInvalid.ShouldHaveValidationErrorFor(memberAccessor: c => c.Password);
        _ = resultInvalid.ShouldHaveValidationErrorFor(memberAccessor: c => c.TenantId);
    }

    /// <summary>
    /// Tests SwitchTenantCommandHandler switching to a valid tenant.
    /// </summary>
    [Fact]
    public async Task SwitchTenantCommandHandler_ValidTenant_ShouldSetTenantAndReturnDto()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var tenant = Tenant.Create(
            code: "NEW_TENANT",
            businessName: "New Company SA",
            countryCode: "CH",
            baseCurrency: Currency.CHF,
            contactEmail: "info@newcompany.ch");

        var tenantRepoMock = new Mock<ITenantRepository>();
        _ = tenantRepoMock.Setup(expression: r => r.GetByIdAsync(
                                                                 targetId,
                                                                 It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: tenant);

        var tenantProviderMock = new Mock<ITenantProvider>();
        var handler = new SwitchTenantCommandHandler(
            tenantRepository: tenantRepoMock.Object,
            tenantProvider: tenantProviderMock.Object,
            mapper: mapper);

        // Act
        var result = await handler.Handle(
            request: new SwitchTenantCommand(TargetTenantId: targetId),
            cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Code.Should().Be(expected: "NEW_TENANT");
        tenantProviderMock.Verify(
            expression: p => p.SetTenant(
                                         tenant.Id,
                                         "NEW_TENANT"),
            times: Times.Once);
    }

    /// <summary>
    /// Tests CreateClientCommandHandler creating an Italian client.
    /// </summary>
    [Fact]
    public async Task CreateClientCommandHandler_ItalianClient_ShouldPersistAndReturnDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientRepoMock = new Mock<IClientRepository>();
        var tenantProviderMock = new Mock<ITenantProvider>();
        _ = tenantProviderMock.SetupGet(expression: t => t.CurrentTenantId).Returns(value: tenantId);

        var handler = new CreateClientCommandHandler(
            clientRepository: clientRepoMock.Object,
            tenantProvider: tenantProviderMock.Object,
            mapper: mapper);
        var command = new CreateClientCommand(
            BusinessName: "Rossi Costruzioni Srl",
            Jurisdiction: Jurisdiction.Italy,
            FiscalCode: "RSSMRA85M01H501Z",
            VatNumber: "12345678901",
            SdiCode: "M5UXCR1",
            PecEmail: "rossi@pec.it",
            SwissUid: null,
            QrIban: null,
            CantonalCode: null,
            Street: "Via Garibaldi 10",
            City: "Milano",
            PostalCode: "20100",
            ProvinceOrCanton: "MI",
            CountryCode: "IT",
            Email: "info@rossi.it",
            PhoneNumber: "+39 02 9876543",
            PaymentTermDays: 30);

        // Act
        var result = await handler.Handle(
            request: command,
            cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.BusinessName.Should().Be(expected: "Rossi Costruzioni Srl");
        _ = result.Jurisdiction.Should().Be(expected: Jurisdiction.Italy);
        clientRepoMock.Verify(
            expression: r => r.AddAsync(
                                        It.IsAny<Client>(),
                                        It.IsAny<CancellationToken>()),
            times: Times.Once);
    }

    /// <summary>
    /// Tests CreateItalianInvoiceCommandHandler creates invoice and calculates totals.
    /// </summary>
    [Fact]
    public async Task CreateItalianInvoiceCommandHandler_ValidInvoice_ShouldAddLinesAndSave()
    {
        // Arrange
        var invoiceRepoMock = new Mock<IItalianInvoiceRepository>();
        var tenantProviderMock = new Mock<ITenantProvider>();
        _ = tenantProviderMock.SetupGet(expression: t => t.CurrentTenantId).Returns(value: Guid.NewGuid());

        var handler =
            new CreateItalianInvoiceCommandHandler(
                repository: invoiceRepoMock.Object,
                tenantProvider: tenantProviderMock.Object,
                mapper: mapper);
        var cedenteAddr = new Address(
            street: "Via Forze Armate 1",
            city: "Milano",
            postalCode: "20152",
            provinceOrCanton: "MI",
            countryCode: "IT");
        var cessionarioAddr = new Address(
            street: "Corso Buenos Aires 10",
            city: "Milano",
            postalCode: "20124",
            provinceOrCanton: "MI",
            countryCode: "IT");

        var lines = new List<CreateInvoiceLineItem>
        {
            new(
                Description: "Sviluppo Modulo Contabilità",
                Quantity: 40m,
                UnitPrice: 90m,
                VatRate: 22m,
                Natura: null),
        };

        var command = new CreateItalianInvoiceCommand(
            DocumentNumber: "INV-2026-101",
            DocumentDate: DateTime.UtcNow,
            ClientId: Guid.NewGuid(),
            CedenteName: "TechCedente SpA",
            CedenteVat: "11111111111",
            CedenteCf: "11111111111",
            CedenteAddress: cedenteAddr,
            CessionarioName: "ClientCommittente Srl",
            CessionarioVat: "22222222222",
            CessionarioCf: "22222222222",
            SdiCode: "SUBM001",
            Pec: null,
            CessionarioAddress: cessionarioAddr,
            DueDate: DateTime.UtcNow.AddDays(value: 30),
            Lines: lines);

        // Act
        var result = await handler.Handle(
            request: command,
            cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.DocumentNumber.Should().Be(expected: "INV-2026-101");
        _ = result.TotalNet.Should().Be(expected: 3600m);
        invoiceRepoMock.Verify(
            expression: r => r.AddAsync(
                                        It.IsAny<ItalianElectronicInvoice>(),
                                        It.IsAny<CancellationToken>()),
            times: Times.Once);
    }

    /// <summary>
    /// Tests CreateSwissQrBillCommandHandler creates bill and saves.
    /// </summary>
    [Fact]
    public async Task CreateSwissQrBillCommandHandler_ValidBill_ShouldCreateAndSave()
    {
        // Arrange
        var billRepoMock = new Mock<ISwissQrBillRepository>();
        var tenantProviderMock = new Mock<ITenantProvider>();
        _ = tenantProviderMock.SetupGet(expression: t => t.CurrentTenantId).Returns(value: Guid.NewGuid());

        var handler = new CreateSwissQrBillCommandHandler(
            repository: billRepoMock.Object,
            tenantProvider: tenantProviderMock.Object,
            mapper: mapper);
        var credAddr = new Address(
            street: "Via Nassa 1",
            city: "Lugano",
            postalCode: "6900",
            provinceOrCanton: "TI",
            countryCode: "CH");
        var debtAddr = new Address(
            street: "Via Cantonale 5",
            city: "Mendrisio",
            postalCode: "6850",
            provinceOrCanton: "TI",
            countryCode: "CH");

        var command = new CreateSwissQrBillCommand(
            BillNumber: "QR-2026-999",
            BillDate: DateTime.UtcNow,
            ClientId: Guid.NewGuid(),
            CreditorName: "SwissCreditor SA",
            CreditorAddress: credAddr,
            CreditorQrIban: "CH4431999123456789012",
            DebtorName: "SwissDebtor Sagl",
            DebtorAddress: debtAddr,
            AmountChf: 1250.00m,
            ReferenceNumber: "210000000003139471430009017",
            DueDate: DateTime.UtcNow.AddDays(value: 30),
            Message: "Fattura servizi ottobre");

        // Act
        var result = await handler.Handle(
            request: command,
            cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.BillNumber.Should().Be(expected: "QR-2026-999");
        _ = result.Amount.Should().Be(expected: 1250.00m);
        billRepoMock.Verify(
            expression: r => r.AddAsync(
                                        It.IsAny<SwissQrBill>(),
                                        It.IsAny<CancellationToken>()),
            times: Times.Once);
    }

    /// <summary>
    /// Tests ClockInCommandHandler clocks in employee and returns DTO.
    /// </summary>
    [Fact]
    public async Task ClockInCommandHandler_ValidClockIn_ShouldCreateTimeEntry()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var repoMock = new Mock<ITimeEntryRepository>();
        _ = repoMock.Setup(expression: r => r.GetActiveClockInAsync(
                                                                    employeeId,
                                                                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: (TimeEntry?)null);

        var tenantProviderMock = new Mock<ITenantProvider>();
        _ = tenantProviderMock.SetupGet(expression: t => t.CurrentTenantId).Returns(value: Guid.NewGuid());

        var handler = new ClockInCommandHandler(
            repository: repoMock.Object,
            tenantProvider: tenantProviderMock.Object,
            mapper: mapper);
        var command = new ClockInCommand(
            EmployeeId: employeeId,
            RecordingType: RecordingType.Detailed,
            Latitude: 45.8,
            Longitude: 9.1,
            BadgeSerial: "BADGE-55");

        // Act
        var result = await handler.Handle(
            request: command,
            cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.EmployeeId.Should().Be(expected: employeeId);
        repoMock.Verify(
            expression: r => r.AddAsync(
                                        It.IsAny<TimeEntry>(),
                                        It.IsAny<CancellationToken>()),
            times: Times.Once);
    }

    /// <summary>
    /// Tests ClockInCommandHandler throws InvalidOperationException when employee is already clocked in.
    /// </summary>
    [Fact]
    public async Task ClockInCommandHandler_AlreadyClockedIn_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var existing = TimeEntry.ClockIn(
            tenantId: Guid.NewGuid(),
            employeeId: employeeId,
            clockInUtc: DateTime.UtcNow);

        var repoMock = new Mock<ITimeEntryRepository>();
        _ = repoMock.Setup(expression: r => r.GetActiveClockInAsync(
                                                                    employeeId,
                                                                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: existing);

        var handler = new ClockInCommandHandler(
            repository: repoMock.Object,
            tenantProvider: new Mock<ITenantProvider>().Object,
            mapper: mapper);

        // Act
        var act = async () => await handler.Handle(
            request: new ClockInCommand(EmployeeId: employeeId),
            cancellationToken: CancellationToken.None);

        // Assert
        _ = await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(expectedWildcardPattern: "*Employee already has an active clock-in session*");
    }

    /// <summary>
    /// Tests ClockOutCommandHandler clocks out employee session.
    /// </summary>
    [Fact]
    public async Task ClockOutCommandHandler_ActiveSession_ShouldClockOutAndReturnDto()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var active = TimeEntry.ClockIn(
            tenantId: Guid.NewGuid(),
            employeeId: employeeId,
            clockInUtc: DateTime.UtcNow.AddHours(value: -8));

        var repoMock = new Mock<ITimeEntryRepository>();
        _ = repoMock.Setup(expression: r => r.GetActiveClockInAsync(
                                                                    employeeId,
                                                                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: active);

        var handler = new ClockOutCommandHandler(
            repository: repoMock.Object,
            mapper: mapper);

        // Act
        var result = await handler.Handle(
            request: new ClockOutCommand(
                                         EmployeeId: employeeId,
                                         BreakMinutes: 45),
            cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.BreakDurationMinutes.Should().Be(expected: 45);
        _ = result.ClockOutUtc.Should().NotBeNull();
        repoMock.Verify(
            expression: r => r.UpdateAsync(
                                           active,
                                           It.IsAny<CancellationToken>()),
            times: Times.Once);
    }

    /// <summary>
    /// Tests CreateTireRecordCommandHandler checks in tire record and returns DTO.
    /// </summary>
    [Fact]
    public async Task CreateTireRecordCommandHandler_ValidRecord_ShouldAddAndReturnDto()
    {
        // Arrange
        var repoMock = new Mock<ITireRecordRepository>();
        var tenantProviderMock = new Mock<ITenantProvider>();
        _ = tenantProviderMock.SetupGet(expression: t => t.CurrentTenantId).Returns(value: Guid.NewGuid());

        var handler = new CreateTireRecordCommandHandler(
            repository: repoMock.Object,
            tenantProvider: tenantProviderMock.Object,
            mapper: mapper);
        var command = new CreateTireRecordCommand(
            VehicleId: Guid.NewGuid(),
            Season: TireSeason.Summer,
            Brand: "Michelin",
            Dimension: "225/45 R17",
            DotCode: "1224",
            TreadDepthFrontLeftMm: 8.0m,
            TreadDepthFrontRightMm: 8.0m,
            TreadDepthRearLeftMm: 7.5m,
            TreadDepthRearRightMm: 7.5m,
            WarehouseRackCoordinate: "RACK-A-01");

        // Act
        var result = await handler.Handle(
            request: command,
            cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Brand.Should().Be(expected: "Michelin");
        _ = result.Status.Should().Be(expected: "InStorage");
        repoMock.Verify(
            expression: r => r.AddAsync(
                                        It.IsAny<TireHotelRecord>(),
                                        It.IsAny<CancellationToken>()),
            times: Times.Once);
    }

    /// <summary>
    /// Tests UpdateTranslationCommandHandler calls localization service and returns Unit.
    /// </summary>
    [Fact]
    public async Task UpdateTranslationCommandHandler_ValidCommand_ShouldCallService()
    {
        // Arrange
        var locMock = new Mock<ILocalizationService>();
        var handler = new UpdateTranslationCommandHandler(localizationService: locMock.Object);
        var command = new UpdateTranslationCommand(
            Locale: "it",
            Key: "Dashboard.Welcome",
            CustomValue: "Benvenuto in FatturaFacile!");

        // Act
        var result = await handler.Handle(
            request: command,
            cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().Be(expected: MediatR.Unit.Value);
        locMock.Verify(
            expression: l => l.SaveCustomTranslationAsync(
                                                          "it",
                                                          "Dashboard.Welcome",
                                                          "Benvenuto in FatturaFacile!",
                                                          It.IsAny<CancellationToken>()),
            times: Times.Once);
    }

    /// <summary>
    /// Tests GetDashboardOverviewQueryHandler aggregates revenue and counts.
    /// </summary>
    [Fact]
    public async Task GetDashboardOverviewQueryHandler_ShouldAggregateKpis()
    {
        // Arrange
        var clientRepoMock = new Mock<IClientRepository>();
        _ = clientRepoMock.Setup(expression: r => r.GetActiveClientsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            value:
            [
                Client.CreateSwiss(
                    tenantId: Guid.NewGuid(),
                    businessName: "Client A",
                    swissUid: "CHE-123.456.789",
                    cantonalCode: "TI",
                    qrIban: null,
                    address: new Address(
                                         street: "Via 1",
                                         city: "Lugano",
                                         postalCode: "6900",
                                         provinceOrCanton: "TI",
                                         countryCode: "CH"),
                    email: "a@test.ch")
            ]);

        var itInvoiceRepoMock = new Mock<IItalianInvoiceRepository>();
        _ = itInvoiceRepoMock.Setup(expression: r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: []);

        var chBillRepoMock = new Mock<ISwissQrBillRepository>();
        _ = chBillRepoMock.Setup(expression: r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: []);

        var tireRepoMock = new Mock<ITireRecordRepository>();
        _ = tireRepoMock.Setup(expression: r => r.GetAllInStorageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: []);

        var handler = new GetDashboardOverviewQueryHandler(
            clientRepo: clientRepoMock.Object,
            itInvoiceRepo: itInvoiceRepoMock.Object,
            chBillRepo: chBillRepoMock.Object,
            tireRepo: tireRepoMock.Object);

        // Act
        var result = await handler.Handle(
            request: new GetDashboardOverviewQuery(),
            cancellationToken: CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.ActiveClientsCount.Should().Be(expected: 1);
    }
}
