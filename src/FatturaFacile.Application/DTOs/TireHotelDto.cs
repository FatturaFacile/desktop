// <copyright file="TireHotelDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of a customer tire set deposited in the seasonal warehouse tire hotel.
/// </summary>
/// <param name="Id">The unique tire storage record identifier.</param>
/// <param name="TenantId">The tenant organization boundary owning this tire hotel record.</param>
/// <param name="VehicleId">The unique identifier of the customer vehicle associated with the tires.</param>
/// <param name="VehiclePlate">The license plate number of the associated vehicle.</param>
/// <param name="Season">The tire seasonal classification (e.g., Summer, Winter, All-Season).</param>
/// <param name="Brand">The tire manufacturer brand name.</param>
/// <param name="Dimension">The standardized dimensional specification (e.g., "205/55 R16 91V").</param>
/// <param name="DotCode">The manufacturing Department of Transportation date code stamped on the tire carcass.</param>
/// <param name="TreadDepthFrontLeftMm">The measured tread depth of the front-left tire, in millimeters.</param>
/// <param name="TreadDepthFrontRightMm">The measured tread depth of the front-right tire, in millimeters.</param>
/// <param name="TreadDepthRearLeftMm">The measured tread depth of the rear-left tire, in millimeters.</param>
/// <param name="TreadDepthRearRightMm">The measured tread depth of the rear-right tire, in millimeters.</param>
/// <param name="WarehouseRackCoordinate">The physical warehouse rack and shelf coordinate where the set is stored.</param>
/// <param name="Status">The current custody status (e.g., Stored, Mounted, Disposed).</param>
/// <param name="StoredDateUtc">The UTC timestamp when the tire set was checked into storage custody.</param>
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
