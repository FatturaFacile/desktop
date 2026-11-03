// <copyright file="CustomerVehicleDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of a customer vehicle registered in automotive workshop management.
/// </summary>
/// <param name="Id">The unique vehicle identifier.</param>
/// <param name="TenantId">The tenant organization boundary owning this vehicle record.</param>
/// <param name="ClientId">The unique identifier of the registered vehicle owner client.</param>
/// <param name="LicensePlate">The vehicle license plate number.</param>
/// <param name="VinChassisNumber">The vehicle identification number (VIN) / chassis identifier.</param>
/// <param name="Brand">The vehicle make or manufacturer brand name.</param>
/// <param name="Model">The specific vehicle model designation.</param>
/// <param name="RegistrationYear">The official year of first vehicle registration.</param>
/// <param name="Jurisdiction">The regional regulatory jurisdiction under which the vehicle is registered.</param>
/// <param name="CurrentKilometers">The latest recorded odometer mileage reading in kilometers.</param>
public record CustomerVehicleDto(
    Guid Id,
    Guid TenantId,
    Guid ClientId,
    string LicensePlate,
    string VinChassisNumber,
    string Brand,
    string Model,
    int RegistrationYear,
    Jurisdiction Jurisdiction,
    int CurrentKilometers);
