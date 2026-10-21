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
        evt.EventId.Should().NotBeEmpty();
        evt.OccurredOnUtc.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(5));
        aggregate.DomainEvents.Should().ContainSingle();

        // Act 2
        aggregate.ClearDomainEvents();

        // Assert 2
        aggregate.DomainEvents.Should().BeEmpty();
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
        account.AccountCode.Should().Be("1000");
        account.Category.Should().Be(AccountCategory.Asset);
        account.IsActive.Should().BeTrue();

        isBalancedBefore.Should().BeFalse();
        isBalancedAfter.Should().BeTrue();
        entry.TotalDebit.Should().Be(1500.00m);
        entry.TotalCredit.Should().Be(1500.00m);
        entry.Lines.Should().HaveCount(2);
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
        settlement.Year.Should().Be(2026);
        settlement.PeriodNumber.Should().Be(1);
        settlement.PeriodType.Should().Be(VatSettlementPeriodType.Monthly);
        settlement.TotalOutputVat.Should().Be(5000m);
        settlement.TotalInputVat.Should().Be(3000m);
        settlement.PriorCreditCarriedForward.Should().Be(500m);
        settlement.IsFinalized.Should().BeTrue();
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
        tenant.Branches.Should().HaveCount(1);
        tenant.Branches.First().IsHeadquarters.Should().BeTrue();

        rate.Rate.Should().Be(0.9550m);
        trans.EffectiveValue.Should().Be("Sichern");

        user.LastLoginUtc.Should().NotBeNull();
        user.PasswordHash.Should().Be("hash2");
        user.PasswordSalt.Should().Be("salt2");
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
        matter.MatterNumber.Should().Be("MAT-100");
        entry.MinutesSpent.Should().Be(240);
        entry.HourlyRate.Should().Be(200m);

        note.GrossProfessionalFees.Should().Be(1000m);
        note.ItCassaPrevidenzialeAmount.Should().Be(40m); // 4% of 1000
        note.TotalVat.Should().Be(228.80m); // 22% of (1000 + 40)
        note.ItRitenutaAccontoAmount.Should().Be(200m); // 20% of 1000
        note.NetPayable.Should().Be(1118.80m); // 1000 + 40 + 228.80 + 50 - 200
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
        vehicle.LicensePlate.Should().Be("AA123BB");
        inspection.Status.Should().Be("Passed");
        tire.Status.Should().Be("MountedOnVehicle");
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
        site.ProjectCode.Should().Be("SITE-2026");
        boq.ItemCode.Should().Be("ITEM-01");
        dico.CertificateNumber.Should().Be("DICO-001");

        sal.TotalCertifiedAmount.Amount.Should().Be(100000m);
        sal.RetentionGuaranteeAmount.Amount.Should().Be(5000m);
        sal.CumulativePercentage.Should().Be(0.25m);
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
        vehicle.LicensePlate.Should().Be("TI-100200");
        booking.TotalRentAmount.Amount.Should().Be(600m);
        booking.DepositAmount.Amount.Should().Be(1000m);
        appraisal.EstimatedRepairCost.Amount.Should().Be(450m);
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
        warehouse.Code.Should().Be("WH-VR");
        ddt.DocumentNumber.Should().Be("DDT-01");
        movement.Quantity.Should().Be(100m);
        movement.MovementType.Should().Be(MovementType.Receipt);
        movement.BatchNumber.Should().Be("BATCH-1");
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
        wc.Code.Should().Be("WC-MILL");
        wc.HourlyMachineCost.Should().Be(75m);
        po.OrderNumber.Should().Be("PO-2026-10");
        po.QuantityToProduce.Should().Be(500m);
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
        session.IsClosed.Should().BeTrue();
        session.CashDiscrepancy.Amount.Should().Be(0m);
        receipt.TotalAmount.Should().Be(65.50m);
        receipt.PaymentMethod.Should().Be(PaymentMethod.CardSumUpWorldline);
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
        list.Name.Should().Be("Listino Retail");
        list.IsDefault.Should().BeTrue();
        quote.QuoteNumber.Should().Be("Q-2026-05");
        quote.TotalAmount.Amount.Should().Be(2500m);
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
        order.WorkOrderNumber.Should().Be("WO-2026-001");
        order.WorkPerformedSummary.Should().Be("Sostituzione pompa di calore");
        order.HoursWorked.Should().Be(3m);
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
        stmt.OpeningBalance.Should().Be(5000m);
        stmt.ClosingBalance.Should().Be(8000m);
        stmt.Lines.Should().HaveCount(1);
        stmt.Lines.First().RemittanceInformation.Should().Be("Incasso bonifico cliente");
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
        userService.IsAuthenticated.Should().BeTrue();
        userService.Username.Should().Be("johndoe");
        userService.FullName.Should().Be("John Doe");
        userService.Role.Should().Be("Admin");

        tenantProvider.HasTenant.Should().BeTrue();
        tenantProvider.CurrentTenantId.Should().Be(tenantId);
        tenantProvider.CurrentTenantCode.Should().Be("TENANT-01");

        // Act 2
        userService.Clear();

        // Assert 2
        userService.IsAuthenticated.Should().BeFalse();
        userService.UserId.Should().BeNull();
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
        services.AddInfrastructure("Data Source=:memory:");
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<ITenantProvider>().Should().NotBeNull();
        provider.GetRequiredService<ICurrentUserService>().Should().NotBeNull();
        provider.GetRequiredService<IPasswordHasher>().Should().NotBeNull();
        provider.GetRequiredService<ILocalizationService>().Should().NotBeNull();
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
