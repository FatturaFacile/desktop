using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.TimeTracking;

public sealed class AbsenceRecord : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; private set; }
    public AbsenceType AbsenceType { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal TotalWorkingDays { get; private set; }
    public bool IsApproved { get; private set; }

    /// <summary>
    /// IT: INPS PUC Protocollo Telematico; CH: KTG Arztzeugnis / APG-EO Meldekarte
    /// </summary>
    public string? OfficialCertificateNumber { get; private set; }
    public string? Notes { get; private set; }

    private AbsenceRecord()
    {
    }

    public static AbsenceRecord Create(
        Guid tenantId,
        Guid employeeId,
        AbsenceType type,
        DateTime start,
        DateTime end,
        decimal days,
        string? certificateNumber = null,
        string? notes = null)
    {
        return new AbsenceRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = employeeId,
            AbsenceType = type,
            StartDate = start,
            EndDate = end,
            TotalWorkingDays = days,
            OfficialCertificateNumber = certificateNumber,
            Notes = notes,
            IsApproved = false,
        };
    }

    public void Approve() => IsApproved = true;
}