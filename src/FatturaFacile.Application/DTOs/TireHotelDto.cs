// <copyright file="TireHotelDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents tire hotel dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="VehicleId">The vehicle id.</param>
/// <param name="VehiclePlate">The vehicle plate.</param>
/// <param name="Season">The season.</param>
/// <param name="Brand">The brand.</param>
/// <param name="Dimension">The dimension.</param>
/// <param name="DotCode">The dot code.</param>
/// <param name="TreadDepthFrontLeftMm">The tread depth front left mm.</param>
/// <param name="TreadDepthFrontRightMm">The tread depth front right mm.</param>
/// <param name="TreadDepthRearLeftMm">The tread depth rear left mm.</param>
/// <param name="TreadDepthRearRightMm">The tread depth rear right mm.</param>
/// <param name="WarehouseRackCoordinate">The warehouse rack coordinate.</param>
/// <param name="Status">The status.</param>
/// <param name="StoredDateUtc">The stored date utc.</param>
public record TireHotelDto(
    Guid Id,
    Guid TenantId,
    Guid VehicleId,
    string VehiclePlate,
    TireSeason Season,
    string Brand,
    string Dimension,
    string DotCode,
    decimal TreadDepthFrontLeftMm,
    decimal TreadDepthFrontRightMm,
    decimal TreadDepthRearLeftMm,
    decimal TreadDepthRearRightMm,
    string WarehouseRackCoordinate,
    string Status,
    DateTime StoredDateUtc);
