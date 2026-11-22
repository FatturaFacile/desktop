// <copyright file="EscalateTicketToRmaCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Escalates a ticket to an RMA. Idempotent: a repeat returns the RMA that already exists.
/// </summary>
/// <param name="Request">The escalation.</param>
public sealed record EscalateTicketToRmaCommand(HelpdeskRmaRequest Request) : IRequest<HelpdeskActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
