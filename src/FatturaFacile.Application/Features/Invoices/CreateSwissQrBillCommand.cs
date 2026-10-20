using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

public record CreateSwissQrBillCommand(
    string BillNumber,
    DateTime BillDate,
    Guid ClientId,
    string CreditorName,
    Address CreditorAddress,
    string CreditorQrIban,
    string DebtorName,
    Address DebtorAddress,
    decimal AmountChf,
    string ReferenceNumber,
    DateTime DueDate,
    string? Message = null) : IRequest<SwissQrBillDto>;