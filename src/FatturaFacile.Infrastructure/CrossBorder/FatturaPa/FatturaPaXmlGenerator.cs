// <copyright file="FatturaPaXmlGenerator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml.Linq;
using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Infrastructure.CrossBorder.FatturaPa;

/// <summary>
/// Represents fattura pa xml generator.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Security",
    "S5332:Using http protocol is insecure",
    Justification = "XML Schema URI specifications per Agenzia delle Entrate and W3C standards")]
public static class FatturaPaXmlGenerator
{
    private static readonly XNamespace P = "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2";

    private static readonly XNamespace Ds = "http://www.w3.org/2000/09/xmldsig#";

    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

    /// <summary>
    /// Generate Xml.
    /// </summary>
    /// <param name="invoice">The invoice.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static string GenerateXml(
        ItalianElectronicInvoice invoice)
    {
        var root = new XElement(
            P + "FatturaElettronica",
            new XAttribute(
                "versione",
                invoice.FormatoTrasmissione),
            new XAttribute(
                XNamespace.Xmlns + "p",
                P.NamespaceName),
            new XAttribute(
                XNamespace.Xmlns + "ds",
                Ds.NamespaceName),
            new XAttribute(
                XNamespace.Xmlns + "xsi",
                Xsi.NamespaceName),
            BuildHeader(invoice),
            BuildBody(invoice));

        var doc = new XDocument(
            new XDeclaration(
                "1.0",
                "UTF-8",
                standalone: null),
            root);
        return doc.ToString();
    }

    private static XElement BuildHeader(
        ItalianElectronicInvoice invoice)
    {
        return new XElement(
            P + "FatturaElettronicaHeader",
            new XElement(
                P + "DatiTrasmissione",
                new XElement(
                    P + "IdTrasmittente",
                    new XElement(
                        P + "IdPaese",
                        "IT"),
                    new XElement(
                        P + "IdCodice",
                        invoice.CedentePartitaIva)),
                new XElement(
                    P + "ProgressivoInvio",
                    invoice.ProgressivoInvio),
                new XElement(
                    P + "FormatoTrasmissione",
                    invoice.FormatoTrasmissione),
                new XElement(
                    P + "CodiceDestinatario",
                    invoice.CessionarioCodiceDestinatario),
                string.IsNullOrWhiteSpace(invoice.CessionarioPec)
                    ? null
                    : new XElement(
                        P + "PECDestinatario",
                        invoice.CessionarioPec)),
            new XElement(
                P + "CedentePrestatore",
                new XElement(
                    P + "DatiAnagrafici",
                    new XElement(
                        P + "IdFiscaleIVA",
                        new XElement(
                            P + "IdPaese",
                            "IT"),
                        new XElement(
                            P + "IdCodice",
                            invoice.CedentePartitaIva)),
                    string.IsNullOrWhiteSpace(invoice.CedenteCodiceFiscale)
                        ? null
                        : new XElement(
                            P + "CodiceFiscale",
                            invoice.CedenteCodiceFiscale),
                    new XElement(
                        P + "Anagrafica",
                        new XElement(
                            P + "Denominazione",
                            invoice.CedenteDenominazione)),
                    new XElement(
                        P + "RegimeFiscale",
                        "RF01")),
                new XElement(
                    P + "Sede",
                    new XElement(
                        P + "Indirizzo",
                        invoice.CedenteIndirizzo.Street),
                    new XElement(
                        P + "CAP",
                        invoice.CedenteIndirizzo.PostalCode),
                    new XElement(
                        P + "Comune",
                        invoice.CedenteIndirizzo.City),
                    new XElement(
                        P + "Provincia",
                        invoice.CedenteIndirizzo.ProvinceOrCanton),
                    new XElement(
                        P + "Nazione",
                        invoice.CedenteIndirizzo.CountryCode))),
            new XElement(
                P + "CessionarioCommittente",
                new XElement(
                    P + "DatiAnagrafici",
                    !string.IsNullOrWhiteSpace(invoice.CessionarioPartitaIva)
                        ? new XElement(
                            P + "IdFiscaleIVA",
                            new XElement(
                                P + "IdPaese",
                                invoice.CessionarioIndirizzo.CountryCode),
                            new XElement(
                                P + "IdCodice",
                                invoice.CessionarioPartitaIva))
                        : null,
                    !string.IsNullOrWhiteSpace(invoice.CessionarioCodiceFiscale)
                        ? new XElement(
                            P + "CodiceFiscale",
                            invoice.CessionarioCodiceFiscale)
                        : null,
                    new XElement(
                        P + "Anagrafica",
                        new XElement(
                            P + "Denominazione",
                            invoice.CessionarioDenominazione))),
                new XElement(
                    P + "Sede",
                    new XElement(
                        P + "Indirizzo",
                        invoice.CessionarioIndirizzo.Street),
                    new XElement(
                        P + "CAP",
                        invoice.CessionarioIndirizzo.PostalCode),
                    new XElement(
                        P + "Comune",
                        invoice.CessionarioIndirizzo.City),
                    new XElement(
                        P + "Provincia",
                        invoice.CessionarioIndirizzo.ProvinceOrCanton),
                    new XElement(
                        P + "Nazione",
                        invoice.CessionarioIndirizzo.CountryCode))));
    }

    private static XElement BuildBody(
        ItalianElectronicInvoice invoice)
    {
        var culture = CultureInfo.InvariantCulture;

        var beniServizi = new XElement(P + "DatiBeniServizi");
        foreach (var line in invoice.Lines)
        {
            beniServizi.Add(
                new XElement(
                    P + "DettaglioLinee",
                    new XElement(
                        P + "NumeroLinea",
                        line.LineNumber),
                    new XElement(
                        P + "Descrizione",
                        line.Description),
                    new XElement(
                        P + "Quantita",
                        line.Quantity.ToString(
                            "0.00",
                            culture)),
                    new XElement(
                        P + "PrezzoUnitario",
                        line.UnitPrice.ToString(
                            "0.00",
                            culture)),
                    new XElement(
                        P + "PrezzoTotale",
                        line.LineTotal.ToString(
                            "0.00",
                            culture)),
                    new XElement(
                        P + "AliquotaIVA",
                        line.VatRate.ToString(
                            "0.00",
                            culture)),
                    string.IsNullOrWhiteSpace(line.NaturaExemptionCode)
                        ? null
                        : new XElement(
                            P + "Natura",
                            line.NaturaExemptionCode)));
        }

        // Summary row
        beniServizi.Add(
            new XElement(
                P + "DatiRiepilogo",
                new XElement(
                    P + "AliquotaIVA",
                    "22.00"),
                new XElement(
                    P + "ImponibileImporto",
                    invoice.TotalNet.ToString(
                        "0.00",
                        culture)),
                new XElement(
                    P + "Imposta",
                    invoice.TotalVat.ToString(
                        "0.00",
                        culture)),
                new XElement(
                    P + "EsigibilitaIVA",
                    "I")));

        return new XElement(
            P + "FatturaElettronicaBody",
            new XElement(
                P + "DatiGenerali",
                new XElement(
                    P + "DatiGeneraliDocumento",
                    new XElement(
                        P + "TipoDocumento",
                        "TD01"),
                    new XElement(
                        P + "Divisa",
                        invoice.Currency.ToString()),
                    new XElement(
                        P + "Data",
                        invoice.DocumentDate.ToString(
                            "yyyy-MM-dd",
                            culture)),
                    new XElement(
                        P + "Numero",
                        invoice.DocumentNumber),
                    new XElement(
                        P + "ImportoTotaleDocumento",
                        invoice.TotalGross.ToString(
                            "0.00",
                            culture)))),
            beniServizi,
            new XElement(
                P + "DatiPagamento",
                new XElement(
                    P + "CondizioniPagamento",
                    "TP02"), // TP02 = Pagamento Completo
                new XElement(
                    P + "DettaglioPagamento",
                    new XElement(
                        P + "ModalitaPagamento",
                        invoice.PaymentMethodCode),
                    new XElement(
                        P + "DataScadenzaPagamento",
                        invoice.PaymentDueDate.ToString(
                            "yyyy-MM-dd",
                            culture)),
                    new XElement(
                        P + "ImportoPagamento",
                        invoice.TotalGross.ToString(
                            "0.00",
                            culture)))));
    }
}
