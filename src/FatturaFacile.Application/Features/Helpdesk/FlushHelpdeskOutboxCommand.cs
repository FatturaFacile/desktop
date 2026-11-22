// <copyright file="FlushHelpdeskOutboxCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Sends the queued time logs and escalations, oldest first, once the server is reachable again.
/// </summary>
public sealed record FlushHelpdeskOutboxCommand : IRequest<HelpdeskFlushResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
