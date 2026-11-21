// <copyright file="CalculateRoutingQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Manufacturing;
using MediatR;

namespace FatturaFacile.Application.Features.Manufacturing;

/// <summary>
/// Query that calculates the lead time and cost of a cached routing offline.
/// </summary>
/// <param name="RoutingId">The routing.</param>
/// <param name="Quantity">The produced quantity (positive).</param>
/// <param name="IncludeOverlap">Whether the operation overlap is applied.</param>
public sealed record CalculateRoutingQuery(Guid RoutingId, decimal Quantity, bool IncludeOverlap = true) : IRequest<RoutingCalculation?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
