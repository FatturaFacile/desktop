// <copyright file="GetOrderDispatchesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Reads the dispatch log of an order.
/// </summary>
/// <param name="OrderId">The order.</param>
public sealed record GetOrderDispatchesQuery(
    Guid OrderId) : IRequest<ProcurementLoad<IReadOnlyList<DispatchView>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
