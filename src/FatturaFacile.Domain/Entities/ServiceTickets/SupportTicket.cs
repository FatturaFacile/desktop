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