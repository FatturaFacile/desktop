using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Represents get invoices overview query handler.
/// </summary>
/// <param name="itRepo">The it repo.</param>
/// <param name="chRepo">The ch repo.</param>
/// <param name="mapper">The mapper.</param>
public sealed class GetInvoicesOverviewQueryHandler(IItalianInvoiceRepository itRepo, ISwissQrBillRepository chRepo, IMapper mapper) : IRequestHandler<GetInvoicesOverviewQuery, InvoicesOverviewDto>
{
    private readonly IItalianInvoiceRepository _itRepo = itRepo;

    private readonly ISwissQrBillRepository _chRepo = chRepo;

    private readonly IMapper _mapper = mapper;

/// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<InvoicesOverviewDto> Handle(GetInvoicesOverviewQuery request, CancellationToken cancellationToken)
    {
        var it = await _itRepo.GetAllAsync(cancellationToken);
        var ch = await _chRepo.GetAllAsync(cancellationToken);

        return new InvoicesOverviewDto(
            _mapper.Map<IReadOnlyList<ItalianInvoiceDto>>(it),
            _mapper.Map<IReadOnlyList<SwissQrBillDto>>(ch));
    }
}
