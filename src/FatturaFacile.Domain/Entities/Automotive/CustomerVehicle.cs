// <copyright file="CustomerVehicle.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

/// <summary>
/// Represents a customer-owned motor vehicle registered in the garage or automotive workshop database,
/// tracking its chassis VIN, registration plate, odometer mileage, and jurisdiction.
/// </summary>
public sealed class CustomerVehicle : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerVehicle"/> class for entity framework or serialization.
    /// </summary>
    private CustomerVehicle()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning this vehicle record.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the customer or fleet client owning the vehicle.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the official license plate number formatted according to jurisdiction conventions (e.g., Italian "AB123CD", Swiss "TI 123456").
    /// </summary>
    public string LicensePlate { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the 17-character Vehicle Identification Number (VIN) chassis code.
    /// </summary>
    public string VinChassisNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the vehicle manufacturer or make (e.g., Fiat, Volkswagen, BMW).
    /// </summary>
    public string Brand { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the specific model designation and trim line (e.g., Panda 1.2 Fire, Golf 2.0 TDI).
    /// </summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the calendar year of initial vehicle road registration.
    /// </summary>
    public int RegistrationYear { get; private set; }

    /// <summary>
    /// Gets the regulatory jurisdiction governing vehicle licensing and statutory technical inspections (e.g., Italy, Switzerland).
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets the most recent odometer mileage reading recorded in kilometers.
    /// </summary>
    public int CurrentKilometers { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this vehicle was registered in the workshop management system.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who registered this vehicle.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this vehicle record was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who last modified this vehicle record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new customer vehicle record in the automotive repair workshop catalog.
    /// </summary>
    /// <param name="data">The parameter object containing customer vehicle details.</param>
    /// <returns>A newly created <see cref="CustomerVehicle"/> aggregate instance.</returns>
    public static CustomerVehicle Create(
        CustomerVehicleData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new CustomerVehicle
        {
            Id                = Guid.NewGuid(),
            TenantId          = data.TenantId,
            ClientId          = data.ClientId,
            LicensePlate      = data.LicensePlate?.Trim().ToUpperInvariant() ?? string.Empty,
            VinChassisNumber  = data.Vin?.Trim().ToUpperInvariant()          ?? string.Empty,
            Brand             = data.Brand?.Trim()                           ?? string.Empty,
            Model             = data.Model?.Trim()                           ?? string.Empty,
            RegistrationYear  = data.Year,
            Jurisdiction      = data.Jurisdiction,
            CurrentKilometers = data.Km,
            CreatedAtUtc      = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Updates the vehicle odometer reading if the provided mileage exceeds the existing recorded value.
    /// </summary>
    /// <param name="km">The newly confirmed odometer reading in kilometers.</param>
    public void UpdateKilometers(int km)
    {
        if (km > CurrentKilometers)
        {
            CurrentKilometers = km;
            UpdatedAtUtc      = DateTime.UtcNow;
        }
    }

}
