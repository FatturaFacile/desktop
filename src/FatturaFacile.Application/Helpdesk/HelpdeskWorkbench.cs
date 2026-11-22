// <copyright file="HelpdeskWorkbench.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// The ticket workbench: the tickets sorted by SLA urgency and the queue of actions waiting for the server.
/// </summary>
/// <param name="Tickets">The tickets, most urgent first.</param>
/// <param name="FromCache">Whether the list is the last cached answer.</param>
/// <param name="SyncedAtUtc">The instant the list was pulled, or null when nothing was ever pulled.</param>
/// <param name="Pending">The actions waiting to be sent.</param>
/// <param name="Conflicts">The actions the server refused.</param>
public sealed record HelpdeskWorkbench(
    IReadOnlyList<HelpdeskTicketRisk> Tickets,
    bool FromCache,
    DateTime? SyncedAtUtc,
    IReadOnlyList<HelpdeskPendingAction> Pending,
    IReadOnlyList<HelpdeskPendingAction> Conflicts);
