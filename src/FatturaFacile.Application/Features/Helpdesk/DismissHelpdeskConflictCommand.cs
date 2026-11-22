// <copyright file="DismissHelpdeskConflictCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Acknowledges a queued action that the server refused so it leaves the list.
/// </summary>
/// <param name="EntryId">The outbox entry.</param>
public sealed record DismissHelpdeskConflictCommand(Guid EntryId) : IRequest, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
