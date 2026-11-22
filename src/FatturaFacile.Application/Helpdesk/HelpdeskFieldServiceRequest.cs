// <copyright file="HelpdeskFieldServiceRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// A request to escalate a ticket to a Field Service work order.
/// </summary>
/// <param name="CommandId">The identifier of the request, reused as idempotency key.</param>
/// <param name="TicketId">The ticket.</param>
/// <param name="TicketNumber">The ticket number, for the queue list.</param>
/// <param name="WorkType">The type of work, when chosen.</param>
/// <param name="ScheduledStart">The planned start, when known.</param>
/// <param name="ScheduledEnd">The planned end, when known.</param>
/// <param name="LocationAddress">The address of the visit, when different from the one of the customer.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record HelpdeskFieldServiceRequest(
    Guid CommandId,
    Guid TicketId,
    string TicketNumber,
    string? WorkType,
    DateTime? ScheduledStart,
    DateTime? ScheduledEnd,
    string? LocationAddress,
    DateTime OccurredAtUtc);
