// <copyright file="RebalanceAllocationCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Rebalances an allocation by shifting, reassigning, splitting or resizing it.
/// </summary>
/// <param name="AllocationId">The allocation.</param>
/// <param name="Rebalance">The strategy and its parameters.</param>
public sealed record RebalanceAllocationCommand(Guid AllocationId, RebalanceRequest Rebalance) : IRequest<PlanningCommandOutcome<AllocationMutationResult>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
