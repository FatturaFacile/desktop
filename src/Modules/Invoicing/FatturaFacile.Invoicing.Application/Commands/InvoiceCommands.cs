using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Enums;

namespace FatturaFacile.Invoicing.Application.Commands;

public sealed record CreateInvoiceCommand(
    Guid TenantId,
    Guid ContactId,
    InvoiceType InvoiceType,
    string InvoiceNumber,
    DateOnly Date,
    DateOnly? DueDate,
    string Currency,
    Jurisdiction Jurisdiction,
    List<CreateInvoiceItemInput> Items,
    bool SplitPayment = false,
    bool ReverseCharge = false,
    string? Notes = null,
    decimal ExchangeRate = 1.0m) : ICommand<InvoiceDto>;

public sealed record RegisterInvoicePaymentCommand(
    Guid TenantId,
    Guid InvoiceId,
    decimal Amount,
    DateTimeOffset PaidAt) : ICommand<InvoiceDto>;

public sealed record CancelInvoiceCommand(
    Guid TenantId,
    Guid InvoiceId,
    string Reason) : ICommand<InvoiceDto>;
