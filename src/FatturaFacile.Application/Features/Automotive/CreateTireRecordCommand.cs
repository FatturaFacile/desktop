using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

/// <summary>
/// Represents create tire record command.
/// </summary>
/// <param name="VehicleId">The vehicle id.</param>
/// <param name="Season">The season.</param>
/// <param name="Brand">The brand.</param>
/// <param name="Dimension">The dimension.</param>
/// <param name="DotCode">The dot code.</param>
/// <param name="TreadDepthFrontLeftMm">The tread depth front left mm.</param>
/// <param name="TreadDepthFrontRightMm">The tread depth front right mm.</param>
/// <param name="TreadDepthRearLeftMm">The tread depth rear left mm.</param>
/// <param name="TreadDepthRearRightMm">The tread depth rear right mm.</param>
/// <param name="WarehouseRackCoordinate">The warehouse rack coordinate.</param>
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
