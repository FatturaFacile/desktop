// <copyright file="ItalianInvoicingStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml.Linq;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies.Italian;

/// <summary>
/// Italian invoicing strategy producing FatturaPA XML compliance metadata for Sistema di Interscambio (SDI).
/// </summary>
public sealed class ItalianInvoicingStrategy : ICountryInvoicingStrategy
{
    /// <inheritdoc/>
    public string CountryCode => "IT";

    /// <inheritdoc/>
    public Task ProcessInvoiceComplianceAsync(
        Invoice invoice,
        Tenant tenant,
        Contact contact,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(contact);

        var sdiCode = !string.IsNullOrWhiteSpace(contact.SdiCode) ? contact.SdiCode : "0000000";

        _ = contact.Pec ?? string.Empty;
        var format = sdiCode.Length == 6 ? "FPA12" : "FPR12";

        var vatClean = (tenant.VatNumber ?? "00000000000").Trim();

        if (vatClean.StartsWith("IT", StringComparison.OrdinalIgnoreCase))
        {
            vatClean = vatClean[2..];
        }

        var fileName =
            $"IT{vatClean}_{invoice.SequentialNumber?.ToString("D5", CultureInfo.InvariantCulture) ?? "00001"}.xml";

        var xmlContent = GenerateFatturaPaXml(invoice, tenant, contact, sdiCode, format);

        if (invoice.SdiMetadata == null)
        {
            invoice.SdiMetadata = new InvoiceSdiMetadata
            {
                InvoiceId = invoice.Id,
                TransmissionFormat = format,
                SdiIdentifier = Guid.NewGuid().ToString("N")[..18].ToUpperInvariant(),
                SdiStatus = "validated",
                FileName = fileName,
                XmlContent = xmlContent,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
            };
        }
        else
        {
            invoice.SdiMetadata.TransmissionFormat = format;
            invoice.SdiMetadata.FileName = fileName;
            invoice.SdiMetadata.XmlContent = xmlContent;
            invoice.SdiMetadata.SdiStatus = "validated";
            invoice.SdiMetadata.UpdatedAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    private static string GenerateFatturaPaXml(
        Invoice invoice,
        Tenant tenant,
        Contact contact,
        string sdiCode,
        string format)
    {
        var vatClean = (tenant.VatNumber ?? "00000000000").Trim();

        if (vatClean.StartsWith("IT", StringComparison.OrdinalIgnoreCase))
        {
            vatClean = vatClean[2..];
        }

        var contactVatClean = (contact.VatNumber ?? "00000000000").Trim();

        if (contactVatClean.StartsWith("IT", StringComparison.OrdinalIgnoreCase))
        {
            contactVatClean = contactVatClean[2..];
        }

        XDocument document = new(
            new XDeclaration("1.0", "UTF-8", standalone: null),
            new XElement(
                "FatturaElettronica",
                new XAttribute("versione", format),
                new XElement(
                    "FatturaElettronicaHeader",
                    new XElement(
                        "DatiTrasmissione",
                        new XElement(
                            "IdTrasmittente",
                            new XElement("IdPaese", tenant.Country),
                            new XElement("IdCodice", vatClean)),
                        new XElement(
                            "ProgressivoInvio",
                            invoice.SequentialNumber?.ToString(CultureInfo.InvariantCulture) ?? "1"),
                        new XElement("FormatoTrasmissione", format),
                        new XElement("CodiceDestinatario", sdiCode),
                        new XElement("PECDestinatario", contact.Pec ?? string.Empty)),
                    new XElement(
                        "CedentePrestatore",
                        new XElement(
                            "DatiAnagrafici",
                            new XElement(
                                "IdFiscaleIVA",
                                new XElement("IdPaese", tenant.Country),
                                new XElement("IdCodice", vatClean)),
                            new XElement("CodiceFiscale", tenant.FiscalCode ?? vatClean),
                            new XElement("Anagrafica", new XElement("Denominazione", tenant.LegalName)),
                            new XElement("RegimeFiscale", "RF01"))),
                    new XElement(
                        "CessionarioCommittente",
                        new XElement(
                            "DatiAnagrafici",
                            new XElement(
                                "IdFiscaleIVA",
                                new XElement("IdPaese", contact.Country),
                                new XElement("IdCodice", contactVatClean)),
                            new XElement("CodiceFiscale", contact.FiscalCode ?? contactVatClean),
                            new XElement(
                                "Anagrafica",
                                new XElement("Denominazione", contact.DisplayName))))),
                new XElement(
                    "FatturaElettronicaBody",
                    new XElement(
                        "DatiGenerali",
                        new XElement(
                            "DatiGeneraliDocumento",
                            new XElement("TipoDocumento", "TD01"),
                            new XElement("Divisa", invoice.Currency),
                            new XElement(
                                "Data",
                                invoice.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                            new XElement("Numero", invoice.InvoiceNumber),
                            new XElement(
                                "ImportoTotaleDocumento",
                                invoice.TotalGross.ToString("F2", CultureInfo.InvariantCulture)))),
                    new XElement(
                        "DatiBeniServizi",
                        new XElement(
                            "DatiRiepilogo",
                            new XElement(
                                "AliquotaIVA",
                                (invoice.Items.FirstOrDefault()?.TaxRate ?? 22m).ToString(
                                    "F2",
                                    CultureInfo.InvariantCulture)),
                            new XElement(
                                "ImponibileImporto",
                                invoice.TotalNet.ToString("F2", CultureInfo.InvariantCulture)),
                            new XElement(
                                "Imposta",
                                invoice.TotalTax.ToString("F2", CultureInfo.InvariantCulture)),
                            new XElement("EsigibilitaIVA", invoice.SplitPayment ? "S" : "I"))))));

        return document.ToString(SaveOptions.DisableFormatting);
    }
}
