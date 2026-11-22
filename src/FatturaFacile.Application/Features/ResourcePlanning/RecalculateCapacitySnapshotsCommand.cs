// <copyright file="RecalculateCapacitySnapshotsCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Recalculates the weekly capacity snapshots of a window.
/// </summary>
/// <param name="Recalculation">The window and the optional employee.</param>
public sealed record RecalculateCapacitySnapshotsCommand(SnapshotRecalculationRequest Recalculation) : IRequest<PlanningCommandOutcome<SnapshotRecalculationResult>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
