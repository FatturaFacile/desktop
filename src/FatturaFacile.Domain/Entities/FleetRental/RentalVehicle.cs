// <copyright file="RentalVehicle.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Represents a commercial motor vehicle in the rental fleet,
/// tracking fleet unit codes, daily rental rates, active odometer readings,
/// onboard electronic toll equipment (Italian Telepass), and cross-border road tax status (Swiss Highway Vignette / Autobahnvignette).
/// </summary>
public sealed class RentalVehicle : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private RentalVehicle()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning the vehicle fleet.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique internal fleet unit code (e.g., "FLT-01").
    /// </summary>
    public string FleetCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the official registration license plate number of the rental vehicle.
    /// </summary>
    public string LicensePlate { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the vehicle make, model, and engine trim description.
    /// </summary>
    public string MakeModel { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the standard 24-hour calendar daily rental tariff.
    /// </summary>
    public Money DailyRate { get; private set; } = Money.Euro(50);

    /// <summary>
    /// Gets the current odometer mileage reading in kilometers.
    /// </summary>
    public int CurrentKilometers { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the vehicle possesses a valid Swiss national motorway tax sticker (Autobahnvignette / e-vignette).
    /// </summary>
    public bool SwissHighwayVignetteActive { get; private set; }

    /// <summary>
    /// Gets the hardware serial number of the onboard electronic toll collection transponder (e.g., Telepass / MooneyGo device), if installed.
    /// </summary>
    public string? TelepassDeviceSerial { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the vehicle is currently unallocated, cleaned, and available for customer dispatch.
    /// </summary>
    public bool IsAvailable { get; private set; } = true;

    /// <summary>
    /// Gets or sets the UTC timestamp when the vehicle was registered in the rental fleet.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who enrolled this vehicle into the fleet.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the vehicle fleet profile was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who last updated this fleet profile.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Registers a new vehicle into the operational rental fleet catalog.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="fleetCode">The unique internal fleet identification code.</param>
    /// <param name="plate">The vehicle license plate number.</param>
    /// <param name="makeModel">The brand, model, and trim description.</param>
    /// <param name="dailyRate">The baseline daily rental charge.</param>
    /// <param name="currentKm">The current odometer mileage reading in kilometers.</param>
    /// <param name="swissVignette">Indicates whether a valid Swiss motorway vignette is equipped.</param>
    /// <param name="telepass">The onboard electronic toll collection transponder serial number.</param>
    /// <returns>A newly created <see cref="RentalVehicle"/> aggregate root.</returns>
    public static RentalVehicle Create(
        Guid tenantId,
        string fleetCode,
        string plate,
        string makeModel,
        Money dailyRate,
        int currentKm,
        bool swissVignette = false,
        string? telepass = null)
    {
        return new RentalVehicle
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FleetCode = fleetCode?.Trim().ToUpperInvariant() ?? string.Empty,
            LicensePlate = plate?.Trim().ToUpperInvariant() ?? string.Empty,
            MakeModel = makeModel?.Trim() ?? string.Empty,
            DailyRate = dailyRate,
            CurrentKilometers = currentKm,
            SwissHighwayVignetteActive = swissVignette,
            TelepassDeviceSerial = telepass,
            IsAvailable = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
