// <copyright file="HelpdeskTimeLogRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// A request to log worked time on a ticket.
/// </summary>
/// <param name="CommandId">The identifier of the request, reused as idempotency key.</param>
/// <param name="TicketId">The ticket.</param>
/// <param name="TicketNumber">The ticket number, for the queue list.</param>
/// <param name="Minutes">The minutes worked.</param>
/// <param name="TechnicianId">The technician, when not the current user.</param>
/// <param name="Description">What was done.</param>
/// <param name="IsBillable">Whether the time may be billed once the contract bank is used up.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
/// <param name="RemainingContractHours">The hours left on the contract bank according to the cached customer context, for the offline estimate.</param>
public sealed record HelpdeskTimeLogRequest(
    Guid CommandId,
    Guid TicketId,
    string TicketNumber,
    int Minutes,
    Guid? TechnicianId,
    string? Description,
    bool IsBillable,
    DateTime OccurredAtUtc,
    decimal? RemainingContractHours = null);
