// <copyright file="CreateItalianInvoiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Represents create italian invoice command.
/// </summary>
/// <param name="DocumentNumber">The document number.</param>
/// <param name="DocumentDate">The document date.</param>
/// <param name="ClientId">The client id.</param>
/// <param name="CedenteName">The cedente name.</param>
/// <param name="CedenteVat">The cedente vat.</param>
/// <param name="CedenteCf">The cedente cf.</param>
/// <param name="CedenteAddress">The cedente address.</param>
/// <param name="CessionarioName">The cessionario name.</param>
/// <param name="CessionarioVat">The cessionario vat.</param>
/// <param name="CessionarioCf">The cessionario cf.</param>
/// <param name="SdiCode">The sdi code.</param>
/// <param name="Pec">The pec.</param>
/// <param name="CessionarioAddress">The cessionario address.</param>
/// <param name="DueDate">The due date.</param>
/// <param name="Lines">The lines.</param>
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
