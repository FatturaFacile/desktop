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
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        var evt = new TestDomainEvent("Payload");

        // Act
        aggregate.AddDomainEvent(evt);

        // Assert
        _ = evt.EventId.Should().NotBeEmpty();
        _ = evt.OccurredOnUtc.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(5));
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
            tenantId,
            "1000",
            "Cassa Contanti",
            AccountCategory.Asset,
            Jurisdiction.Italy);
        var offsetAccount = GeneralLedgerAccount.Create(
            tenantId,
            "4000",
            "Ricavi Vendite",
            AccountCategory.Revenue,
            Jurisdiction.Italy);

        var entry = GeneralLedgerEntry.Create(
            tenantId,
            "JE-2026-001",
            DateTime.UtcNow,
            "Incasso fattura",
            Currency.EUR);

        // Act
        entry.AddLine(
            account.Id,
            1500.00m,
            0m,
            "Entrata cassa");
        var isBalancedBefore = entry.IsBalanced;

        entry.AddLine(
            offsetAccount.Id,
            0m,
            1500.00m,
            "Storno credito");
        var isBalancedAfter = entry.IsBalanced;

        // Assert
        _ = account.AccountCode.Should().Be("1000");
        _ = account.Category.Should().Be(AccountCategory.Asset);
        _ = account.IsActive.Should().BeTrue();

        _ = isBalancedBefore.Should().BeFalse();
        _ = isBalancedAfter.Should().BeTrue();
        _ = entry.TotalDebit.Should().Be(1500.00m);
        _ = entry.TotalCredit.Should().Be(1500.00m);
        _ = entry.Lines.Should().HaveCount(2);
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
            tenantId,
            2026,
            1,
            VatSettlementPeriodType.Monthly,
            Jurisdiction.Italy,
            outputVat: 5000m,
            inputVat: 3000m,
            priorCredit: 500m);

        // Act
        settlement.FinalizeSettlement();

        // Assert
        _ = settlement.Year.Should().Be(2026);
        _ = settlement.PeriodNumber.Should().Be(1);
        _ = settlement.PeriodType.Should().Be(VatSettlementPeriodType.Monthly);
        _ = settlement.TotalOutputVat.Should().Be(5000m);
        _ = settlement.TotalInputVat.Should().Be(3000m);
        _ = settlement.PriorCreditCarriedForward.Should().Be(500m);
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
            "CORP-IT",
            "Corporation IT SpA",
            "IT",
            Currency.EUR,
            "contact@corp.it");
        var address = new Address(
            "Via Roma 1",
            "Roma",
            "00100",
            "RM",
            "IT");

        // Act
        tenant.AddBranch(
            "HQ",
            "Headquarters Roma",
            address,
            true);

        var rate = ExchangeRate.Create(
            Currency.EUR,
            Currency.CHF,
            0.9550m,
            DateTime.UtcNow,
            "ECB");
        var trans = AppTranslation.Create(
            "de",
            "Buttons.Save",
            "Speichern",
            null,
            tenant.Id);
        trans.UpdateCustomValue("Sichern");

        var user = User.Create(
            tenant.Id,
            "admin",
            "admin@corp.it",
            "Administrator",
            "hash1",
            "salt1",
            UserRole.SuperAdmin);
        user.RecordLogin();
        user.ChangePassword(
            "hash2",
            "salt2");

        // Assert
        _ = tenant.Branches.Should().HaveCount(1);
        _ = tenant.Branches.First().IsHeadquarters.Should().BeTrue();

        _ = rate.Rate.Should().Be(0.9550m);
        _ = trans.EffectiveValue.Should().Be("Sichern");

        _ = user.LastLoginUtc.Should().NotBeNull();
        _ = user.PasswordHash.Should().Be("hash2");
        _ = user.PasswordSalt.Should().Be("salt2");
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
            tenantId,
            "MAT-100",
            clientId,
            "Fiscale Internazionale",
            "Consulenza");
        var entry = BillableTimeEntry.Create(
            tenantId,
            matter.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            240,
            200m,
            Currency.EUR,
            "Analisi normativa");

        // Act
        var note = ProfessionalFeeNote.CreateItalian(
            tenantId,
            "NOTA-2026-001",
            matter.Id,
            DateTime.UtcNow,
            grossFees: 1000m,
            nonTaxableExpenses: 50m,
            vatRate: 22m,
            cassaPercent: 4m,
            ritenutaPercent: 20m);

        // Assert
        _ = matter.MatterNumber.Should().Be("MAT-100");
        _ = entry.MinutesSpent.Should().Be(240);
        _ = entry.HourlyRate.Should().Be(200m);

        _ = note.GrossProfessionalFees.Should().Be(1000m);
        _ = note.ItCassaPrevidenzialeAmount.Should().Be(40m); // 4% of 1000
        _ = note.TotalVat.Should().Be(228.80m); // 22% of (1000 + 40)
        _ = note.ItRitenutaAccontoAmount.Should().Be(200m); // 20% of 1000
        _ = note.NetPayable.Should().Be(1118.80m); // 1000 + 40 + 228.80 + 50 - 200
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
            tenantId,
            clientId,
            "AA123BB",
            "ZFA12345678901234",
            "Fiat",
            "500",
            2021,
            Jurisdiction.Italy,
            15000);

        var inspection =
            StatutoryInspectionRecord.Schedule(
                tenantId,
                vehicle.Id,
                InspectionType.ItalianRevisione,
                DateTime.UtcNow);
        var tire = TireHotelRecord.CheckIn(
            tenantId,
            vehicle.Id,
            TireSeason.Winter,
            "Michelin",
            "205/55R16",
            "DOT2024",
            6.5m,
            6.5m,
            6.0m,
            6.0m,
            "BIN-12A");

        // Act
        inspection.MarkCompleted(
            true,
            "Superata senza rilievi",
            DateTime.UtcNow.AddYears(2));
        tire.MountOnVehicle();

        // Assert
        _ = vehicle.LicensePlate.Should().Be("AA123BB");
        _ = inspection.Status.Should().Be("Passed");
        _ = tire.Status.Should().Be("MountedOnVehicle");
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
            "Via Cantiere 5",
            "Milano",
            "20100",
            "MI",
            "IT");
        var site = ConstructionSite.Create(
            tenantId,
            "SITE-2026",
            "Edificio Alfa",
            clientId,
            address,
            "Mario Bianchi",
            DateTime.UtcNow,
            Money.Euro(1000000m));
        var boq = BillOfQuantitiesItem.Create(
            tenantId,
            site.Id,
            "ITEM-01",
            StandardType.ItalianComputo,
            "Demolizioni",
            "m2",
            200m,
            50m);
        var dico = DeclarationOfConformity.Create(
            tenantId,
            site.Id,
            "DICO-001",
            "DM 37/08",
            "Ing. Rossi",
            DateTime.UtcNow,
            "Impianto Elettrico");

        // Act
        var sal = ProgressBillingSal.Create(
            tenantId,
            site.Id,
            1,
            DateTime.UtcNow,
            0.25m,
            Money.Euro(100000m),
            Money.Euro(5000m));

        // Assert
        _ = site.ProjectCode.Should().Be("SITE-2026");
        _ = boq.ItemCode.Should().Be("ITEM-01");
        _ = dico.CertificateNumber.Should().Be("DICO-001");

        _ = sal.TotalCertifiedAmount.Amount.Should().Be(100000m);
        _ = sal.RetentionGuaranteeAmount.Amount.Should().Be(5000m);
        _ = sal.CumulativePercentage.Should().Be(0.25m);
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
            tenantId,
            "FL-01",
            "TI-100200",
            "Audi A4 Avant",
            Money.Chf(120m),
            25000);
        var booking = RentalBooking.Create(
            tenantId,
            "BOOK-01",
            vehicle.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(5),
            "Lugano",
            "Lugano",
            Money.Chf(600m),
            Money.Chf(1000m));
        var appraisal = DamageAppraisal.Create(
            tenantId,
            booking.Id,
            "Carrozzeria",
            "{}",
            DamageSeverity.Minor,
            Money.Chf(450m));

        // Assert
        _ = vehicle.LicensePlate.Should().Be("TI-100200");
        _ = booking.TotalRentAmount.Amount.Should().Be(600m);
        _ = booking.DepositAmount.Amount.Should().Be(1000m);
        _ = appraisal.EstimatedRepairCost.Amount.Should().Be(450m);
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
            "Via Logistica 10",
            "Verona",
            "37100",
            "VR",
            "IT");
        var warehouse = Warehouse.Create(
            tenantId,
            "WH-VR",
            "Hub Verona",
            address,
            10000m);
        var ddt = TransportDocument.Create(
            tenantId,
            "DDT-01",
            DateTime.UtcNow,
            Guid.NewGuid(),
            address,
            "Vendita",
            "DHL Express",
            5,
            25.5m);
        var movement = StockMovement.Create(
            tenantId,
            Guid.NewGuid(),
            warehouse.Id,
            100m,
            MovementType.Receipt,
            "BATCH-1",
            "SN-1",
            "PO-100");

        // Assert
        _ = warehouse.Code.Should().Be("WH-VR");
        _ = ddt.DocumentNumber.Should().Be("DDT-01");
        _ = movement.Quantity.Should().Be(100m);
        _ = movement.MovementType.Should().Be(MovementType.Receipt);
        _ = movement.BatchNumber.Should().Be("BATCH-1");
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
            tenantId,
            "WC-MILL",
            "Fresatura CNC",
            75m,
            8m);
        var po = ProductionOrder.Create(
            tenantId,
            "PO-2026-10",
            Guid.NewGuid(),
            500m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(10));

        // Assert
        _ = wc.Code.Should().Be("WC-MILL");
        _ = wc.HourlyMachineCost.Should().Be(75m);
        _ = po.OrderNumber.Should().Be("PO-2026-10");
        _ = po.QuantityToProduce.Should().Be(500m);
    }

    /// <summary>
    /// Tests Retail POS session open, close, and receipt issuance.
    /// </summary>
    [Fact]
    public void RetailPos_SessionAndReceipt_ExecuteFlowCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cashierId = Guid.NewGuid();
        var session = PosTerminalSession.Open(
            tenantId,
            "POS-01",
            cashierId,
            Money.Euro(150m));
        var receipt = RetailReceipt.Create(
            tenantId,
            session.Id,
            "REC-101",
            65.50m,
            Currency.EUR,
            PaymentMethod.CardSumUpWorldline,
            "RT-LOT-999");

        // Act
        session.CloseSession(
            Money.Euro(500m),
            Money.Euro(500m));

        // Assert
        _ = session.IsClosed.Should().BeTrue();
        _ = session.CashDiscrepancy.Amount.Should().Be(0m);
        _ = receipt.TotalAmount.Should().Be(65.50m);
        _ = receipt.PaymentMethod.Should().Be(PaymentMethod.CardSumUpWorldline);
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
            tenantId,
            "Listino Retail",
            Currency.EUR,
            true);
        var quote = Quote.Create(
            tenantId,
            "Q-2026-05",
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(30),
            Money.Euro(2500m));

        // Assert
        _ = list.Name.Should().Be("Listino Retail");
        _ = list.IsDefault.Should().BeTrue();
        _ = quote.QuoteNumber.Should().Be("Q-2026-05");
        _ = quote.TotalAmount.Amount.Should().Be(2500m);
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
            tenantId,
            "WO-2026-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            hoursWorked: 3m,
            hourlyRate: 85m,
            Currency.EUR,
            "Sostituzione pompa di calore");

        // Assert
        _ = order.WorkOrderNumber.Should().Be("WO-2026-001");
        _ = order.WorkPerformedSummary.Should().Be("Sostituzione pompa di calore");
        _ = order.HoursWorked.Should().Be(3m);
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
            tenantId,
            "STMT-01",
            "IT60A0000000000000000000000",
            Currency.EUR,
            5000m,
            8000m,
            DateTime.UtcNow);

        // Act
        stmt.AddTransaction(
            DateTime.UtcNow,
            DateTime.UtcNow,
            3000m,
            "Incasso bonifico cliente",
            "IT12X0000000000000000000000",
            "Cliente SpA");

        // Assert
        _ = stmt.OpeningBalance.Should().Be(5000m);
        _ = stmt.ClosingBalance.Should().Be(8000m);
        _ = stmt.Lines.Should().HaveCount(1);
        _ = stmt.Lines.First().RemittanceInformation.Should().Be("Incasso bonifico cliente");
    }

    /// <summary>
    /// Tests CurrentUserService and TenantProvider infrastructure services.
    /// </summary>
    [Fact]
    public void Security_CurrentUserServiceAndTenantProvider_ManageContext()
    {
        // Arrange
        var userService = new CurrentUserService();
        var tenantProvider = new TenantProvider();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        userService.SetUser(
            userId,
            "johndoe",
            "John Doe",
            "Admin");
        tenantProvider.SetTenant(
            tenantId,
            "TENANT-01");

        // Assert
        _ = userService.IsAuthenticated.Should().BeTrue();
        _ = userService.Username.Should().Be("johndoe");
        _ = userService.FullName.Should().Be("John Doe");
        _ = userService.Role.Should().Be("Admin");

        _ = tenantProvider.HasTenant.Should().BeTrue();
        _ = tenantProvider.CurrentTenantId.Should().Be(tenantId);
        _ = tenantProvider.CurrentTenantCode.Should().Be("TENANT-01");

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
        _ = services.AddInfrastructure("Data Source=:memory:");
        using var provider = services.BuildServiceProvider();

        // Assert
        _ = provider.GetRequiredService<ITenantProvider>().Should().NotBeNull();
        _ = provider.GetRequiredService<ICurrentUserService>().Should().NotBeNull();
        _ = provider.GetRequiredService<IPasswordHasher>().Should().NotBeNull();
        _ = provider.GetRequiredService<ILocalizationService>().Should().NotBeNull();
    }

    private sealed record TestDomainEvent(string Data) : DomainEventBase;

    private sealed class TestAggregateRoot : AggregateRoot<Guid>
    {
        public TestAggregateRoot(
            Guid id)
            : base(id)
        {
        }
    }
}
