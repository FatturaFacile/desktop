// <copyright file="RentalVehicleData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Data transfer object holding fleet specifications and rental rates required to create a new <see cref="RentalVehicle"/>.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="FleetCode">The internal corporate fleet serial number.</param>
/// <param name="Plate">The vehicle license plate number.</param>
/// <param name="MakeModel">The brand, model, and trim description.</param>
/// <param name="DailyRate">The baseline daily rental charge.</param>
/// <param name="CurrentKm">The current odometer mileage reading in kilometers.</param>
/// <param name="SwissVignette">Indicates whether a valid Swiss motorway vignette is equipped.</param>
/// <param name="Telepass">The onboard electronic toll collection transponder serial number.</param>
public sealed record RentalVehicleData(
    Guid    TenantId,
    string  FleetCode,
    string  Plate,
    string  MakeModel,
    Money   DailyRate,
    int     CurrentKm,
    bool    SwissVignette = false,
    string? Telepass      = null);
