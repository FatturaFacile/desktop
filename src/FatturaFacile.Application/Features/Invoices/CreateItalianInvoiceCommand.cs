// <copyright file="CreateItalianInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Command to create and issue a new Italian electronic invoice (FatturaPA) conforming to Agenzia delle Entrate SDI parameters.
/// </summary>
/// <param name="DocumentNumber">The sequential invoice document number.</param>
/// <param name="DocumentDate">The official fiscal invoice date.</param>
/// <param name="ClientId">The unique identifier of the destination client customer.</param>
/// <param name="CedenteName">The legal denomination or full name of the issuing company (Cedente/Prestatore).</param>
/// <param name="CedenteVat">The Italian VAT number of the issuer.</param>
/// <param name="CedenteCf">The Italian Codice Fiscale of the issuer.</param>
/// <param name="CedenteAddress">The registered fiscal address value object of the issuer.</param>
/// <param name="CessionarioName">The legal denomination or full name of the customer (Cessionario/Committente).</param>
/// <param name="CessionarioVat">The customer VAT identification number, if registered.</param>
/// <param name="CessionarioCf">The customer Codice Fiscale, if applicable.</param>
/// <param name="SdiCode">The 7-character recipient routing code (Codice Destinatario) for SDI dispatch.</param>
/// <param name="Pec">The certified electronic mail address (PEC) for electronic delivery fallback.</param>
/// <param name="CessionarioAddress">The registered fiscal address value object of the customer.</param>
/// <param name="DueDate">The invoice payment maturity due date.</param>
/// <param name="Lines">The collection of invoice item lines detailing quantities, prices, and VAT rates.</param>
public record CreateItalianInvoiceCommand(
    string DocumentNumber,
    DateTime DocumentDate,
    Guid ClientId,
    string CedenteName,
    string CedenteVat,
    string CedenteCf,
    Address CedenteAddress,
    string CessionarioName,
    string? CessionarioVat,
    string? CessionarioCf,
    string SdiCode,
    string? Pec,
    Address CessionarioAddress,
    DateTime DueDate,
    IList<CreateInvoiceLineItem> Lines) : IRequest<ItalianInvoiceDto>;
