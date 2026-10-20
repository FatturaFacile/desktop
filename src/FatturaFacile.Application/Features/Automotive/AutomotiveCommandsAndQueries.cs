using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

public record CreateTireRecordCommand(
    Guid VehicleId,
    TireSeason Season,
    string Brand,
    string Dimension,
    string DotCode,
    decimal TreadDepthFrontLeftMm,
    decimal TreadDepthFrontRightMm,
    decimal TreadDepthRearLeftMm,
    decimal TreadDepthRearRightMm,
    string WarehouseRackCoordinate) : IRequest<TireHotelDto>;

public sealed class CreateTireRecordCommandHandler(ITireRecordRepository repository, ITenantProvider tenantProvider, IMapper mapper) : IRequestHandler<CreateTireRecordCommand, TireHotelDto>
{
    private readonly ITireRecordRepository _repository = repository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly IMapper _mapper = mapper;

    public async Task<TireHotelDto> Handle(CreateTireRecordCommand request, CancellationToken cancellationToken)
    {
        var record = TireHotelRecord.CheckIn(
            _tenantProvider.CurrentTenantId,
            request.VehicleId,
            request.Season,
            request.Brand,
            request.Dimension,
            request.DotCode,
            request.TreadDepthFrontLeftMm,
            request.TreadDepthFrontRightMm,
            request.TreadDepthRearLeftMm,
            request.TreadDepthRearRightMm,
            request.WarehouseRackCoordinate);

        await _repository.AddAsync(record, cancellationToken);
        return _mapper.Map<TireHotelDto>(record);
    }
}

public record GetTireRecordsQuery() : IRequest<IReadOnlyList<TireHotelDto>>;

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
