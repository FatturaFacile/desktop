// <copyright file="ItalianInvoicingStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
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
    public Task ProcessInvoiceComplianceAsync(Invoice invoice, Tenant tenant, Contact contact, CancellationToken cancellationToken = default)
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

        var fileName = $"IT{vatClean}_{invoice.SequentialNumber?.ToString("D5", CultureInfo.InvariantCulture) ?? "00001"}.xml";

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

    private static string GenerateFatturaPaXml(Invoice invoice, Tenant tenant, Contact contact, string sdiCode, string format)
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

        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <p:FatturaElettronica versione="{format}" xmlns:p="http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2">
              <FatturaElettronicaHeader>
                <DatiTrasmissione>
                  <IdTrasmittente>
                    <IdPaese>{tenant.Country}</IdPaese>
                    <IdCodice>{vatClean}</IdCodice>
                  </IdTrasmittente>
                  <ProgressivoInvio>{invoice.SequentialNumber?.ToString(CultureInfo.InvariantCulture) ?? "1"}</ProgressivoInvio>
                  <FormatoTrasmissione>{format}</FormatoTrasmissione>
                  <CodiceDestinatario>{sdiCode}</CodiceDestinatario>
                  <PECDestinatario>{contact.Pec ?? string.Empty}</PECDestinatario>
                </DatiTrasmissione>
                <CedentePrestatore>
                  <DatiAnagrafici>
                    <IdFiscaleIVA>
                      <IdPaese>{tenant.Country}</IdPaese>
                      <IdCodice>{vatClean}</IdCodice>
                    </IdFiscaleIVA>
                    <CodiceFiscale>{tenant.FiscalCode ?? vatClean}</CodiceFiscale>
                    <Anagrafica>
                      <Denominazione>{tenant.LegalName}</Denominazione>
                    </Anagrafica>
                    <RegimeFiscale>RF01</RegimeFiscale>
                  </DatiAnagrafici>
                </CedentePrestatore>
                <CessionarioCommittente>
                  <DatiAnagrafici>
                    <IdFiscaleIVA>
                      <IdPaese>{contact.Country}</IdPaese>
                      <IdCodice>{contactVatClean}</IdCodice>
                    </IdFiscaleIVA>
                    <CodiceFiscale>{contact.FiscalCode ?? contactVatClean}</CodiceFiscale>
                    <Anagrafica>
                      <Denominazione>{contact.DisplayName}</Denominazione>
                    </Anagrafica>
                  </DatiAnagrafici>
                </CessionarioCommittente>
              </FatturaElettronicaHeader>
              <FatturaElettronicaBody>
                <DatiGenerali>
                  <DatiGeneraliDocumento>
                    <TipoDocumento>TD01</TipoDocumento>
                    <Divisa>{invoice.Currency}</Divisa>
                    <Data>{invoice.Date:yyyy-MM-dd}</Data>
                    <Numero>{invoice.InvoiceNumber}</Numero>
                    <ImportoTotaleDocumento>{invoice.TotalGross.ToString("F2", CultureInfo.InvariantCulture)}</ImportoTotaleDocumento>
                  </DatiGeneraliDocumento>
                </DatiGenerali>
                <DatiBeniServizi>
                  <DatiRiepilogo>
                    <AliquotaIVA>{(invoice.Items.FirstOrDefault()?.TaxRate ?? 22m).ToString("F2", CultureInfo.InvariantCulture)}</AliquotaIVA>
                    <ImponibileImporto>{invoice.TotalNet.ToString("F2", CultureInfo.InvariantCulture)}</ImponibileImporto>
                    <Imposta>{invoice.TotalTax.ToString("F2", CultureInfo.InvariantCulture)}</Imposta>
                    <EsigibilitaIVA>{(invoice.SplitPayment ? "S" : "I")}</EsigibilitaIVA>
                  </DatiRiepilogo>
                </DatiBeniServizi>
              </FatturaElettronicaBody>
            </p:FatturaElettronica>
            """;
    }
}
