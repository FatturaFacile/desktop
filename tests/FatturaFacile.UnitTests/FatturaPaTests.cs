// <copyright file="FatturaPaTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Xml.Linq;
using FatturaFacile.Application.Strategies.Italian;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests for FatturaPA XML generation and pre-transmission validation.
/// </summary>
public sealed class FatturaPaTests
{
    /// <summary>
    /// Verifies the namespace, mandatory blocks and VAT summary grouping by rate and nature.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Strategy_GeneratesSchemaShapedXml()
    {
        var tenant = TestData.ItalianTenant();
        var contact = TestData.ItalianContact();
        var invoice = TestData.Invoice(
            tenant.Id,
            "EUR",
            TestData.Line("Consulenza", 2m, 500m, 22m),
            TestData.Line("Licenza", 1m, 300m, 22m),
            TestData.Line("Formazione esente", 1m, 100m, 0m, "N4"));

        await new ItalianInvoicingStrategy().ProcessInvoiceComplianceAsync(invoice, tenant, contact);

        Assert.NotNull(invoice.SdiMetadata);
        Assert.Null(invoice.SdiMetadata.ValidationErrors);
        Assert.Equal("generated", invoice.SdiMetadata.SdiStatus);
        Assert.Null(invoice.SdiMetadata.SdiIdentifier);
        Assert.Equal("IT01234567890_00001.xml", invoice.SdiMetadata.FileName);
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", invoice.SdiMetadata.XmlContent, StringComparison.OrdinalIgnoreCase);

        var xml = XDocument.Parse(invoice.SdiMetadata.XmlContent!);
        var root = xml.Root!;

        Assert.Equal(FatturaPaXmlBuilder.FatturaPaNamespace + "FatturaElettronica", root.Name);
        Assert.Equal("FPR12", root.Attribute("versione")!.Value);
        Assert.Equal(3, root.Descendants("DettaglioLinee").Count());

        var summaries = root.Descendants("DatiRiepilogo").ToList();
        Assert.Equal(2, summaries.Count);
        Assert.Equal("1300.00", summaries[0].Element("ImponibileImporto")!.Value);
        Assert.Equal("286.00", summaries[0].Element("Imposta")!.Value);
        Assert.Equal("N4", summaries[1].Element("Natura")!.Value);

        Assert.Equal("1686.00", root.Descendants("ImportoTotaleDocumento").Single().Value);
        Assert.Equal("2.00", root.Descendants("ImportoBollo").Single().Value);
        Assert.Equal("TP02", root.Descendants("CondizioniPagamento").Single().Value);
        _ = Assert.Single(root.Descendants("Sede"), static s => s.Parent!.Name == "CedentePrestatore");
    }

    /// <summary>
    /// Verifies that missing mandatory data marks the file invalid instead of producing a file SDI would reject.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Strategy_MarksInvalidData()
    {
        var tenant = TestData.ItalianTenant();
        tenant.PostalCode = null;
        var contact = TestData.ItalianContact();
        var invoice = TestData.Invoice(tenant.Id, "EUR", TestData.Line("Servizio", 1m, 100m, 0m));

        await new ItalianInvoicingStrategy().ProcessInvoiceComplianceAsync(invoice, tenant, contact);

        Assert.NotNull(invoice.SdiMetadata);
        Assert.Equal("invalid", invoice.SdiMetadata.SdiStatus);
        Assert.Null(invoice.SdiMetadata.XmlContent);
        Assert.Contains("CAP del cedente", invoice.SdiMetadata.ValidationErrors, StringComparison.Ordinal);
        Assert.Contains("Natura", invoice.SdiMetadata.ValidationErrors, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies withholding tax, split payment chargeability and the FPA12 format for public administrations.
    /// </summary>
    [Fact]
    public void Builder_HandlesWithholdingSplitPaymentAndPublicAdministration()
    {
        var tenant = TestData.ItalianTenant();
        var contact = TestData.ItalianContact();
        contact.IsPublicAdministration = true;
        contact.SdiCode = "UFY9MH";
        var invoice = TestData.Invoice(tenant.Id, "EUR", TestData.Line("Parcella", 1m, 1000m, 22m));
        invoice.SplitPayment = true;
        invoice.WithholdingTaxEnabled = true;
        invoice.WithholdingTaxPercent = 20m;

        Assert.Empty(FatturaPaValidator.Validate(invoice, tenant, contact));

        var root = FatturaPaXmlBuilder.Build(invoice, tenant, contact, "00001").Root!;

        Assert.Equal("FPA12", root.Attribute("versione")!.Value);
        Assert.Equal("UFY9MH", root.Descendants("CodiceDestinatario").Single().Value);
        Assert.Equal("200.00", root.Descendants("ImportoRitenuta").Single().Value);
        Assert.Equal("S", root.Descendants("EsigibilitaIVA").Single().Value);

        // 1000 + 220 VAT - 200 withholding - 220 VAT paid by the PA (split payment).
        Assert.Equal("800.00", root.Descendants("ImportoPagamento").Single().Value);
    }
}
