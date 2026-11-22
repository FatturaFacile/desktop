// <copyright file="GetHelpdeskCustomerContextQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Loads the customer 360 side panel of a ticket, with the cache as offline fallback.
/// </summary>
/// <param name="TicketId">The ticket.</param>
public sealed record GetHelpdeskCustomerContextQuery(Guid TicketId) : IRequest<HelpdeskLoad<HelpdeskCustomerContext>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
