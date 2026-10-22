// <copyright file="CreateTireRecordCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Automotive;

/// <summary>
/// Command to check in and record a seasonal customer tire set into warehouse hotel storage.
/// </summary>
/// <param name="VehicleId">The unique identifier of the customer vehicle linked to the tire set.</param>
/// <param name="Season">The tire seasonality type (e.g., Summer, Winter, All-Season).</param>
/// <param name="Brand">The tire manufacturer brand name.</param>
/// <param name="Dimension">The tire dimensional specification (e.g., "225/45 R17 94W").</param>
/// <param name="DotCode">The manufacturing Department of Transportation date code stamped on the tires.</param>
/// <param name="TreadDepthFrontLeftMm">The remaining tread depth measured on the front-left tire in millimeters.</param>
/// <param name="TreadDepthFrontRightMm">The remaining tread depth measured on the front-right tire in millimeters.</param>
/// <param name="TreadDepthRearLeftMm">The remaining tread depth measured on the rear-left tire in millimeters.</param>
/// <param name="TreadDepthRearRightMm">The remaining tread depth measured on the rear-right tire in millimeters.</param>
/// <param name="WarehouseRackCoordinate">The storage location coordinate (rack/shelf/bin) inside the tire warehouse.</param>
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
