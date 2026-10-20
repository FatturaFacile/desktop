// <copyright file="CustomerVehicle.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

/// <summary>
/// Represents customer vehicle.
/// </summary>
public sealed class CustomerVehicle : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; private set; }

/// <summary>
    /// Gets the iT: AB123CD, CH: TI 123456.
    /// </summary>
    public string LicensePlate { get; private set; } = string.Empty;

/// <summary>
    /// Gets the vin chassis number.
    /// </summary>
    public string VinChassisNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the brand.
    /// </summary>
    public string Brand { get; private set; } = string.Empty;

/// <summary>
    /// Gets the model.
    /// </summary>
    public string Model { get; private set; } = string.Empty;

/// <summary>
    /// Gets the registration year.
    /// </summary>
    public int RegistrationYear { get; private set; }

/// <summary>
    /// Gets the jurisdiction.
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

/// <summary>
    /// Gets the current kilometers.
    /// </summary>
    public int CurrentKilometers { get; private set; }

/// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

/// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

private CustomerVehicle()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="clientId">The client id.</param>
    /// <param name="licensePlate">The license plate.</param>
    /// <param name="vin">The vin.</param>
    /// <param name="brand">The brand.</param>
    /// <param name="model">The model.</param>
    /// <param name="year">The year.</param>
    /// <param name="jurisdiction">The jurisdiction.</param>
    /// <param name="km">The km.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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

/// <summary>
    /// Update Kilometers.
    /// </summary>
    /// <param name="km">The km.</param>
    public void UpdateKilometers(int km)
    {
        if (km > CurrentKilometers)
        {
            CurrentKilometers = km;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
