// <copyright file="ItalianInvoiceDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an Italian electronic invoice (FatturaPA) compliant with Agenzia delle Entrate SDI specifications.
/// </summary>
/// <param name="Id">The unique electronic invoice identifier.</param>
/// <param name="TenantId">The tenant organization boundary owning this electronic invoice.</param>
/// <param name="DocumentNumber">The sequential commercial document number.</param>
/// <param name="DocumentDate">The official tax issuance date of the invoice.</param>
/// <param name="FormatoTrasmissione">The SDI transmission format version identifier (e.g., "FPR12" for B2B/B2C or "FPA12" for Public Administration).</param>
/// <param name="ProgressivoInvio">The alphanumeric unique dispatch progression code for transmission identification.</param>
/// <param name="ClientId">The unique identifier of the recipient client.</param>
/// <param name="CedenteDenominazione">The legal company denomination or full name of the billing party (Cedente/Prestatore).</param>
/// <param name="CedentePartitaIva">The Italian VAT number of the billing party.</param>
/// <param name="CessionarioDenominazione">The legal company denomination or full name of the customer party (Cessionario/Committente).</param>
/// <param name="CessionarioPartitaIva">The VAT identification number of the customer party, if registered.</param>
/// <param name="CessionarioCodiceDestinatario">The SDI 7-character recipient routing code or '0000000' for PEC/consumer routing.</param>
/// <param name="TotalNet">The total taxable base amount excluding VAT.</param>
/// <param name="TotalVat">The aggregate value-added tax calculated across all line items.</param>
/// <param name="TotalGross">The total gross payable document amount inclusive of tax.</param>
/// <param name="Currency">The three-letter ISO currency code (typically "EUR").</param>
/// <param name="PaymentDueDate">The maturity payment due date for financial settlement.</param>
/// <param name="SdiStatus">The current lifecycle state within the SDI exchange system.</param>
/// <param name="SdiIdentifier">The official transmission identifier assigned upon reception by SDI, if available.</param>
/// <param name="Lines">The collection of detailed item and service billing lines comprising the invoice.</param>
public record ItalianInvoiceDto(
    Guid                  Id,
    Guid                  TenantId,
    string                DocumentNumber,
    DateTime              DocumentDate,
    string                FormatoTrasmissione,
    string                ProgressivoInvio,
    Guid                  ClientId,
    string                CedenteDenominazione,
    string                CedentePartitaIva,
    string                CessionarioDenominazione,
    string?               CessionarioPartitaIva,
    string                CessionarioCodiceDestinatario,
    decimal               TotalNet,
    decimal               TotalVat,
    decimal               TotalGross,
    string                Currency,
    DateTime              PaymentDueDate,
    SdiTransmissionStatus SdiStatus,
    string?               SdiIdentifier,
    IList<InvoiceLineDto> Lines);
