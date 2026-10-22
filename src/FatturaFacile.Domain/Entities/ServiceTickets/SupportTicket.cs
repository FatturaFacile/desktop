// <copyright file="SupportTicket.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.ServiceTickets;

/// <summary>
/// Represents support ticket.
/// </summary>
public sealed class SupportTicket : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SupportTicket"/> class for entity framework or serialization.
    /// </summary>
    private SupportTicket()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the ticket number.
    /// </summary>
    public string TicketNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the title.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the priority.
    /// </summary>
    public RiskLevel Priority { get; private set; } = RiskLevel.Medium;

    /// <summary>
    /// Gets the sla due date.
    /// </summary>
    public DateTime SlaDueDate { get; private set; }

    /// <summary>
    /// Gets the open, Assigned, InProgress, Resolved, Closed.
    /// </summary>
    public string Status { get; private set; } = "Open";

    /// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="ticketNumber">The ticket number.</param>
    /// <param name="clientId">The client id.</param>
    /// <param name="title">The title.</param>
    /// <param name="description">The description.</param>
    /// <param name="priority">The priority.</param>
    /// <param name="slaDueDate">The sla due date.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static SupportTicket Create(
        Guid      tenantId,
        string    ticketNumber,
        Guid      clientId,
        string    title,
        string    description,
        RiskLevel priority,
        DateTime  slaDueDate)
    {
        return new SupportTicket
               {
                   Id           = Guid.NewGuid(),
                   TenantId     = tenantId,
                   TicketNumber = ticketNumber,
                   ClientId     = clientId,
                   Title        = title,
                   Description  = description,
                   Priority     = priority,
                   SlaDueDate   = slaDueDate,
                   Status       = "Open",
                   CreatedAtUtc = DateTime.UtcNow,
               };
    }

    /// <summary>
    /// Mark Resolved.
    /// </summary>
    public void MarkResolved()
    {
        Status       = "Resolved";
        UpdatedAtUtc = DateTime.UtcNow;
    }

}
