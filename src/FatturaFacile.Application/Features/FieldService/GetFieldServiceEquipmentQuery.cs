// <copyright file="GetFieldServiceEquipmentQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Reads the cached customer equipment.
/// </summary>
public sealed record GetFieldServiceEquipmentQuery : IRequest<FieldServiceCached<IReadOnlyList<FieldServiceEquipment>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
