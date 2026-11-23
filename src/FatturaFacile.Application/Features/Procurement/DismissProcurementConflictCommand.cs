// <copyright file="DismissProcurementConflictCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Acknowledges a queued write the server refused so it leaves the list.
/// </summary>
/// <param name="EntryId">The outbox entry.</param>
public sealed record DismissProcurementConflictCommand(
    Guid EntryId) : IRequest, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
