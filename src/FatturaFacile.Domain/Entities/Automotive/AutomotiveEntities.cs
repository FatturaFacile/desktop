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

public sealed class TireHotelRecord : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid VehicleId { get; private set; }
    public TireSeason Season { get; private set; }
    public string Brand { get; private set; } = string.Empty;
    /// <summary>
    /// e.g., 225/45 R17 91W
    /// </summary>
    public string Dimension { get; private set; } = string.Empty;
    /// <summary>
    /// e.g., 2423
    /// </summary>
    public string DotCode { get; private set; } = string.Empty;

    /// <summary>
    /// Tread depth measured in mm
    /// </summary>
    public decimal TreadDepthFrontLeftMm { get; private set; }
    public decimal TreadDepthFrontRightMm { get; private set; }
    public decimal TreadDepthRearLeftMm { get; private set; }
    public decimal TreadDepthRearRightMm { get; private set; }

    /// <summary>
    /// Warehouse Location
    /// </summary>
    public string WarehouseRackCoordinate { get; private set; } = string.Empty; // e.g., RACK-A-04-2
    /// <summary>
    /// InStorage, MountedOnVehicle, Disposed
    /// </summary>
    public string Status { get; private set; } = "InStorage";
    public DateTime StoredDateUtc { get; private set; }
    public DateTime? RetrievedDateUtc { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private TireHotelRecord()
    {
    }

    public static TireHotelRecord CheckIn(
        Guid tenantId,
        Guid vehicleId,
        TireSeason season,
        string brand,
        string dimension,
        string dotCode,
        decimal flMm,
        decimal frMm,
        decimal rlMm,
        decimal rrMm,
        string rackCoordinate)
    {
        return new TireHotelRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VehicleId = vehicleId,
            Season = season,
            Brand = brand.Trim(),
            Dimension = dimension.Trim(),
            DotCode = dotCode.Trim(),
            TreadDepthFrontLeftMm = flMm,
            TreadDepthFrontRightMm = frMm,
            TreadDepthRearLeftMm = rlMm,
            TreadDepthRearRightMm = rrMm,
            WarehouseRackCoordinate = rackCoordinate.Trim().ToUpperInvariant(),
            Status = "InStorage",
            StoredDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void MountOnVehicle()
    {
        Status = "MountedOnVehicle";
        RetrievedDateUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

public sealed class StatutoryInspectionRecord : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid VehicleId { get; private set; }
    /// <summary>
    /// Italian Revisione or Swiss MFK Collaudo
    /// </summary>
    public InspectionType Type { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    /// <summary>
    /// Scheduled, Passed, Failed
    /// </summary>
    public string Status { get; private set; } = "Scheduled";
    public string? InspectorCenterNotes { get; private set; }
    public DateTime? NextInspectionDueDate { get; private set; }

    private StatutoryInspectionRecord()
    {
    }

    public static StatutoryInspectionRecord Schedule(Guid tenantId, Guid vehicleId, InspectionType type, DateTime date)
    {
        return new StatutoryInspectionRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VehicleId = vehicleId,
            Type = type,
            ScheduledDate = date,
            Status = "Scheduled",
        };
    }

    public void MarkCompleted(bool passed, string? notes, DateTime nextDue)
    {
        CompletedDate = DateTime.UtcNow;
        Status = passed ? "Passed" : "Failed";
        InspectorCenterNotes = notes;
        NextInspectionDueDate = nextDue;
    }
}
