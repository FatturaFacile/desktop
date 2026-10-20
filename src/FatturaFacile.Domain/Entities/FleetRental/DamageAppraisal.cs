using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

public sealed class DamageAppraisal : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid RentalBookingId { get; private set; }

    /// <summary>
    /// CheckIn, CheckOut
    /// </summary>
    public string InspectionType { get; private set; } = "CheckIn";

    /// <summary>
    /// 2D diagram coordinate points
    /// </summary>
    public string DamageCoordinatesJson { get; private set; } = "[]";
    public DamageSeverity Severity { get; private set; }
    public Money EstimatedRepairCost { get; private set; } = Money.Euro(0);
    public DateTime AppraisalDateUtc { get; private set; }

    private DamageAppraisal()
    {
    }

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