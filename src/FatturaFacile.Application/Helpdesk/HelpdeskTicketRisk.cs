// <copyright file="HelpdeskTicketRisk.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// A ticket from the breach-risk query, with its SLA targets.
/// </summary>
/// <param name="TicketId">The server identifier.</param>
/// <param name="TicketNumber">The ticket number.</param>
/// <param name="Subject">The subject.</param>
/// <param name="Priority">The priority.</param>
/// <param name="Status">The status.</param>
/// <param name="CustomerId">The customer, when known.</param>
/// <param name="AssignedTeamId">The assigned queue, when any.</param>
/// <param name="AssignedTechnicianId">The assigned technician, when any.</param>
/// <param name="RiskPercent">The highest elapsed share over the open targets.</param>
/// <param name="Targets">The targets that are at risk or breached.</param>
public sealed record HelpdeskTicketRisk(
    Guid TicketId,
    string TicketNumber,
    string Subject,
    string Priority,
    string Status,
    Guid? CustomerId,
    Guid? AssignedTeamId,
    Guid? AssignedTechnicianId,
    double RiskPercent,
    IReadOnlyList<HelpdeskBreachTarget> Targets)
{
    /// <summary>
    /// Gets a value indicating whether any target is already breached.
    /// </summary>
    public bool IsBreached => Targets.Any(target => target.IsBreached);

    /// <summary>
    /// Gets the earliest due instant over the targets, or null when none carries one.
    /// </summary>
    public DateTime? EarliestDueAt => Targets.Min(target => target.DueAt);
}
