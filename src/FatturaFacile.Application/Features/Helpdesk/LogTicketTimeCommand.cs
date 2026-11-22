// <copyright file="LogTicketTimeCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Logs worked time on a ticket. Saved in the outbox first and sent when the server is reachable; the answer says whether the
/// time was deducted from the contract hour bank or is billable.
/// </summary>
/// <param name="Request">The time entry.</param>
public sealed record LogTicketTimeCommand(HelpdeskTimeLogRequest Request) : IRequest<HelpdeskActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
