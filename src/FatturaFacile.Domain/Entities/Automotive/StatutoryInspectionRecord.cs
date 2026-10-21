// <copyright file="StatutoryInspectionRecord.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

/// <summary>
/// Represents statutory inspection record.
/// </summary>
public sealed class StatutoryInspectionRecord : Entity<Guid>, ITenantEntity
{
    private StatutoryInspectionRecord()
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
    /// Gets the italian Revisione or Swiss MFK Collaudo.
    /// </summary>
    public InspectionType Type { get; private set; }

    /// <summary>
    /// Gets the scheduled date.
    /// </summary>
    public DateTime ScheduledDate { get; private set; }

    /// <summary>
    /// Gets the completed date.
    /// </summary>
    public DateTime? CompletedDate { get; private set; }

    /// <summary>
    /// Gets the scheduled, Passed, Failed.
    /// </summary>
    public string Status { get; private set; } = "Scheduled";

    /// <summary>
    /// Gets the inspector center notes.
    /// </summary>
    public string? InspectorCenterNotes { get; private set; }

    /// <summary>
    /// Gets the next inspection due date.
    /// </summary>
    public DateTime? NextInspectionDueDate { get; private set; }

    /// <summary>
    /// Schedule.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="vehicleId">The vehicle id.</param>
    /// <param name="type">The type.</param>
    /// <param name="date">The date.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static StatutoryInspectionRecord Schedule(
        Guid tenantId,
        Guid vehicleId,
        InspectionType type,
        DateTime date)
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

    /// <summary>
    /// Mark Completed.
    /// </summary>
    /// <param name="passed">The passed.</param>
    /// <param name="notes">The notes.</param>
    /// <param name="nextDue">The next due.</param>
    public void MarkCompleted(
        bool passed,
        string? notes,
        DateTime nextDue)
    {
        CompletedDate = DateTime.UtcNow;
        Status = passed ? "Passed" : "Failed";
        InspectorCenterNotes = notes;
        NextInspectionDueDate = nextDue;
    }
}
