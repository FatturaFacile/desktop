// <copyright file="GetPurchaseOrderQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Reads one purchase order with its lines.
/// </summary>
/// <param name="OrderId">The order.</param>
public sealed record GetPurchaseOrderQuery(
    Guid OrderId) : IRequest<ProcurementLoad<PurchaseOrderView>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
