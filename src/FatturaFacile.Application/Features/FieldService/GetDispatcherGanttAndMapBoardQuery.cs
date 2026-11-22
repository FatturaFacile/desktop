// <copyright file="GetDispatcherGanttAndMapBoardQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Reads the cached dispatcher board (unassigned queue and technician lanes). It never calls the server, so it works offline.
/// </summary>
public sealed record GetDispatcherGanttAndMapBoardQuery : IRequest<FieldServiceCached<FieldServiceBoard>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
