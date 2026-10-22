// <copyright file="StatutoryInspectionRecord.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

/// <summary>
/// Represents a mandatory periodic roadworthiness vehicle inspection record under applicable traffic legislation
/// (such as the Italian biennial/quadrennial Revisione Ministeriale or the Swiss Motorfahrzeugkontrolle / MFK Collaudo).
/// </summary>
public sealed class StatutoryInspectionRecord : Entity<Guid>, ITenantEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="StatutoryInspectionRecord"/> class for entity framework or serialization.
    /// </summary>
    private StatutoryInspectionRecord()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization conducting or booking the inspection.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the target vehicle undergoing technical inspection.
    /// </summary>
    public Guid VehicleId { get; private set; }

    /// <summary>
    /// Gets the statutory inspection classification category (e.g., Italian Revisione or Swiss MFK Collaudo).
    /// </summary>
    public InspectionType Type { get; private set; }

    /// <summary>
    /// Gets the planned appointment date for the statutory technical inspection.
    /// </summary>
    public DateTime ScheduledDate { get; private set; }

    /// <summary>
    /// Gets the timestamp when the vehicle roadworthiness inspection test was formally finalized.
    /// </summary>
    public DateTime? CompletedDate { get; private set; }

    /// <summary>
    /// Gets the current inspection outcome status (e.g., "Scheduled", "Passed", or "Failed").
    /// </summary>
    public string Status { get; private set; } = "Scheduled";

    /// <summary>
    /// Gets official remarks, exhaust emission metrics, or defect notices logged by the certified vehicle inspection center.
    /// </summary>
    public string? InspectorCenterNotes { get; private set; }

    /// <summary>
    /// Gets the subsequent legally mandated inspection deadline date computed following test completion.
    /// </summary>
    public DateTime? NextInspectionDueDate { get; private set; }

    /// <summary>
    /// Schedules a new statutory technical roadworthiness inspection for a vehicle.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="vehicleId">The target vehicle identifier.</param>
    /// <param name="type">The statutory inspection type (e.g., Revisione or MFK Collaudo).</param>
    /// <param name="date">The scheduled appointment date.</param>
    /// <returns>A new <see cref="StatutoryInspectionRecord"/> instance with "Scheduled" status.</returns>
    public static StatutoryInspectionRecord Schedule(
        Guid           tenantId,
        Guid           vehicleId,
        InspectionType type,
        DateTime       date)
    {
        return new StatutoryInspectionRecord
               {
                   Id            = Guid.NewGuid(),
                   TenantId      = tenantId,
                   VehicleId     = vehicleId,
                   Type          = type,
                   ScheduledDate = date,
                   Status        = "Scheduled",
               };
    }

    /// <summary>
    /// Finalizes the inspection record with the outcome, test notes, and subsequent statutory renewal deadline.
    /// </summary>
    /// <param name="passed">Indicates whether the vehicle passed all roadworthiness safety and emissions checks.</param>
    /// <param name="notes">Official observations, brake efficiency values, or defect notes from the inspection technician.</param>
    /// <param name="nextDue">The mandatory future deadline date for the next periodic inspection.</param>
    public void MarkCompleted(
        bool     passed,
        string?  notes,
        DateTime nextDue)
    {
        CompletedDate         = DateTime.UtcNow;
        Status                = passed ? "Passed" : "Failed";
        InspectorCenterNotes  = notes;
        NextInspectionDueDate = nextDue;
    }

}
