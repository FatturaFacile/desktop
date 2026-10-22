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
            tenantId,
            "admin",
            "admin@fatturafacile.it",
            "Administrator",
            "hashedPassword",
            "saltValue",
            UserRole.Admin);

        var userRepoMock = new Mock<IUserRepository>();
        _ = userRepoMock.Setup(r => r.GetByUsernameAsync(
                "admin",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var tenant = Tenant.Create(
            "ACME",
            "Acme Corporation",
            "IT",
            Currency.EUR,
            "info@acme.it");
        var tenantRepoMock = new Mock<ITenantRepository>();
        _ = tenantRepoMock.Setup(r => r.GetByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var tenantProviderMock = new Mock<ITenantProvider>();
        var currentUserMock = new Mock<ICurrentUserService>();
        var hasherMock = new Mock<IPasswordHasher>();
        _ = hasherMock.Setup(h => h.VerifyHash(
                "Secret123!",
                "hashedPassword",
                "saltValue"))
            .Returns(true);

        var handler = new LoginCommandHandler(
            userRepoMock.Object,
            tenantRepoMock.Object,
            tenantProviderMock.Object,
            currentUserMock.Object,
            hasherMock.Object,
            mapper);

        // Act
        var result = await handler.Handle(
            new LoginCommand(
                "admin",
                "Secret123!",
                tenantId),
            CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Username.Should().Be("admin");
        _ = result.Role.Should().Be(UserRole.Admin);
        tenantProviderMock.Verify(
            t => t.SetTenant(
                tenant.Id,
                "ACME"),
            Times.Once);
        currentUserMock.Verify(
            c => c.SetUser(
                user.Id,
                "admin",
                "Administrator",
                nameof(UserRole.Admin)),
            Times.Once);
    }

    /// <summary>
    /// Tests LoginCommandHandler with invalid tenant throws UnauthorizedAccessException.
    /// </summary>
    [Fact]
    public async Task LoginCommandHandler_NonExistentTenant_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var tenantRepoMock = new Mock<ITenantRepository>();
        _ = tenantRepoMock.Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var handler = new LoginCommandHandler(
            new Mock<IUserRepository>().Object,
            tenantRepoMock.Object,
            new Mock<ITenantProvider>().Object,
            new Mock<ICurrentUserService>().Object,
            new Mock<IPasswordHasher>().Object,
            mapper);

        // Act
        var act = async () =>
            await handler.Handle(
                new LoginCommand(
                    "admin",
                    "pwd",
                    Guid.NewGuid()),
                CancellationToken.None);

        // Assert
        _ = await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Selected tenant does not exist*");
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
            tenantId,
            "admin",
            "admin@test.it",
            "Admin",
            "hash",
            "salt",
            UserRole.Admin);

        var userRepoMock = new Mock<IUserRepository>();
        _ = userRepoMock.Setup(r => r.GetByUsernameAsync(
                "admin",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var tenantRepoMock = new Mock<ITenantRepository>();
        _ = tenantRepoMock.Setup(r => r.GetByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Tenant.Create(
                    "CODE",
                    "Company",
                    "IT",
                    Currency.EUR,
                    "info@company.it"));

        var hasherMock = new Mock<IPasswordHasher>();
        _ = hasherMock.Setup(h => h.VerifyHash(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(false);

        var handler = new LoginCommandHandler(
            userRepoMock.Object,
            tenantRepoMock.Object,
            new Mock<ITenantProvider>().Object,
            new Mock<ICurrentUserService>().Object,
            hasherMock.Object,
            mapper);

        // Act
        var act = async () =>
            await handler.Handle(
                new LoginCommand(
                    "admin",
                    "wrong_password",
                    tenantId),
                CancellationToken.None);

        // Assert
        _ = await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid credentials*");
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
            new LoginCommand(
                "admin",
                "pwd",
                Guid.NewGuid()));
        resultValid.ShouldNotHaveAnyValidationErrors();

        var resultInvalid = validator.TestValidate(
            new LoginCommand(
                "",
                "",
                Guid.Empty));
        _ = resultInvalid.ShouldHaveValidationErrorFor(c => c.Username);
        _ = resultInvalid.ShouldHaveValidationErrorFor(c => c.Password);
        _ = resultInvalid.ShouldHaveValidationErrorFor(c => c.TenantId);
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
            "NEW_TENANT",
            "New Company SA",
            "CH",
            Currency.CHF,
            "info@newcompany.ch");

        var tenantRepoMock = new Mock<ITenantRepository>();
        _ = tenantRepoMock.Setup(r => r.GetByIdAsync(
                targetId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var tenantProviderMock = new Mock<ITenantProvider>();
        var handler = new SwitchTenantCommandHandler(
            tenantRepoMock.Object,
            tenantProviderMock.Object,
            mapper);

        // Act
        var result = await handler.Handle(
            new SwitchTenantCommand(targetId),
            CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Code.Should().Be("NEW_TENANT");
        tenantProviderMock.Verify(
            p => p.SetTenant(
                tenant.Id,
                "NEW_TENANT"),
            Times.Once);
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
        _ = tenantProviderMock.SetupGet(t => t.CurrentTenantId).Returns(tenantId);

        var handler = new CreateClientCommandHandler(
            clientRepoMock.Object,
            tenantProviderMock.Object,
            mapper);
        var command = new CreateClientCommand(
            "Rossi Costruzioni Srl",
            Jurisdiction.Italy,
            "RSSMRA85M01H501Z",
            "12345678901",
            "M5UXCR1",
            "rossi@pec.it",
            null,
            null,
            null,
            "Via Garibaldi 10",
            "Milano",
            "20100",
            "MI",
            "IT",
            "info@rossi.it",
            "+39 02 9876543",
            30);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.BusinessName.Should().Be("Rossi Costruzioni Srl");
        _ = result.Jurisdiction.Should().Be(Jurisdiction.Italy);
        clientRepoMock.Verify(
            r => r.AddAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
        _ = tenantProviderMock.SetupGet(t => t.CurrentTenantId).Returns(Guid.NewGuid());

        var handler =
            new CreateItalianInvoiceCommandHandler(
                invoiceRepoMock.Object,
                tenantProviderMock.Object,
                mapper);
        var cedenteAddr = new Address(
            "Via Forze Armate 1",
            "Milano",
            "20152",
            "MI",
            "IT");
        var cessionarioAddr = new Address(
            "Corso Buenos Aires 10",
            "Milano",
            "20124",
            "MI",
            "IT");

        var lines = new List<CreateInvoiceLineItem>
        {
            new(
                "Sviluppo Modulo Contabilità",
                40m,
                90m,
                22m,
                null),
        };

        var command = new CreateItalianInvoiceCommand(
            "INV-2026-101",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "TechCedente SpA",
            "11111111111",
            "11111111111",
            cedenteAddr,
            "ClientCommittente Srl",
            "22222222222",
            "22222222222",
            "SUBM001",
            null,
            cessionarioAddr,
            DateTime.UtcNow.AddDays(30),
            lines);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.DocumentNumber.Should().Be("INV-2026-101");
        _ = result.TotalNet.Should().Be(3600m);
        invoiceRepoMock.Verify(
            r => r.AddAsync(
                It.IsAny<ItalianElectronicInvoice>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
        _ = tenantProviderMock.SetupGet(t => t.CurrentTenantId).Returns(Guid.NewGuid());

        var handler = new CreateSwissQrBillCommandHandler(
            billRepoMock.Object,
            tenantProviderMock.Object,
            mapper);
        var credAddr = new Address(
            "Via Nassa 1",
            "Lugano",
            "6900",
            "TI",
            "CH");
        var debtAddr = new Address(
            "Via Cantonale 5",
            "Mendrisio",
            "6850",
            "TI",
            "CH");

        var command = new CreateSwissQrBillCommand(
            "QR-2026-999",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "SwissCreditor SA",
            credAddr,
            "CH4431999123456789012",
            "SwissDebtor Sagl",
            debtAddr,
            1250.00m,
            "210000000003139471430009017",
            DateTime.UtcNow.AddDays(30),
            "Fattura servizi ottobre");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.BillNumber.Should().Be("QR-2026-999");
        _ = result.Amount.Should().Be(1250.00m);
        billRepoMock.Verify(
            r => r.AddAsync(
                It.IsAny<SwissQrBill>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
        _ = repoMock.Setup(r => r.GetActiveClockInAsync(
                employeeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeEntry?)null);

        var tenantProviderMock = new Mock<ITenantProvider>();
        _ = tenantProviderMock.SetupGet(t => t.CurrentTenantId).Returns(Guid.NewGuid());

        var handler = new ClockInCommandHandler(
            repoMock.Object,
            tenantProviderMock.Object,
            mapper);
        var command = new ClockInCommand(
            employeeId,
            RecordingType.Detailed,
            45.8,
            9.1,
            "BADGE-55");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.EmployeeId.Should().Be(employeeId);
        repoMock.Verify(
            r => r.AddAsync(
                It.IsAny<TimeEntry>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
            Guid.NewGuid(),
            employeeId,
            DateTime.UtcNow);

        var repoMock = new Mock<ITimeEntryRepository>();
        _ = repoMock.Setup(r => r.GetActiveClockInAsync(
                employeeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var handler = new ClockInCommandHandler(
            repoMock.Object,
            new Mock<ITenantProvider>().Object,
            mapper);

        // Act
        var act = async () => await handler.Handle(
            new ClockInCommand(employeeId),
            CancellationToken.None);

        // Assert
        _ = await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Employee already has an active clock-in session*");
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
            Guid.NewGuid(),
            employeeId,
            DateTime.UtcNow.AddHours(-8));

        var repoMock = new Mock<ITimeEntryRepository>();
        _ = repoMock.Setup(r => r.GetActiveClockInAsync(
                employeeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(active);

        var handler = new ClockOutCommandHandler(
            repoMock.Object,
            mapper);

        // Act
        var result = await handler.Handle(
            new ClockOutCommand(
                employeeId,
                45),
            CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.BreakDurationMinutes.Should().Be(45);
        _ = result.ClockOutUtc.Should().NotBeNull();
        repoMock.Verify(
            r => r.UpdateAsync(
                active,
                It.IsAny<CancellationToken>()),
            Times.Once);
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
        _ = tenantProviderMock.SetupGet(t => t.CurrentTenantId).Returns(Guid.NewGuid());

        var handler = new CreateTireRecordCommandHandler(
            repoMock.Object,
            tenantProviderMock.Object,
            mapper);
        var command = new CreateTireRecordCommand(
            Guid.NewGuid(),
            TireSeason.Summer,
            "Michelin",
            "225/45 R17",
            "1224",
            8.0m,
            8.0m,
            7.5m,
            7.5m,
            "RACK-A-01");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Brand.Should().Be("Michelin");
        _ = result.Status.Should().Be("InStorage");
        repoMock.Verify(
            r => r.AddAsync(
                It.IsAny<TireHotelRecord>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests UpdateTranslationCommandHandler calls localization service and returns Unit.
    /// </summary>
    [Fact]
    public async Task UpdateTranslationCommandHandler_ValidCommand_ShouldCallService()
    {
        // Arrange
        var locMock = new Mock<ILocalizationService>();
        var handler = new UpdateTranslationCommandHandler(locMock.Object);
        var command = new UpdateTranslationCommand(
            "it",
            "Dashboard.Welcome",
            "Benvenuto in FatturaFacile!");

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        _ = result.Should().Be(MediatR.Unit.Value);
        locMock.Verify(
            l => l.SaveCustomTranslationAsync(
                "it",
                "Dashboard.Welcome",
                "Benvenuto in FatturaFacile!",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests GetDashboardOverviewQueryHandler aggregates revenue and counts.
    /// </summary>
    [Fact]
    public async Task GetDashboardOverviewQueryHandler_ShouldAggregateKpis()
    {
        // Arrange
        var clientRepoMock = new Mock<IClientRepository>();
        _ = clientRepoMock.Setup(r => r.GetActiveClientsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                Client.CreateSwiss(
                    Guid.NewGuid(),
                    "Client A",
                    "CHE-123.456.789",
                    "TI",
                    null,
                    new Address(
                        "Via 1",
                        "Lugano",
                        "6900",
                        "TI",
                        "CH"),
                    "a@test.ch")
            ]);

        var itInvoiceRepoMock = new Mock<IItalianInvoiceRepository>();
        _ = itInvoiceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var chBillRepoMock = new Mock<ISwissQrBillRepository>();
        _ = chBillRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var tireRepoMock = new Mock<ITireRecordRepository>();
        _ = tireRepoMock.Setup(r => r.GetAllInStorageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetDashboardOverviewQueryHandler(
            clientRepoMock.Object,
            itInvoiceRepoMock.Object,
            chBillRepoMock.Object,
            tireRepoMock.Object);

        // Act
        var result = await handler.Handle(
            new GetDashboardOverviewQuery(),
            CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.ActiveClientsCount.Should().Be(1);
    }
}
