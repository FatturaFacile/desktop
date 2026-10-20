using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.ServiceTickets;

public sealed class SupportTicket : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string TicketNumber { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public RiskLevel Priority { get; private set; } = RiskLevel.Medium;
    public DateTime SlaDueDate { get; private set; }
    /// <summary>
    /// Open, Assigned, InProgress, Resolved, Closed
    /// </summary>
    public string Status { get; private set; } = "Open";

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private SupportTicket()
    {
    }

    public static SupportTicket Create(
        Guid tenantId,
        string ticketNumber,
        Guid clientId,
        string title,
        string description,
        RiskLevel priority,
        DateTime slaDueDate)
    {
        return new SupportTicket
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TicketNumber = ticketNumber,
            ClientId = clientId,
            Title = title,
            Description = description,
            Priority = priority,
            SlaDueDate = slaDueDate,
            Status = "Open",
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void MarkResolved()
    {
        Status = "Resolved";
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

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
