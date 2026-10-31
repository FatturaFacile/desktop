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
/// Contains extended integration and unit tests for Infrastructure persistence,
/// security, cross-border parsers, and localization.
/// </summary>
public sealed class InfrastructureExtendedTests
{

    /// <summary>
    /// Tests that the multi-tenant query filter on <see cref="FatturaFacileDbContext"/>
    /// hides entities belonging to other tenants.
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
        _ = tenantProviderMock.Setup(expression: t => t.CurrentTenantId).Returns(value: tenantA);

        var userMock = new Mock<ICurrentUserService>();

        await using var context = new FatturaFacileDbContext(
                                                             options: options,
                                                             tenantProvider: tenantProviderMock.Object,
                                                             currentUserService: userMock.Object);

        var clientA = Client.CreateItalian(
                                           tenantId: tenantA,
                                           businessName: "Client A",
                                           fiscalCode: new FiscalCode(value: "RSSMRA80A01F205X"),
                                           vatNumber: new VatNumber(
                                                                    value: "12345678901",
                                                                    countryCode: "IT"),
                                           sdiCode: "0000000",
                                           pecEmail: "a@pec.it",
                                           address: new Address(
                                                                street: "Via A",
                                                                city: "Milano",
                                                                postalCode: "20100",
                                                                provinceOrCanton: "MI",
                                                                countryCode: "IT"),
                                           email: "a@client.it");

        var clientB = Client.CreateItalian(
                                           tenantId: tenantB,
                                           businessName: "Client B",
                                           fiscalCode: new FiscalCode(value: "RSSMRA80A01F205X"),
                                           vatNumber: new VatNumber(
                                                                    value: "12345678902",
                                                                    countryCode: "IT"),
                                           sdiCode: "0000000",
                                           pecEmail: "b@pec.it",
                                           address: new Address(
                                                                street: "Via B",
                                                                city: "Milano",
                                                                postalCode: "20100",
                                                                provinceOrCanton: "MI",
                                                                countryCode: "IT"),
                                           email: "b@client.it");

        context.Clients.AddRange(
                                 entities: [clientA, clientB]);

        _ = await context.SaveChangesAsync();

        // Act
        var visibleClients = await context.Clients.ToListAsync();

        // Assert
        _ = visibleClients.Should().HaveCount(expected: 1);
        _ = visibleClients[index: 0].BusinessName.Should().Be(expected: "Client A");
    }

    /// <summary>
    /// Tests that <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>
    /// automatically populates audit tracking properties.
    /// </summary>
    [Fact]
    public async Task DbContext_SaveChangesAsync_SetsAuditProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        await using var context = CreateInMemoryDbContext(
                                                          tenantId: tenantId,
                                                          username: "AuditorUser");

        var client = Client.CreateItalian(
                                          tenantId: tenantId,
                                          businessName: "Audit Client",
                                          fiscalCode: new FiscalCode(value: "RSSMRA80A01F205X"),
                                          vatNumber: new VatNumber(
                                                                   value: "12345678901",
                                                                   countryCode: "IT"),
                                          sdiCode: "0000000",
                                          pecEmail: "audit@pec.it",
                                          address: new Address(
                                                               street: "Via C",
                                                               city: "Roma",
                                                               postalCode: "00100",
                                                               provinceOrCanton: "RM",
                                                               countryCode: "IT"),
                                          email: "audit@client.it");

        // Act
        _ = context.Clients.Add(entity: client);
        _ = await context.SaveChangesAsync();

        // Assert
        _ = client.CreatedBy.Should().Be(expected: "AuditorUser");

        _ = client.CreatedAtUtc.Should()
                  .BeCloseTo(
                             nearbyTime: DateTime.UtcNow,
                             precision: TimeSpan.FromSeconds(seconds: 2));

        // Act 2 - Update
        client.Deactivate();
        _ = await context.SaveChangesAsync();

        // Assert 2
        _ = client.UpdatedBy.Should().Be(expected: "AuditorUser");
        _ = client.UpdatedAtUtc.Should().NotBeNull();
    }

    /// <summary>
    /// Creates a new in-memory <see cref="FatturaFacileDbContext"/> configured for the specified tenant and user.
    /// </summary>
    /// <param name="tenantId">
    /// The unique identifier of the tenant.
    /// </param>
    /// <param name="username">
    /// The username of the simulated current user. Defaults to <c>"TestUser"</c>.
    /// </param>
    /// <param name="dbName">
    /// The name of the in-memory database, or <see langword="null"/> to generate a unique database name.
    /// </param>
    /// <returns>
    /// A configured instance of <see cref="FatturaFacileDbContext"/>.
    /// </returns>
    private static FatturaFacileDbContext CreateInMemoryDbContext(
        Guid    tenantId,
        string  username = "TestUser",
        string? dbName   = null)
    {
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
                     .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                     .Options;

        var tenantMock = new Mock<ITenantProvider>();
        _ = tenantMock.Setup(expression: t => t.CurrentTenantId).Returns(value: tenantId);

        var userMock = new Mock<ICurrentUserService>();
        _ = userMock.Setup(expression: u => u.Username).Returns(value: username);

        return new FatturaFacileDbContext(
                                          options: options,
                                          tenantProvider: tenantMock.Object,
                                          currentUserService: userMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="DbInitializer.SeedAsync"/>
    /// seeds initial tenants, users, and documents, and is idempotent.
    /// </summary>
    [Fact]
    public async Task DbInitializer_SeedAsync_SeedsDataAndIsIdempotent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        await using var context = CreateInMemoryDbContext(
                                                          tenantId: tenantId,
                                                          username: "System");

        var hasher = new Pbkdf2PasswordHasher();

        // Act
        await DbInitializer.SeedAsync(
                                      context: context,
                                      passwordHasher: hasher);

        // Assert
        var tenantsCount = await context.Tenants.CountAsync();
        _ = tenantsCount.Should().Be(expected: 2);

        var usersCount = await context.Users.IgnoreQueryFilters().CountAsync();
        _ = usersCount.Should().Be(expected: 1);

        // Act 2 - Call seed again (idempotency test)
        await DbInitializer.SeedAsync(
                                      context: context,
                                      passwordHasher: hasher);

        // Assert 2
        var tenantsCountAfter = await context.Tenants.CountAsync();
        _ = tenantsCountAfter.Should().Be(expected: 2);
    }

    /// <summary>
    /// Tests repository CRUD and specialized query operations against an in-memory database context.
    /// </summary>
    [Fact]
    public async Task Repositories_CrudAndSpecializedQueries_ExecuteSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        await using var context = CreateInMemoryDbContext(
                                                          tenantId: tenantId,
                                                          username: "RepoUser");

        var clientRepo  = new ClientRepository(dbContext: context);
        var invoiceRepo = new ItalianInvoiceRepository(dbContext: context);
        var qrBillRepo  = new SwissQrBillRepository(dbContext: context);
        var tenantRepo  = new FatturaFacile.Infrastructure.Persistence.Repositories.TenantRepository(dbContext: context);
        var timeRepo    = new TimeEntryRepository(dbContext: context);
        var tireRepo    = new TireRecordRepository(dbContext: context);
        var transRepo   = new TranslationRepository(dbContext: context);
        var userRepo    = new UserRepository(dbContext: context);

        // Clients
        var client = Client.CreateItalian(
                                          tenantId: tenantId,
                                          businessName: "Client Repo Test",
                                          fiscalCode: new FiscalCode(value: "RSSMRA80A01F205X"),
                                          vatNumber: new VatNumber(
                                                                   value: "12345678901",
                                                                   countryCode: "IT"),
                                          sdiCode: "0000000",
                                          pecEmail: "repo@pec.it",
                                          address: new Address(
                                                               street: "Via R",
                                                               city: "Monza",
                                                               postalCode: "20900",
                                                               provinceOrCanton: "MB",
                                                               countryCode: "IT"),
                                          email: "repo@client.it");

        await clientRepo.AddAsync(entity: client);
        _ = await context.SaveChangesAsync();

        var activeClients = await clientRepo.GetActiveClientsAsync();
        _ = activeClients.Should().HaveCount(expected: 1);

        // Invoices
        var invoice = ItalianElectronicInvoice.Create(
                                                      tenantId: tenantId,
                                                      documentNumber: "INV-REPO-1",
                                                      documentDate: DateTime.UtcNow,
                                                      clientId: client.Id,
                                                      cedenteName: "Cedente",
                                                      cedenteVat: "12345678901",
                                                      cedenteCf: "12345678901",
                                                      cedenteAddress: new Address(
                                                                                  street: "Via C",
                                                                                  city: "Milano",
                                                                                  postalCode: "20100",
                                                                                  provinceOrCanton: "MI",
                                                                                  countryCode: "IT"),
                                                      cessionarioName: "Cessionario",
                                                      cessionarioVat: "98765432101",
                                                      cessionarioCf: "RSSMRA80A01F205X",
                                                      sdiCode: "0000000",
                                                      pec: "ces@pec.it",
                                                      cessionarioAddress: new Address(
                                                                                      street: "Via D",
                                                                                      city: "Roma",
                                                                                      postalCode: "00100",
                                                                                      provinceOrCanton: "RM",
                                                                                      countryCode: "IT"),
                                                      dueDate: DateTime.UtcNow.AddDays(value: 30));

        await invoiceRepo.AddAsync(entity: invoice);
        _ = await context.SaveChangesAsync();

        var pendingSdi = await invoiceRepo.GetPendingSdiAsync();
        _ = pendingSdi.Should().HaveCount(expected: 1);

        // QR Bills
        var qrBill = SwissQrBill.Create(
                                        tenantId: tenantId,
                                        billNumber: "QR-REPO-1",
                                        billDate: DateTime.UtcNow,
                                        clientId: client.Id,
                                        creditorName: "Gotthard AG",
                                        creditorAddress: new Address(
                                                                     street: "Via S",
                                                                     city: "Lugano",
                                                                     postalCode: "6900",
                                                                     provinceOrCanton: "TI",
                                                                     countryCode: "CH"),
                                        creditorQrIban: new QrIban(value: "CH4431999123000889012"),
                                        debtorName: "Client AG",
                                        debtorAddress: new Address(
                                                                   street: "Via N",
                                                                   city: "Lugano",
                                                                   postalCode: "6900",
                                                                   provinceOrCanton: "TI",
                                                                   countryCode: "CH"),
                                        rawAmount: Money.Chf(amount: 500m),
                                        referenceNumber: "210000000003139471430009017",
                                        dueDate: DateTime.UtcNow.AddDays(value: 30));

        await qrBillRepo.AddAsync(entity: qrBill);
        _ = await context.SaveChangesAsync();

        var unpaidBills = await qrBillRepo.GetUnpaidBillsAsync();
        _ = unpaidBills.Should().HaveCount(expected: 1);

        // Tenants
        var tenant = Tenant.Create(
                                   code: "T-REP",
                                   businessName: "Tenant Rep",
                                   countryCode: "IT",
                                   baseCurrency: Currency.EUR,
                                   contactEmail: "admin@rep.it",
                                   vatNumber: "12345678901");

        await tenantRepo.AddAsync(entity: tenant);
        _ = await context.SaveChangesAsync();

        var activeTenants = await tenantRepo.GetActiveTenantsAsync();
        _ = activeTenants.Should().HaveCount(expected: 1);

        // Time Entries
        var empId = Guid.NewGuid();

        var timeEntry = TimeEntry.ClockIn(
                                          tenantId: tenantId,
                                          employeeId: empId,
                                          clockInUtc: DateTime.UtcNow.AddHours(value: -4));

        timeEntry.ClockOut(clockOutUtc: DateTime.UtcNow);
        await timeRepo.AddAsync(entity: timeEntry);
        _ = await context.SaveChangesAsync();

        var entries = await timeRepo.GetEntriesByEmployeeAndDateRangeAsync(
                                                                           employeeId: empId,
                                                                           startUtc: DateTime.UtcNow.AddDays(value: -1),
                                                                           endUtc: DateTime.UtcNow.AddDays(value: 1));

        _ = entries.Should().HaveCount(expected: 1);

        // Tire Records
        var vehicleId = Guid.NewGuid();

        var tireRecord = TireHotelRecord.CheckIn(
                                                 tenantId: tenantId,
                                                 vehicleId: vehicleId,
                                                 season: TireSeason.Winter,
                                                 brand: "Pirelli",
                                                 dimension: "205/55 R16",
                                                 dotCode: "2222",
                                                 flMm: 7m,
                                                 frMm: 7m,
                                                 rlMm: 7m,
                                                 rrMm: 7m,
                                                 rackCoordinate: "RACK-1");

        await tireRepo.AddAsync(entity: tireRecord);
        _ = await context.SaveChangesAsync();

        var tiresInStorage = await tireRepo.GetAllInStorageAsync();
        _ = tiresInStorage.Should().HaveCount(expected: 1);

        var tiresByVehicle = await tireRepo.GetInStorageByVehicleAsync(vehicleId: vehicleId);
        _ = tiresByVehicle.Should().HaveCount(expected: 1);

        // Translations
        var translation = AppTranslation.Create(
                                                locale: "it",
                                                key: "Test.Key",
                                                defaultValue: "Valore Default",
                                                customValue: null,
                                                tenantId: tenantId);

        await transRepo.AddAsync(entity: translation);
        _ = await context.SaveChangesAsync();

        var translations = await transRepo.GetByLocaleAsync(
                                                            locale: "it",
                                                            tenantId: tenantId);

        _ = translations.Should().HaveCount(expected: 1);

        // Users
        var user = User.Create(
                               tenantId: tenantId,
                               username: "repouser",
                               email: "repo@user.com",
                               fullName: "Repo User",
                               passwordHash: "hash",
                               salt: "salt",
                               role: UserRole.Admin);

        await userRepo.AddAsync(entity: user);
        _ = await context.SaveChangesAsync();

        var foundUser = await userRepo.GetByUsernameAsync(username: "repouser");
        _ = foundUser.Should().NotBeNull();
        _ = foundUser!.Email.Should().Be(expected: "repo@user.com");
    }

    /// <summary>
    /// Tests that <see cref="TenantProvider"/> and <see cref="CurrentUserService"/> hold and update state correctly.
    /// </summary>
    [Fact]
    public void Security_TenantProviderAndCurrentUserService_HoldState()
    {
        // TenantProvider
        var provider = new TenantProvider();
        _ = provider.CurrentTenantId.Should().BeEmpty();
        _ = provider.CurrentTenantCode.Should().BeEmpty();
        _ = provider.HasTenant.Should().BeFalse();

        var newTenantId = Guid.NewGuid();

        provider.SetTenant(
                           tenantId: newTenantId,
                           tenantCode: "TENANT-XYZ");

        _ = provider.CurrentTenantId.Should().Be(expected: newTenantId);
        _ = provider.CurrentTenantCode.Should().Be(expected: "TENANT-XYZ");
        _ = provider.HasTenant.Should().BeTrue();

        // CurrentUserService
        var userService = new CurrentUserService();
        _ = userService.IsAuthenticated.Should().BeFalse();

        var userId = Guid.NewGuid();

        userService.SetUser(
                            userId: userId,
                            username: "john_doe",
                            fullName: "John Doe",
                            role: "SuperAdmin");

        _ = userService.IsAuthenticated.Should().BeTrue();
        _ = userService.UserId.Should().Be(expected: userId);
        _ = userService.Username.Should().Be(expected: "john_doe");
        _ = userService.FullName.Should().Be(expected: "John Doe");
        _ = userService.Role.Should().Be(expected: "SuperAdmin");

        userService.Clear();
        _ = userService.IsAuthenticated.Should().BeFalse();
        _ = userService.Username.Should().BeNull();
    }

    /// <summary>
    /// Tests <see cref="SqliteLocalizationService"/> caching, locale resolution,
    /// fallback behavior, and language change events.
    /// </summary>
    [Fact]
    public async Task Localization_SqliteLocalizationService_ProvidesTranslationsAndTriggersEvent()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbName   = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid();

        await using (var context = CreateInMemoryDbContext(
                                                           tenantId: tenantId,
                                                           username: "System",
                                                           dbName: dbName))
        {
            var transIt = AppTranslation.Create(
                                                locale: "it",
                                                key: "Dashboard.Welcome",
                                                defaultValue: "Benvenuto",
                                                customValue: null,
                                                tenantId: tenantId);

            var transEn = AppTranslation.Create(
                                                locale: "en",
                                                key: "Dashboard.Welcome",
                                                defaultValue: "Welcome",
                                                customValue: null,
                                                tenantId: tenantId);

            context.AppTranslations.AddRange(
                                             entities: [transIt, transEn]);

            _ = await context.SaveChangesAsync();
        }

        _ = services.AddScoped<IFatturaFacileDbContext>(implementationFactory: _ => CreateInMemoryDbContext(
                                                                                                            tenantId: tenantId,
                                                                                                            username: "System",
                                                                                                            dbName: dbName));

        using var serviceProvider = services.BuildServiceProvider();

        var locService = new SqliteLocalizationService(serviceProvider: serviceProvider);
        var eventFired = false;
        locService.LanguageChanged += (s, e) => eventFired = true;

        // Act
        locService.SetLocale(locale: "en");
        var valEn = await locService.GetStringAsync(key: "Dashboard.Welcome");

        var valFallback = locService.GetString(
                                               key: "NonExistentKey",
                                               defaultFallback: "DefaultFallback");

        await locService.SaveCustomTranslationAsync(
                                                    locale: "en",
                                                    key: "Dashboard.Welcome",
                                                    customValue: "Welcome Back!");

        var allEn = await locService.GetAllTranslationsAsync(locale: "en");

        // Assert
        _ = eventFired.Should().BeTrue();
        _ = locService.CurrentLocale.Should().Be(expected: "en");
        _ = valEn.Should().Be(expected: "Welcome");
        _ = valFallback.Should().Be(expected: "DefaultFallback");
        _ = allEn.Should().Contain(predicate: t => t.Key == "Dashboard.Welcome" && t.EffectiveValue == "Welcome Back!");
    }

    /// <summary>
    /// Tests that <see cref="Iso20022PaymentGenerator.GeneratePain001(Iso20022PaymentInstruction)"/>
    /// generates valid pain.001.001.03 XML content.
    /// </summary>
    [Fact]
    public void CrossBorder_Iso20022PaymentGenerator_GeneratesValidPain001Xml()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var instruction = Iso20022PaymentInstruction.Create(
                                                            tenantId: tenantId,
                                                            debtorIban: "IT60A0000000000000000000000",
                                                            creditorIban: "IT12X0000000000000000000000",
                                                            creditorName: "Vendor Services SpA",
                                                            amount: Money.Euro(amount: 2500.50m),
                                                            remittance: "Payment for Q1 Services",
                                                            executionDate: new DateTime(
                                                                                        year: 2026,
                                                                                        month: 4,
                                                                                        day: 15,
                                                                                        hour: 0,
                                                                                        minute: 0,
                                                                                        second: 0,
                                                                                        kind: DateTimeKind.Utc));

        // Act
        var xml = Iso20022PaymentGenerator.GeneratePain001(instruction: instruction);

        // Assert
        _ = xml.Should().NotBeNullOrWhiteSpace();
        _ = xml.Should().Contain(expected: "pain.001.001.03");
        _ = xml.Should().Contain(expected: "<InstdAmt Ccy=\"EUR\">2500.50</InstdAmt>");
        _ = xml.Should().Contain(expected: "<Nm>Vendor Services SpA</Nm>");
    }

    /// <summary>
    /// Tests that <see cref="Camt053BankStatementParser.Parse(string)"/>
    /// correctly parses valid camt.053 XML and gracefully handles malformed XML.
    /// </summary>
    [Fact]
    public void CrossBorder_Camt053BankStatementParser_ParsesValidAndHandlesMalformedXml()
    {
        // Arrange
        const string validXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
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

        const string malformedXml = "<NotValidXml>";

        // Act
        var (opening, closing, entries)    = Camt053BankStatementParser.Parse(xmlContent: validXml);
        var (mOpening, mClosing, mEntries) = Camt053BankStatementParser.Parse(xmlContent: malformedXml);

        // Assert
        _ = opening.Should().Be(expected: 10000.00m);
        _ = closing.Should().Be(expected: 11500.00m);
        _ = entries.Should().HaveCount(expected: 2);
        _ = entries[index: 0].Amount.Should().Be(expected: 2000.00m);
        _ = entries[index: 0].Remittance.Should().Be(expected: "Customer payment A");
        _ = entries[index: 1].Amount.Should().Be(expected: -500.00m);
        _ = entries[index: 1].Remittance.Should().Be(expected: "Supplier invoice payment");

        _ = mOpening.Should().Be(expected: 0m);
        _ = mClosing.Should().Be(expected: 0m);
        _ = mEntries.Should().BeEmpty();
    }

}
