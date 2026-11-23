// <copyright file="CertificazioneUnicaTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Certificazioni Uniche built from the withholdings of the year, their PDF layout and the telematic working file.
/// </summary>
public sealed class CertificazioneUnicaTests
{
    private static readonly Guid Lawyer = Guid.NewGuid();
    private static readonly Guid Agent = Guid.NewGuid();

    private static readonly Dictionary<Guid, Contact> Contacts = new()
    {
        [Lawyer] = new Contact { Id = Lawyer, FirstName = "Anna", LastName = "Bianchi", FiscalCode = "BNCNNA80A41H501Z", VatNumber = "01234567897" },
        [Agent] = new Contact { Id = Agent, CompanyName = "Agenzia Verdi", FiscalCode = "12345678903" },
    };

    /// <summary>
    /// One certificate is built per percipient and causale, with the amounts summed and the withholdings paid with the F24 told.
    /// </summary>
    [Fact]
    public void Build_GroupsPerPercipientAndReasonAndTellsWhatWasPaid()
    {
        List<WithholdingLine> lines =
        [
            Line(Lawyer, month: 1, "A", "1040", gross: 1000m, withheld: 200m),
            Line(Lawyer, month: 2, "A", "1040", gross: 500m, withheld: 100m),
            Line(Agent, month: 2, "R", "1040", gross: 2000m, withheld: 230m),
            Line(Lawyer, month: 1, "A", "1040", gross: 300m, withheld: 60m, year: 2026),
        ];
        List<WithholdingF24Payment> payments =
        [
            new() { Year = 2027, Month = 1, TaxCode = "1040", PaidDate = new DateTime(2027, 2, 16, 0, 0, 0, DateTimeKind.Unspecified) },
            new() { Year = 2027, Month = 2, TaxCode = "1040", PaidDate = null },
        ];

        var certificates = CertificazioneUnicaBuilder.Build(2027, lines, Contacts, payments);

        Assert.Equal(2, certificates.Count);
        var lawyer = Assert.Single(certificates, static c => string.Equals(c.Reason, "A", StringComparison.Ordinal));
        Assert.Equal("Bianchi Anna", lawyer.RecipientName);
        Assert.Equal("BNCNNA80A41H501Z", lawyer.RecipientTaxCode);
        Assert.Equal(1500m, lawyer.GrossPaid);
        Assert.Equal(300m, lawyer.WithheldAmount);
        Assert.Equal(200m, lawyer.WithheldPaidAmount);
        var agent = Assert.Single(certificates, static c => string.Equals(c.Reason, "R", StringComparison.Ordinal));
        Assert.Equal("Agenzia Verdi", agent.RecipientName);
        Assert.Equal(0m, agent.WithheldPaidAmount);
        Assert.All(certificates, static c => Assert.Equal(WithholdingCertificateStatus.Draft, c.Status));
    }

    /// <summary>
    /// The telematic working file has a header, a record per certificate and a trailer, with invariant amounts and no separator
    /// inside a value.
    /// </summary>
    [Fact]
    public void TelematicFile_HasHeaderRecordsAndTrailer()
    {
        WithholdingCertificate certificate = new() { Year = 2027, RecipientName = "Rossi | Figli", RecipientTaxCode = "abc", Reason = "A", GrossPaid = 1500m, TaxableAmount = 1200m, WithheldAmount = 240m, WithheldPaidAmount = 200m };

        var text = Encoding.UTF8.GetString(CuTelematicFileBuilder.Build("01234567897", 2027, [certificate]));
        var records = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(3, records.Length);
        Assert.Equal("A|CU|2028|01234567897|1", records[0]);
        Assert.Equal("B|2027|ABC|ROSSI   FIGLI|A|1500.00|1200.00|240.00|200.00", records[1]);
        Assert.Equal("Z|1", records[2]);
        Assert.Equal("CU_01234567897_2027.txt", CuTelematicFileBuilder.FileName("01234567897", 2027));
    }

    /// <summary>
    /// The PDF layout has a row per certificate and the totals and the notice in the footer.
    /// </summary>
    [Fact]
    public void PdfContent_HasARowPerCertificate()
    {
        WithholdingCertificate first = new() { RecipientName = "Bianchi Anna", Reason = "A", WithheldAmount = 300m, WithheldPaidAmount = 200m };
        WithholdingCertificate second = new() { RecipientName = "Agenzia Verdi", Reason = "R", WithheldAmount = 230m };

        var document = CuPdfContent.Build(2027, "Alpha Srl", [first, second], static key => key);

        Assert.Equal(7, document.Headers.Count);
        Assert.Equal(2, document.Rows.Count);
        Assert.Equal("Bianchi Anna", document.Rows[0][0]);
        Assert.Equal("Alpha Srl", document.Subtitle);
        Assert.Equal(2, document.Footer.Count);
    }

    private static WithholdingLine Line(Guid supplier, int month, string reason, string code, decimal gross, decimal withheld, int year = 2027) => new()
    {
        SupplierId = supplier,
        Year = year,
        Month = month,
        Reason = reason,
        F24Code = code,
        GrossPaid = gross,
        TaxableAmount = gross * 0.8m,
        WithheldAmount = withheld,
    };
}
