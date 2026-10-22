// <copyright file="DomainEntitiesExtendedTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

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

    /// <summary>
    /// Tests that the sample domain event base records accurate occurrence timestamps.
    /// </summary>
    [Fact]
    public void DomainEventBase_WhenInstantiated_SetsOccurredOnUtc()
    {
        // Arrange & Act
        var evt = new SampleDomainEvent();

        // Assert
        _ = evt.OccurredOnUtc.Should()
               .BeCloseTo(
                          nearbyTime: DateTime.UtcNow,
                          precision: TimeSpan.FromSeconds(seconds: 2));
    }

    /// <summary>
    /// Tests value object equality, inequality, and hash codes.
    /// </summary>
    [Fact]
    public void ValueObject_EqualityAndInequality_WorksCorrectly()
    {
        // Arrange
        var vo1 = new SampleValueObject(
                                        prop1: "Alfa",
                                        prop2: 100);

        var vo2 = new SampleValueObject(
                                        prop1: "Alfa",
                                        prop2: 100);

        var vo3 = new SampleValueObject(
                                        prop1: "Beta",
                                        prop2: 200);

        SampleValueObject? nullVo = null;

        // Act & Assert
        _ = vo1.Equals(other: vo2).Should().BeTrue();
        _ = (vo1 == vo2).Should().BeTrue();
        _ = (vo1 != vo3).Should().BeTrue();
        _ = vo1.Equals(other: nullVo).Should().BeFalse();
        _ = vo1.Equals(obj: new object()).Should().BeFalse();
        _ = vo1.GetHashCode().Should().Be(expected: vo2.GetHashCode());
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
        var account = GeneralLedgerAccount.Create(
                                                  tenantId: tenantId,
                                                  code: " 1020 ",
                                                  name: " Bank Account ",
                                                  category: AccountCategory.Asset,
                                                  jurisdiction: Jurisdiction.Switzerland);

        // Assert
        _ = account.Id.Should().NotBeEmpty();
        _ = account.TenantId.Should().Be(expected: tenantId);
        _ = account.AccountCode.Should().Be(expected: "1020");
        _ = account.Name.Should().Be(expected: "Bank Account");
        _ = account.Category.Should().Be(expected: AccountCategory.Asset);
        _ = account.Jurisdiction.Should().Be(expected: Jurisdiction.Switzerland);
        _ = account.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Tests general ledger entry line addition, balancing, and totals.
    /// </summary>
    [Fact]
    public void GeneralLedgerEntry_CreateAndAddLines_CalculatesDebitCreditAndBalancing()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var entry = GeneralLedgerEntry.Create(
                                              tenantId: tenantId,
                                              entryNumber: "PNL-01",
                                              postingDate: DateTime.UtcNow,
                                              description: "Year End Closing",
                                              currency: Currency.EUR,
                                              referenceDoc: "DOC-123");

        var debitAcc  = Guid.NewGuid();
        var creditAcc = Guid.NewGuid();

        // Act
        entry.AddLine(
                      accountId: debitAcc,
                      debit: 1000m,
                      credit: 0m,
                      lineMemo: "Debit line");

        entry.AddLine(
                      accountId: creditAcc,
                      debit: 0m,
                      credit: 1000m,
                      lineMemo: "Credit line");

        // Assert
        _ = entry.TenantId.Should().Be(expected: tenantId);
        _ = entry.EntryNumber.Should().Be(expected: "PNL-01");
        _ = entry.ReferenceDocument.Should().Be(expected: "DOC-123");
        _ = entry.Lines.Should().HaveCount(expected: 2);
        _ = entry.TotalDebit.Should().Be(expected: 1000m);
        _ = entry.TotalCredit.Should().Be(expected: 1000m);
        _ = entry.IsBalanced.Should().BeTrue();
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
        var periodPayable = VatSettlementPeriod.Create(
                                                       tenantId: tenantId,
                                                       year: 2026,
                                                       periodNumber: 1,
                                                       type: VatSettlementPeriodType.Monthly,
                                                       jurisdiction: Jurisdiction.Italy,
                                                       outputVat: 5000m,
                                                       inputVat: 3000m,
                                                       priorCredit: 500m);

        periodPayable.FinalizeSettlement();

        // Act - Credit scenario
        var periodCredit = VatSettlementPeriod.Create(
                                                      tenantId: tenantId,
                                                      year: 2026,
                                                      periodNumber: 2,
                                                      type: VatSettlementPeriodType.Monthly,
                                                      jurisdiction: Jurisdiction.Italy,
                                                      outputVat: 2000m,
                                                      inputVat: 4000m,
                                                      priorCredit: 100m);

        // Assert
        _ = periodPayable.NetVatPayable.Should().Be(expected: 1500m);
        _ = periodPayable.NetVatCredit.Should().Be(expected: 0m);
        _ = periodPayable.IsFinalized.Should().BeTrue();
        _ = periodPayable.FinalizedAtUtc.Should().NotBeNull();

        _ = periodCredit.NetVatPayable.Should().Be(expected: 0m);
        _ = periodCredit.NetVatCredit.Should().Be(expected: 2100m);
        _ = periodCredit.IsFinalized.Should().BeFalse();
    }

    /// <summary>
    /// Tests contact and supplier entity creation and properties.
    /// </summary>
    [Fact]
    public void SupplierAndContact_Create_InitializesProperties()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var address = new Address(
                                  street: "Via Fornaci 1",
                                  city: "Lugano",
                                  postalCode: "6900",
                                  provinceOrCanton: "TI",
                                  countryCode: "CH");

        var vat = new VatNumber(
                                value: "CHE111222333",
                                countryCode: "CH");

        // Act
        var supplier = Supplier.Create(
                                       tenantId: tenantId,
                                       name: "Tech Supply SA",
                                       jurisdiction: Jurisdiction.Switzerland,
                                       address: address,
                                       vatNumber: vat,
                                       paymentTermsDays: 45);

        var client = Client.CreateItalian(
                                          tenantId: tenantId,
                                          businessName: "Client With Contacts",
                                          fiscalCode: new FiscalCode(value: "RSSMRA80A01F205X"),
                                          vatNumber: new VatNumber(
                                                                   value: "12345678901",
                                                                   countryCode: "IT"),
                                          sdiCode: "0000000",
                                          pecEmail: "contact@pec.it",
                                          address: address,
                                          email: "info@contact.it");

        client.AddContact(
                          firstName: "Mario",
                          lastName: "Rossi",
                          email: "mario@rossi.it",
                          phone: "+39 333 1234567",
                          role: "CTO",
                          isPrimary: true);

        // Assert
        _ = supplier.Id.Should().NotBeEmpty();
        _ = supplier.TenantId.Should().Be(expected: tenantId);
        _ = supplier.Name.Should().Be(expected: "Tech Supply SA");
        _ = supplier.VatNumber.Should().Be(expected: vat);
        _ = supplier.PaymentTermsDays.Should().Be(expected: 45);
        _ = supplier.IsActive.Should().BeTrue();

        _ = client.Contacts.Should().HaveCount(expected: 1);
        var contact = client.Contacts.First();
        _ = contact.FirstName.Should().Be(expected: "Mario");
        _ = contact.LastName.Should().Be(expected: "Rossi");
        _ = contact.IsPrimary.Should().BeTrue();
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
        var vehicle = CustomerVehicle.Create(
                                             tenantId: tenantId,
                                             clientId: clientId,
                                             licensePlate: "AA123BB",
                                             vin: "VIN12345678901234",
                                             brand: "Audi",
                                             model: "A4",
                                             year: 2022,
                                             jurisdiction: Jurisdiction.Italy,
                                             km: 45000);

        var inspection = StatutoryInspectionRecord.Schedule(
                                                            tenantId: tenantId,
                                                            vehicleId: vehicle.Id,
                                                            type: InspectionType.ItalianRevisione,
                                                            date: DateTime.UtcNow);

        inspection.MarkCompleted(
                                 passed: true,
                                 notes: "Everything in order",
                                 nextDue: DateTime.UtcNow.AddYears(value: 2));

        // Assert
        _ = vehicle.LicensePlate.Should().Be(expected: "AA123BB");
        _ = vehicle.VinChassisNumber.Should().Be(expected: "VIN12345678901234");
        _ = vehicle.CurrentKilometers.Should().Be(expected: 45000);

        _ = inspection.VehicleId.Should().Be(expected: vehicle.Id);
        _ = inspection.Status.Should().Be(expected: "Passed");
        _ = inspection.InspectorCenterNotes.Should().Be(expected: "Everything in order");
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

        var addr = new Address(
                               street: "Via Cantiere 5",
                               city: "Como",
                               postalCode: "22100",
                               provinceOrCanton: "CO",
                               countryCode: "IT");

        var budget = Money.Euro(amount: 500000m);

        // Act
        var site = ConstructionSite.Create(
                                           tenantId: tenantId,
                                           projectCode: "CANT-2026-01",
                                           name: "Ristrutturazione Edificio",
                                           clientId: clientId,
                                           address: addr,
                                           manager: "Geom. Rossi",
                                           start: DateTime.UtcNow,
                                           budget: budget);

        var item = BillOfQuantitiesItem.Create(
                                               tenantId: tenantId,
                                               siteId: site.Id,
                                               code: "01.01",
                                               standard: StandardType.ItalianComputo,
                                               desc: "Demolizione tramezzi",
                                               uom: "m2",
                                               qty: 150m,
                                               unitPrice: 25m);

        var sal = ProgressBillingSal.Create(
                                            tenantId: tenantId,
                                            siteId: site.Id,
                                            number: 1,
                                            date: DateTime.UtcNow,
                                            percent: 35m,
                                            certifiedAmount: Money.Euro(amount: 50000m),
                                            retentionAmount: Money.Euro(amount: 2500m));

        var cert = DeclarationOfConformity.Create(
                                                  tenantId: tenantId,
                                                  siteId: site.Id,
                                                  certNum: "CERT-001",
                                                  standard: "DM 37/08",
                                                  engineer: "Ing. Neri",
                                                  issueDate: DateTime.UtcNow,
                                                  systemType: "Impianto Elettrico");

        // Assert
        _ = site.ProjectCode.Should().Be(expected: "CANT-2026-01");
        _ = site.TotalContractBudget.Amount.Should().Be(expected: 500000m);

        _ = item.Subtotal.Should().Be(expected: 3750m);
        _ = sal.SalNumber.Should().Be(expected: 1);
        _ = sal.CumulativePercentage.Should().Be(expected: 35m);
        _ = cert.StandardReference.Should().Be(expected: "DM 37/08");
        _ = cert.DeclarantEngineerName.Should().Be(expected: "Ing. Neri");
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
        var vehicle = RentalVehicle.Create(
                                           tenantId: tenantId,
                                           fleetCode: "FLT-01",
                                           plate: "TI-12345",
                                           makeModel: "BMW 118i",
                                           dailyRate: Money.Chf(amount: 90m),
                                           currentKm: 20000);

        var booking = RentalBooking.Create(
                                           tenantId: tenantId,
                                           bookingNumber: "BOOK-01",
                                           vehicleId: vehicle.Id,
                                           clientId: clientId,
                                           start: DateTime.UtcNow,
                                           end: DateTime.UtcNow.AddDays(value: 3),
                                           pickup: "Lugano Stazione",
                                           ret: "Lugano Stazione",
                                           total: Money.Chf(amount: 270m),
                                           deposit: Money.Chf(amount: 500m));

        var damage = DamageAppraisal.Create(
                                            tenantId: tenantId,
                                            bookingId: booking.Id,
                                            type: "CheckOut",
                                            coordsJson: "[]",
                                            severity: DamageSeverity.Minor,
                                            repairCost: Money.Chf(amount: 350m));

        // Assert
        _ = vehicle.LicensePlate.Should().Be(expected: "TI-12345");
        _ = vehicle.DailyRate.Amount.Should().Be(expected: 90m);
        _ = booking.TotalRentAmount.Amount.Should().Be(expected: 270m);
        _ = damage.Severity.Should().Be(expected: DamageSeverity.Minor);
        _ = damage.EstimatedRepairCost.Amount.Should().Be(expected: 350m);
    }

    /// <summary>
    /// Tests inventory domain entities: catalog, warehouse, stock movement, and transport document.
    /// </summary>
    [Fact]
    public void Inventory_CatalogWarehouseAndStock_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var addr = new Address(
                               street: "Via Logistica 10",
                               city: "Milano",
                               postalCode: "20100",
                               provinceOrCanton: "MI",
                               countryCode: "IT");

        // Act
        var catalog = ItemCatalog.Create(
                                         tenantId: tenantId,
                                         code: "SKU-999",
                                         description: "Bullone M8",
                                         category: "Bulloneria",
                                         uom: "PZ",
                                         unitCost: 0.50m,
                                         salePrice: 1.20m,
                                         currency: Currency.EUR,
                                         vatRate: 22.0m);

        var warehouse = Warehouse.Create(
                                         tenantId: tenantId,
                                         code: "WH-MAIN",
                                         name: "Magazzino Centrale",
                                         address: addr,
                                         capacity: 1000m);

        var movement = StockMovement.Create(
                                            tenantId: tenantId,
                                            itemId: catalog.Id,
                                            warehouseId: warehouse.Id,
                                            quantity: 100m,
                                            type: MovementType.Receipt,
                                            batch: "LOT-A",
                                            serial: "SN-1",
                                            refDoc: "PO-100");

        var ddt = TransportDocument.Create(
                                           tenantId: tenantId,
                                           number: "DDT-001",
                                           date: DateTime.UtcNow,
                                           clientId: Guid.NewGuid(),
                                           destination: addr,
                                           reason: "Vendita",
                                           carrier: "Mittente",
                                           packages: 1,
                                           weightKg: 10m);

        // Assert
        _ = catalog.Code.Should().Be(expected: "SKU-999");
        _ = warehouse.Code.Should().Be(expected: "WH-MAIN");
        _ = movement.Quantity.Should().Be(expected: 100m);
        _ = movement.MovementType.Should().Be(expected: MovementType.Receipt);
        _ = ddt.DocumentNumber.Should().Be(expected: "DDT-001");
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
        var bom = BillOfMaterials.Create(
                                         tenantId: tenantId,
                                         finishedItemId: Guid.NewGuid(),
                                         code: "BOM-001",
                                         revision: "1.0");

        bom.AddComponent(
                         componentItemId: Guid.NewGuid(),
                         quantity: 2m,
                         scrapRate: 0.05m);

        var wc = WorkCenter.Create(
                                   tenantId: tenantId,
                                   code: "WC-CNC",
                                   name: "Centro di Lavoro CNC",
                                   cost: 85.00m);

        var order = ProductionOrder.Create(
                                           tenantId: tenantId,
                                           orderNumber: "ORD-2026-001",
                                           bomId: bom.Id,
                                           quantity: 10m,
                                           start: DateTime.UtcNow,
                                           end: DateTime.UtcNow.AddDays(value: 7));

        order.ReportProgress(qtyDone: 10m);

        // Assert
        _ = bom.BomCode.Should().Be(expected: "BOM-001");
        _ = bom.Lines.Should().HaveCount(expected: 1);
        _ = wc.HourlyMachineCost.Should().Be(expected: 85.00m);
        _ = order.QuantityToProduce.Should().Be(expected: 10m);
        _ = order.Status.Should().Be(expected: ProductionStatus.Completed);
    }

    /// <summary>
    /// Tests payroll slip summary and employee absence records.
    /// </summary>
    [Fact]
    public void PayrollHr_PayrollSummaryAndAbsence_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var empId    = Guid.NewGuid();

        // Act
        var payroll = PayrollSlipSummary.CreateItalian(
                                                       tenantId: tenantId,
                                                       employeeId: empId,
                                                       year: 2026,
                                                       month: 5,
                                                       grossEur: 3000m,
                                                       inpsInailEur: 300m,
                                                       irpefEur: 500m);

        var absence = AbsenceRecord.Create(
                                           tenantId: tenantId,
                                           employeeId: empId,
                                           type: AbsenceType.Ferie,
                                           start: new DateTime(
                                                               year: 2026,
                                                               month: 5,
                                                               day: 10,
                                                               hour: 0,
                                                               minute: 0,
                                                               second: 0,
                                                               kind: DateTimeKind.Utc),
                                           end: new DateTime(
                                                             year: 2026,
                                                             month: 5,
                                                             day: 15,
                                                             hour: 0,
                                                             minute: 0,
                                                             second: 0,
                                                             kind: DateTimeKind.Utc),
                                           days: 5m,
                                           certificateNumber: "INPS-12345",
                                           notes: "Approved vacation");

        absence.Approve();

        // Assert
        _ = payroll.GrossSalary.Should().Be(expected: 3000m);
        _ = payroll.NetSalaryPaid.Should().Be(expected: 2200m);
        _ = absence.TotalWorkingDays.Should().Be(expected: 5m);
        _ = absence.AbsenceType.Should().Be(expected: AbsenceType.Ferie);
        _ = absence.IsApproved.Should().BeTrue();
    }

    /// <summary>
    /// Tests purchasing passive invoice and payment instructions.
    /// </summary>
    [Fact]
    public void Purchasing_OrderPassiveInvoiceAndPaymentInstruction_WorksCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var supId    = Guid.NewGuid();

        // Act
        var po = PurchaseOrder.Create(
                                      tenantId: tenantId,
                                      orderNumber: "PO-2026/01",
                                      supplierId: supId,
                                      total: Money.Euro(amount: 1500m));

        var invoice = PassiveInvoice.Create(
                                            tenantId: tenantId,
                                            supplierId: supId,
                                            documentNumber: "FAT-FORN-99",
                                            documentDate: DateTime.UtcNow,
                                            totalNet: 1229.51m,
                                            totalVat: 270.49m,
                                            currency: Currency.EUR,
                                            paymentDueDate: DateTime.UtcNow.AddDays(value: 30));

        var payment = Iso20022PaymentInstruction.Create(
                                                        tenantId: tenantId,
                                                        debtorIban: "IT60A0000000000000000000000",
                                                        creditorIban: "IT12X0000000000000000000000",
                                                        creditorName: "Supplier SpA",
                                                        amount: Money.Euro(amount: 1500m),
                                                        remittance: "Salvataggio Fattura",
                                                        executionDate: DateTime.UtcNow.AddDays(value: 2));

        // Assert
        _ = po.OrderNumber.Should().Be(expected: "PO-2026/01");
        _ = invoice.DocumentNumber.Should().Be(expected: "FAT-FORN-99");
        _ = payment.Amount.Amount.Should().Be(expected: 1500m);
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
        var session = PosTerminalSession.Open(
                                              tenantId: tenantId,
                                              terminalCode: "POS-01",
                                              cashierId: Guid.NewGuid(),
                                              cashFloat: Money.Euro(amount: 200m));

        var receipt = RetailReceipt.Create(
                                           tenantId: tenantId,
                                           sessionId: session.Id,
                                           receiptNumber: "REC-001",
                                           amount: 120m,
                                           currency: Currency.EUR,
                                           paymentMethod: PaymentMethod.Cash,
                                           rtLot: "RT-SN-123456",
                                           zReport: "Z-99");

        session.CloseSession(
                             countedCash: Money.Euro(amount: 320m),
                             calculatedCash: Money.Euro(amount: 320m));

        // Assert
        _ = session.TerminalCode.Should().Be(expected: "POS-01");
        _ = session.IsClosed.Should().BeTrue();
        _ = receipt.TotalAmount.Should().Be(expected: 120m);
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
        var quote = Quote.Create(
                                 tenantId: tenantId,
                                 quoteNumber: "PREV-01",
                                 clientId: clientId,
                                 expiry: DateTime.UtcNow.AddDays(value: 30),
                                 total: Money.Euro(amount: 2500m));

        var order = SalesOrder.Create(
                                      tenantId: tenantId,
                                      orderNumber: "ORD-CLI-01",
                                      clientId: clientId,
                                      currency: Currency.EUR);

        var priceList = PriceList.Create(
                                         tenantId: tenantId,
                                         name: "LISTINO-BASE",
                                         currency: Currency.EUR,
                                         isDefault: true);

        // Assert
        _ = quote.QuoteNumber.Should().Be(expected: "PREV-01");
        _ = quote.TotalAmount.Amount.Should().Be(expected: 2500m);
        _ = order.OrderNumber.Should().Be(expected: "ORD-CLI-01");
        _ = priceList.Name.Should().Be(expected: "LISTINO-BASE");
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
                                          tenantId: tenantId,
                                          ticketNumber: "TICK-001",
                                          clientId: clientId,
                                          title: "Errore rete",
                                          description: "Impossibile collegarsi alla stampante",
                                          priority: RiskLevel.Medium,
                                          slaDueDate: DateTime.UtcNow.AddHours(value: 24));

        var workOrder = TechnicianWorkOrder.Create(
                                                   tenantId: tenantId,
                                                   workOrderNumber: "WO-001",
                                                   ticketId: ticket.Id,
                                                   technicianId: Guid.NewGuid(),
                                                   serviceDate: DateTime.UtcNow,
                                                   hoursWorked: 2.5m,
                                                   hourlyRate: 65m,
                                                   currency: Currency.EUR,
                                                   summary: "Risolto problema IP statico");

        workOrder.SignByCustomer(
                                 signatoryName: "Mario Rossi",
                                 signatureData: "data:image/svg+xml;base64,PHN2Zz48L3N2Zz4=");

        // Assert
        _ = ticket.Title.Should().Be(expected: "Errore rete");
        _ = ticket.Priority.Should().Be(expected: RiskLevel.Medium);
        _ = workOrder.IsSignedByCustomer.Should().BeTrue();
        _ = workOrder.HoursWorked.Should().Be(expected: 2.5m);
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
        var matter = ClientMatter.Create(
                                         tenantId: tenantId,
                                         matterNumber: "MAT-01",
                                         clientId: clientId,
                                         title: "Contenzioso Tributario",
                                         practiceArea: "TaxAdvisory");

        matter.VerifyKyc(riskLevel: RiskLevel.Medium);

        var time = BillableTimeEntry.Create(
                                            tenantId: tenantId,
                                            matterId: matter.Id,
                                            userId: Guid.NewGuid(),
                                            date: DateTime.UtcNow,
                                            minutes: 180, // 3 ore
                                            rate: 250m,
                                            currency: Currency.EUR,
                                            description: "Preparazione ricorso");

        var feeNote = ProfessionalFeeNote.CreateItalian(
                                                        tenantId: tenantId,
                                                        docNumber: "NOTA-01",
                                                        matterId: matter.Id,
                                                        date: DateTime.UtcNow,
                                                        grossFees: 750m,
                                                        nonTaxableExpenses: 50m,
                                                        vatRate: 22.0m,
                                                        cassaPercent: 4.0m,
                                                        ritenutaPercent: 20.0m);

        // Assert
        _ = matter.MatterNumber.Should().Be(expected: "MAT-01");
        _ = matter.KycAmlVerified.Should().BeTrue();
        _ = time.TotalFee.Should().Be(expected: 750m);
        _ = feeNote.DocumentNumber.Should().Be(expected: "NOTA-01");
        _ = feeNote.ItRitenutaAccontoAmount.Should().Be(expected: 150m);
        _ = feeNote.NetPayable.Should().BeGreaterThan(expected: 0m);
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
                                             tenantId: tenantId,
                                             statementNumber: "STMT-01",
                                             accountIban: "CH1234567890",
                                             currency: Currency.CHF,
                                             openingBalance: 10000m,
                                             closingBalance: 12500m,
                                             statementDate: DateTime.UtcNow);

        statement.AddTransaction(
                                 bookingDate: DateTime.UtcNow,
                                 valueDate: DateTime.UtcNow,
                                 amount: 2500m,
                                 remittance: "Pagamento fattura cliente",
                                 counterpartyIban: "CH123",
                                 counterpartyName: "Cliente SA");

        // Assert
        _ = statement.AccountIban.Should().Be(expected: "CH1234567890");
        _ = statement.OpeningBalance.Should().Be(expected: 10000m);
        _ = statement.ClosingBalance.Should().Be(expected: 12500m);
        _ = statement.Lines.Should().HaveCount(expected: 1);
    }

    /// <summary>
    /// Tests dashboard KPI summary, tax deadlines, branches, and exchange rates.
    /// </summary>
    [Fact]
    public void DashboardAndAdmin_Entities_WorkCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var branchAddr = new Address(
                                     street: "Via Cantonale 1",
                                     city: "Mendrisio",
                                     postalCode: "6850",
                                     provinceOrCanton: "TI",
                                     countryCode: "CH");

        // Act
        var kpi = ExecutiveKpiSummary.Create(
                                             tenantId: tenantId,
                                             period: "2026-Q1",
                                             revenueEur: Money.Euro(amount: 100000m),
                                             revenueChf: Money.Chf(amount: 80000m),
                                             receivablesEur: Money.Euro(amount: 20000m),
                                             receivablesChf: Money.Chf(amount: 15000m),
                                             vatEur: Money.Euro(amount: 5000m),
                                             vatChf: Money.Chf(amount: 3000m),
                                             nextTaxDeadline: DateTime.UtcNow.AddDays(value: 15),
                                             jurisdiction: Jurisdiction.Italy);

        var taxDeadline = TaxDeadlineAlert.Create(
                                                  tenantId: tenantId,
                                                  title: "Liquidazione IVA Trimestrale",
                                                  description: "Versamento F24",
                                                  dueDate: DateTime.UtcNow.AddDays(value: 15),
                                                  estimatedAmount: Money.Euro(amount: 4500m),
                                                  jurisdiction: Jurisdiction.Italy);

        var tenant = Tenant.Create(
                                   code: "T-01",
                                   businessName: "Main Tenant",
                                   countryCode: "CH",
                                   baseCurrency: Currency.CHF,
                                   contactEmail: "admin@main.ch");

        tenant.AddBranch(
                         code: "BR-01",
                         name: "Filiale Ticino",
                         address: branchAddr,
                         isHeadquarters: false);

        var rate = ExchangeRate.Create(
                                       baseCurr: Currency.EUR,
                                       targetCurr: Currency.CHF,
                                       rate: 0.96m,
                                       date: DateTime.UtcNow,
                                       source: "ECB");

        // Assert
        _ = kpi.TotalRevenueEur.Amount.Should().Be(expected: 100000m);
        _ = taxDeadline.Title.Should().Be(expected: "Liquidazione IVA Trimestrale");
        _ = (taxDeadline.DueDate - DateTime.UtcNow).TotalDays.Should().BeGreaterThan(expected: 0);
        _ = tenant.Branches.Should().HaveCount(expected: 1);
        _ = rate.Rate.Should().Be(expected: 0.96m);
    }

    /// <summary>
    /// Represents a sample domain event used for testing.
    /// </summary>
    private sealed record SampleDomainEvent : DomainEventBase;

    /// <summary>
    /// Represents a sample value object implementation used to verify structural equality and hash code evaluation.
    /// </summary>
    /// <param name="prop1">The first string component.</param>
    /// <param name="prop2">The second integer component.</param>
    private sealed class SampleValueObject(string prop1, int prop2) : ValueObject
    {

        /// <summary>
        /// Gets the primary string property used in equality comparison.
        /// </summary>
        public string Prop1 { get; } = prop1;

        /// <summary>
        /// Gets the secondary integer property used in equality comparison.
        /// </summary>
        public int Prop2 { get; } = prop2;

        /// <summary>
        /// Returns the sequence of atomic components that determine value object equality.
        /// </summary>
        /// <returns>An enumerable of atomic values comprising this value object.</returns>
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Prop1;
            yield return Prop2;
        }

    }

}
