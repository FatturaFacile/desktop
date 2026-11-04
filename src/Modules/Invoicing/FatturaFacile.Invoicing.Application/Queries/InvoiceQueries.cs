using AutoMapper;
using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Application.Shared.Results;
using FatturaFacile.Invoicing.Application.DTOs;
using FatturaFacile.Invoicing.Domain.Repositories;

namespace FatturaFacile.Invoicing.Application.Queries;

public sealed record GetInvoiceByIdQuery(Guid TenantId, Guid InvoiceId) : IQuery<InvoiceDto>;

public sealed class GetInvoiceByIdQueryHandler : IQueryHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IInvoiceRepository _repository;
    private readonly IMapper _mapper;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice == null || invoice.TenantId != request.TenantId)
        {
            return Result.Failure<InvoiceDto>("Invoice not found.", "INVOICE_NOT_FOUND");
        }

        return Result.Success(_mapper.Map<InvoiceDto>(invoice));
    }
}
