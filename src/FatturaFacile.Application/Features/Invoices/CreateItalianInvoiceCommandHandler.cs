using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

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