// <copyright file="GetMesWorkCentersQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Mes;

/// <summary>
/// Query for the work centres an operator can pick (from the server, or the local cache when offline).
/// </summary>
public sealed record GetMesWorkCentersQuery : IRequest<IReadOnlyList<MesWorkCenter>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeRead;
}
