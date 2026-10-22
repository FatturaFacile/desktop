// <copyright file="InfrastructureServicesTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.ValueObjects;
using FatturaFacile.Infrastructure.CrossBorder.FatturaPa;
using FatturaFacile.Infrastructure.CrossBorder.SwissQr;
using FatturaFacile.Infrastructure.Security;

namespace FatturaFacile.Application.UnitTests;

/// <summary>
/// Contains unit tests that verify the behavior of infrastructure services,
/// including password hashing and cross-border invoice generation.
/// </summary>
public sealed class InfrastructureServicesTests
{

    /// <summary>
    /// Verifies that a password hash can be created and successfully verified
    /// using the original password while rejecting an invalid password.
    /// </summary>
    [Fact]
    public void Pbkdf2PasswordHasher_CreateAndVerify_ShouldSucceedForMatchingPassword()
    {
        // Arrange
        var          hasher   = new Pbkdf2PasswordHasher();
        const string password = "SuperSecretPassword123!";

        // Act
        hasher.CreateHash(
                          password: password,
                          hash: out var hash,
                          salt: out var salt);

        // Assert
        _ = hash.Should().NotBeNullOrWhiteSpace();
        _ = salt.Should().NotBeNullOrWhiteSpace();

        _ = hasher.VerifyHash(
                              password: password,
                              hash: hash,
                              salt: salt)
                  .Should()
                  .BeTrue();

        _ = hasher.VerifyHash(
                              password: "WrongPassword!",
                              hash: hash,
                              salt: salt)
                  .Should()
                  .BeFalse();
    }

    /// <summary>
    /// Verifies that password verification returns <see langword="false"/>
    /// when invalid, empty, or malformed input values are provided.
    /// </summary>
    /// <param name="pwd">
    /// The password to verify.
    /// </param>
    /// <param name="hash">
    /// The password hash to validate against.
    /// </param>
    /// <param name="salt">
    /// The salt associated with the password hash.
    /// </param>
    [Theory]
    [InlineData(
                   data: ["", "hash", "salt"])]
    [InlineData(
                   data: ["pwd", "", "salt"])]
    [InlineData(
                   data: ["pwd", "hash", ""])]
    [InlineData(
                   data: ["pwd", "not_base_64", "not_base_64"])]
    public void Pbkdf2PasswordHasher_VerifyHash_InvalidInputs_ShouldReturnFalse(
        string pwd,
        string hash,
        string salt)
    {
        // Arrange
        var hasher = new Pbkdf2PasswordHasher();

        // Act & Assert
        _ = hasher.VerifyHash(
                              password: pwd,
                              hash: hash,
                              salt: salt)
                  .Should()
                  .BeFalse();
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentException"/> is thrown when
    /// attempting to create a password hash from an empty password.
    /// </summary>
    [Fact]
    public void Pbkdf2PasswordHasher_CreateHash_EmptyPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var hasher = new Pbkdf2PasswordHasher();

        // Act
        var act = () => hasher.CreateHash(
                                          password: string.Empty,
                                          hash: out _,
                                          salt: out _);

        // Assert
        _ = act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that a Swiss QR bill payload is generated in the expected SPC
    /// format and contains the required creditor, debtor, and payment data.
    /// </summary>
    [Fact]
    public void SwissQrBillGenerator_GenerateQrPayload_ShouldProduceValidSpcStructure()
    {
        // Arrange
        var bill = SwissQrBill.Create(
                                      tenantId: Guid.NewGuid(),
                                      billNumber: "QR-2026-001",
                                      billDate: new DateTime(
                                                             year: 2026,
                                                             month: 10,
                                                             day: 20,
                                                             hour: 0,
                                                             minute: 0,
                                                             second: 0,
                                                             kind: DateTimeKind.Utc),
                                      clientId: Guid.NewGuid(),
                                      creditorName: "Swiss Cloud Services AG",
                                      creditorAddress: new Address(
                                                                   street: "Via Cantonale 10",
                                                                   city: "Lugano",
                                                                   postalCode: "6900",
                                                                   provinceOrCanton: "TI",
                                                                   countryCode: "CH"),
                                      creditorQrIban: new QrIban(value: "CH4431999123456789012"),
                                      debtorName: "Client Solutions Sagl",
                                      debtorAddress: new Address(
                                                                 street: "Via Nassa 2",
                                                                 city: "Lugano",
                                                                 postalCode: "6900",
                                                                 provinceOrCanton: "TI",
                                                                 countryCode: "CH"),
                                      rawAmount: Money.Chf(amount: 450.00m),
                                      referenceNumber: "210000000003139471430009017",
                                      dueDate: new DateTime(
                                                            year: 2026,
                                                            month: 11,
                                                            day: 20,
                                                            hour: 0,
                                                            minute: 0,
                                                            second: 0,
                                                            kind: DateTimeKind.Utc),
                                      message: "Fattura licenza cloud");

        // Act
        var payload = SwissQrBillGenerator.GenerateQrPayload(bill: bill);

        // Assert
        _ = payload.Should().StartWith(expected: "SPC\n0200\n1\n");
        _ = payload.Should().Contain(expected: "CH4431999123456789012");
        _ = payload.Should().Contain(expected: "Swiss Cloud Services AG");
        _ = payload.Should().Contain(expected: "450.00\nCHF\n");
        _ = payload.Should().Contain(expected: "QRR\n21 00000 00003 13947 14300 09017\n");
        _ = payload.Should().EndWith(expected: "EPD\n");
    }

    /// <summary>
    /// Verifies that the Modulo 10 recursive checksum digit is calculated
    /// correctly for the specified reference number.
    /// </summary>
    /// <param name="input">
    /// The reference number used to calculate the checksum digit.
    /// </param>
    /// <param name="expectedDigit">
    /// The expected Modulo 10 recursive checksum digit.
    /// </param>
    [Theory]
    [InlineData(
                   data: ["21000000000313947143000901", 7])]
    [InlineData(
                   data: ["00000000000000000000000000", 0])]
    [InlineData(
                   data: ["123456", 5])]
    public void SwissQrBillGenerator_CalculateModulo10_ShouldComputeCorrectChecksumDigit(
        string input,
        int    expectedDigit)
    {
        // Act
        var digit = SwissQrBillGenerator.CalculateModulo10(input: input);

        // Assert
        _ = digit.Should().Be(expected: expectedDigit);
    }

    /// <summary>
    /// Verifies that the generated SVG preview contains valid SVG markup and
    /// includes the expected invoice information.
    /// </summary>
    [Fact]
    public void SwissQrBillGenerator_GenerateSvgPreview_ShouldContainSvgMarkupAndSwissCross()
    {
        // Arrange
        var bill = SwissQrBill.Create(
                                      tenantId: Guid.NewGuid(),
                                      billNumber: "QR-2026-002",
                                      billDate: DateTime.UtcNow,
                                      clientId: Guid.NewGuid(),
                                      creditorName: "Swiss Creditor AG",
                                      creditorAddress: new Address(
                                                                   street: "Piazza Riforma 1",
                                                                   city: "Lugano",
                                                                   postalCode: "6900",
                                                                   provinceOrCanton: "TI",
                                                                   countryCode: "CH"),
                                      creditorQrIban: new QrIban(value: "CH4431999123456789012"),
                                      debtorName: "Swiss Debtor Sagl",
                                      debtorAddress: new Address(
                                                                 street: "Via Maggio 4",
                                                                 city: "Lugano",
                                                                 postalCode: "6900",
                                                                 provinceOrCanton: "TI",
                                                                 countryCode: "CH"),
                                      rawAmount: Money.Chf(amount: 850.50m),
                                      referenceNumber: "210000000003139471430009017",
                                      dueDate: DateTime.UtcNow.AddDays(value: 30));

        // Act
        var svg = SwissQrBillGenerator.GenerateSvgPreview(bill: bill);

        // Assert
        _ = svg.Should().Contain(expected: "<svg");
        _ = svg.Should().Contain(expected: "</svg>");
        _ = svg.Should().Contain(expected: "850.50");
        _ = svg.Should().Contain(expected: "CHF");
        _ = svg.Should().Contain(expected: "Swiss Creditor AG");
        _ = svg.Should().Contain(expected: "Swiss Debtor Sagl");
    }

    /// <summary>
    /// Verifies that a valid FatturaPA XML document is generated and contains
    /// the required transmission, supplier, customer, and line-item data.
    /// </summary>
    [Fact]
    public void FatturaPaXmlGenerator_GenerateXml_ShouldProduceValidXmlStructure()
    {
        // Arrange
        var cedenteAddr = new Address(
                                      street: "Via Torino 1",
                                      city: "Milano",
                                      postalCode: "20100",
                                      provinceOrCanton: "MI",
                                      countryCode: "IT");

        var cessionarioAddr = new Address(
                                          street: "Via Roma 5",
                                          city: "Torino",
                                          postalCode: "10100",
                                          provinceOrCanton: "TO",
                                          countryCode: "IT");

        var invoice = ItalianElectronicInvoice.Create(
                                                      tenantId: Guid.NewGuid(),
                                                      documentNumber: "FPA-2026-001",
                                                      documentDate: new DateTime(
                                                                                 year: 2026,
                                                                                 month: 10,
                                                                                 day: 20,
                                                                                 hour: 0,
                                                                                 minute: 0,
                                                                                 second: 0,
                                                                                 kind: DateTimeKind.Utc),
                                                      clientId: Guid.NewGuid(),
                                                      cedenteName: "Azienda Fornitrice SpA",
                                                      cedenteVat: "12345678901",
                                                      cedenteCf: "12345678901",
                                                      cedenteAddress: cedenteAddr,
                                                      cessionarioName: "Azienda Cliente Srl",
                                                      cessionarioVat: "98765432109",
                                                      cessionarioCf: "98765432109",
                                                      sdiCode: "M5UXCR1",
                                                      pec: "cliente@pec.it",
                                                      cessionarioAddress: cessionarioAddr,
                                                      dueDate: new DateTime(
                                                                            year: 2026,
                                                                            month: 11,
                                                                            day: 20,
                                                                            hour: 0,
                                                                            minute: 0,
                                                                            second: 0,
                                                                            kind: DateTimeKind.Utc));

        invoice.AddLine(
                        description: "Servizi Cloud e Hosting",
                        quantity: 12m,
                        unitPrice: 150m,
                        vatRate: 22m);

        // Act
        var xml = FatturaPaXmlGenerator.GenerateXml(invoice: invoice);

        // Assert
        _ = xml.Should().NotBeNullOrWhiteSpace();
        _ = xml.Should().Contain(expected: "FatturaElettronica");
        _ = xml.Should().Contain(expected: "FatturaElettronicaHeader");
        _ = xml.Should().Contain(expected: "DatiTrasmissione");
        _ = xml.Should().Contain(expected: "M5UXCR1");
        _ = xml.Should().Contain(expected: "CedentePrestatore");
        _ = xml.Should().Contain(expected: "Azienda Fornitrice SpA");
        _ = xml.Should().Contain(expected: "12345678901");
        _ = xml.Should().Contain(expected: "CessionarioCommittente");
        _ = xml.Should().Contain(expected: "Azienda Cliente Srl");
        _ = xml.Should().Contain(expected: "DatiBeniServizi");
        _ = xml.Should().Contain(expected: "Servizi Cloud e Hosting");
        _ = xml.Should().Contain(expected: "1800.00");
    }

}
