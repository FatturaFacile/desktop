// <copyright file="GetResourceAllocationsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Loads the allocations that overlap a window.
/// </summary>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="EmployeeId">The employee filter, or null.</param>
/// <param name="ProjectId">The project filter, or null.</param>
/// <param name="BookingType">The booking type filter, or null.</param>
public sealed record GetResourceAllocationsQuery(DateOnly From, DateOnly To, Guid? EmployeeId, Guid? ProjectId, ResourceBookingType? BookingType) : IRequest<IReadOnlyList<ResourceAllocation>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
