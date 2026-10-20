// <copyright file="CustomerVehicle.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

public sealed class CustomerVehicle : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid ClientId { get; private set; }

    /// <summary>
    /// IT: AB123CD, CH: TI 123456
    /// </summary>
    public string LicensePlate { get; private set; } = string.Empty;

    public string VinChassisNumber { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int RegistrationYear { get; private set; }

    public Jurisdiction Jurisdiction { get; private set; }

    public int CurrentKilometers { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private CustomerVehicle()
    {
    }

    public static CustomerVehicle Create(
        Guid tenantId,
        Guid clientId,
        string licensePlate,
        string vin,
        string brand,
        string model,
        int year,
        Jurisdiction jurisdiction,
        int km = 0)
    {
        return new CustomerVehicle
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClientId = clientId,
            LicensePlate = licensePlate.Trim().ToUpperInvariant(),
            VinChassisNumber = vin.Trim().ToUpperInvariant(),
            Brand = brand.Trim(),
            Model = model.Trim(),
            RegistrationYear = year,
            Jurisdiction = jurisdiction,
            CurrentKilometers = km,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void UpdateKilometers(int km)
    {
        if (km > CurrentKilometers)
        {
            CurrentKilometers = km;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
