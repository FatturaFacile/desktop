// <copyright file="TireHotelRecord.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

/// <summary>
/// Represents tire hotel record.
/// </summary>
public sealed class TireHotelRecord : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private TireHotelRecord()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the vehicle id.
    /// </summary>
    public Guid VehicleId { get; private set; }

    /// <summary>
    /// Gets the season.
    /// </summary>
    public TireSeason Season { get; private set; }

    /// <summary>
    /// Gets the brand.
    /// </summary>
    public string Brand { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the e.g., 225/45 R17 91W.
    /// </summary>
    public string Dimension { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the e.g., 2423.
    /// </summary>
    public string DotCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the tread depth measured in mm.
    /// </summary>
    public decimal TreadDepthFrontLeftMm { get; private set; }

    /// <summary>
    /// Gets the tread depth front right mm.
    /// </summary>
    public decimal TreadDepthFrontRightMm { get; private set; }

    /// <summary>
    /// Gets the tread depth rear left mm.
    /// </summary>
    public decimal TreadDepthRearLeftMm { get; private set; }

    /// <summary>
    /// Gets the tread depth rear right mm.
    /// </summary>
    public decimal TreadDepthRearRightMm { get; private set; }

    /// <summary>
    /// Gets the warehouse Location.
    /// </summary>
    public string WarehouseRackCoordinate { get; private set; } = string.Empty; // e.g., RACK-A-04-2

    /// <summary>
    /// Gets the inStorage, MountedOnVehicle, Disposed.
    /// </summary>
    public string Status { get; private set; } = "InStorage";

    /// <summary>
    /// Gets the stored date utc.
    /// </summary>
    public DateTime StoredDateUtc { get; private set; }

    /// <summary>
    /// Gets the retrieved date utc.
    /// </summary>
    public DateTime? RetrievedDateUtc { get; private set; }

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

    /// <summary>
    /// Check In.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="vehicleId">The vehicle id.</param>
    /// <param name="season">The season.</param>
    /// <param name="brand">The brand.</param>
    /// <param name="dimension">The dimension.</param>
    /// <param name="dotCode">The dot code.</param>
    /// <param name="flMm">The fl mm.</param>
    /// <param name="frMm">The fr mm.</param>
    /// <param name="rlMm">The rl mm.</param>
    /// <param name="rrMm">The rr mm.</param>
    /// <param name="rackCoordinate">The rack coordinate.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
            Brand = brand?.Trim() ?? string.Empty,
            Dimension = dimension?.Trim() ?? string.Empty,
            DotCode = dotCode?.Trim() ?? string.Empty,
            TreadDepthFrontLeftMm = flMm,
            TreadDepthFrontRightMm = frMm,
            TreadDepthRearLeftMm = rlMm,
            TreadDepthRearRightMm = rrMm,
            WarehouseRackCoordinate = rackCoordinate?.Trim().ToUpperInvariant() ?? string.Empty,
            Status = "InStorage",
            StoredDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Mount On Vehicle.
    /// </summary>
    public void MountOnVehicle()
    {
        Status = "MountedOnVehicle";
        RetrievedDateUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
