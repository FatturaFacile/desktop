// <copyright file="GetCapacitySnapshotsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Loads the stored weekly capacity snapshots of a window.
/// </summary>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="EmployeeId">The employee filter, or null.</param>
public sealed record GetCapacitySnapshotsQuery(DateOnly From, DateOnly To, Guid? EmployeeId) : IRequest<IReadOnlyList<ResourceCapacitySnapshot>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
