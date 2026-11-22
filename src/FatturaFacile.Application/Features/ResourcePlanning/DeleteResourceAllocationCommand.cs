// <copyright file="DeleteResourceAllocationCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Deletes an allocation.
/// </summary>
/// <param name="AllocationId">The allocation.</param>
public sealed record DeleteResourceAllocationCommand(Guid AllocationId) : IRequest<PlanningCommandOutcome<bool>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
