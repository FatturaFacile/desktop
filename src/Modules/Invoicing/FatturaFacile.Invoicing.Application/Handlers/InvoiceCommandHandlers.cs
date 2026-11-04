using AutoMapper;
using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Application.Shared.Results;
using FatturaFacile.Invoicing.Application.Commands;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Repositories;

namespace FatturaFacile.Invoicing.Application.Handlers;

public sealed class InvoiceCommandHandlers :
    ICommandHandler<CreateInvoiceCommand, InvoiceDto>,
    ICommandHandler<RegisterInvoicePaymentCommand, InvoiceDto>,
    ICommandHandler<CancelInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _repository;
    private readonly IMapper _mapper;

    public InvoiceCommandHandlers(IInvoiceRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoice = Invoice.Create(
            command.TenantId,
            command.ContactId,
            command.InvoiceType,
            command.InvoiceNumber,
            command.Date,
            command.DueDate,
            command.Currency,
            command.Jurisdiction,
            command.SplitPayment,
            command.ReverseCharge,
            command.Notes,
            command.ExchangeRate);

        foreach (var item in command.Items)
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

    public async Task<Result<InvoiceDto>> Handle(RegisterInvoicePaymentCommand command, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice == null || invoice.TenantId != command.TenantId)
        {
            return Result.Failure<InvoiceDto>("Invoice not found.", "INVOICE_NOT_FOUND");
        }

        invoice.RegisterPayment(command.Amount, command.PaidAt);
        await _repository.UpdateAsync(invoice, cancellationToken);

        var dto = _mapper.Map<InvoiceDto>(invoice);
        return Result.Success(dto);
    }

    public async Task<Result<InvoiceDto>> Handle(CancelInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice == null || invoice.TenantId != command.TenantId)
        {
            return Result.Failure<InvoiceDto>("Invoice not found.", "INVOICE_NOT_FOUND");
        }

        try
        {
            invoice.Cancel(command.Reason);
            await _repository.UpdateAsync(invoice, cancellationToken);
            return Result.Success(_mapper.Map<InvoiceDto>(invoice));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<InvoiceDto>(ex.Message, "CANNOT_CANCEL");
        }
    }
}
