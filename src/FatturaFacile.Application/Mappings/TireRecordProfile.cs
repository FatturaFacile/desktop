using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Automotive;

namespace FatturaFacile.Application.Mappings;

public sealed class TireRecordProfile : Profile
{
    public TireRecordProfile()
    {
        _ = CreateMap<CustomerVehicle, CustomerVehicleDto>();

        _ = CreateMap<TireHotelRecord, TireHotelDto>()
            .ForCtorParam(nameof(TireHotelDto.VehiclePlate), opt => opt.MapFrom(src => $"VEH-{src.VehicleId.ToString().Substring(0, 6)}"));
    }
}
