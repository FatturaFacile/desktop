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
/// Domain event dummy implementation for testing aggregate root event handling.
/// </summary>
public sealed record TestDomainEvent(string EventName) : IDomainEvent
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
public sealed class TestAggregateRoot : AggregateRoot<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAggregateRoot"/> class.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    public TestAggregateRoot(
        Guid id)
        : base(id)
    {
    }
}

/// <summary>
/// Unit tests for domain entities and aggregate roots.
/// </summary>
public sealed class DomainEntityTests
{
    /// <summary>
    /// Tests aggregate root domain events collection and clearing.
    /// </summary>
    [Fact]
    public void AggregateRoot_DomainEvents_ShouldAddAndClearCorrectly()
    {
        // Arrange
        var root = new TestAggregateRoot(Guid.NewGuid());
        var evt = new TestDomainEvent("OrderCreated");

        // Act
        root.AddDomainEvent(evt);

        // Assert
        _ = root.DomainEvents.Should().ContainSingle().Which.Should().Be(evt);

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
        var root = new TestAggregateRoot(Guid.NewGuid());

        // Act
        var act = () => root.AddDomainEvent(null!);

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
        var id = Guid.NewGuid();
        var entity1 = new TestAggregateRoot(id);
        var entity2 = new TestAggregateRoot(id);
        var entity3 = new TestAggregateRoot(Guid.NewGuid());

        // Assert
        _ = (entity1 == entity2).Should().BeTrue();
        _ = (entity1 != entity3).Should().BeTrue();
        _ = entity1.Equals(entity2).Should().BeTrue();
        _ = entity1.Equals((object)entity2).Should().BeTrue();
        _ = entity1.Equals(null).Should().BeFalse();
        _ = entity1.Equals(new object()).Should().BeFalse();
        _ = entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    /// <summary>
    /// Tests Client CreateItalian factory with valid parameters.
    /// </summary>
    [Fact]
    public void Client_CreateItalian_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cf = new FiscalCode("RSSMRA85M01H501Z");
        var vat = new VatNumber(
            "12345678901",
            "IT");
        var address = new Address(
            "Via Dante 10",
            "Milano",
            "20121",
            "MI",
            "IT");

        // Act
        var client = Client.CreateItalian(
            tenantId,
            "Mario Rossi SpA",
            cf,
            vat,
            "ABC1234",
            "mariorossi@pec.it",
            address,
            "info@mariorossi.it",
            "+39 02 123456",
            60);

        // Assert
        _ = client.TenantId.Should().Be(tenantId);
        _ = client.BusinessName.Should().Be("Mario Rossi SpA");
        _ = client.Jurisdiction.Should().Be(Jurisdiction.Italy);
        _ = client.FiscalCode.Should().Be(cf);
        _ = client.VatNumber.Should().Be(vat);
        _ = client.SdiCode.Should().Be("ABC1234");
        _ = client.PecEmail.Should().Be("mariorossi@pec.it");
        _ = client.Address.Should().Be(address);
        _ = client.PaymentTermDays.Should().Be(60);
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
        var cf = new FiscalCode("RSSMRA85M01H501Z");
        var vat = new VatNumber(
            "12345678901",
            "IT");
        var address = new Address(
            "Via Dante 10",
            "Milano",
            "20121",
            "MI",
            "IT");

        // Act
        var act = () => Client.CreateItalian(
            tenantId,
            "   ",
            cf,
            vat,
            "0000000",
            "",
            address,
            "info@test.it");

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
        var qrIban = new QrIban("CH4431999123456789012");
        var address = new Address(
            "Via Nassa 12",
            "Lugano",
            "6900",
            "TI",
            "CH");

        // Act
        var client = Client.CreateSwiss(
            tenantId,
            "Alps Engineering Sagl",
            "CHE-123.456.789 MWST",
            "TI",
            qrIban,
            address,
            "alps@lugano.ch");

        client.AddContact(
            "Paolo",
            "Bianchi",
            "p.bianchi@alps.ch",
            "+41 91 123",
            "Director",
            true);
        client.UpdateCommercialTerms(
            45,
            Money.Chf(25000));
        client.Deactivate();

        // Assert
        _ = client.Jurisdiction.Should().Be(Jurisdiction.Switzerland);
        _ = client.SwissUid.Should().Be("CHE-123.456.789 MWST");
        _ = client.CantonalCode.Should().Be("TI");
        _ = client.QrIban.Should().Be(qrIban);
        _ = client.Contacts.Should().ContainSingle();
        _ = client.PaymentTermDays.Should().Be(45);
        _ = client.CreditLimit.Amount.Should().Be(25000);
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
            "Via Roma 1",
            "Milano",
            "20100",
            "MI",
            "IT");
        var cessionarioAddress = new Address(
            "Corso Italia 2",
            "Roma",
            "00100",
            "RM",
            "IT");

        var invoice = ItalianElectronicInvoice.Create(
            tenantId,
            "INV-2026-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Cedente SpA",
            "11111111111",
            "RSSMRA85M01H501Z",
            cedenteAddress,
            "Cessionario Srl",
            "22222222222",
            null,
            "SUBM123",
            null,
            cessionarioAddress,
            DateTime.UtcNow.AddDays(30));

        // Act
        invoice.AddLine(
            "Consulenza Software",
            10m,
            100m,
            22m);
        invoice.AddLine(
            "Licenza Annuale",
            1m,
            500m,
            22m);

        // Assert
        // Net: 10 * 100 + 1 * 500 = 1500
        // Vat: 1000 * 0.22 = 220, 500 * 0.22 = 110 => Total VAT = 330
        // Gross: 1500 + 330 = 1830
        _ = invoice.TotalNet.Should().Be(1500m);
        _ = invoice.TotalVat.Should().Be(330m);
        _ = invoice.TotalGross.Should().Be(1830m);
        _ = invoice.Lines.Should().HaveCount(2);

        // Test SDI lifecycle transitions
        invoice.MarkSentToSdi("SDI-998877");
        _ = invoice.SdiStatus.Should().Be(SdiTransmissionStatus.Sent);
        _ = invoice.SdiIdentifier.Should().Be("SDI-998877");

        invoice.MarkDelivered();
        _ = invoice.SdiStatus.Should().Be(SdiTransmissionStatus.Delivered);

        invoice.MarkRejected("Codice Destinatario non valido");
        _ = invoice.SdiStatus.Should().Be(SdiTransmissionStatus.Rejected);
        _ = invoice.SdiErrorMessage.Should().Be("Codice Destinatario non valido");
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
            "Piazza Riforma 1",
            "Lugano",
            "6900",
            "TI",
            "CH");
        var debtorAddress = new Address(
            "Bahnhofstrasse 10",
            "Zurich",
            "8001",
            "ZH",
            "CH");
        var qrIban = new QrIban("CH4431999123456789012");
        var rawAmount = Money.Chf(100.03m); // Rounds to 100.05

        // Act
        var bill = SwissQrBill.Create(
            tenantId,
            "QR-2026-005",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Swiss IT Solutions AG",
            creditorAddress,
            qrIban,
            "Debtor AG",
            debtorAddress,
            rawAmount,
            "21 00000 00003 13947 14300 09017",
            DateTime.UtcNow.AddDays(30),
            "Fattura n. QR-2026-005");

        bill.MarkAsPaid(DateTime.UtcNow);

        // Assert
        _ = bill.Amount.Amount.Should().Be(100.05m);
        _ = bill.ReferenceNumber.Should().Be("210000000003139471430009017");
        _ = bill.IsPaid.Should().BeTrue();
        _ = bill.PaidDateUtc.Should().NotBeNull();
    }

    /// <summary>
    /// Tests Employee cross-border commuter and residence country calculation.
    /// </summary>
    [Theory]
    [InlineData(
        true,
        "TI",
        "IT")]
    [InlineData(
        false,
        "TI",
        "CH")]
    [InlineData(
        false,
        null,
        "IT")]
    public void Employee_Create_ShouldCalculateFiscalResidenceCountry(
        bool isFrontaliere,
        string? canton,
        string expectedResidence)
    {
        // Arrange
        var address = new Address(
            "Via Dogana 1",
            "Como",
            "22100",
            "CO",
            "IT");

        // Act
        var employee = Employee.Create(
            Guid.NewGuid(),
            "EMP-007",
            "Giovanni",
            "Verdi",
            new DateTime(
                1990,
                5,
                12),
            WorkerType.Employee,
            isFrontaliere,
            address,
            "Software Architect",
            6500m,
            Currency.CHF,
            canton);

        // Assert
        _ = employee.FiscalResidenceCountry.Should().Be(expectedResidence);
        _ = employee.IsFrontaliereCrossBorder.Should().Be(isFrontaliere);
        _ = employee.EmployeeCode.Should().Be("EMP-007");
    }

    /// <summary>
    /// Tests TimeEntry clock in, clock out, and total hours computation.
    /// </summary>
    [Fact]
    public void TimeEntry_ClockInAndClockOut_ShouldCalculateNetHours()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var clockIn = new DateTime(
            2026,
            10,
            20,
            8,
            0,
            0,
            DateTimeKind.Utc);
        var clockOut = new DateTime(
            2026,
            10,
            20,
            17,
            30,
            0,
            DateTimeKind.Utc); // 9.5 hours = 570 mins

        // Act
        var entry = TimeEntry.ClockIn(
            tenantId,
            employeeId,
            clockIn,
            RecordingType.Detailed,
            45.8,
            9.08,
            "TAG-1234");
        _ = entry.TotalNetHours.Should().Be(0m); // before clock out

        entry.ClockOut(
            clockOut,
            60); // 570 - 60 = 510 mins = 8.50 hours

        // Assert
        _ = entry.TotalNetHours.Should().Be(8.50m);
        _ = entry.BreakDurationMinutes.Should().Be(60);
        _ = entry.BadgeSerialNumber.Should().Be("TAG-1234");
    }

    /// <summary>
    /// Tests TimeEntry clock out with timestamp earlier than clock in throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void TimeEntry_ClockOutEarlierThanClockIn_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var entry = TimeEntry.ClockIn(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow);

        // Act
        var act = () => entry.ClockOut(DateTime.UtcNow.AddHours(-1));

        // Assert
        _ = act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Clock out cannot be earlier than clock in*");
    }

    /// <summary>
    /// Tests TireHotelRecord check-in and mounting on vehicle.
    /// </summary>
    [Fact]
    public void TireHotelRecord_CheckInAndMount_ShouldUpdateStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        // Act
        var record = TireHotelRecord.CheckIn(
            tenantId,
            vehicleId,
            TireSeason.Winter,
            "Pirelli",
            "205/55 R16",
            "3824",
            7.5m,
            7.5m,
            6.8m,
            6.8m,
            "RACK-B-02");

        _ = record.Status.Should().Be("InStorage");
        _ = record.WarehouseRackCoordinate.Should().Be("RACK-B-02");

        record.MountOnVehicle();

        // Assert
        _ = record.Status.Should().Be("MountedOnVehicle");
        _ = record.RetrievedDateUtc.Should().NotBeNull();
        _ = record.UpdatedAtUtc.Should().NotBeNull();
    }
}
