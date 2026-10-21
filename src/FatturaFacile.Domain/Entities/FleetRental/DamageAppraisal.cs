// <copyright file="DamageAppraisal.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Represents damage appraisal.
/// </summary>
public sealed class DamageAppraisal : Entity<Guid>, ITenantEntity
{
    private DamageAppraisal()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the rental booking id.
    /// </summary>
    public Guid RentalBookingId { get; private set; }

    /// <summary>
    /// Gets the checkIn, CheckOut.
    /// </summary>
    public string InspectionType { get; private set; } = "CheckIn";

    /// <summary>
    /// Gets the 2D diagram coordinate points.
    /// </summary>
    public string DamageCoordinatesJson { get; private set; } = "[]";

    /// <summary>
    /// Gets the severity.
    /// </summary>
    public DamageSeverity Severity { get; private set; }

    /// <summary>
    /// Gets the estimated repair cost.
    /// </summary>
    public Money EstimatedRepairCost { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the appraisal date utc.
    /// </summary>
    public DateTime AppraisalDateUtc { get; private set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="bookingId">The booking id.</param>
    /// <param name="type">The type.</param>
    /// <param name="coordsJson">The coords json.</param>
    /// <param name="severity">The severity.</param>
    /// <param name="repairCost">The repair cost.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
