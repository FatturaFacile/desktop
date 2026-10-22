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
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="vehicleId">The vehicle identifier.</param>
    /// <param name="season">The tire set season (Summer, Winter, AllSeason).</param>
    /// <param name="brand">The tire manufacturer brand.</param>
    /// <param name="dimension">The tire dimension specification.</param>
    /// <param name="dotCode">The production date DOT code.</param>
    /// <param name="flMm">Front-left tire tread depth in millimeters.</param>
    /// <param name="frMm">Front-right tire tread depth in millimeters.</param>
    /// <param name="rlMm">Rear-left tire tread depth in millimeters.</param>
    /// <param name="rrMm">Rear-right tire tread depth in millimeters.</param>
    /// <param name="rackCoordinate">The allocated warehouse rack bin or shelf coordinate.</param>
    /// <returns>A new <see cref="TireHotelRecord"/> aggregate instance checked into storage.</returns>
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
    /// Marks the tire set as retrieved from storage and remounted onto the customer's vehicle during seasonal changeover.
    /// </summary>
    public void MountOnVehicle()
    {
        Status = "MountedOnVehicle";
        RetrievedDateUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
