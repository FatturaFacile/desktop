// <copyright file="HelpdeskTicketRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Helpdesk;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A ticket of the workbench list, formatted for display.
/// </summary>
/// <param name="Ticket">The ticket with its SLA targets.</param>
/// <param name="RiskText">The formatted risk share.</param>
/// <param name="State">The SLA state: Breached or AtRisk, for the colour of the row.</param>
/// <param name="DueText">The formatted earliest due instant.</param>
/// <param name="TargetsText">The localized names of the targets at risk.</param>
/// <param name="HasPending">Whether an action on the ticket waits in the outbox.</param>
internal sealed record HelpdeskTicketRow(HelpdeskTicketRisk Ticket, string RiskText, string State, string DueText, string TargetsText, bool HasPending)
{
    /// <summary>
    /// Gets the ticket number.
    /// </summary>
    public string Number => Ticket.TicketNumber;

    /// <summary>
    /// Gets the subject.
    /// </summary>
    public string Subject => Ticket.Subject;

    /// <summary>
    /// Gets the priority.
    /// </summary>
    public string Priority => Ticket.Priority;

    /// <summary>
    /// Gets the status.
    /// </summary>
    public string Status => Ticket.Status;
}
