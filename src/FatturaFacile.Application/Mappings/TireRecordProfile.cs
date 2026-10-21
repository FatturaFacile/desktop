// <copyright file="TireRecordProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Automotive;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// Represents tire record profile.
/// </summary>
public sealed class TireRecordProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TireRecordProfile"/> class.
    /// </summary>
    public TireRecordProfile()
    {
        _ = CreateMap<CustomerVehicle, CustomerVehicleDto>();

        _ = CreateMap<TireHotelRecord, TireHotelDto>()
            .ForCtorParam(
                nameof(TireHotelDto.VehiclePlate),
                opt => opt.MapFrom(src => $"VEH-{src.VehicleId.ToString().Substring(0, 6)}"));
    }
}
