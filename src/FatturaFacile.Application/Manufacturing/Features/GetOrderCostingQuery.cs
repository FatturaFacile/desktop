// <copyright file="GetOrderCostingQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Manufacturing.Costing;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Computes standard versus actual cost and WIP of one order from the local data (offline).
/// </summary>
/// <param name="OrderId">The local order identifier.</param>
public sealed record GetOrderCostingQuery(Guid OrderId) : IRequest<WipCostResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.ManufacturingRead;
}
