// <copyright file="GetMesBoardQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Mes;

/// <summary>
/// Query for the operator board of a work centre: the server board when reachable (after flushing the outbox), otherwise the cached one, with the pending commands projected onto it.
/// </summary>
/// <param name="WorkCenterId">The work centre.</param>
public sealed record GetMesBoardQuery(Guid WorkCenterId) : IRequest<MesBoardView?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeRead;
}
