// <copyright file="InfrastructureExtendedTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Purchasing;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;
using FatturaFacile.Infrastructure.CrossBorder.Iso20022;
using FatturaFacile.Infrastructure.Localization;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Persistence.Repositories;
using FatturaFacile.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Contains extended integration and unit tests for Infrastructure persistence, security, cross-border parsers, and localization.
/// </summary>
public sealed class InfrastructureExtendedTests
{
    /// <summary>
    /// Tests the multi-tenant query filter on FatturaFacileDbContext.
    /// </summary>
    [Fact]
    public async Task DbContext_MultiTenantQueryFilter_HidesOtherTenantData()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseInMemoryDatabase(databaseName: "TenantFilterDb")
            .Options;

        var tenantProviderMock = new Mock<ITenantProvider>();
        _ = tenantProviderMock.Setup(t => t.CurrentTenantId).Returns(tenantA);

        var userMock = new Mock<ICurrentUserService>();

        await using var context = new FatturaFacileDbContext(
            options,
            tenantProviderMock.Object,
            userMock.Object);

        var clientA = Client.CreateItalian(
            tenantA,
            "Client A",
            new FiscalCode("RSSMRA80A01F205X"),
            new VatNumber(
                "12345678901",
                "IT"),
            "0000000",
            "a@pec.it",
            new Address(
                "Via A",
                "Milano",
                "20100",
                "MI",
                "IT"),
            "a@client.it");

        var clientB = Client.CreateItalian(
            tenantB,
            "Client B",
            new FiscalCode("RSSMRA80A01F205X"),
            new VatNumber(
                "12345678902",
                "IT"),
            "0000000",
            "b@pec.it",
            new Address(
                "Via B",
                "Milano",
                "20100",
                "MI",
                "IT"),
            "b@client.it");

        context.Clients.AddRange(
            clientA,
            clientB);
        _ = await context.SaveChangesAsync();

        // Act
        var visibleClients = await context.Clients.ToListAsync();

        // Assert
        visibleClients.Should().HaveCount(1);
        visibleClients[0].BusinessName.Should().Be("Client A");
    }

    private static FatturaFacileDbContext CreateInMemoryDbContext(
        Guid tenantId,
        string username = "TestUser",
        string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var tenantMock = new Mock<ITenantProvider>();
        _ = tenantMock.Setup(t => t.CurrentTenantId).Returns(tenantId);

        var userMock = new Mock<ICurrentUserService>();
        _ = userMock.Setup(u => u.Username).Returns(username);

        return new FatturaFacileDbContext(
            options,
            tenantMock.Object,
            userMock.Object);
    }

    /// <summary>
    /// Tests that SaveChangesAsync automatically sets audit properties.
    /// </summary>
    [Fact]
    public async Task DbContext_SaveChangesAsync_SetsAuditProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        await using var context = CreateInMemoryDbContext(
            tenantId,
            "AuditorUser");

        var client = Client.CreateItalian(
            tenantId,
            "Audit Client",
            new FiscalCode("RSSMRA80A01F205X"),
            new VatNumber(
                "12345678901",
                "IT"),
            "0000000",
            "audit@pec.it",
            new Address(
                "Via C",
                "Roma",
                "00100",
                "RM",
                "IT"),
            "audit@client.it");

        // Act
        _ = context.Clients.Add(client);
        _ = await context.SaveChangesAsync();

        // Assert
        client.CreatedBy.Should().Be("AuditorUser");
        client.CreatedAtUtc.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));

        // Act 2 - Update
        client.Deactivate();
        _ = await context.SaveChangesAsync();

        // Assert 2
        client.UpdatedBy.Should().Be("AuditorUser");
        client.UpdatedAtUtc.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that DbInitializer.SeedAsync seeds initial tenants, users, and documents, and is idempotent.
    /// </summary>
    [Fact]
    public async Task DbInitializer_SeedAsync_SeedsDataAndIsIdempotent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        await using var context = CreateInMemoryDbContext(
            tenantId,
            "System");
        var hasher = new Pbkdf2PasswordHasher();

        // Act
        await DbInitializer.SeedAsync(
            context,
            hasher);

        // Assert
        var tenantsCount = await context.Tenants.CountAsync();
        tenantsCount.Should().Be(2);

        var usersCount = await context.Users.IgnoreQueryFilters().CountAsync();
        usersCount.Should().Be(1);

        // Act 2 - Call seed again (idempotency test)
        await DbInitializer.SeedAsync(
            context,
            hasher);

        // Assert 2
        var tenantsCountAfter = await context.Tenants.CountAsync();
        tenantsCountAfter.Should().Be(2);
    }

    /// <summary>
    /// Tests repository query operations against in-memory context.
    /// </summary>
    [Fact]
    public async Task Repositories_CrudAndSpecializedQueries_ExecuteSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        await using var context = CreateInMemoryDbContext(
            tenantId,
            "RepoUser");

        var clientRepo = new ClientRepository(context);
        var invoiceRepo = new ItalianInvoiceRepository(context);
        var qrBillRepo = new SwissQrBillRepository(context);
        var tenantRepo = new TenantRepository(context);
        var timeRepo = new TimeEntryRepository(context);
        var tireRepo = new TireRecordRepository(context);
        var transRepo = new TranslationRepository(context);
        var userRepo = new UserRepository(context);

        // Clients
        var client = Client.CreateItalian(
            tenantId,
            "Client Repo Test",
            new FiscalCode("RSSMRA80A01F205X"),
            new VatNumber(
                "12345678901",
                "IT"),
            "0000000",
            "repo@pec.it",
            new Address(
                "Via R",
                "Monza",
                "20900",
                "MB",
                "IT"),
            "repo@client.it");
        await clientRepo.AddAsync(client);
        _ = await context.SaveChangesAsync();

        var activeClients = await clientRepo.GetActiveClientsAsync();
        activeClients.Should().HaveCount(1);

        // Invoices
        var invoice = ItalianElectronicInvoice.Create(
            tenantId,
            "INV-REPO-1",
            DateTime.UtcNow,
            client.Id,
            "Cedente",
            "12345678901",
            "12345678901",
            new Address(
                "Via C",
                "Milano",
                "20100",
                "MI",
                "IT"),
            "Cessionario",
            "98765432101",
            "RSSMRA80A01F205X",
            "0000000",
            "ces@pec.it",
            new Address(
                "Via D",
                "Roma",
                "00100",
                "RM",
                "IT"),
            DateTime.UtcNow.AddDays(30));
        await invoiceRepo.AddAsync(invoice);
        _ = await context.SaveChangesAsync();

        var pendingSdi = await invoiceRepo.GetPendingSdiAsync();
        pendingSdi.Should().HaveCount(1);

        // QR Bills
        var qrBill = SwissQrBill.Create(
            tenantId,
            "QR-REPO-1",
            DateTime.UtcNow,
            client.Id,
            "Gotthard AG",
            new Address(
                "Via S",
                "Lugano",
                "6900",
                "TI",
                "CH"),
            new QrIban("CH4431999123000889012"),
            "Client AG",
            new Address(
                "Via N",
                "Lugano",
                "6900",
                "TI",
                "CH"),
            Money.Chf(500m),
            "210000000003139471430009017",
            DateTime.UtcNow.AddDays(30));
        await qrBillRepo.AddAsync(qrBill);
        _ = await context.SaveChangesAsync();

        var unpaidBills = await qrBillRepo.GetUnpaidBillsAsync();
        unpaidBills.Should().HaveCount(1);

        // Tenants
        var tenant = Tenant.Create(
            "T-REP",
            "Tenant Rep",
            "IT",
            Currency.EUR,
            "admin@rep.it",
            vatNumber: "12345678901");
        await tenantRepo.AddAsync(tenant);
        _ = await context.SaveChangesAsync();

        var activeTenants = await tenantRepo.GetActiveTenantsAsync();
        activeTenants.Should().HaveCount(1);

        // Time Entries
        var empId = Guid.NewGuid();
        var timeEntry = TimeEntry.ClockIn(
            tenantId,
            empId,
            DateTime.UtcNow.AddHours(-4));
        timeEntry.ClockOut(DateTime.UtcNow);
        await timeRepo.AddAsync(timeEntry);
        _ = await context.SaveChangesAsync();

        var entries =
            await timeRepo.GetEntriesByEmployeeAndDateRangeAsync(
                empId,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(1));
        entries.Should().HaveCount(1);

        // Tire Records
        var vehicleId = Guid.NewGuid();
        var tireRecord = TireHotelRecord.CheckIn(
            tenantId,
            vehicleId,
            TireSeason.Winter,
            "Pirelli",
            "205/55 R16",
            "2222",
            7m,
            7m,
            7m,
            7m,
            "RACK-1");
        await tireRepo.AddAsync(tireRecord);
        _ = await context.SaveChangesAsync();

        var tiresInStorage = await tireRepo.GetAllInStorageAsync();
        tiresInStorage.Should().HaveCount(1);

        var tiresByVehicle = await tireRepo.GetInStorageByVehicleAsync(vehicleId);
        tiresByVehicle.Should().HaveCount(1);

        // Translations
        var translation = AppTranslation.Create(
            "it",
            "Test.Key",
            "Valore Default",
            null,
            tenantId);
        await transRepo.AddAsync(translation);
        _ = await context.SaveChangesAsync();

        var translations = await transRepo.GetByLocaleAsync(
            "it",
            tenantId);
        translations.Should().HaveCount(1);

        // Users
        var user = User.Create(
            tenantId,
            "repouser",
            "repo@user.com",
            "Repo User",
            "hash",
            "salt",
            UserRole.Admin);
        await userRepo.AddAsync(user);
        _ = await context.SaveChangesAsync();

        var foundUser = await userRepo.GetByUsernameAsync("repouser");
        foundUser.Should().NotBeNull();
        foundUser!.Email.Should().Be("repo@user.com");
    }

    /// <summary>
    /// Tests TenantProvider and CurrentUserService state holding and property updates.
    /// </summary>
    [Fact]
    public void Security_TenantProviderAndCurrentUserService_HoldState()
    {
        // TenantProvider
        var provider = new TenantProvider();
        provider.CurrentTenantId.Should().BeEmpty();
        provider.CurrentTenantCode.Should().BeEmpty();
        provider.HasTenant.Should().BeFalse();

        var newTenantId = Guid.NewGuid();
        provider.SetTenant(
            newTenantId,
            "TENANT-XYZ");

        provider.CurrentTenantId.Should().Be(newTenantId);
        provider.CurrentTenantCode.Should().Be("TENANT-XYZ");
        provider.HasTenant.Should().BeTrue();

        // CurrentUserService
        var userService = new CurrentUserService();
        userService.IsAuthenticated.Should().BeFalse();

        var userId = Guid.NewGuid();
        userService.SetUser(
            userId,
            "john_doe",
            "John Doe",
            "SuperAdmin");

        userService.IsAuthenticated.Should().BeTrue();
        userService.UserId.Should().Be(userId);
        userService.Username.Should().Be("john_doe");
        userService.FullName.Should().Be("John Doe");
        userService.Role.Should().Be("SuperAdmin");

        userService.Clear();
        userService.IsAuthenticated.Should().BeFalse();
        userService.Username.Should().BeNull();
    }

    /// <summary>
    /// Tests SqliteLocalizationService caching, locale resolution, fallback, and language change event.
    /// </summary>
    [Fact]
    public async Task Localization_SqliteLocalizationService_ProvidesTranslationsAndTriggersEvent()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();
        await using (var context = CreateInMemoryDbContext(
                         tenantId,
                         "System",
                         dbName))
        {
            var transIt = AppTranslation.Create(
                "it",
                "Dashboard.Welcome",
                "Benvenuto",
                null,
                tenantId);
            var transEn = AppTranslation.Create(
                "en",
                "Dashboard.Welcome",
                "Welcome",
                null,
                tenantId);
            context.AppTranslations.AddRange(
                transIt,
                transEn);
            _ = await context.SaveChangesAsync();
        }

        services.AddScoped<IFatturaFacileDbContext>(_ => CreateInMemoryDbContext(
            tenantId,
            "System",
            dbName));
        using var serviceProvider = services.BuildServiceProvider();

        var locService = new SqliteLocalizationService(serviceProvider);
        var eventFired = false;
        locService.LanguageChanged += (s, e) => eventFired = true;

        // Act
        locService.SetLocale("en");
        var valEn = await locService.GetStringAsync("Dashboard.Welcome");
        var valFallback = locService.GetString(
            "NonExistentKey",
            "DefaultFallback");

        await locService.SaveCustomTranslationAsync(
            "en",
            "Dashboard.Welcome",
            "Welcome Back!");
        var allEn = await locService.GetAllTranslationsAsync("en");

        // Assert
        eventFired.Should().BeTrue();
        locService.CurrentLocale.Should().Be("en");
        valEn.Should().Be("Welcome");
        valFallback.Should().Be("DefaultFallback");
        allEn.Should().Contain(t => t.Key == "Dashboard.Welcome" && t.EffectiveValue == "Welcome Back!");
    }

    /// <summary>
    /// Tests Iso20022PaymentGenerator XML generation.
    /// </summary>
    [Fact]
    public void CrossBorder_Iso20022PaymentGenerator_GeneratesValidPain001Xml()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var instruction = Iso20022PaymentInstruction.Create(
            tenantId,
            "IT60A0000000000000000000000",
            "IT12X0000000000000000000000",
            "Vendor Services SpA",
            Money.Euro(2500.50m),
            "Payment for Q1 Services",
            new DateTime(
                2026,
                4,
                15,
                0,
                0,
                0,
                DateTimeKind.Utc));

        // Act
        var xml = Iso20022PaymentGenerator.GeneratePain001(instruction);

        // Assert
        xml.Should().NotBeNullOrWhiteSpace();
        xml.Should().Contain("pain.001.001.03");
        xml.Should().Contain("<InstdAmt Ccy=\"EUR\">2500.50</InstdAmt>");
        xml.Should().Contain("<Nm>Vendor Services SpA</Nm>");
    }

    /// <summary>
    /// Tests Camt053BankStatementParser parsing valid and invalid XML content.
    /// </summary>
    [Fact]
    public void CrossBorder_Camt053BankStatementParser_ParsesValidAndHandlesMalformedXml()
    {
        // Arrange
        var validXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                       "<Document xmlns=\"urn:iso:std:iso:20022:tech:xsd:camt.053.001.02\">\n" +
                       "  <BkToCstmrStmt>\n" +
                       "    <Stmt>\n" +
                       "      <Bal>\n" +
                       "        <Tp><CdOrPrtry><Cd>OPBD</Cd></CdOrPrtry></Tp>\n" +
                       "        <Amt>10000.00</Amt>\n" +
                       "      </Bal>\n" +
                       "      <Bal>\n" +
                       "        <Tp><CdOrPrtry><Cd>CLBD</Cd></CdOrPrtry></Tp>\n" +
                       "        <Amt>11500.00</Amt>\n" +
                       "      </Bal>\n" +
                       "      <Ntry>\n" +
                       "        <Amt>2000.00</Amt>\n" +
                       "        <CdtDbtInd>CRDT</CdtDbtInd>\n" +
                       "        <BookgDt><Dt>2026-04-10</Dt></BookgDt>\n" +
                       "        <NtryDtls><TxDtls><RmtInf><Ustrd>Customer payment A</Ustrd></RmtInf></TxDtls></NtryDtls>\n" +
                       "      </Ntry>\n" +
                       "      <Ntry>\n" +
                       "        <Amt>500.00</Amt>\n" +
                       "        <CdtDbtInd>DBIT</CdtDbtInd>\n" +
                       "        <BookgDt><Dt>2026-04-12</Dt></BookgDt>\n" +
                       "        <NtryDtls><TxDtls><RmtInf><Ustrd>Supplier invoice payment</Ustrd></RmtInf></TxDtls></NtryDtls>\n" +
                       "      </Ntry>\n" +
                       "    </Stmt>\n" +
                       "  </BkToCstmrStmt>\n" +
                       "</Document>";

        var malformedXml = "<NotValidXml>";

        // Act
        var (opening, closing, entries) = Camt053BankStatementParser.Parse(validXml);
        var (mOpening, mClosing, mEntries) = Camt053BankStatementParser.Parse(malformedXml);

        // Assert
        opening.Should().Be(10000.00m);
        closing.Should().Be(11500.00m);
        entries.Should().HaveCount(2);
        entries[0].Amount.Should().Be(2000.00m);
        entries[0].Remittance.Should().Be("Customer payment A");
        entries[1].Amount.Should().Be(-500.00m);
        entries[1].Remittance.Should().Be("Supplier invoice payment");

        mOpening.Should().Be(0m);
        mClosing.Should().Be(0m);
        mEntries.Should().BeEmpty();
    }
}
