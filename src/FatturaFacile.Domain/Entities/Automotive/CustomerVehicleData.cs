// <copyright file="CustomerVehicleData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

/// <summary>
/// Data transfer object holding values required to create a new <see cref="CustomerVehicle"/>.
/// </summary>
/// <param name="TenantId">The unique identifier of the owning tenant workshop.</param>
/// <param name="ClientId">The unique identifier of the vehicle owner client.</param>
/// <param name="LicensePlate">The vehicle registration plate number.</param>
/// <param name="Vin">The 17-character chassis vehicle identification number (VIN).</param>
/// <param name="Brand">The vehicle manufacturer brand.</param>
/// <param name="Model">The commercial model name.</param>
/// <param name="Year">The year of initial road registration.</param>
/// <param name="Jurisdiction">The statutory jurisdiction governing inspections and registration.</param>
/// <param name="Km">The initial odometer mileage reading in kilometers.</param>
public sealed record CustomerVehicleData(
    Guid         TenantId,
    Guid         ClientId,
    string       LicensePlate,
    string       Vin,
    string       Brand,
    string       Model,
    int          Year,
    Jurisdiction Jurisdiction,
    int          Km = 0);
