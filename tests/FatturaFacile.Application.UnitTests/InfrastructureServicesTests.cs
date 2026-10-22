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
        var hasher = new Pbkdf2PasswordHasher();
        const string password = "SuperSecretPassword123!";

        // Act
        hasher.CreateHash(
            password,
            out var hash,
            out var salt);

        // Assert
        _ = hash.Should().NotBeNullOrWhiteSpace();
        _ = salt.Should().NotBeNullOrWhiteSpace();

        _ = hasher.VerifyHash(
            password,
            hash,
            salt).Should().BeTrue();

        _ = hasher.VerifyHash(
            "WrongPassword!",
            hash,
            salt).Should().BeFalse();
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
        "",
        "hash",
        "salt")]
    [InlineData(
        "pwd",
        "",
        "salt")]
    [InlineData(
        "pwd",
        "hash",
        "")]
    [InlineData(
        "pwd",
        "not_base_64",
        "not_base_64")]
    public void Pbkdf2PasswordHasher_VerifyHash_InvalidInputs_ShouldReturnFalse(
        string pwd,
        string hash,
        string salt)
    {
        // Arrange
        var hasher = new Pbkdf2PasswordHasher();

        // Act & Assert
        _ = hasher.VerifyHash(
            pwd,
            hash,
            salt).Should().BeFalse();
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
            string.Empty,
            out _,
            out _);

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
            Guid.NewGuid(),
            "QR-2026-001",
            new DateTime(
                2026,
                10,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc),
            Guid.NewGuid(),
            "Swiss Cloud Services AG",
            new Address(
                "Via Cantonale 10",
                "Lugano",
                "6900",
                "TI",
                "CH"),
            new QrIban("CH4431999123456789012"),
            "Client Solutions Sagl",
            new Address(
                "Via Nassa 2",
                "Lugano",
                "6900",
                "TI",
                "CH"),
            Money.Chf(450.00m),
            "210000000003139471430009017",
            new DateTime(
                2026,
                11,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc),
            "Fattura licenza cloud");

        // Act
        var payload = SwissQrBillGenerator.GenerateQrPayload(bill);

        // Assert
        _ = payload.Should().StartWith("SPC\n0200\n1\n");
        _ = payload.Should().Contain("CH4431999123456789012");
        _ = payload.Should().Contain("Swiss Cloud Services AG");
        _ = payload.Should().Contain("450.00\nCHF\n");
        _ = payload.Should().Contain("QRR\n21 00000 00003 13947 14300 09017\n");
        _ = payload.Should().EndWith("EPD\n");
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
        "21000000000313947143000901",
        7)]
    [InlineData(
        "00000000000000000000000000",
        0)]
    [InlineData(
        "123456",
        5)]
    public void SwissQrBillGenerator_CalculateModulo10_ShouldComputeCorrectChecksumDigit(
        string input,
        int expectedDigit)
    {
        // Act
        var digit = SwissQrBillGenerator.CalculateModulo10(input);

        // Assert
        _ = digit.Should().Be(expectedDigit);
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
            Guid.NewGuid(),
            "QR-2026-002",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Swiss Creditor AG",
            new Address(
                "Piazza Riforma 1",
                "Lugano",
                "6900",
                "TI",
                "CH"),
            new QrIban("CH4431999123456789012"),
            "Swiss Debtor Sagl",
            new Address(
                "Via Maggio 4",
                "Lugano",
                "6900",
                "TI",
                "CH"),
            Money.Chf(850.50m),
            "210000000003139471430009017",
            DateTime.UtcNow.AddDays(30));

        // Act
        var svg = SwissQrBillGenerator.GenerateSvgPreview(bill);

        // Assert
        _ = svg.Should().Contain("<svg");
        _ = svg.Should().Contain("</svg>");
        _ = svg.Should().Contain("850.50");
        _ = svg.Should().Contain("CHF");
        _ = svg.Should().Contain("Swiss Creditor AG");
        _ = svg.Should().Contain("Swiss Debtor Sagl");
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
            "Via Torino 1",
            "Milano",
            "20100",
            "MI",
            "IT");

        var cessionarioAddr = new Address(
            "Via Roma 5",
            "Torino",
            "10100",
            "TO",
            "IT");

        var invoice = ItalianElectronicInvoice.Create(
            Guid.NewGuid(),
            "FPA-2026-001",
            new DateTime(
                2026,
                10,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc),
            Guid.NewGuid(),
            "Azienda Fornitrice SpA",
            "12345678901",
            "12345678901",
            cedenteAddr,
            "Azienda Cliente Srl",
            "98765432109",
            "98765432109",
            "M5UXCR1",
            "cliente@pec.it",
            cessionarioAddr,
            new DateTime(
                2026,
                11,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc));

        invoice.AddLine(
            "Servizi Cloud e Hosting",
            12m,
            150m,
            22m);

        // Act
        var xml = FatturaPaXmlGenerator.GenerateXml(invoice);

        // Assert
        _ = xml.Should().NotBeNullOrWhiteSpace();
        _ = xml.Should().Contain("FatturaElettronica");
        _ = xml.Should().Contain("FatturaElettronicaHeader");
        _ = xml.Should().Contain("DatiTrasmissione");
        _ = xml.Should().Contain("M5UXCR1");
        _ = xml.Should().Contain("CedentePrestatore");
        _ = xml.Should().Contain("Azienda Fornitrice SpA");
        _ = xml.Should().Contain("12345678901");
        _ = xml.Should().Contain("CessionarioCommittente");
        _ = xml.Should().Contain("Azienda Cliente Srl");
        _ = xml.Should().Contain("DatiBeniServizi");
        _ = xml.Should().Contain("Servizi Cloud e Hosting");
        _ = xml.Should().Contain("1800.00");
    }
}