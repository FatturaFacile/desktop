// <copyright file="HelpdeskPendingAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// A queued or refused helpdesk action, for the queue list of the workbench.
/// </summary>
/// <param name="Id">The entry identifier.</param>
/// <param name="Action">The action.</param>
/// <param name="TicketId">The ticket.</param>
/// <param name="TicketNumber">The ticket number.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
/// <param name="Error">The reason given by the server when it refused the action.</param>
public sealed record HelpdeskPendingAction(Guid Id, HelpdeskAction Action, Guid TicketId, string TicketNumber, DateTime OccurredAtUtc, string? Error);
