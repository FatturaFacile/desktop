// <copyright file="GetMaintenanceContractsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Reads the cached maintenance contracts.
/// </summary>
public sealed record GetMaintenanceContractsQuery : IRequest<FieldServiceCached<IReadOnlyList<MaintenanceContract>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
