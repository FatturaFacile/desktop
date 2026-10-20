using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

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