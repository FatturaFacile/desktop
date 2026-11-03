// <copyright file="TireHotelCheckInData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

/// <summary>
/// Data transfer object holding tire specifications and tread depth measurements for checking a tire set into storage.
/// </summary>
/// <param name="TenantId">The unique tenant workshop identifier.</param>
/// <param name="VehicleId">The associated vehicle identifier.</param>
/// <param name="Season">The tire set season (Summer, Winter, AllSeason).</param>
/// <param name="Brand">The tire manufacturer brand.</param>
/// <param name="Dimension">The tire dimension specification.</param>
/// <param name="DotCode">The production date DOT code.</param>
/// <param name="FlMm">Front-left tire tread depth in millimeters.</param>
/// <param name="FrMm">Front-right tire tread depth in millimeters.</param>
/// <param name="RlMm">Rear-left tire tread depth in millimeters.</param>
/// <param name="RrMm">Rear-right tire tread depth in millimeters.</param>
/// <param name="RackCoordinate">The allocated warehouse rack bin or shelf coordinate.</param>
public sealed record TireHotelCheckInData(
    Guid       TenantId,
    Guid       VehicleId,
    TireSeason Season,
    string     Brand,
    string     Dimension,
    string     DotCode,
    decimal    FlMm,
    decimal    FrMm,
    decimal    RlMm,
    decimal    RrMm,
    string     RackCoordinate);
