// <copyright file="DamageAppraisal.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Represents a visual technical inspection report and damage appraisal performed on a rental vehicle
/// during customer pick-up (CheckOut) or drop-off return (CheckIn), capturing bodywork coordinates, severity, and repair estimates.
/// </summary>
public sealed class DamageAppraisal : Entity<Guid>, ITenantEntity
{
    private DamageAppraisal()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant rental company.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the associated rental booking or reservation.
    /// </summary>
    public Guid RentalBookingId { get; private set; }

    /// <summary>
    /// Gets the inspection phase classification indicating whether damage was noted at customer handover ("CheckOut") or return ("CheckIn").
    /// </summary>
    public string InspectionType { get; private set; } = "CheckIn";

    /// <summary>
    /// Gets serialized JSON representation of 2D schematic car blueprint coordinates identifying the physical locations of scratches, dents, or broken components.
    /// </summary>
    public string DamageCoordinatesJson { get; private set; } = "[]";

    /// <summary>
    /// Gets the assessed severity classification level of the vehicle damage (e.g., Minor, Moderate, Severe).
    /// </summary>
    public DamageSeverity Severity { get; private set; }

    /// <summary>
    /// Gets the estimated monetary cost required to restore the vehicle to pre-rental condition.
    /// </summary>
    public Money EstimatedRepairCost { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the UTC timestamp when the vehicle appraisal was performed.
    /// </summary>
    public DateTime AppraisalDateUtc { get; private set; }

    /// <summary>
    /// Creates a new damage appraisal inspection record for a fleet vehicle rental booking.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="bookingId">The associated rental reservation identifier.</param>
    /// <param name="type">The inspection type stage ("CheckOut" at pickup or "CheckIn" upon return).</param>
    /// <param name="coordsJson">JSON coordinate markers on the vehicle diagram schema.</param>
    /// <param name="severity">The evaluated severity level of the detected damage.</param>
    /// <param name="repairCost">The estimated monetary repair cost.</param>
    /// <returns>A newly recorded <see cref="DamageAppraisal"/> entity.</returns>
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
