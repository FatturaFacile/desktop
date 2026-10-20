using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

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