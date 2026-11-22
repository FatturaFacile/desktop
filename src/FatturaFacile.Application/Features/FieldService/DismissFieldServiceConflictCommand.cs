// <copyright file="DismissFieldServiceConflictCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Dismisses a field operation the server rejected, after the dispatcher has dealt with it.
/// </summary>
/// <param name="EntryId">The outbox entry.</param>
public sealed record DismissFieldServiceConflictCommand(Guid EntryId) : IRequest<Unit>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
