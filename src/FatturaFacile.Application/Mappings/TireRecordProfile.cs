// <copyright file="TireRecordProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Automotive;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring mappings for automotive vehicles and tire hotel records to data transfer objects.
/// </summary>
public sealed class TireRecordProfile : Profile
{

    /// <summary>
    /// Initializes a new instance of the <see cref="TireRecordProfile"/> class and configures mapping expressions.
    /// </summary>
    public TireRecordProfile()
    {
        _ = CreateMap<CustomerVehicle, CustomerVehicleDto>();

        _ = CreateMap<TireHotelRecord, TireHotelDto>()
           .ForCtorParam(
                         ctorParamName: nameof(TireHotelDto.VehiclePlate),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => $"VEH-{src.VehicleId.ToString().Substring(0, 6)}"));
    }

}
