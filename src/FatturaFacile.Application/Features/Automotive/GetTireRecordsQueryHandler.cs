using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

/// <summary>
/// Represents get tire records query handler.
/// </summary>
/// <param name="repository">The repository.</param>
/// <param name="mapper">The mapper.</param>
public sealed class GetTireRecordsQueryHandler(ITireRecordRepository repository, IMapper mapper) : IRequestHandler<GetTireRecordsQuery, IReadOnlyList<TireHotelDto>>
{
    private readonly ITireRecordRepository _repository = repository;

    private readonly IMapper _mapper = mapper;

/// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<TireHotelDto>> Handle(GetTireRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await _repository.GetAllInStorageAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TireHotelDto>>(records);
    }
}
