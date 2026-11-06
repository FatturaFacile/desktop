// <copyright file="FatturaPaXmlBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies.Italian;

/// <summary>
/// Builds a FatturaPA XML document (schema v1.2, formats FPR12 / FPA12) with all mandatory blocks:
/// DatiTrasmissione, CedentePrestatore, CessionarioCommittente, DatiGeneraliDocumento, DettaglioLinee,
/// DatiRiepilogo and, when applicable, DatiRitenuta, DatiBollo and DatiPagamento.
/// </summary>
public static class FatturaPaXmlBuilder
{
    /// <summary>
    /// The FatturaPA v1.2 target namespace.
    /// </summary>
    public static readonly XNamespace FatturaPaNamespace = "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2";

    private static readonly XNamespace _xmlDsigNamespace = "http://www.w3.org/2000/09/xmldsig#";

    /// <summary>
    /// Builds the FatturaPA XML document.
    /// </summary>
    /// <param name="invoice">The invoice (lines in <see cref="Invoice.Items"/>).</param>
    /// <param name="tenant">The issuer.</param>
    /// <param name="contact">The customer.</param>
    /// <param name="transmissionProgressive">The ProgressivoInvio (max 10 alphanumeric characters).</param>
    /// <returns>The XML document.</returns>
    public static XDocument Build(Invoice invoice, Tenant tenant, Contact contact, string transmissionProgressive)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentException.ThrowIfNullOrWhiteSpace(transmissionProgressive);

        var format = contact.IsPublicAdministration ? "FPA12" : "FPR12";
        var summaries = FatturaPaRules.ComputeVatSummaries(invoice.Items);
        var totalTaxable = summaries.Sum(static s => s.TaxableAmount);
        var totalTax = summaries.Sum(static s => s.TaxAmount);
        var documentTotal = totalTaxable + totalTax;

        var withholding = ComputeWithholding(invoice);
        var stampDuty = invoice.StampDutyApplied && invoice.StampDutyAmount is > 0m
            ? invoice.StampDutyAmount.Value
            : FatturaPaRules.ComputeStampDuty(invoice.Items);

        var withholdingElement = withholding > 0m
            ? new XElement(
                "DatiRitenuta",
                new XElement("TipoRitenuta", "RT01"),
                new XElement("ImportoRitenuta", Amount(withholding)),
                new XElement("AliquotaRitenuta", Amount(invoice.WithholdingTaxPercent ?? 0m)),
                new XElement("CausalePagamento", "A"))
            : null;

        var stampDutyElement = stampDuty > 0m
            ? new XElement("DatiBollo", new XElement("BolloVirtuale", "SI"), new XElement("ImportoBollo", Amount(stampDuty)))
            : null;

        var causaleElement = string.IsNullOrWhiteSpace(invoice.Notes)
            ? null
            : new XElement("Causale", Truncate(invoice.Notes, 200));

        XElement root = new(
            FatturaPaNamespace + "FatturaElettronica",
            new XAttribute("versione", format),
            new XAttribute(XNamespace.Xmlns + "ds", _xmlDsigNamespace),
            new XAttribute(XNamespace.Xmlns + "p", FatturaPaNamespace),
            new XElement(
                "FatturaElettronicaHeader",
                BuildTransmission(tenant, contact, format, transmissionProgressive),
                BuildIssuer(tenant),
                BuildCustomer(contact)),
            new XElement(
                "FatturaElettronicaBody",
                new XElement(
                    "DatiGenerali",
                    new XElement(
                        "DatiGeneraliDocumento",
                        new XElement("TipoDocumento", FatturaPaRules.GetDocumentType(invoice.InvoiceType) ?? "TD01"),
                        new XElement("Divisa", invoice.Currency),
                        new XElement("Data", invoice.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                        new XElement("Numero", invoice.InvoiceNumber),
                        withholdingElement,
                        stampDutyElement,
                        new XElement("ImportoTotaleDocumento", Amount(documentTotal)),
                        causaleElement)),
                new XElement(
                    "DatiBeniServizi",
                    BuildLines(invoice),
                    summaries.Select(s => BuildSummary(s, invoice.SplitPayment))),
                BuildPayment(invoice, tenant, documentTotal - withholding - (invoice.SplitPayment ? totalTax : 0m))));

        return new XDocument(new XDeclaration("1.0", "UTF-8", standalone: null), root);
    }

    /// <summary>
    /// Builds the SDI file name: country code + IdCodice of the transmitter + "_" + 5-character progressive.
    /// </summary>
    /// <param name="tenant">The issuer acting as transmitter.</param>
    /// <param name="fileProgressive">The file progressive (5 alphanumeric characters).</param>
    /// <returns>The file name.</returns>
    public static string BuildFileName(Tenant tenant, string fileProgressive)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileProgressive);
        return $"IT{TransmitterCode(tenant)}_{fileProgressive}.xml";
    }

    /// <summary>
    /// Serializes the document as UTF-8 (without BOM) including the XML declaration.
    /// </summary>
    /// <param name="document">The FatturaPA document.</param>
    /// <returns>The XML text.</returns>
    public static string Serialize(XDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        XmlWriterSettings settings = new()
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
        };

        using MemoryStream stream = new();

        using (var writer = XmlWriter.Create(stream, settings))
        {
            document.Save(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static XElement BuildTransmission(Tenant tenant, Contact contact, string format, string progressive)
    {
        var recipientCode = FatturaPaRules.ResolveRecipientCode(contact);
        var usePec = string.Equals(recipientCode, "0000000", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(contact.Pec);

        return new XElement(
            "DatiTrasmissione",
            new XElement(
                "IdTrasmittente",
                new XElement("IdPaese", "IT"),
                new XElement("IdCodice", TransmitterCode(tenant))),
            new XElement("ProgressivoInvio", Truncate(progressive, 10)),
            new XElement("FormatoTrasmissione", format),
            new XElement("CodiceDestinatario", recipientCode),
            usePec ? new XElement("PECDestinatario", contact.Pec!.Trim()) : null);
    }

    private static XElement BuildIssuer(Tenant tenant)
    {
        var vat = FatturaPaRules.CleanVatNumber(tenant.VatNumber);

        return new XElement(
            "CedentePrestatore",
            new XElement(
                "DatiAnagrafici",
                new XElement(
                    "IdFiscaleIVA",
                    new XElement("IdPaese", "IT"),
                    new XElement("IdCodice", vat)),
                string.IsNullOrWhiteSpace(tenant.FiscalCode) ? null : new XElement("CodiceFiscale", tenant.FiscalCode.Trim().ToUpperInvariant()),
                new XElement(
                    "Anagrafica",
                    new XElement("Denominazione", Truncate(string.IsNullOrWhiteSpace(tenant.LegalName) ? tenant.Name : tenant.LegalName, 80))),
                new XElement("RegimeFiscale", tenant.TaxRegime)),
            BuildAddress(tenant.Street, tenant.HouseNumber, tenant.PostalCode, tenant.City, tenant.Province, "IT"));
    }

    private static XElement BuildCustomer(Contact contact)
    {
        var isItalian = string.Equals(contact.Country, "IT", StringComparison.OrdinalIgnoreCase);
        var country = contact.Country.ToUpperInvariant();
        var foreignVatSource = isItalian ? null : contact.UidNumber;
        var vatSource = !string.IsNullOrWhiteSpace(contact.VatNumber) ? contact.VatNumber : foreignVatSource;
        var vat = FatturaPaRules.CleanVatNumber(vatSource);

        XElement? vatElement = null;

        if (!string.IsNullOrEmpty(vat))
        {
            vatElement = new XElement("IdFiscaleIVA", new XElement("IdPaese", country), new XElement("IdCodice", vat));
        }
        else if (!isItalian)
        {
            // Foreign customer without VAT number: conventional identifier accepted by SDI.
            vatElement = new XElement("IdFiscaleIVA", new XElement("IdPaese", country), new XElement("IdCodice", "99999999999"));
        }

        var registry = string.IsNullOrWhiteSpace(contact.CompanyName)
            ? new XElement(
                "Anagrafica",
                new XElement("Nome", Truncate(contact.FirstName ?? string.Empty, 60)),
                new XElement("Cognome", Truncate(contact.LastName ?? string.Empty, 60)))
            : new XElement("Anagrafica", new XElement("Denominazione", Truncate(contact.CompanyName, 80)));

        var fiscalCodeElement = isItalian && !string.IsNullOrWhiteSpace(contact.FiscalCode)
            ? new XElement("CodiceFiscale", contact.FiscalCode.Trim().ToUpperInvariant())
            : null;

        return new XElement(
            "CessionarioCommittente",
            new XElement("DatiAnagrafici", vatElement, fiscalCodeElement, registry),
            BuildAddress(contact.Street, contact.HouseNumber, contact.PostalCode, contact.City, isItalian ? contact.Province : null, country));
    }

    private static XElement BuildAddress(string? street, string? houseNumber, string? postalCode, string? city, string? province, string country)
    {
        var isItalian = string.Equals(country, "IT", StringComparison.Ordinal);
        var cap = isItalian ? (postalCode ?? string.Empty).Trim() : "00000";

        return new XElement(
            "Sede",
            new XElement("Indirizzo", Truncate(street ?? string.Empty, 60)),
            string.IsNullOrWhiteSpace(houseNumber) ? null : new XElement("NumeroCivico", Truncate(houseNumber, 8)),
            new XElement("CAP", cap),
            new XElement("Comune", Truncate(city ?? string.Empty, 60)),
            isItalian && !string.IsNullOrWhiteSpace(province) ? new XElement("Provincia", province.Trim().ToUpperInvariant()) : null,
            new XElement("Nazione", country));
    }

    private static IEnumerable<XElement> BuildLines(Invoice invoice)
    {
        var lineNumber = 1;

        foreach (var item in invoice.Items)
        {
            var discountElement = item.DiscountPercentage > 0m
                ? new XElement("ScontoMaggiorazione", new XElement("Tipo", "SC"), new XElement("Percentuale", Amount(item.DiscountPercentage)))
                : null;

            yield return new XElement(
                "DettaglioLinee",
                new XElement("NumeroLinea", lineNumber++),
                new XElement("Descrizione", Truncate(item.Description, 1000)),
                new XElement("Quantita", Quantity(item.Quantity)),
                string.IsNullOrWhiteSpace(item.Uom) ? null : new XElement("UnitaMisura", Truncate(item.Uom, 10)),
                new XElement("PrezzoUnitario", Quantity(item.UnitPrice)),
                discountElement,
                new XElement("PrezzoTotale", Quantity(item.LineTotal)),
                new XElement("AliquotaIVA", Amount(item.TaxRate)),
                invoice.WithholdingTaxEnabled ? new XElement("Ritenuta", "SI") : null,
                item.TaxRate == 0m && !string.IsNullOrWhiteSpace(item.VatNature) ? new XElement("Natura", item.VatNature) : null);
        }
    }

    private static XElement BuildSummary(FatturaPaVatSummary summary, bool splitPayment)
    {
        var chargeability = splitPayment ? "S" : "I";

        return new XElement(
            "DatiRiepilogo",
            new XElement("AliquotaIVA", Amount(summary.Rate)),
            summary.Nature is null ? null : new XElement("Natura", summary.Nature),
            new XElement("ImponibileImporto", Amount(summary.TaxableAmount)),
            new XElement("Imposta", Amount(summary.TaxAmount)),
            summary.Nature is null ? new XElement("EsigibilitaIVA", chargeability) : null);
    }

    private static XElement? BuildPayment(Invoice invoice, Tenant tenant, decimal amountDue)
    {
        var iban = !string.IsNullOrWhiteSpace(invoice.BankIban) ? invoice.BankIban : tenant.Iban;
        var dueDateElement = invoice.DueDate is { } due
            ? new XElement("DataScadenzaPagamento", due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            : null;

        return string.IsNullOrWhiteSpace(iban)
            ? null
            : new XElement(
                "DatiPagamento",
                new XElement("CondizioniPagamento", "TP02"),
                new XElement(
                    "DettaglioPagamento",
                    new XElement("ModalitaPagamento", "MP05"),
                    dueDateElement,
                    new XElement("ImportoPagamento", Amount(amountDue)),
                    new XElement("IBAN", iban.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant())));
    }

    private static decimal ComputeWithholding(Invoice invoice)
    {
        if (!invoice.WithholdingTaxEnabled)
        {
            return 0m;
        }

        if (invoice.WithholdingTaxAmount is > 0m)
        {
            return invoice.WithholdingTaxAmount.Value;
        }

        var taxable = invoice.Items.Sum(static i => i.LineTotal);
        return Math.Round(taxable * (invoice.WithholdingTaxPercent ?? 0m) / 100m, 2, MidpointRounding.AwayFromZero);
    }

    private static string TransmitterCode(Tenant tenant)
    {
        return !string.IsNullOrWhiteSpace(tenant.FiscalCode)
                ? tenant.FiscalCode.Trim().ToUpperInvariant()
                : FatturaPaRules.CleanVatNumber(tenant.VatNumber);
    }

    private static string Amount(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Quantity(decimal value) => value.ToString("0.00######", CultureInfo.InvariantCulture);

    private static string Truncate(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];
}
