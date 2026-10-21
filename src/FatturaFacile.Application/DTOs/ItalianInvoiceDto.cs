// <copyright file="ItalianInvoiceDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents italian invoice dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="DocumentNumber">The document number.</param>
/// <param name="DocumentDate">The document date.</param>
/// <param name="FormatoTrasmissione">The formato trasmissione.</param>
/// <param name="ProgressivoInvio">The progressivo invio.</param>
/// <param name="ClientId">The client id.</param>
/// <param name="CedenteDenominazione">The cedente denominazione.</param>
/// <param name="CedentePartitaIva">The cedente partita iva.</param>
/// <param name="CessionarioDenominazione">The cessionario denominazione.</param>
/// <param name="CessionarioPartitaIva">The cessionario partita iva.</param>
/// <param name="CessionarioCodiceDestinatario">The cessionario codice destinatario.</param>
/// <param name="TotalNet">The total net.</param>
/// <param name="TotalVat">The total vat.</param>
/// <param name="TotalGross">The total gross.</param>
/// <param name="Currency">The currency.</param>
/// <param name="PaymentDueDate">The payment due date.</param>
/// <param name="SdiStatus">The sdi status.</param>
/// <param name="SdiIdentifier">The sdi identifier.</param>
/// <param name="Lines">The lines.</param>
public record ItalianInvoiceDto(
    Guid Id,
    Guid TenantId,
    string DocumentNumber,
    DateTime DocumentDate,
    string FormatoTrasmissione,
    string ProgressivoInvio,
    Guid ClientId,
    string CedenteDenominazione,
    string CedentePartitaIva,
    string CessionarioDenominazione,
    string? CessionarioPartitaIva,
    string CessionarioCodiceDestinatario,
    decimal TotalNet,
    decimal TotalVat,
    decimal TotalGross,
    string Currency,
    DateTime PaymentDueDate,
    SdiTransmissionStatus SdiStatus,
    string? SdiIdentifier,
    IList<InvoiceLineDto> Lines);
