// <copyright file="ConvertAllocationToHardCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Converts a soft tentative booking into a hard commitment.
/// </summary>
/// <param name="AllocationId">The allocation.</param>
/// <param name="Conversion">The project and WBS node to commit on.</param>
public sealed record ConvertAllocationToHardCommand(Guid AllocationId, ConvertToHardRequest Conversion) : IRequest<PlanningCommandOutcome<AllocationMutationResult>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
