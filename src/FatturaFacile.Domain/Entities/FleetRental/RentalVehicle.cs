using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Represents rental vehicle.
/// </summary>
public sealed class RentalVehicle : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the fLT-01.
    /// </summary>
    public string FleetCode { get; private set; } = string.Empty;

/// <summary>
    /// Gets the license plate.
    /// </summary>
    public string LicensePlate { get; private set; } = string.Empty;

/// <summary>
    /// Gets the make model.
    /// </summary>
    public string MakeModel { get; private set; } = string.Empty;

/// <summary>
    /// Gets the daily rate.
    /// </summary>
    public Money DailyRate { get; private set; } = Money.Euro(50);

/// <summary>
    /// Gets the current kilometers.
    /// </summary>
    public int CurrentKilometers { get; private set; }

/// <summary>
    /// Gets a value indicating whether swiss highway vignette active.
    /// </summary>
    public bool SwissHighwayVignetteActive { get; private set; }

/// <summary>
    /// Gets the telepass device serial.
    /// </summary>
    public string? TelepassDeviceSerial { get; private set; }

/// <summary>
    /// Gets a value indicating whether available.
    /// </summary>
    public bool IsAvailable { get; private set; } = true;

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

private RentalVehicle()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="fleetCode">The fleet code.</param>
    /// <param name="plate">The plate.</param>
    /// <param name="makeModel">The make model.</param>
    /// <param name="dailyRate">The daily rate.</param>
    /// <param name="currentKm">The current km.</param>
    /// <param name="swissVignette">The swiss vignette.</param>
    /// <param name="telepass">The telepass.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
            FleetCode = fleetCode.Trim().ToUpperInvariant(),
            LicensePlate = plate.Trim().ToUpperInvariant(),
            MakeModel = makeModel.Trim(),
            DailyRate = dailyRate,
            CurrentKilometers = currentKm,
            SwissHighwayVignetteActive = swissVignette,
            TelepassDeviceSerial = telepass,
            IsAvailable = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
