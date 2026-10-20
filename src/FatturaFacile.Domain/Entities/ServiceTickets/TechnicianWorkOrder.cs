using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.ServiceTickets;

public sealed class TechnicianWorkOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// RAP-2026/001
    /// </summary>
    public string WorkOrderNumber { get; private set; } = string.Empty;
    public Guid SupportTicketId { get; private set; }
    public Guid TechnicianUserId { get; private set; }
    public DateTime ServiceDate { get; private set; }
    public decimal HoursWorked { get; private set; }
    public decimal HourlyRate { get; private set; }
    public Currency Currency { get; private set; } = Currency.EUR;
    public string WorkPerformedSummary { get; private set; } = string.Empty;
    public string? SparePartsUsedJson { get; private set; }

    /// <summary>
    /// Customer Signoff & Signature
    /// </summary>
    public string? CustomerSignatoryName { get; private set; }
    public string? DigitalSignatureSvgOrBase64 { get; private set; }
    public bool IsSignedByCustomer { get; private set; }
    public DateTime? SignedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private TechnicianWorkOrder()
    {
    }

    public static TechnicianWorkOrder Create(
        Guid tenantId,
        string workOrderNumber,
        Guid ticketId,
        Guid technicianId,
        DateTime serviceDate,
        decimal hoursWorked,
        decimal hourlyRate,
        Currency currency,
        string summary,
        string? spareParts = null)
    {
        return new TechnicianWorkOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkOrderNumber = workOrderNumber,
            SupportTicketId = ticketId,
            TechnicianUserId = technicianId,
            ServiceDate = serviceDate,
            HoursWorked = hoursWorked,
            HourlyRate = hourlyRate,
            Currency = currency,
            WorkPerformedSummary = summary,
            SparePartsUsedJson = spareParts,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void SignByCustomer(string signatoryName, string signatureData)
    {
        CustomerSignatoryName = signatoryName;
        DigitalSignatureSvgOrBase64 = signatureData;
        IsSignedByCustomer = true;
        SignedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}