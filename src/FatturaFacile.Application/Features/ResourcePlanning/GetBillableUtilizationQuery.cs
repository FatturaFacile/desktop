// <copyright file="GetBillableUtilizationQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Loads the billable utilization report of a period.
/// </summary>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="GroupBy">The grouping: resource, department or cost center.</param>
public sealed record GetBillableUtilizationQuery(DateOnly From, DateOnly To, UtilizationGroupBy GroupBy) : IRequest<UtilizationReport>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
