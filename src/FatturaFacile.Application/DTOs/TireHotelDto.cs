using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

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