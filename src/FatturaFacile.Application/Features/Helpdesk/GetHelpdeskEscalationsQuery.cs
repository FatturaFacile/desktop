// <copyright file="GetHelpdeskEscalationsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Loads the SLA escalation history of a ticket, with the cache as offline fallback.
/// </summary>
/// <param name="TicketId">The ticket.</param>
public sealed record GetHelpdeskEscalationsQuery(Guid TicketId) : IRequest<HelpdeskLoad<IReadOnlyList<HelpdeskEscalation>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
