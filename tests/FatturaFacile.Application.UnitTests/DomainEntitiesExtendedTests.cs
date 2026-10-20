using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities.Accounting;
using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Entities.Advisory;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Entities.Construction;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Dashboard;
using FatturaFacile.Domain.Entities.FleetRental;
using FatturaFacile.Domain.Entities.Inventory;
using FatturaFacile.Domain.Entities.Manufacturing;
using FatturaFacile.Domain.Entities.PayrollHr;
using FatturaFacile.Domain.Entities.Purchasing;
using FatturaFacile.Domain.Entities.RetailPos;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Entities.ServiceTickets;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Entities.Treasury;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Contains extended unit tests for all domain entity models and value object equality behaviors.
/// </summary>
public sealed class DomainEntitiesExtendedTests
{
    private sealed record SampleDomainEvent : DomainEventBase;

    private sealed class SampleValueObject(string prop1, int prop2) : ValueObject
    {
        public string Prop1 { get; } = prop1;

        public int Prop2 { get; } = prop2;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Prop1;
            yield return Prop2;
        }
    }

    /// <summary>
    /// Tests that the sample domain event base records accurate occurrence timestamps.
    /// </summary>
    [Fact]
    public void DomainEventBase_WhenInstantiated_SetsOccurredOnUtc()
    {
        // Arrange & Act
        var evt = new SampleDomainEvent();

        // Assert
        evt.OccurredOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Tests value object equality, inequality, and hash codes.
    /// </summary>
    [Fact]
    public void ValueObject_EqualityAndInequality_WorksCorrectly()
    {
        // Arrange
        var vo1 = new SampleValueObject("Alfa", 100);
        var vo2 = new SampleValueObject("Alfa", 100);
        var vo3 = new SampleValueObject("Beta", 200);
        SampleValueObject? nullVo = null;

        // Act & Assert
        vo1.Equals(vo2).Should().BeTrue();
        (vo1 == vo2).Should().BeTrue();
        (vo1 != vo3).Should().BeTrue();
        vo1.Equals(nullVo).Should().BeFalse();
        vo1.Equals(new object()).Should().BeFalse();
        vo1.GetHashCode().Should().Be(vo2.GetHashCode());
    }

    /// <summary>
    /// Tests general ledger account creation and properties.
    /// </summary>
    [Fact]
    public void GeneralLedgerAccount_Create_InitializesProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var account = GeneralLedgerAccount.Create(tenantId, " 1020 ", " Bank Account ", AccountCategory.Asset, Jurisdiction.Switzerland);

        // Assert
        account.Id.Should().NotBeEmpty();
        account.TenantId.Should().Be(tenantId);
        account.AccountCode.Should().Be("1020");
        account.Name.Should().Be("Bank Account");
        account.Category.Should().Be(AccountCategory.Asset);
        account.Jurisdiction.Should().Be(Jurisdiction.Switzerland);
        account.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Tests general ledger entry line addition, balancing, and totals.
    /// </summary>
    [Fact]
    public void GeneralLedgerEntry_CreateAndAddLines_CalculatesDebitCreditAndBalancing()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entry = GeneralLedgerEntry.Create(tenantId, "PNL-01", DateTime.UtcNow, "Year End Closing", Currency.EUR, "DOC-123");
        var debitAcc = Guid.NewGuid();
        var creditAcc = Guid.NewGuid();

        // Act
        entry.AddLine(debitAcc, 1000m, 0m, "Debit line");
        entry.AddLine(creditAcc, 0m, 1000m, "Credit line");

        // Assert
        entry.TenantId.Should().Be(tenantId);
        entry.EntryNumber.Should().Be("PNL-01");
        entry.ReferenceDocument.Should().Be("DOC-123");
        entry.Lines.Should().HaveCount(2);
        entry.TotalDebit.Should().Be(1000m);
        entry.TotalCredit.Should().Be(1000m);
        entry.IsBalanced.Should().BeTrue();
    }

    /// <summary>
    /// Tests vat settlement period creation, payable/credit calculation, and finalization.
    /// </summary>
    [Fact]
    public void VatSettlementPeriod_CreateAndFinalize_CalculatesPayableAndCredit()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act - Payable scenario
        var periodPayable = VatSettlementPeriod.Create(tenantId, 2026, 1, VatSettlementPeriodType.Monthly, Jurisdiction.Italy, 5000m, 3000m, 500m);
        periodPayable.FinalizeSettlement();

        // Act - Credit scenario
        var periodCredit = VatSettlementPeriod.Create(tenantId, 2026, 2, VatSettlementPeriodType.Monthly, Jurisdiction.Italy, 2000m, 4000m, 100m);

        // Assert
        periodPayable.NetVatPayable.Should().Be(1500m);
        periodPayable.NetVatCredit.Should().Be(0m);
        periodPayable.IsFinalized.Should().BeTrue();
        periodPayable.FinalizedAtUtc.Should().NotBeNull();

        periodCredit.NetVatPayable.Should().Be(0m);
        periodCredit.NetVatCredit.Should().Be(2100m);
        periodCredit.IsFinalized.Should().BeFalse();
    }

    /// <summary>
    /// Tests contact and supplier entity creation and properties.
    /// </summary>
    [Fact]
    public void SupplierAndContact_Create_InitializesProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var address = new Address("Via Fornaci 1", "Lugano", "6900", "TI", "CH");
        var vat = new VatNumber("CHE111222333", "CH");

        // Act
        var supplier = Supplier.Create(tenantId, "Tech Supply SA", Jurisdiction.Switzerland, address, vatNumber: vat, paymentTermsDays: 45);
        var client = Client.CreateItalian(
            tenantId,
            "Client With Contacts",
            new FiscalCode("RSSMRA80A01F205X"),
            new VatNumber("12345678901", "IT"),
            "0000000",
            "contact@pec.it",
            address,
            "info@contact.it");

        client.AddContact("Mario", "Rossi", "mario@rossi.it", "+39 333 1234567", "CTO", isPrimary: true);

        // Assert
        supplier.Id.Should().NotBeEmpty();
        supplier.TenantId.Should().Be(tenantId);
        supplier.Name.Should().Be("Tech Supply SA");
        supplier.VatNumber.Should().Be(vat);
        supplier.PaymentTermsDays.Should().Be(45);
        supplier.IsActive.Should().BeTrue();

        client.Contacts.Should().HaveCount(1);
        var contact = client.Contacts.First();
        contact.FirstName.Should().Be("Mario");
        contact.LastName.Should().Be("Rossi");
        contact.IsPrimary.Should().BeTrue();
    }

    /// <summary>
    /// Tests customer vehicle and statutory inspection record creation and pass status.
    /// </summary>
    [Fact]
    public void Automotive_VehicleAndInspection_InitializesAndRecords()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        // Act
        var vehicle = CustomerVehicle.Create(tenantId, clientId, "AA123BB", "VIN12345678901234", "Audi", "A4", 2022, Jurisdiction.Italy, 45000);
        var inspection = StatutoryInspectionRecord.Schedule(
            tenantId,
            vehicle.Id,
            InspectionType.ItalianRevisione,
            DateTime.UtcNow);

        inspection.MarkCompleted(true, "Everything in order", DateTime.UtcNow.AddYears(2));

        // Assert
        vehicle.LicensePlate.Should().Be("AA123BB");
        vehicle.VinChassisNumber.Should().Be("VIN12345678901234");
        vehicle.CurrentKilometers.Should().Be(45000);

        inspection.VehicleId.Should().Be(vehicle.Id);
        inspection.Status.Should().Be("Passed");
        inspection.InspectorCenterNotes.Should().Be("Everything in order");
    }

    /// <summary>
    /// Tests construction domain entities: site, bill of quantities, progress billing, and declaration of conformity.
    /// </summary>
    [Fact]
    public void Construction_SiteSalAndConformity_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var addr = new Address("Via Cantiere 5", "Como", "22100", "CO", "IT");
        var budget = Money.Euro(500000m);

        // Act
        var site = ConstructionSite.Create(tenantId, "CANT-2026-01", "Ristrutturazione Edificio", clientId, addr, "Geom. Rossi", DateTime.UtcNow, budget);
        var item = BillOfQuantitiesItem.Create(tenantId, site.Id, "01.01", StandardType.ItalianComputo, "Demolizione tramezzi", "m2", 150m, 25m);
        var sal = ProgressBillingSal.Create(tenantId, site.Id, 1, DateTime.UtcNow, 35m, Money.Euro(50000m), Money.Euro(2500m));
        var cert = DeclarationOfConformity.Create(tenantId, site.Id, "CERT-001", "DM 37/08", "Ing. Neri", DateTime.UtcNow, "Impianto Elettrico");

        // Assert
        site.ProjectCode.Should().Be("CANT-2026-01");
        site.TotalContractBudget.Amount.Should().Be(500000m);

        item.Subtotal.Should().Be(3750m);
        sal.SalNumber.Should().Be(1);
        sal.CumulativePercentage.Should().Be(35m);
        cert.StandardReference.Should().Be("DM 37/08");
        cert.DeclarantEngineerName.Should().Be("Ing. Neri");
    }

    /// <summary>
    /// Tests fleet rental vehicle, booking, and damage appraisal.
    /// </summary>
    [Fact]
    public void FleetRental_VehicleBookingAndDamage_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        // Act
        var vehicle = RentalVehicle.Create(tenantId, "FLT-01", "TI-12345", "BMW 118i", Money.Chf(90m), 20000);
        var booking = RentalBooking.Create(
            tenantId,
            "BOOK-01",
            vehicle.Id,
            clientId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(3),
            "Lugano Stazione",
            "Lugano Stazione",
            Money.Chf(270m),
            Money.Chf(500m));

        var damage = DamageAppraisal.Create(
            tenantId,
            booking.Id,
            "CheckOut",
            "[]",
            DamageSeverity.Minor,
            Money.Chf(350m));

        // Assert
        vehicle.LicensePlate.Should().Be("TI-12345");
        vehicle.DailyRate.Amount.Should().Be(90m);
        booking.TotalRentAmount.Amount.Should().Be(270m);
        damage.Severity.Should().Be(DamageSeverity.Minor);
        damage.EstimatedRepairCost.Amount.Should().Be(350m);
    }

    /// <summary>
    /// Tests inventory domain entities: catalog, warehouse, stock movement, and transport document.
    /// </summary>
    [Fact]
    public void Inventory_CatalogWarehouseAndStock_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var addr = new Address("Via Logistica 10", "Milano", "20100", "MI", "IT");

        // Act
        var catalog = ItemCatalog.Create(tenantId, "SKU-999", "Bullone M8", "Bulloneria", "PZ", 0.50m, 1.20m, Currency.EUR, 22.0m);
        var warehouse = Warehouse.Create(tenantId, "WH-MAIN", "Magazzino Centrale", addr, 1000m);
        var movement = StockMovement.Create(tenantId, catalog.Id, warehouse.Id, 100m, MovementType.Receipt, "LOT-A", "SN-1", "PO-100");
        var ddt = TransportDocument.Create(tenantId, "DDT-001", DateTime.UtcNow, Guid.NewGuid(), addr, "Vendita", "Mittente", 1, 10m);

        // Assert
        catalog.Code.Should().Be("SKU-999");
        warehouse.Code.Should().Be("WH-MAIN");
        movement.Quantity.Should().Be(100m);
        movement.MovementType.Should().Be(MovementType.Receipt);
        ddt.DocumentNumber.Should().Be("DDT-001");
    }

    /// <summary>
    /// Tests manufacturing bill of materials, work center, and production order.
    /// </summary>
    [Fact]
    public void Manufacturing_BomWorkCenterAndProductionOrder_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var bom = BillOfMaterials.Create(tenantId, Guid.NewGuid(), "BOM-001", "1.0");
        bom.AddComponent(Guid.NewGuid(), 2m, 0.05m);
        var wc = WorkCenter.Create(tenantId, "WC-CNC", "Centro di Lavoro CNC", 85.00m);
        var order = ProductionOrder.Create(tenantId, "ORD-2026-001", bom.Id, 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        order.ReportProgress(10m);

        // Assert
        bom.BomCode.Should().Be("BOM-001");
        bom.Lines.Should().HaveCount(1);
        wc.HourlyMachineCost.Should().Be(85.00m);
        order.QuantityToProduce.Should().Be(10m);
        order.Status.Should().Be(ProductionStatus.Completed);
    }

    /// <summary>
    /// Tests payroll slip summary and employee absence records.
    /// </summary>
    [Fact]
    public void PayrollHr_PayrollSummaryAndAbsence_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var empId = Guid.NewGuid();

        // Act
        var payroll = PayrollSlipSummary.CreateItalian(
            tenantId,
            empId,
            2026,
            5,
            3000m,
            300m,
            500m);

        var absence = AbsenceRecord.Create(
            tenantId,
            empId,
            AbsenceType.Ferie,
            new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            5m,
            "INPS-12345",
            "Approved vacation");

        absence.Approve();

        // Assert
        payroll.GrossSalary.Should().Be(3000m);
        payroll.NetSalaryPaid.Should().Be(2200m);
        absence.TotalWorkingDays.Should().Be(5m);
        absence.AbsenceType.Should().Be(AbsenceType.Ferie);
        absence.IsApproved.Should().BeTrue();
    }

    /// <summary>
    /// Tests purchasing passive invoice and payment instructions.
    /// </summary>
    [Fact]
    public void Purchasing_OrderPassiveInvoiceAndPaymentInstruction_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var supId = Guid.NewGuid();

        // Act
        var po = PurchaseOrder.Create(tenantId, "PO-2026/01", supId, Money.Euro(1500m));
        var invoice = PassiveInvoice.Create(
            tenantId,
            supId,
            "FAT-FORN-99",
            DateTime.UtcNow,
            1229.51m,
            270.49m,
            Currency.EUR,
            DateTime.UtcNow.AddDays(30));

        var payment = Iso20022PaymentInstruction.Create(
            tenantId,
            "IT60A0000000000000000000000",
            "IT12X0000000000000000000000",
            "Supplier SpA",
            Money.Euro(1500m),
            "Salvataggio Fattura",
            DateTime.UtcNow.AddDays(2));

        // Assert
        po.OrderNumber.Should().Be("PO-2026/01");
        invoice.DocumentNumber.Should().Be("FAT-FORN-99");
        payment.Amount.Amount.Should().Be(1500m);
    }

    /// <summary>
    /// Tests retail POS terminal session and receipts.
    /// </summary>
    [Fact]
    public void RetailPos_SessionAndReceipt_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var session = PosTerminalSession.Open(tenantId, "POS-01", Guid.NewGuid(), Money.Euro(200m));
        var receipt = RetailReceipt.Create(
            tenantId,
            session.Id,
            "REC-001",
            120m,
            Currency.EUR,
            PaymentMethod.Cash,
            "RT-SN-123456",
            "Z-99");

        session.CloseSession(Money.Euro(320m), Money.Euro(320m));

        // Assert
        session.TerminalCode.Should().Be("POS-01");
        session.IsClosed.Should().BeTrue();
        receipt.TotalAmount.Should().Be(120m);
    }

    /// <summary>
    /// Tests sales order, quote, and price list.
    /// </summary>
    [Fact]
    public void Sales_OrderQuoteAndPriceList_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        // Act
        var quote = Quote.Create(tenantId, "PREV-01", clientId, DateTime.UtcNow.AddDays(30), Money.Euro(2500m));
        var order = SalesOrder.Create(tenantId, "ORD-CLI-01", clientId, Currency.EUR);
        var priceList = PriceList.Create(tenantId, "LISTINO-BASE", Currency.EUR, true);

        // Assert
        quote.QuoteNumber.Should().Be("PREV-01");
        quote.TotalAmount.Amount.Should().Be(2500m);
        order.OrderNumber.Should().Be("ORD-CLI-01");
        priceList.Name.Should().Be("LISTINO-BASE");
    }

    /// <summary>
    /// Tests service tickets and work orders.
    /// </summary>
    [Fact]
    public void ServiceTickets_TicketAndWorkOrder_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        // Act
        var ticket = SupportTicket.Create(
            tenantId,
            "TICK-001",
            clientId,
            "Errore rete",
            "Impossibile collegarsi alla stampante",
            RiskLevel.Medium,
            DateTime.UtcNow.AddHours(24));

        var workOrder = TechnicianWorkOrder.Create(
            tenantId,
            "WO-001",
            ticket.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            2.5m,
            65m,
            Currency.EUR,
            "Risolto problema IP statico");

        workOrder.SignByCustomer("Mario Rossi", "data:image/svg+xml;base64,PHN2Zz48L3N2Zz4=");

        // Assert
        ticket.Title.Should().Be("Errore rete");
        ticket.Priority.Should().Be(RiskLevel.Medium);
        workOrder.IsSignedByCustomer.Should().BeTrue();
        workOrder.HoursWorked.Should().Be(2.5m);
    }

    /// <summary>
    /// Tests advisory client matter, billable time entry, and professional fee note.
    /// </summary>
    [Fact]
    public void Advisory_MatterFeeNoteAndBillableTime_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        // Act
        var matter = ClientMatter.Create(tenantId, "MAT-01", clientId, "Contenzioso Tributario", "TaxAdvisory");
        matter.VerifyKyc(RiskLevel.Medium);

        var time = BillableTimeEntry.Create(
            tenantId,
            matter.Id,
            Guid.NewGuid(),
            DateTime.UtcNow,
            180, // 3 ore
            250m,
            Currency.EUR,
            "Preparazione ricorso");

        var feeNote = ProfessionalFeeNote.CreateItalian(
            tenantId,
            "NOTA-01",
            matter.Id,
            DateTime.UtcNow,
            750m,
            50m,
            22.0m,
            4.0m,
            20.0m);

        // Assert
        matter.MatterNumber.Should().Be("MAT-01");
        matter.KycAmlVerified.Should().BeTrue();
        time.TotalFee.Should().Be(750m);
        feeNote.DocumentNumber.Should().Be("NOTA-01");
        feeNote.ItRitenutaAccontoAmount.Should().Be(150m);
        feeNote.NetPayable.Should().BeGreaterThan(0m);
    }

    /// <summary>
    /// Tests treasury bank statements and transaction lines.
    /// </summary>
    [Fact]
    public void Treasury_BankStatementAndLines_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var statement = BankStatement.Create(
            tenantId,
            "STMT-01",
            "CH1234567890",
            Currency.CHF,
            10000m,
            12500m,
            DateTime.UtcNow);

        statement.AddTransaction(DateTime.UtcNow, DateTime.UtcNow, 2500m, "Pagamento fattura cliente", "CH123", "Cliente SA");

        // Assert
        statement.AccountIban.Should().Be("CH1234567890");
        statement.OpeningBalance.Should().Be(10000m);
        statement.ClosingBalance.Should().Be(12500m);
        statement.Lines.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests dashboard KPI summary, tax deadlines, branches, and exchange rates.
    /// </summary>
    [Fact]
    public void DashboardAndAdmin_Entities_WorkCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var branchAddr = new Address("Via Cantonale 1", "Mendrisio", "6850", "TI", "CH");

        // Act
        var kpi = ExecutiveKpiSummary.Create(
            tenantId,
            "2026-Q1",
            Money.Euro(100000m),
            Money.Chf(80000m),
            Money.Euro(20000m),
            Money.Chf(15000m),
            Money.Euro(5000m),
            Money.Chf(3000m),
            DateTime.UtcNow.AddDays(15),
            Jurisdiction.Italy);

        var taxDeadline = TaxDeadlineAlert.Create(
            tenantId,
            "Liquidazione IVA Trimestrale",
            "Versamento F24",
            DateTime.UtcNow.AddDays(15),
            Money.Euro(4500m),
            Jurisdiction.Italy);

        var tenant = Tenant.Create("T-01", "Main Tenant", "CH", Currency.CHF, "admin@main.ch");
        tenant.AddBranch("BR-01", "Filiale Ticino", branchAddr, isHeadquarters: false);

        var rate = ExchangeRate.Create(Currency.EUR, Currency.CHF, 0.96m, DateTime.UtcNow, "ECB");

        // Assert
        kpi.TotalRevenueEur.Amount.Should().Be(100000m);
        taxDeadline.Title.Should().Be("Liquidazione IVA Trimestrale");
        (taxDeadline.DueDate - DateTime.UtcNow).TotalDays.Should().BeGreaterThan(0);
        tenant.Branches.Should().HaveCount(1);
        rate.Rate.Should().Be(0.96m);
    }
}
