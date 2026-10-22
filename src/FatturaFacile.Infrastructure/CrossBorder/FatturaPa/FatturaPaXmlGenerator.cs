// <copyright file="FatturaPaXmlGenerator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml.Linq;

using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Infrastructure.CrossBorder.FatturaPa;

/// <summary>
/// Generates compliant FatturaPA XML documents according to Italian Agenzia delle Entrate specifications (v1.2).
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
                                                    category: "Security",
                                                    checkId: "S5332:Using http protocol is insecure",
                                                    Justification =
                                                        "XML Schema URI specifications per Agenzia delle Entrate and W3C standards")]
public static class FatturaPaXmlGenerator
{

    /// <summary>
    /// The XML namespace for FatturaPA v1.2 schema.
    /// </summary>
    private static readonly XNamespace P = "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2";

    /// <summary>
    /// The XML namespace for digital signature schema.
    /// </summary>
    private static readonly XNamespace Ds = "http://www.w3.org/2000/09/xmldsig#";

    /// <summary>
    /// The XML namespace for XML Schema instance.
    /// </summary>
    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

    /// <summary>
    /// Serializes an <see cref="ItalianElectronicInvoice"/> domain entity into a standard FatturaPA v1.2 XML document string.
    /// </summary>
    /// <param name="invoice">The Italian electronic invoice domain entity to serialize.</param>
    /// <returns>A string containing the serialized UTF-8 XML document formatted for SDI transmission.</returns>
    public static string GenerateXml(ItalianElectronicInvoice invoice)
    {
        var root = new XElement(
                                name: P + "FatturaElettronica",
                                content:
                                [
                                    new XAttribute(
                                                   name: "versione",
                                                   value: invoice.FormatoTrasmissione),
                                    new XAttribute(
                                                   name: XNamespace.Xmlns + "p",
                                                   value: P.NamespaceName),
                                    new XAttribute(
                                                   name: XNamespace.Xmlns + "ds",
                                                   value: Ds.NamespaceName),
                                    new XAttribute(
                                                   name: XNamespace.Xmlns + "xsi",
                                                   value: Xsi.NamespaceName),
                                    BuildHeader(invoice: invoice),
                                    BuildBody(invoice: invoice)
                                ]);

        var doc = new XDocument(
                                declaration: new XDeclaration(
                                                              version: "1.0",
                                                              encoding: "UTF-8",
                                                              standalone: null),
                                content: root);

        return doc.ToString();
    }

    /// <summary>
    /// Builds the FatturaElettronicaHeader XML element.
    /// </summary>
    /// <param name="invoice">The invoice entity containing transmission and participant data.</param>
    /// <returns>The constructed header XML element.</returns>
    private static XElement BuildHeader(ItalianElectronicInvoice invoice)
    {
        return new XElement(
                            name: P + "FatturaElettronicaHeader",
                            content:
                            [
                                new XElement(
                                             name: P + "DatiTrasmissione",
                                             content:
                                             [
                                                 new XElement(
                                                              name: P + "IdTrasmittente",
                                                              content:
                                                              [
                                                                  new XElement(
                                                                               name: P + "IdPaese",
                                                                               content: "IT"),
                                                                  new XElement(
                                                                               name: P + "IdCodice",
                                                                               content: invoice.CedentePartitaIva)
                                                              ]),
                                                 new XElement(
                                                              name: P + "ProgressivoInvio",
                                                              content: invoice.ProgressivoInvio),
                                                 new XElement(
                                                              name: P + "FormatoTrasmissione",
                                                              content: invoice.FormatoTrasmissione),
                                                 new XElement(
                                                              name: P + "CodiceDestinatario",
                                                              content: invoice.CessionarioCodiceDestinatario),
                                                 string.IsNullOrWhiteSpace(value: invoice.CessionarioPec)
                                                     ? null
                                                     : new XElement(
                                                                    name: P + "PECDestinatario",
                                                                    content: invoice.CessionarioPec)
                                             ]),
                                new XElement(
                                             name: P + "CedentePrestatore",
                                             content:
                                             [
                                                 new XElement(
                                                              name: P + "DatiAnagrafici",
                                                              content:
                                                              [
                                                                  new XElement(
                                                                               name: P + "IdFiscaleIVA",
                                                                               content:
                                                                               [
                                                                                   new XElement(
                                                                                                name: P + "IdPaese",
                                                                                                content: "IT"),
                                                                                   new XElement(
                                                                                                name: P + "IdCodice",
                                                                                                content: invoice.CedentePartitaIva)
                                                                               ]),
                                                                  string.IsNullOrWhiteSpace(value: invoice.CedenteCodiceFiscale)
                                                                      ? null
                                                                      : new XElement(
                                                                                     name: P + "CodiceFiscale",
                                                                                     content: invoice.CedenteCodiceFiscale),
                                                                  new XElement(
                                                                               name: P + "Anagrafica",
                                                                               content: new XElement(
                                                                                                     name: P + "Denominazione",
                                                                                                     content: invoice.CedenteDenominazione)),
                                                                  new XElement(
                                                                               name: P + "RegimeFiscale",
                                                                               content: "RF01")
                                                              ]),
                                                 new XElement(
                                                              name: P + "Sede",
                                                              content:
                                                              [
                                                                  new XElement(
                                                                               name: P + "Indirizzo",
                                                                               content: invoice.CedenteIndirizzo.Street),
                                                                  new XElement(
                                                                               name: P + "CAP",
                                                                               content: invoice.CedenteIndirizzo.PostalCode),
                                                                  new XElement(
                                                                               name: P + "Comune",
                                                                               content: invoice.CedenteIndirizzo.City),
                                                                  new XElement(
                                                                               name: P + "Provincia",
                                                                               content: invoice.CedenteIndirizzo.ProvinceOrCanton),
                                                                  new XElement(
                                                                               name: P + "Nazione",
                                                                               content: invoice.CedenteIndirizzo.CountryCode)
                                                              ])
                                             ]),
                                new XElement(
                                             name: P + "CessionarioCommittente",
                                             content:
                                             [
                                                 new XElement(
                                                              name: P + "DatiAnagrafici",
                                                              content:
                                                              [
                                                                  !string.IsNullOrWhiteSpace(value: invoice.CessionarioPartitaIva)
                                                                      ? new XElement(
                                                                                     name: P + "IdFiscaleIVA",
                                                                                     content:
                                                                                     [
                                                                                         new XElement(
                                                                                                      name: P + "IdPaese",
                                                                                                      content: invoice.CessionarioIndirizzo
                                                                                                                      .CountryCode),
                                                                                         new XElement(
                                                                                                      name: P + "IdCodice",
                                                                                                      content: invoice.CessionarioPartitaIva)
                                                                                     ])
                                                                      : null,
                                                                  !string.IsNullOrWhiteSpace(value: invoice.CessionarioCodiceFiscale)
                                                                      ? new XElement(
                                                                                     name: P + "CodiceFiscale",
                                                                                     content: invoice.CessionarioCodiceFiscale)
                                                                      : null,
                                                                  new XElement(
                                                                               name: P + "Anagrafica",
                                                                               content: new XElement(
                                                                                                     name: P + "Denominazione",
                                                                                                     content: invoice.CessionarioDenominazione))
                                                              ]),
                                                 new XElement(
                                                              name: P + "Sede",
                                                              content:
                                                              [
                                                                  new XElement(
                                                                               name: P + "Indirizzo",
                                                                               content: invoice.CessionarioIndirizzo.Street),
                                                                  new XElement(
                                                                               name: P + "CAP",
                                                                               content: invoice.CessionarioIndirizzo.PostalCode),
                                                                  new XElement(
                                                                               name: P + "Comune",
                                                                               content: invoice.CessionarioIndirizzo.City),
                                                                  new XElement(
                                                                               name: P + "Provincia",
                                                                               content: invoice.CessionarioIndirizzo.ProvinceOrCanton),
                                                                  new XElement(
                                                                               name: P + "Nazione",
                                                                               content: invoice.CessionarioIndirizzo.CountryCode)
                                                              ])
                                             ])
                            ]);
    }

    /// <summary>
    /// Builds the FatturaElettronicaBody XML element.
    /// </summary>
    /// <param name="invoice">The invoice entity containing document data and lines.</param>
    /// <returns>The constructed body XML element.</returns>
    private static XElement BuildBody(ItalianElectronicInvoice invoice)
    {
        var culture = CultureInfo.InvariantCulture;

        var beniServizi = new XElement(name: P + "DatiBeniServizi");

        foreach (var line in invoice.Lines)
        {
            beniServizi.Add(
                            content: new XElement(
                                                  name: P + "DettaglioLinee",
                                                  content:
                                                  [
                                                      new XElement(
                                                                   name: P + "NumeroLinea",
                                                                   content: line.LineNumber),
                                                      new XElement(
                                                                   name: P + "Descrizione",
                                                                   content: line.Description),
                                                      new XElement(
                                                                   name: P + "Quantita",
                                                                   content: line.Quantity.ToString(
                                                                                                   format: "0.00",
                                                                                                   provider: culture)),
                                                      new XElement(
                                                                   name: P + "PrezzoUnitario",
                                                                   content: line.UnitPrice.ToString(
                                                                                                    format: "0.00",
                                                                                                    provider: culture)),
                                                      new XElement(
                                                                   name: P + "PrezzoTotale",
                                                                   content: line.LineTotal.ToString(
                                                                                                    format: "0.00",
                                                                                                    provider: culture)),
                                                      new XElement(
                                                                   name: P + "AliquotaIVA",
                                                                   content: line.VatRate.ToString(
                                                                                                  format: "0.00",
                                                                                                  provider: culture)),
                                                      string.IsNullOrWhiteSpace(value: line.NaturaExemptionCode)
                                                          ? null
                                                          : new XElement(
                                                                         name: P + "Natura",
                                                                         content: line.NaturaExemptionCode)
                                                  ]));
        }

        // Summary row
        beniServizi.Add(
                        content: new XElement(
                                              name: P + "DatiRiepilogo",
                                              content:
                                              [
                                                  new XElement(
                                                               name: P + "AliquotaIVA",
                                                               content: "22.00"),
                                                  new XElement(
                                                               name: P + "ImponibileImporto",
                                                               content: invoice.TotalNet.ToString(
                                                                                                  format: "0.00",
                                                                                                  provider: culture)),
                                                  new XElement(
                                                               name: P + "Imposta",
                                                               content: invoice.TotalVat.ToString(
                                                                                                  format: "0.00",
                                                                                                  provider: culture)),
                                                  new XElement(
                                                               name: P + "EsigibilitaIVA",
                                                               content: "I")
                                              ]));

        return new XElement(
                            name: P + "FatturaElettronicaBody",
                            content:
                            [
                                new XElement(
                                             name: P + "DatiGenerali",
                                             content: new XElement(
                                                                   name: P + "DatiGeneraliDocumento",
                                                                   content:
                                                                   [
                                                                       new XElement(
                                                                                    name: P + "TipoDocumento",
                                                                                    content: "TD01"),
                                                                       new XElement(
                                                                                    name: P + "Divisa",
                                                                                    content: invoice.Currency.ToString()),
                                                                       new XElement(
                                                                                    name: P + "Data",
                                                                                    content: invoice.DocumentDate.ToString(
                                                                                                                           format: "yyyy-MM-dd",
                                                                                                                           provider: culture)),
                                                                       new XElement(
                                                                                    name: P + "Numero",
                                                                                    content: invoice.DocumentNumber),
                                                                       new XElement(
                                                                                    name: P + "ImportoTotaleDocumento",
                                                                                    content: invoice.TotalGross.ToString(
                                                                                                                         format: "0.00",
                                                                                                                         provider: culture))
                                                                   ])),
                                beniServizi,
                                new XElement(
                                             name: P + "DatiPagamento",
                                             content:
                                             [
                                                 new XElement(
                                                              name: P + "CondizioniPagamento",
                                                              content: "TP02"),

                                                 // TP02 = Pagamento Completo
                                                 new XElement(
                                                              name: P + "DettaglioPagamento",
                                                              content:
                                                              [
                                                                  new XElement(
                                                                               name: P + "ModalitaPagamento",
                                                                               content: invoice.PaymentMethodCode),
                                                                  new XElement(
                                                                               name: P + "DataScadenzaPagamento",
                                                                               content: invoice.PaymentDueDate.ToString(
                                                                                                                        format: "yyyy-MM-dd",
                                                                                                                        provider: culture)),
                                                                  new XElement(
                                                                               name: P + "ImportoPagamento",
                                                                               content: invoice.TotalGross.ToString(
                                                                                                                    format: "0.00",
                                                                                                                    provider: culture))
                                                              ])
                                             ])
                            ]);
    }

}
