// <copyright file="GetPurchaseOrdersQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Reads the purchase orders, optionally of one status.
/// </summary>
/// <param name="Status">The wire status, or null for all.</param>
public sealed record GetPurchaseOrdersQuery(
    string? Status) : IRequest<ProcurementLoad<IReadOnlyList<PurchaseOrderView>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
