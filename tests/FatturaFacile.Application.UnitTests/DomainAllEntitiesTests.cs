// <copyright file="DomainAllEntitiesTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities.Accounting;
using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Entities.Advisory;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Entities.Construction;
using FatturaFacile.Domain.Entities.FleetRental;
using FatturaFacile.Domain.Entities.Inventory;
using FatturaFacile.Domain.Entities.Manufacturing;
using FatturaFacile.Domain.Entities.RetailPos;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Entities.ServiceTickets;
using FatturaFacile.Domain.Entities.Treasury;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;
using FatturaFacile.Infrastructure;
using FatturaFacile.Infrastructure.Security;

using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Contains automated unit tests verifying domain entities, aggregates, and security services.
/// </summary>
public sealed class DomainAllEntitiesTests
{

    /// <summary>
    /// Tests the DomainEventBase and AggregateRoot event management semantics.
    /// </summary>
    [Fact]
    public void Common_DomainEventBaseAndAggregateRoot_ManageEventsCorrectly()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(id: Guid.NewGuid());
        var evt       = new TestDomainEvent(Data: "Payload");

        // Act
        aggregate.AddDomainEvent(domainEvent: evt);

        // Assert
        _ = evt.EventId.Should().NotBeEmpty();

        _ = evt.OccurredOnUtc.Should()
               .BeCloseTo(
                          nearbyTime: DateTime.UtcNow,
                          precision: TimeSpan.FromSeconds(seconds: 5));

        _ = aggregate.DomainEvents.Should().ContainSingle();

        // Act 2
        aggregate.ClearDomainEvents();

        // Assert 2
        _ = aggregate.DomainEvents.Should().BeEmpty();
    }

    /// <summary>
    /// Tests GeneralLedgerAccount, GeneralLedgerEntry and GeneralLedgerLine calculations and balancing.
    /// </summary>
    [Fact]
    public void Accounting_GeneralLedger_CreatesAndBalancesCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var account = GeneralLedgerAccount.Create(
                                                  tenantId: tenantId,
                                                  code: "1000",
                                                  name: "Cassa Contanti",
                                                  category: AccountCategory.Asset,
                                                  jurisdiction: Jurisdiction.Italy);

        var offsetAccount = GeneralLedgerAccount.Create(
                                                        tenantId: tenantId,
                                                        code: "4000",
                                                        name: "Ricavi Vendite",
                                                        category: AccountCategory.Revenue,
                                                        jurisdiction: Jurisdiction.Italy);

        var entry = GeneralLedgerEntry.Create(
                                              tenantId: tenantId,
                                              entryNumber: "JE-2026-001",
                                              postingDate: DateTime.UtcNow,
                                              description: "Incasso fattura",
                                              currency: Currency.EUR);

        // Act
        entry.AddLine(
                      accountId: account.Id,
                      debit: 1500.00m,
                      credit: 0m,
                      lineMemo: "Entrata cassa");

        var isBalancedBefore = entry.IsBalanced;

        entry.AddLine(
                      accountId: offsetAccount.Id,
                      debit: 0m,
                      credit: 1500.00m,
                      lineMemo: "Storno credito");

        var isBalancedAfter = entry.IsBalanced;

        // Assert
        _ = account.AccountCode.Should().Be(expected: "1000");
        _ = account.Category.Should().Be(expected: AccountCategory.Asset);
        _ = account.IsActive.Should().BeTrue();

        _ = isBalancedBefore.Should().BeFalse();
        _ = isBalancedAfter.Should().BeTrue();
        _ = entry.TotalDebit.Should().Be(expected: 1500.00m);
        _ = entry.TotalCredit.Should().Be(expected: 1500.00m);
        _ = entry.Lines.Should().HaveCount(expected: 2);
    }

    /// <summary>
    /// Tests VatSettlementPeriod creation and finalization.
    /// </summary>
    [Fact]
    public void Accounting_VatSettlementPeriod_CreatesAndFinalizes()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var settlement = VatSettlementPeriod.Create(
                                                    tenantId: tenantId,
                                                    year: 2026,
                                                    periodNumber: 1,
                                                    type: VatSettlementPeriodType.Monthly,
                                                    jurisdiction: Jurisdiction.Italy,
                                                    outputVat: 5000m,
                                                    inputVat: 3000m,
                                                    priorCredit: 500m);

        // Act
        settlement.FinalizeSettlement();

        // Assert
        _ = settlement.Year.Should().Be(expected: 2026);
        _ = settlement.PeriodNumber.Should().Be(expected: 1);
        _ = settlement.PeriodType.Should().Be(expected: VatSettlementPeriodType.Monthly);
        _ = settlement.TotalOutputVat.Should().Be(expected: 5000m);
        _ = settlement.TotalInputVat.Should().Be(expected: 3000m);
        _ = settlement.PriorCreditCarriedForward.Should().Be(expected: 500m);
        _ = settlement.IsFinalized.Should().BeTrue();
    }

    /// <summary>
    /// Tests Tenant branch addition, ExchangeRate, AppTranslation, and User login/password operations.
    /// </summary>
    [Fact]
    public void Administration_Entities_ExecuteOperationsCorrectly()
    {
        // Arrange
        var tenant = Tenant.Create(
                                   code: "CORP-IT",
                                   businessName: "Corporation IT SpA",
                                   countryCode: "IT",
                                   baseCurrency: Currency.EUR,
                                   contactEmail: "contact@corp.it");

        var address = new Address(
                                  street: "Via Roma 1",
                                  city: "Roma",
                                  postalCode: "00100",
                                  provinceOrCanton: "RM",
                                  countryCode: "IT");

        // Act
        tenant.AddBranch(
                         code: "HQ",
                         name: "Headquarters Roma",
                         address: address,
                         isHeadquarters: true);

        var rate = ExchangeRate.Create(
                                       baseCurr: Currency.EUR,
                                       targetCurr: Currency.CHF,
                                       rate: 0.9550m,
                                       date: DateTime.UtcNow,
                                       source: "ECB");

        var trans = AppTranslation.Create(
                                          locale: "de",
                                          key: "Buttons.Save",
                                          defaultValue: "Speichern",
                                          customValue: null,
                                          tenantId: tenant.Id);

        trans.UpdateCustomValue(customValue: "Sichern");

        var user = User.Create(
                               tenantId: tenant.Id,
                               username: "admin",
                               email: "admin@corp.it",
                               fullName: "Administrator",
                               passwordHash: "hash1",
                               salt: "salt1",
                               role: UserRole.SuperAdmin);

        user.RecordLogin();

        user.ChangePassword(
                            newHash: "hash2",
                            newSalt: "salt2");

        // Assert
        _ = tenant.Branches.Should().HaveCount(expected: 1);
        _ = tenant.Branches.First().IsHeadquarters.Should().BeTrue();

        _ = rate.Rate.Should().Be(expected: 0.9550m);
        _ = trans.EffectiveValue.Should().Be(expected: "Sichern");

        _ = user.LastLoginUtc.Should().NotBeNull();
        _ = user.PasswordHash.Should().Be(expected: "hash2");
        _ = user.PasswordSalt.Should().Be(expected: "salt2");
    }

    /// <summary>
    /// Tests Advisory ClientMatter, BillableTimeEntry, and ProfessionalFeeNote calculations.
    /// </summary>
    [Fact]
    public void Advisory_ProfessionalFeeNote_CalculatesItalianTaxAndSocialSecurity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        var matter = ClientMatter.Create(
                                         tenantId: tenantId,
                                         matterNumber: "MAT-100",
                                         clientId: clientId,
                                         title: "Fiscale Internazionale",
                                         practiceArea: "Consulenza");

        var entry = BillableTimeEntry.Create(
                                             tenantId: tenantId,
                                             matterId: matter.Id,
                                             userId: Guid.NewGuid(),
                                             date: DateTime.UtcNow,
                                             minutes: 240,
                                             rate: 200m,
                                             currency: Currency.EUR,
                                             description: "Analisi normativa");

        // Act
        var note = ProfessionalFeeNote.CreateItalian(
                                                     tenantId: tenantId,
                                                     docNumber: "NOTA-2026-001",
                                                     matterId: matter.Id,
                                                     date: DateTime.UtcNow,
                                                     grossFees: 1000m,
                                                     nonTaxableExpenses: 50m,
                                                     vatRate: 22m,
                                                     cassaPercent: 4m,
                                                     ritenutaPercent: 20m);

        // Assert
        _ = matter.MatterNumber.Should().Be(expected: "MAT-100");
        _ = entry.MinutesSpent.Should().Be(expected: 240);
        _ = entry.HourlyRate.Should().Be(expected: 200m);

        _ = note.GrossProfessionalFees.Should().Be(expected: 1000m);
        _ = note.ItCassaPrevidenzialeAmount.Should().Be(expected: 40m); // 4% of 1000
        _ = note.TotalVat.Should().Be(expected: 228.80m);               // 22% of (1000 + 40)
        _ = note.ItRitenutaAccontoAmount.Should().Be(expected: 200m);   // 20% of 1000
        _ = note.NetPayable.Should().Be(expected: 1118.80m);            // 1000 + 40 + 228.80 + 50 - 200
    }

    /// <summary>
    /// Tests Automotive vehicle management, inspection scheduling, and tire hotel check-in/mount.
    /// </summary>
    [Fact]
    public void Automotive_Entities_ExecuteOperationsCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        var vehicle = CustomerVehicle.Create(
                                             tenantId: tenantId,
                                             clientId: clientId,
                                             licensePlate: "AA123BB",
                                             vin: "ZFA12345678901234",
                                             brand: "Fiat",
                                             model: "500",
                                             year: 2021,
                                             jurisdiction: Jurisdiction.Italy,
                                             km: 15000);

        var inspection = StatutoryInspectionRecord.Schedule(
                                                            tenantId: tenantId,
                                                            vehicleId: vehicle.Id,
                                                            type: InspectionType.ItalianRevisione,
                                                            date: DateTime.UtcNow);

        var tire = TireHotelRecord.CheckIn(
                                           tenantId: tenantId,
                                           vehicleId: vehicle.Id,
                                           season: TireSeason.Winter,
                                           brand: "Michelin",
                                           dimension: "205/55R16",
                                           dotCode: "DOT2024",
                                           flMm: 6.5m,
                                           frMm: 6.5m,
                                           rlMm: 6.0m,
                                           rrMm: 6.0m,
                                           rackCoordinate: "BIN-12A");

        // Act
        inspection.MarkCompleted(
                                 passed: true,
                                 notes: "Superata senza rilievi",
                                 nextDue: DateTime.UtcNow.AddYears(value: 2));

        tire.MountOnVehicle();

        // Assert
        _ = vehicle.LicensePlate.Should().Be(expected: "AA123BB");
        _ = inspection.Status.Should().Be(expected: "Passed");
        _ = tire.Status.Should().Be(expected: "MountedOnVehicle");
    }

    /// <summary>
    /// Tests Construction site, bill of quantities, declaration of conformity, and SAL progress billing.
    /// </summary>
    [Fact]
    public void Construction_ProgressBillingSal_CalculatesRetentionAndNet()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        var address = new Address(
                                  street: "Via Cantiere 5",
                                  city: "Milano",
                                  postalCode: "20100",
                                  provinceOrCanton: "MI",
                                  countryCode: "IT");

        var site = ConstructionSite.Create(
                                           tenantId: tenantId,
                                           projectCode: "SITE-2026",
                                           name: "Edificio Alfa",
                                           clientId: clientId,
                                           address: address,
                                           manager: "Mario Bianchi",
                                           start: DateTime.UtcNow,
                                           budget: Money.Euro(amount: 1000000m));

        var boq = BillOfQuantitiesItem.Create(
                                              tenantId: tenantId,
                                              siteId: site.Id,
                                              code: "ITEM-01",
                                              standard: StandardType.ItalianComputo,
                                              desc: "Demolizioni",
                                              uom: "m2",
                                              qty: 200m,
                                              unitPrice: 50m);

        var dico = DeclarationOfConformity.Create(
                                                  tenantId: tenantId,
                                                  siteId: site.Id,
                                                  certNum: "DICO-001",
                                                  standard: "DM 37/08",
                                                  engineer: "Ing. Rossi",
                                                  issueDate: DateTime.UtcNow,
                                                  systemType: "Impianto Elettrico");

        // Act
        var sal = ProgressBillingSal.Create(
                                            tenantId: tenantId,
                                            siteId: site.Id,
                                            number: 1,
                                            date: DateTime.UtcNow,
                                            percent: 0.25m,
                                            certifiedAmount: Money.Euro(amount: 100000m),
                                            retentionAmount: Money.Euro(amount: 5000m));

        // Assert
        _ = site.ProjectCode.Should().Be(expected: "SITE-2026");
        _ = boq.ItemCode.Should().Be(expected: "ITEM-01");
        _ = dico.CertificateNumber.Should().Be(expected: "DICO-001");

        _ = sal.TotalCertifiedAmount.Amount.Should().Be(expected: 100000m);
        _ = sal.RetentionGuaranteeAmount.Amount.Should().Be(expected: 5000m);
        _ = sal.CumulativePercentage.Should().Be(expected: 0.25m);
    }

    /// <summary>
    /// Tests Fleet rental vehicle, booking, and damage appraisal creation.
    /// </summary>
    [Fact]
    public void FleetRental_Entities_CreateCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var vehicle = RentalVehicle.Create(
                                           tenantId: tenantId,
                                           fleetCode: "FL-01",
                                           plate: "TI-100200",
                                           makeModel: "Audi A4 Avant",
                                           dailyRate: Money.Chf(amount: 120m),
                                           currentKm: 25000);

        var booking = RentalBooking.Create(
                                           tenantId: tenantId,
                                           bookingNumber: "BOOK-01",
                                           vehicleId: vehicle.Id,
                                           clientId: Guid.NewGuid(),
                                           start: DateTime.UtcNow,
                                           end: DateTime.UtcNow.AddDays(value: 5),
                                           pickup: "Lugano",
                                           ret: "Lugano",
                                           total: Money.Chf(amount: 600m),
                                           deposit: Money.Chf(amount: 1000m));

        var appraisal = DamageAppraisal.Create(
                                               tenantId: tenantId,
                                               bookingId: booking.Id,
                                               type: "Carrozzeria",
                                               coordsJson: "{}",
                                               severity: DamageSeverity.Minor,
                                               repairCost: Money.Chf(amount: 450m));

        // Assert
        _ = vehicle.LicensePlate.Should().Be(expected: "TI-100200");
        _ = booking.TotalRentAmount.Amount.Should().Be(expected: 600m);
        _ = booking.DepositAmount.Amount.Should().Be(expected: 1000m);
        _ = appraisal.EstimatedRepairCost.Amount.Should().Be(expected: 450m);
    }

    /// <summary>
    /// Tests Inventory warehouse, transport document, and stock movements.
    /// </summary>
    [Fact]
    public void Inventory_Entities_CreateCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var address = new Address(
                                  street: "Via Logistica 10",
                                  city: "Verona",
                                  postalCode: "37100",
                                  provinceOrCanton: "VR",
                                  countryCode: "IT");

        var warehouse = Warehouse.Create(
                                         tenantId: tenantId,
                                         code: "WH-VR",
                                         name: "Hub Verona",
                                         address: address,
                                         capacity: 10000m);

        var ddt = TransportDocument.Create(
                                           tenantId: tenantId,
                                           number: "DDT-01",
                                           date: DateTime.UtcNow,
                                           clientId: Guid.NewGuid(),
                                           destination: address,
                                           reason: "Vendita",
                                           carrier: "DHL Express",
                                           packages: 5,
                                           weightKg: 25.5m);

        var movement = StockMovement.Create(
                                            tenantId: tenantId,
                                            itemId: Guid.NewGuid(),
                                            warehouseId: warehouse.Id,
                                            quantity: 100m,
                                            type: MovementType.Receipt,
                                            batch: "BATCH-1",
                                            serial: "SN-1",
                                            refDoc: "PO-100");

        // Assert
        _ = warehouse.Code.Should().Be(expected: "WH-VR");
        _ = ddt.DocumentNumber.Should().Be(expected: "DDT-01");
        _ = movement.Quantity.Should().Be(expected: 100m);
        _ = movement.MovementType.Should().Be(expected: MovementType.Receipt);
        _ = movement.BatchNumber.Should().Be(expected: "BATCH-1");
    }

    /// <summary>
    /// Tests Manufacturing work center and production order creation.
    /// </summary>
    [Fact]
    public void Manufacturing_Entities_CreateCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var wc = WorkCenter.Create(
                                   tenantId: tenantId,
                                   code: "WC-MILL",
                                   name: "Fresatura CNC",
                                   cost: 75m,
                                   capacityHours: 8m);

        var po = ProductionOrder.Create(
                                        tenantId: tenantId,
                                        orderNumber: "PO-2026-10",
                                        bomId: Guid.NewGuid(),
                                        quantity: 500m,
                                        start: DateTime.UtcNow,
                                        end: DateTime.UtcNow.AddDays(value: 10));

        // Assert
        _ = wc.Code.Should().Be(expected: "WC-MILL");
        _ = wc.HourlyMachineCost.Should().Be(expected: 75m);
        _ = po.OrderNumber.Should().Be(expected: "PO-2026-10");
        _ = po.QuantityToProduce.Should().Be(expected: 500m);
    }

    /// <summary>
    /// Tests Retail POS session open, close, and receipt issuance.
    /// </summary>
    [Fact]
    public void RetailPos_SessionAndReceipt_ExecuteFlowCorrectly()
    {
        // Arrange
        var tenantId  = Guid.NewGuid();
        var cashierId = Guid.NewGuid();

        var session = PosTerminalSession.Open(
                                              tenantId: tenantId,
                                              terminalCode: "POS-01",
                                              cashierId: cashierId,
                                              cashFloat: Money.Euro(amount: 150m));

        var receipt = RetailReceipt.Create(
                                           tenantId: tenantId,
                                           sessionId: session.Id,
                                           receiptNumber: "REC-101",
                                           amount: 65.50m,
                                           currency: Currency.EUR,
                                           paymentMethod: PaymentMethod.CardSumUpWorldline,
                                           rtLot: "RT-LOT-999");

        // Act
        session.CloseSession(
                             countedCash: Money.Euro(amount: 500m),
                             calculatedCash: Money.Euro(amount: 500m));

        // Assert
        _ = session.IsClosed.Should().BeTrue();
        _ = session.CashDiscrepancy.Amount.Should().Be(expected: 0m);
        _ = receipt.TotalAmount.Should().Be(expected: 65.50m);
        _ = receipt.PaymentMethod.Should().Be(expected: PaymentMethod.CardSumUpWorldline);
    }

    /// <summary>
    /// Tests PriceList and Quote in Sales domain.
    /// </summary>
    [Fact]
    public void Sales_PriceListAndQuote_CreateCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var list = PriceList.Create(
                                    tenantId: tenantId,
                                    name: "Listino Retail",
                                    currency: Currency.EUR,
                                    isDefault: true);

        var quote = Quote.Create(
                                 tenantId: tenantId,
                                 quoteNumber: "Q-2026-05",
                                 clientId: Guid.NewGuid(),
                                 expiry: DateTime.UtcNow.AddDays(value: 30),
                                 total: Money.Euro(amount: 2500m));

        // Assert
        _ = list.Name.Should().Be(expected: "Listino Retail");
        _ = list.IsDefault.Should().BeTrue();
        _ = quote.QuoteNumber.Should().Be(expected: "Q-2026-05");
        _ = quote.TotalAmount.Amount.Should().Be(expected: 2500m);
    }

    /// <summary>
    /// Tests TechnicianWorkOrder creation.
    /// </summary>
    [Fact]
    public void ServiceTickets_TechnicianWorkOrder_CreatesCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var order = TechnicianWorkOrder.Create(
                                               tenantId: tenantId,
                                               workOrderNumber: "WO-2026-001",
                                               ticketId: Guid.NewGuid(),
                                               technicianId: Guid.NewGuid(),
                                               serviceDate: DateTime.UtcNow,
                                               hoursWorked: 3m,
                                               hourlyRate: 85m,
                                               currency: Currency.EUR,
                                               summary: "Sostituzione pompa di calore");

        // Assert
        _ = order.WorkOrderNumber.Should().Be(expected: "WO-2026-001");
        _ = order.WorkPerformedSummary.Should().Be(expected: "Sostituzione pompa di calore");
        _ = order.HoursWorked.Should().Be(expected: 3m);
    }

    /// <summary>
    /// Tests BankStatement creation and transaction lines.
    /// </summary>
    [Fact]
    public void Treasury_BankStatement_AddsTransactionsCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var stmt = BankStatement.Create(
                                        tenantId: tenantId,
                                        statementNumber: "STMT-01",
                                        accountIban: "IT60A0000000000000000000000",
                                        currency: Currency.EUR,
                                        openingBalance: 5000m,
                                        closingBalance: 8000m,
                                        statementDate: DateTime.UtcNow);

        // Act
        stmt.AddTransaction(
                            bookingDate: DateTime.UtcNow,
                            valueDate: DateTime.UtcNow,
                            amount: 3000m,
                            remittance: "Incasso bonifico cliente",
                            counterpartyIban: "IT12X0000000000000000000000",
                            counterpartyName: "Cliente SpA");

        // Assert
        _ = stmt.OpeningBalance.Should().Be(expected: 5000m);
        _ = stmt.ClosingBalance.Should().Be(expected: 8000m);
        _ = stmt.Lines.Should().HaveCount(expected: 1);
        _ = stmt.Lines.First().RemittanceInformation.Should().Be(expected: "Incasso bonifico cliente");
    }

    /// <summary>
    /// Tests CurrentUserService and TenantProvider infrastructure services.
    /// </summary>
    [Fact]
    public void Security_CurrentUserServiceAndTenantProvider_ManageContext()
    {
        // Arrange
        var userService    = new CurrentUserService();
        var tenantProvider = new TenantProvider();
        var userId         = Guid.NewGuid();
        var tenantId       = Guid.NewGuid();

        // Act
        userService.SetUser(
                            userId: userId,
                            username: "johndoe",
                            fullName: "John Doe",
                            role: "Admin");

        tenantProvider.SetTenant(
                                 tenantId: tenantId,
                                 tenantCode: "TENANT-01");

        // Assert
        _ = userService.IsAuthenticated.Should().BeTrue();
        _ = userService.Username.Should().Be(expected: "johndoe");
        _ = userService.FullName.Should().Be(expected: "John Doe");
        _ = userService.Role.Should().Be(expected: "Admin");

        _ = tenantProvider.HasTenant.Should().BeTrue();
        _ = tenantProvider.CurrentTenantId.Should().Be(expected: tenantId);
        _ = tenantProvider.CurrentTenantCode.Should().Be(expected: "TENANT-01");

        // Act 2
        userService.Clear();

        // Assert 2
        _ = userService.IsAuthenticated.Should().BeFalse();
        _ = userService.UserId.Should().BeNull();
    }

    /// <summary>
    /// Tests DependencyInjection AddInfrastructure registration.
    /// </summary>
    [Fact]
    public void Infrastructure_AddInfrastructure_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _ = services.AddInfrastructure(connectionString: "Data Source=:memory:");
        using var provider = services.BuildServiceProvider();

        // Assert
        _ = provider.GetRequiredService<ITenantProvider>().Should().NotBeNull();
        _ = provider.GetRequiredService<ICurrentUserService>().Should().NotBeNull();
        _ = provider.GetRequiredService<IPasswordHasher>().Should().NotBeNull();
        _ = provider.GetRequiredService<ILocalizationService>().Should().NotBeNull();
    }

    /// <summary>
    /// Represents a dummy domain event used for testing event registration and clearing.
    /// </summary>
    /// <param name="Data">The event data payload.</param>
    private sealed record TestDomainEvent(string Data) : DomainEventBase;

    /// <summary>
    /// Represents a dummy aggregate root used to verify domain event dispatching and aggregate state behaviors.
    /// </summary>
    private sealed class TestAggregateRoot : AggregateRoot<Guid>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAggregateRoot"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the aggregate.</param>
        public TestAggregateRoot(Guid id)
            : base(id: id)
        {
        }

    }

}
