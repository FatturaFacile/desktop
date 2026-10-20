using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

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
    List<CreateInvoiceLineItem> Lines) : IRequest<ItalianInvoiceDto>;