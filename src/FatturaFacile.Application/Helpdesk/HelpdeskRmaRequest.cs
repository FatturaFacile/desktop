// <copyright file="HelpdeskRmaRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// A request to escalate a ticket to an RMA.
/// </summary>
/// <param name="CommandId">The identifier of the request, reused as idempotency key.</param>
/// <param name="TicketId">The ticket.</param>
/// <param name="TicketNumber">The ticket number, for the queue list.</param>
/// <param name="SerialNumber">The serial number of the unit.</param>
/// <param name="EquipmentPassportId">The equipment passport of the unit.</param>
/// <param name="Goodwill">Whether the repair is a goodwill gesture outside the warranty.</param>
/// <param name="ReportedSymptom">The symptom the customer reported.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record HelpdeskRmaRequest(
    Guid CommandId,
    Guid TicketId,
    string TicketNumber,
    string? SerialNumber,
    Guid? EquipmentPassportId,
    bool Goodwill,
    string? ReportedSymptom,
    DateTime OccurredAtUtc);
