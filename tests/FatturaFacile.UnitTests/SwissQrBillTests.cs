// <copyright file="SwissQrBillTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Validation;
using FatturaFacile.Application.Strategies.Swiss;
using FatturaFacile.Infrastructure.Services;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests for IBAN validation, QR / creditor references and the Swiss QR-bill payload (SPC 0200).
/// </summary>
public sealed class SwissQrBillTests
{
    /// <summary>
    /// Verifies IBAN MOD 97 validation.
    /// </summary>
    /// <param name="iban">The IBAN.</param>
    /// <param name="expected">The expected validity.</param>
    [Theory]
    [InlineData("CH93 0076 2011 6238 5295 7", true)]
    [InlineData("CH4431999123000889012", true)]
    [InlineData("IT60X0542811101000000123456", true)]
    [InlineData("CH4431999123000889013", false)]
    [InlineData("CH93007620116238529", false)]
    public void IbanValidator_ValidatesChecksum(string iban, bool expected) => Assert.Equal(expected, IbanValidator.IsValid(iban));

    /// <summary>
    /// Verifies QR-IBAN detection by institution id 30000-31999.
    /// </summary>
    [Fact]
    public void IbanValidator_DetectsQrIban()
    {
        Assert.True(IbanValidator.IsSwissQrIban("CH4431999123000889012"));
        Assert.False(IbanValidator.IsSwissQrIban("CH9300762011623852957"));
    }

    /// <summary>
    /// Verifies the QR reference check digit against the SIX sample reference.
    /// </summary>
    [Fact]
    public void QrReference_MatchesSixSample()
    {
        Assert.True(SwissPaymentReference.IsValidQrReference("21 00000 00003 13947 14300 09017"));
        Assert.False(SwissPaymentReference.IsValidQrReference("210000000003139471430009018"));
        Assert.Equal("000000000000000000020260010", SwissPaymentReference.CreateQrReference("FAT-2026-001"));
    }

    /// <summary>
    /// Verifies ISO 11649 creditor reference generation and validation.
    /// </summary>
    [Fact]
    public void CreditorReference_IsIso11649()
    {
        Assert.Equal("RF18539007547034", SwissPaymentReference.CreateCreditorReference("539007547034"));
        Assert.True(SwissPaymentReference.IsValidCreditorReference("RF18 5390 0754 7034"));
        Assert.False(SwissPaymentReference.IsValidCreditorReference("RF19539007547034"));
    }

    /// <summary>
    /// Verifies the payload structure (31 elements, header, structured addresses, trailer).
    /// </summary>
    [Fact]
    public void PayloadBuilder_ProducesSpc0200Payload()
    {
        SwissQrBillRequest request = new(
            "CH4431999123000889012",
            new SwissQrBillAddress("Robert Schneider AG", "Rue du Lac", "1268", "2501", "Biel", "CH"),
            1949.75m,
            "CHF",
            new SwissQrBillAddress("Pia-Maria Rutschmann-Schnyder", "Grosse Marktgasse", "28", "9400", "Rorschach", "CH"),
            "QRR",
            "210000000003139471430009017",
            "Ordine del 15.10.2026");

        Assert.Empty(SwissQrBillPayloadBuilder.Validate(request));

        var lines = SwissQrBillPayloadBuilder.Build(request).Split("\r\n");

        Assert.Equal(31, lines.Length);
        string[] expectedHeader = ["SPC", "0200", "1", "CH4431999123000889012", "S"];
        Assert.Equal(expectedHeader, lines[..5]);
        Assert.Equal("1949.75", lines[18]);
        Assert.Equal("CHF", lines[19]);
        Assert.Equal("QRR", lines[27]);
        Assert.Equal("EPD", lines[30]);
    }

    /// <summary>
    /// Verifies the business rule: a QR reference requires a QR-IBAN.
    /// </summary>
    [Fact]
    public void PayloadBuilder_RejectsQrReferenceWithoutQrIban()
    {
        SwissQrBillRequest request = new(
            "CH9300762011623852957",
            new SwissQrBillAddress("Robert Schneider AG", "Rue du Lac", "1268", "2501", "Biel", "CH"),
            100m,
            "CHF",
            Debtor: null,
            "QRR",
            "210000000003139471430009017",
            UnstructuredMessage: null);

        Assert.Contains(SwissQrBillPayloadBuilder.Validate(request), static e => e.Contains("QR-IBAN", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that the real encoder produces a square QR matrix and that the SVG embeds it with the Swiss cross.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Encoder_And_Renderer_ProduceQrBillSvg()
    {
        var strategy = new SwissInvoicingStrategy(new QrCoderQrCodeEncoder());
        var tenant = TestData.SwissTenant(qrIban: "CH4431999123000889012");
        var contact = TestData.SwissContact();
        var invoice = TestData.Invoice(tenant.Id, "CHF", TestData.Line("Consulenza", 2m, 625.25m, 8.1m));

        await strategy.ProcessInvoiceComplianceAsync(invoice, tenant, contact);

        Assert.NotNull(invoice.QrBillData);
        Assert.Null(invoice.QrBillData.ValidationErrors);
        Assert.Equal("QRR", invoice.QrBillData.ReferenceType);
        Assert.StartsWith("SPC\r\n0200\r\n1\r\n", invoice.QrBillData.QrPayload, StringComparison.Ordinal);
        Assert.Contains("<path fill=\"#000000\" d=\"M", invoice.QrBillData.QrBillSvg, StringComparison.Ordinal);
        Assert.Contains("Sezione pagamento", invoice.QrBillData.QrBillSvg, StringComparison.Ordinal);
    }
}
