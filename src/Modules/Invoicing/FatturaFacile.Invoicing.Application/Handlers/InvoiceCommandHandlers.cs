// <copyright file="InvoiceCommandHandlers.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Application.Shared.Results;
using FatturaFacile.Invoicing.Application.Commands;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Repositories;

namespace FatturaFacile.Invoicing.Application.Handlers;

/// <summary>
/// CQRS handler for executing invoice lifecycle commands: creation, payment registration, and cancellation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvoiceCommandHandlers"/> class.
/// </remarks>
/// <param name="repository">The invoice repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class InvoiceCommandHandlers(IInvoiceRepository repository, IMapper mapper)
    : ICommandHandler<CreateInvoiceCommand, InvoiceDto>,
      ICommandHandler<RegisterInvoicePaymentCommand, InvoiceDto>,
      ICommandHandler<CancelInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _repository = repository;

    private readonly IMapper _mapper = mapper;

    /// <inheritdoc />
    public Task<Result<InvoiceDto>> Handle(
        CreateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleCreateInvoiceInternalAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<InvoiceDto>> Handle(
        RegisterInvoicePaymentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleRegisterPaymentInternalAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<InvoiceDto>> Handle(
        CancelInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleCancelInvoiceInternalAsync(request, cancellationToken);
    }

    private async Task<Result<InvoiceDto>> HandleCreateInvoiceInternalAsync(
        CreateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = Invoice.Create(
            new InvoiceCreateRequest(
                request.TenantId,
                request.ContactId,
                request.InvoiceType,
                request.InvoiceNumber,
                request.Date,
                request.DueDate,
                request.Currency,
                request.Jurisdiction,
                request.SplitPayment,
                request.ReverseCharge,
                request.Notes,
                request.ExchangeRate));

        foreach (var item in request.Items)
        {
            invoice.AddItem(
                item.ProductId,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.TaxRate,
                item.DiscountPercentage);
        }

        await _repository.AddAsync(invoice, cancellationToken);
        var dto = _mapper.Map<InvoiceDto>(invoice);
        return Result.Success(dto);
    }

    private async Task<Result<InvoiceDto>> HandleRegisterPaymentInternalAsync(
        RegisterInvoicePaymentCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(request.InvoiceId, cancellationToken);

        if (invoice == null || invoice.TenantId != request.TenantId)
        {
            return Result.Failure<InvoiceDto>("Invoice not found.", "INVOICE_NOT_FOUND");
        }

        invoice.RegisterPayment(request.Amount, request.PaidAt);
        await _repository.UpdateAsync(invoice, cancellationToken);

        var dto = _mapper.Map<InvoiceDto>(invoice);
        return Result.Success(dto);
    }

    private async Task<Result<InvoiceDto>> HandleCancelInvoiceInternalAsync(
        CancelInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(request.InvoiceId, cancellationToken);

        if (invoice == null || invoice.TenantId != request.TenantId)
        {
            return Result.Failure<InvoiceDto>("Invoice not found.", "INVOICE_NOT_FOUND");
        }

        try
        {
            invoice.Cancel(request.Reason);
            await _repository.UpdateAsync(invoice, cancellationToken);
            var dto = _mapper.Map<InvoiceDto>(invoice);
            return Result.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<InvoiceDto>(ex.Message, "CANNOT_CANCEL_INVOICE");
        }
    }
}
