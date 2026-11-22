// <copyright file="GetFieldWorkOrdersQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Reads the cached list of field work orders.
/// </summary>
public sealed record GetFieldWorkOrdersQuery : IRequest<FieldServiceCached<IReadOnlyList<FieldServiceWorkOrder>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
