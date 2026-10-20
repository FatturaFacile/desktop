using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
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

public sealed class RentalBooking : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string BookingNumber { get; private set; } = string.Empty;
    public Guid RentalVehicleId { get; private set; }
    public Guid ClientId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string PickupLocation { get; private set; } = string.Empty;
    public string ReturnLocation { get; private set; } = string.Empty;
    public Money TotalRentAmount { get; private set; } = Money.Euro(0);
    public Money DepositAmount { get; private set; } = Money.Euro(0);
    /// <summary>
    /// Reserved, Active, Completed, Cancelled
    /// </summary>
    public string Status { get; private set; } = "Reserved";

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private RentalBooking()
    {
    }

    public static RentalBooking Create(
        Guid tenantId,
        string bookingNumber,
        Guid vehicleId,
        Guid clientId,
        DateTime start,
        DateTime end,
        string pickup,
        string ret,
        Money total,
        Money deposit)
    {
        return new RentalBooking
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BookingNumber = bookingNumber,
            RentalVehicleId = vehicleId,
            ClientId = clientId,
            StartDate = start,
            EndDate = end,
            PickupLocation = pickup,
            ReturnLocation = ret,
            TotalRentAmount = total,
            DepositAmount = deposit,
            Status = "Reserved",
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}

public sealed class DamageAppraisal : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid RentalBookingId { get; private set; }
    /// <summary>
    /// CheckIn, CheckOut
    /// </summary>
    public string InspectionType { get; private set; } = "CheckIn";
    /// <summary>
    /// 2D diagram coordinate points
    /// </summary>
    public string DamageCoordinatesJson { get; private set; } = "[]";
    public DamageSeverity Severity { get; private set; }
    public Money EstimatedRepairCost { get; private set; } = Money.Euro(0);
    public DateTime AppraisalDateUtc { get; private set; }

    private DamageAppraisal()
    {
    }

    public static DamageAppraisal Create(
        Guid tenantId,
        Guid bookingId,
        string type,
        string coordsJson,
        DamageSeverity severity,
        Money repairCost)
    {
        return new DamageAppraisal
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RentalBookingId = bookingId,
            InspectionType = type,
            DamageCoordinatesJson = coordsJson,
            Severity = severity,
            EstimatedRepairCost = repairCost,
            AppraisalDateUtc = DateTime.UtcNow,
        };
    }
}
