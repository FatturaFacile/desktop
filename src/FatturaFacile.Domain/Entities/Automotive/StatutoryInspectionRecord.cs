using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Automotive;

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