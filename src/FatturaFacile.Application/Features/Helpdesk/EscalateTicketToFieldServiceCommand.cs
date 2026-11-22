// <copyright file="EscalateTicketToFieldServiceCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Escalates a ticket to a Field Service work order. Idempotent: a repeat returns the work order that already exists.
/// </summary>
/// <param name="Request">The escalation.</param>
public sealed record EscalateTicketToFieldServiceCommand(HelpdeskFieldServiceRequest Request) : IRequest<HelpdeskActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
