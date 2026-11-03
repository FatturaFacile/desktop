// <copyright file="TireHotelRecord.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

/// <summary>
/// Represents a seasonal tire storage depository record ("tire hotel" / deposito pneumatici),
/// tracking winter/summer sets, remaining tread depths across all four wheels, manufacturing DOT age codes,
/// and physical warehouse rack storage coordinates.
/// </summary>
public sealed class TireHotelRecord : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="TireHotelRecord"/> class for entity framework or serialization.
    /// </summary>
    private TireHotelRecord()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization providing tire storage.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the customer vehicle to which the tire set belongs.
    /// </summary>
    public Guid VehicleId { get; private set; }

    /// <summary>
    /// Gets the seasonal classification of the stored tire set (e.g., Summer, Winter, or AllSeason).
    /// </summary>
    public TireSeason Season { get; private set; }

    /// <summary>
    /// Gets the tire brand and manufacturer (e.g., Michelin, Pirelli, Continental).
    /// </summary>
    public string Brand { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the standardized tire size specification (e.g., "225/45 R17 91W").
    /// </summary>
    public string Dimension { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the 4-digit Department of Transportation (DOT) production week and year code (e.g., "2423" for week 24 of 2023).
    /// </summary>
    public string DotCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the measured tread depth in millimeters for the front-left tire.
    /// </summary>
    public decimal TreadDepthFrontLeftMm { get; private set; }

    /// <summary>
    /// Gets the measured tread depth in millimeters for the front-right tire.
    /// </summary>
    public decimal TreadDepthFrontRightMm { get; private set; }

    /// <summary>
    /// Gets the measured tread depth in millimeters for the rear-left tire.
    /// </summary>
    public decimal TreadDepthRearLeftMm { get; private set; }

    /// <summary>
    /// Gets the measured tread depth in millimeters for the rear-right tire.
    /// </summary>
    public decimal TreadDepthRearRightMm { get; private set; }

    /// <summary>
    /// Gets the physical warehouse storage location or rack compartment coordinate (e.g., "RACK-A-04-2").
    /// </summary>
    public string WarehouseRackCoordinate { get; private set; } = string.Empty; // e.g., RACK-A-04-2

    /// <summary>
    /// Gets the current depository status (e.g., "InStorage", "MountedOnVehicle", "Disposed").
    /// </summary>
    public string Status { get; private set; } = "InStorage";

    /// <summary>
    /// Gets the UTC timestamp when the tire set was received and checked into warehouse storage.
    /// </summary>
    public DateTime StoredDateUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the tire set was retrieved from storage for re-mounting or collection.
    /// </summary>
    public DateTime? RetrievedDateUtc { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this tire depository record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who registered this depository record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this tire depository record was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who last updated this depository record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Checks a customer vehicle tire set into the warehouse depository following a seasonal tire changeover.
    /// </summary>
    /// <param name="data">The parameter object containing tire details and tread measurements.</param>
    /// <returns>A new <see cref="TireHotelRecord"/> aggregate instance checked into storage.</returns>
    public static TireHotelRecord CheckIn(
        TireHotelCheckInData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new TireHotelRecord
        {
            Id                      = Guid.NewGuid(),
            TenantId                = data.TenantId,
            VehicleId               = data.VehicleId,
            Season                  = data.Season,
            Brand                   = data.Brand?.Trim()     ?? string.Empty,
            Dimension               = data.Dimension?.Trim() ?? string.Empty,
            DotCode                 = data.DotCode?.Trim()   ?? string.Empty,
            TreadDepthFrontLeftMm   = data.FlMm,
            TreadDepthFrontRightMm  = data.FrMm,
            TreadDepthRearLeftMm    = data.RlMm,
            TreadDepthRearRightMm   = data.RrMm,
            WarehouseRackCoordinate = data.RackCoordinate?.Trim().ToUpperInvariant() ?? string.Empty,
            Status                  = "InStorage",
            StoredDateUtc           = DateTime.UtcNow,
            CreatedAtUtc            = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Marks the tire set as retrieved from storage and remounted onto the customer's vehicle during seasonal changeover.
    /// </summary>
    public void MountOnVehicle()
    {
        Status           = "MountedOnVehicle";
        RetrievedDateUtc = DateTime.UtcNow;
        UpdatedAtUtc     = DateTime.UtcNow;
    }

}
