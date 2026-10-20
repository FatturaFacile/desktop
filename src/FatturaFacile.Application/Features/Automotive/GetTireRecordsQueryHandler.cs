using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

public sealed class GetTireRecordsQueryHandler(ITireRecordRepository repository, IMapper mapper) : IRequestHandler<GetTireRecordsQuery, IReadOnlyList<TireHotelDto>>
{
    private readonly ITireRecordRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<IReadOnlyList<TireHotelDto>> Handle(GetTireRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await _repository.GetAllInStorageAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TireHotelDto>>(records);
    }
}