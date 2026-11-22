// <copyright file="SetUtilizationTargetCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Sets a billable utilization target (upsert); both scopes empty is the default of the tenant.
/// </summary>
/// <param name="Target">The target.</param>
public sealed record SetUtilizationTargetCommand(UtilizationTargetRequest Target) : IRequest<PlanningCommandOutcome<UtilizationTarget>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
