using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

public sealed class RentalVehicle : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// FLT-01
    /// </summary>
    public string FleetCode { get; private set; } = string.Empty;
    public string LicensePlate { get; private set; } = string.Empty;
    public string MakeModel { get; private set; } = string.Empty;
    public Money DailyRate { get; private set; } = Money.Euro(50);
    public int CurrentKilometers { get; private set; }
    public bool SwissHighwayVignetteActive { get; private set; }
    public string? TelepassDeviceSerial { get; private set; }
    public bool IsAvailable { get; private set; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private RentalVehicle()
    {
    }

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