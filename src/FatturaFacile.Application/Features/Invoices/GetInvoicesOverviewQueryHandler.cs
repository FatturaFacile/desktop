using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

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