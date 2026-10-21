// <copyright file="CustomerVehicleDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents customer vehicle dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="ClientId">The client id.</param>
/// <param name="LicensePlate">The license plate.</param>
/// <param name="VinChassisNumber">The vin chassis number.</param>
/// <param name="Brand">The brand.</param>
/// <param name="Model">The model.</param>
/// <param name="RegistrationYear">The registration year.</param>
/// <param name="Jurisdiction">The jurisdiction.</param>
/// <param name="CurrentKilometers">The current kilometers.</param>
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
