// <copyright file="DomainEntityTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.PayrollHr;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Unit tests for domain entities and aggregate roots.
/// </summary>
public sealed class DomainEntityTests
{
    /// <summary>
    /// Domain event dummy implementation for testing aggregate root event handling.
    /// </summary>
    private sealed record TestDomainEvent(string EventName) : IDomainEvent
    {
        /// <summary>
        /// Gets the event id.
        /// </summary>
        public Guid EventId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the event timestamp.
        /// </summary>
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Dummy aggregate root implementation for verifying base class operations.
    /// </summary>
    private sealed class TestAggregateRoot : AggregateRoot<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAggregateRoot"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        public TestAggregateRoot(Guid id)
            : base(id: id)
        {
        }
    }

    /// <summary>
    /// Tests aggregate root domain events collection and clearing.
    /// </summary>
    [Fact]
    public void AggregateRoot_DomainEvents_ShouldAddAndClearCorrectly()
    {
        // Arrange
        var root = new TestAggregateRoot(id: Guid.NewGuid());
        var evt  = new TestDomainEvent(EventName: "OrderCreated");

        // Act
        root.AddDomainEvent(domainEvent: evt);

        // Assert
        _ = root.DomainEvents.Should().ContainSingle().Which.Should().Be(expected: evt);

        // Act & Assert
        root.ClearDomainEvents();
        _ = root.DomainEvents.Should().BeEmpty();
    }

    /// <summary>
    /// Tests aggregate root throws ArgumentNullException when null domain event is added.
    /// </summary>
    [Fact]
    public void AggregateRoot_AddNullDomainEvent_ShouldThrowArgumentNullException()
    {
        // Arrange
        var root = new TestAggregateRoot(id: Guid.NewGuid());

        // Act
        var act = () => root.AddDomainEvent(domainEvent: null!);

        // Assert
        _ = act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests Entity equality operators and methods.
    /// </summary>
    [Fact]
    public void Entity_Equality_ShouldBeBasedOnTypeAndId()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var entity1 = new TestAggregateRoot(id: id);
        var entity2 = new TestAggregateRoot(id: id);
        var entity3 = new TestAggregateRoot(id: Guid.NewGuid());

        // Assert
        _ = (entity1 == entity2).Should().BeTrue();
        _ = (entity1 != entity3).Should().BeTrue();
        _ = entity1.Equals(other: entity2).Should().BeTrue();
        _ = entity1.Equals(obj: (object)entity2).Should().BeTrue();
        _ = (entity1.Equals(other: (Entity<Guid>?)null)).Should().BeFalse();
        _ = (entity1!.Equals(obj: new object())).Should().BeFalse();
        _ = entity1.GetHashCode().Should().Be(expected: entity2.GetHashCode());
    }

    /// <summary>
    /// Tests Client CreateItalian factory with valid parameters.
    /// </summary>
    [Fact]
    public void Client_CreateItalian_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cf       = new FiscalCode(value: "RSSMRA85M01H501Z");

        var vat = new VatNumber(
                                value: "12345678901",
                                countryCode: "IT");

        var address = new Address(
                                  street: "Via Dante 10",
                                  city: "Milano",
                                  postalCode: "20121",
                                  provinceOrCanton: "MI",
                                  countryCode: "IT");

        // Act
        var client = Client.CreateItalian(
                                          tenantId: tenantId,
                                          businessName: "Mario Rossi SpA",
                                          fiscalCode: cf,
                                          vatNumber: vat,
                                          sdiCode: "ABC1234",
                                          pecEmail: "mariorossi@pec.it",
                                          address: address,
                                          email: "info@mariorossi.it",
                                          phone: "+39 02 123456",
                                          paymentTermDays: 60);

        // Assert
        _ = client.TenantId.Should().Be(expected: tenantId);
        _ = client.BusinessName.Should().Be(expected: "Mario Rossi SpA");
        _ = client.Jurisdiction.Should().Be(expected: Jurisdiction.Italy);
        _ = client.FiscalCode.Should().Be(expected: cf);
        _ = client.VatNumber.Should().Be(expected: vat);
        _ = client.SdiCode.Should().Be(expected: "ABC1234");
        _ = client.PecEmail.Should().Be(expected: "mariorossi@pec.it");
        _ = client.Address.Should().Be(expected: address);
        _ = client.PaymentTermDays.Should().Be(expected: 60);
        _ = client.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Tests Client CreateItalian throws on empty business name.
    /// </summary>
    [Fact]
    public void Client_CreateItalian_EmptyBusinessName_ShouldThrowArgumentException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cf       = new FiscalCode(value: "RSSMRA85M01H501Z");

        var vat = new VatNumber(
                                value: "12345678901",
                                countryCode: "IT");

        var address = new Address(
                                  street: "Via Dante 10",
                                  city: "Milano",
                                  postalCode: "20121",
                                  provinceOrCanton: "MI",
                                  countryCode: "IT");

        // Act
        var act = () => Client.CreateItalian(
                                             tenantId: tenantId,
                                             businessName: "   ",
                                             fiscalCode: cf,
                                             vatNumber: vat,
                                             sdiCode: "0000000",
                                             pecEmail: "",
                                             address: address,
                                             email: "info@test.it");

        // Assert
        _ = act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests Client CreateSwiss factory and commercial updates.
    /// </summary>
    [Fact]
    public void Client_CreateSwiss_AndUpdateCommercialTerms_ShouldModifyEntity()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var qrIban   = new QrIban(value: "CH4431999123456789012");

        var address = new Address(
                                  street: "Via Nassa 12",
                                  city: "Lugano",
                                  postalCode: "6900",
                                  provinceOrCanton: "TI",
                                  countryCode: "CH");

        // Act
        var client = Client.CreateSwiss(
                                        tenantId: tenantId,
                                        businessName: "Alps Engineering Sagl",
                                        swissUid: "CHE-123.456.789 MWST",
                                        cantonalCode: "TI",
                                        qrIban: qrIban,
                                        address: address,
                                        email: "alps@lugano.ch");

        client.AddContact(
                          firstName: "Paolo",
                          lastName: "Bianchi",
                          email: "p.bianchi@alps.ch",
                          phone: "+41 91 123",
                          role: "Director",
                          isPrimary: true);

        client.UpdateCommercialTerms(
                                     paymentTermDays: 45,
                                     creditLimit: Money.Chf(amount: 25000));

        client.Deactivate();

        // Assert
        _ = client.Jurisdiction.Should().Be(expected: Jurisdiction.Switzerland);
        _ = client.SwissUid.Should().Be(expected: "CHE-123.456.789 MWST");
        _ = client.CantonalCode.Should().Be(expected: "TI");
        _ = client.QrIban.Should().Be(expected: qrIban);
        _ = client.Contacts.Should().ContainSingle();
        _ = client.PaymentTermDays.Should().Be(expected: 45);
        _ = client.CreditLimit.Amount.Should().Be(expected: 25000);
        _ = client.IsActive.Should().BeFalse();
        _ = client.UpdatedAtUtc.Should().NotBeNull();
    }

    /// <summary>
    /// Tests ItalianElectronicInvoice line addition and total recalculation.
    /// </summary>
    [Fact]
    public void ItalianElectronicInvoice_AddLine_ShouldCalculateTotalsAndRecalculate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var cedenteAddress = new Address(
                                         street: "Via Roma 1",
                                         city: "Milano",
                                         postalCode: "20100",
                                         provinceOrCanton: "MI",
                                         countryCode: "IT");

        var cessionarioAddress = new Address(
                                             street: "Corso Italia 2",
                                             city: "Roma",
                                             postalCode: "00100",
                                             provinceOrCanton: "RM",
                                             countryCode: "IT");

        var invoice = ItalianElectronicInvoice.Create(
                                                      tenantId: tenantId,
                                                      documentNumber: "INV-2026-001",
                                                      documentDate: DateTime.UtcNow,
                                                      clientId: Guid.NewGuid(),
                                                      cedenteName: "Cedente SpA",
                                                      cedenteVat: "11111111111",
                                                      cedenteCf: "RSSMRA85M01H501Z",
                                                      cedenteAddress: cedenteAddress,
                                                      cessionarioName: "Cessionario Srl",
                                                      cessionarioVat: "22222222222",
                                                      cessionarioCf: null,
                                                      sdiCode: "SUBM123",
                                                      pec: null,
                                                      cessionarioAddress: cessionarioAddress,
                                                      dueDate: DateTime.UtcNow.AddDays(value: 30));

        // Act
        invoice.AddLine(
                        description: "Consulenza Software",
                        quantity: 10m,
                        unitPrice: 100m,
                        vatRate: 22m);

        invoice.AddLine(
                        description: "Licenza Annuale",
                        quantity: 1m,
                        unitPrice: 500m,
                        vatRate: 22m);

        // Assert
        // Net: 10 * 100 + 1 * 500 = 1500
        // Vat: 1000 * 0.22 = 220, 500 * 0.22 = 110 => Total VAT = 330
        // Gross: 1500 + 330 = 1830
        _ = invoice.TotalNet.Should().Be(expected: 1500m);
        _ = invoice.TotalVat.Should().Be(expected: 330m);
        _ = invoice.TotalGross.Should().Be(expected: 1830m);
        _ = invoice.Lines.Should().HaveCount(expected: 2);

        // Test SDI lifecycle transitions
        invoice.MarkSentToSdi(sdiIdentifier: "SDI-998877");
        _ = invoice.SdiStatus.Should().Be(expected: SdiTransmissionStatus.Sent);
        _ = invoice.SdiIdentifier.Should().Be(expected: "SDI-998877");

        invoice.MarkDelivered();
        _ = invoice.SdiStatus.Should().Be(expected: SdiTransmissionStatus.Delivered);

        invoice.MarkRejected(errorMessage: "Codice Destinatario non valido");
        _ = invoice.SdiStatus.Should().Be(expected: SdiTransmissionStatus.Rejected);
        _ = invoice.SdiErrorMessage.Should().Be(expected: "Codice Destinatario non valido");
    }

    /// <summary>
    /// Tests SwissQrBill creation with cash rounding and payment transition.
    /// </summary>
    [Fact]
    public void SwissQrBill_Create_ShouldRoundCashAndMarkAsPaid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var creditorAddress = new Address(
                                          street: "Piazza Riforma 1",
                                          city: "Lugano",
                                          postalCode: "6900",
                                          provinceOrCanton: "TI",
                                          countryCode: "CH");

        var debtorAddress = new Address(
                                        street: "Bahnhofstrasse 10",
                                        city: "Zurich",
                                        postalCode: "8001",
                                        provinceOrCanton: "ZH",
                                        countryCode: "CH");

        var qrIban    = new QrIban(value: "CH4431999123456789012");
        var rawAmount = Money.Chf(amount: 100.03m); // Rounds to 100.05

        // Act
        var bill = SwissQrBill.Create(
                                      tenantId: tenantId,
                                      billNumber: "QR-2026-005",
                                      billDate: DateTime.UtcNow,
                                      clientId: Guid.NewGuid(),
                                      creditorName: "Swiss IT Solutions AG",
                                      creditorAddress: creditorAddress,
                                      creditorQrIban: qrIban,
                                      debtorName: "Debtor AG",
                                      debtorAddress: debtorAddress,
                                      rawAmount: rawAmount,
                                      referenceNumber: "21 00000 00003 13947 14300 09017",
                                      dueDate: DateTime.UtcNow.AddDays(value: 30),
                                      message: "Fattura n. QR-2026-005");

        bill.MarkAsPaid(paidDate: DateTime.UtcNow);

        // Assert
        _ = bill.Amount.Amount.Should().Be(expected: 100.05m);
        _ = bill.ReferenceNumber.Should().Be(expected: "210000000003139471430009017");
        _ = bill.IsPaid.Should().BeTrue();
        _ = bill.PaidDateUtc.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that <see cref="Employee.Create"/> correctly computes the fiscal residence country
    /// depending on cross-border commuter status and Swiss cantonal assignment.
    /// </summary>
    /// <param name="isFrontaliere">Indicates whether the employee is a cross-border commuter.</param>
    /// <param name="canton">The Swiss canton code, or null if not applicable.</param>
    /// <param name="expectedResidence">The ISO country code expected for fiscal residence.</param>
    [Theory]
    [InlineData(
                   data: [true, "TI", "IT"])]
    [InlineData(
                   data: [false, "TI", "CH"])]
    [InlineData(
                   data: [false, null, "IT"])]
    public void Employee_Create_ShouldCalculateFiscalResidenceCountry(
        bool    isFrontaliere,
        string? canton,
        string  expectedResidence)
    {
        // Arrange
        var address = new Address(
                                  street: "Via Dogana 1",
                                  city: "Como",
                                  postalCode: "22100",
                                  provinceOrCanton: "CO",
                                  countryCode: "IT");

        // Act
        var employee = Employee.Create(
                                       tenantId: Guid.NewGuid(),
                                       code: "EMP-007",
                                       firstName: "Giovanni",
                                       lastName: "Verdi",
                                       dob: new DateTime(
                                                         year: 1990,
                                                         month: 5,
                                                         day: 12),
                                       workerType: WorkerType.Employee,
                                       isFrontaliere: isFrontaliere,
                                       address: address,
                                       jobTitle: "Software Architect",
                                       salary: 6500m,
                                       currency: Currency.CHF,
                                       canton: canton);

        // Assert
        _ = employee.FiscalResidenceCountry.Should().Be(expected: expectedResidence);
        _ = employee.IsFrontaliereCrossBorder.Should().Be(expected: isFrontaliere);
        _ = employee.EmployeeCode.Should().Be(expected: "EMP-007");
    }

    /// <summary>
    /// Tests TimeEntry clock in, clock out, and total hours computation.
    /// </summary>
    [Fact]
    public void TimeEntry_ClockInAndClockOut_ShouldCalculateNetHours()
    {
        // Arrange
        var tenantId   = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var clockIn = new DateTime(
                                   year: 2026,
                                   month: 10,
                                   day: 20,
                                   hour: 8,
                                   minute: 0,
                                   second: 0,
                                   kind: DateTimeKind.Utc);

        var clockOut = new DateTime(
                                    year: 2026,
                                    month: 10,
                                    day: 20,
                                    hour: 17,
                                    minute: 30,
                                    second: 0,
                                    kind: DateTimeKind.Utc); // 9.5 hours = 570 mins

        // Act
        var entry = TimeEntry.ClockIn(
                                      tenantId: tenantId,
                                      employeeId: employeeId,
                                      clockInUtc: clockIn,
                                      recordingType: RecordingType.Detailed,
                                      lat: 45.8,
                                      lon: 9.08,
                                      badgeSerial: "TAG-1234");

        _ = entry.TotalNetHours.Should().Be(expected: 0m); // before clock out

        entry.ClockOut(
                       clockOutUtc: clockOut,
                       breakMinutes: 60); // 570 - 60 = 510 mins = 8.50 hours

        // Assert
        _ = entry.TotalNetHours.Should().Be(expected: 8.50m);
        _ = entry.BreakDurationMinutes.Should().Be(expected: 60);
        _ = entry.BadgeSerialNumber.Should().Be(expected: "TAG-1234");
    }

    /// <summary>
    /// Tests TimeEntry clock out with timestamp earlier than clock in throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void TimeEntry_ClockOutEarlierThanClockIn_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var entry = TimeEntry.ClockIn(
                                      tenantId: Guid.NewGuid(),
                                      employeeId: Guid.NewGuid(),
                                      clockInUtc: DateTime.UtcNow);

        // Act
        var act = () => entry.ClockOut(clockOutUtc: DateTime.UtcNow.AddHours(value: -1));

        // Assert
        _ = act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage(expectedWildcardPattern: "*Clock out cannot be earlier than clock in*");
    }

    /// <summary>
    /// Tests TireHotelRecord check-in and mounting on vehicle.
    /// </summary>
    [Fact]
    public void TireHotelRecord_CheckInAndMount_ShouldUpdateStatus()
    {
        // Arrange
        var tenantId  = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        // Act
        var record = TireHotelRecord.CheckIn(
                                             tenantId: tenantId,
                                             vehicleId: vehicleId,
                                             season: TireSeason.Winter,
                                             brand: "Pirelli",
                                             dimension: "205/55 R16",
                                             dotCode: "3824",
                                             flMm: 7.5m,
                                             frMm: 7.5m,
                                             rlMm: 6.8m,
                                             rrMm: 6.8m,
                                             rackCoordinate: "RACK-B-02");

        _ = record.Status.Should().Be(expected: "InStorage");
        _ = record.WarehouseRackCoordinate.Should().Be(expected: "RACK-B-02");

        record.MountOnVehicle();

        // Assert
        _ = record.Status.Should().Be(expected: "MountedOnVehicle");
        _ = record.RetrievedDateUtc.Should().NotBeNull();
        _ = record.UpdatedAtUtc.Should().NotBeNull();
    }
}
