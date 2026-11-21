// <copyright file="DismissMesConflictCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Mes;

/// <summary>
/// Command that dismisses a refused operator command once the operator has seen it.
/// </summary>
/// <param name="CommandId">The refused command.</param>
public sealed record DismissMesConflictCommand(Guid CommandId) : IRequest, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
