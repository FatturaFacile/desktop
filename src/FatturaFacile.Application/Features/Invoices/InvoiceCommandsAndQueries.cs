using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;
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

public record CreateInvoiceLineItem(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    string? Natura = null);

public sealed class CreateItalianInvoiceCommandHandler(IItalianInvoiceRepository repository, ITenantProvider tenantProvider, IMapper mapper) : IRequestHandler<CreateItalianInvoiceCommand, ItalianInvoiceDto>
{
    private readonly IItalianInvoiceRepository _repository = repository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly IMapper _mapper = mapper;

    public async Task<ItalianInvoiceDto> Handle(CreateItalianInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = ItalianElectronicInvoice.Create(
            _tenantProvider.CurrentTenantId,
            request.DocumentNumber,
            request.DocumentDate,
            request.ClientId,
            request.CedenteName,
            request.CedenteVat,
            request.CedenteCf,
            request.CedenteAddress,
            request.CessionarioName,
            request.CessionarioVat,
            request.CessionarioCf,
            request.SdiCode,
            request.Pec,
            request.CessionarioAddress,
            request.DueDate);

        foreach (var line in request.Lines)
        {
            invoice.AddLine(line.Description, line.Quantity, line.UnitPrice, line.VatRate, line.Natura);
        }

        await _repository.AddAsync(invoice, cancellationToken);
        return _mapper.Map<ItalianInvoiceDto>(invoice);
    }
}

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

public sealed class CreateSwissQrBillCommandHandler(ISwissQrBillRepository repository, ITenantProvider tenantProvider, IMapper mapper) : IRequestHandler<CreateSwissQrBillCommand, SwissQrBillDto>
{
    private readonly ISwissQrBillRepository _repository = repository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly IMapper _mapper = mapper;

    public async Task<SwissQrBillDto> Handle(CreateSwissQrBillCommand request, CancellationToken cancellationToken)
    {
        var qrIban = new QrIban(request.CreditorQrIban);
        var rawAmount = Money.Chf(request.AmountChf);

        var bill = SwissQrBill.Create(
            _tenantProvider.CurrentTenantId,
            request.BillNumber,
            request.BillDate,
            request.ClientId,
            request.CreditorName,
            request.CreditorAddress,
            qrIban,
            request.DebtorName,
            request.DebtorAddress,
            rawAmount,
            request.ReferenceNumber,
            request.DueDate,
            request.Message);

        await _repository.AddAsync(bill, cancellationToken);
        return _mapper.Map<SwissQrBillDto>(bill);
    }
}

public record GetInvoicesOverviewQuery() : IRequest<InvoicesOverviewDto>;

public record InvoicesOverviewDto(
    IReadOnlyList<ItalianInvoiceDto> ItalianInvoices,
    IReadOnlyList<SwissQrBillDto> SwissQrBills);

public sealed class GetInvoicesOverviewQueryHandler(IItalianInvoiceRepository itRepo, ISwissQrBillRepository chRepo, IMapper mapper) : IRequestHandler<GetInvoicesOverviewQuery, InvoicesOverviewDto>
{
    private readonly IItalianInvoiceRepository _itRepo = itRepo;
    private readonly ISwissQrBillRepository _chRepo = chRepo;
    private readonly IMapper _mapper = mapper;

    public async Task<InvoicesOverviewDto> Handle(GetInvoicesOverviewQuery request, CancellationToken cancellationToken)
    {
        var it = await _itRepo.GetAllAsync(cancellationToken);
        var ch = await _chRepo.GetAllAsync(cancellationToken);

        return new InvoicesOverviewDto(
            _mapper.Map<IReadOnlyList<ItalianInvoiceDto>>(it),
            _mapper.Map<IReadOnlyList<SwissQrBillDto>>(ch));
    }
}
