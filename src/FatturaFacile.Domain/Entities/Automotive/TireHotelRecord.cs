using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

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