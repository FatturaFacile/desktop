// <copyright file="CreateResourceAllocationCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Creates an allocation; the outcome carries the overload check and the warnings, or the violations of the server.
/// </summary>
/// <param name="Allocation">The allocation to create.</param>
public sealed record CreateResourceAllocationCommand(AllocationRequest Allocation) : IRequest<PlanningCommandOutcome<AllocationMutationResult>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
