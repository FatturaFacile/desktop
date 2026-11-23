// <copyright file="DismissReceivablesConflictCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Acknowledges a queued action that the server refused so it leaves the list.
/// </summary>
/// <param name="EntryId">The outbox entry.</param>
public sealed record DismissReceivablesConflictCommand(Guid EntryId) : IRequest, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
