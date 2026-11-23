// <copyright file="GetRebatePayablesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Reads, live from the server, the open supplier invoices a settlement of a supplier can be set off against.
/// </summary>
/// <param name="SupplierId">The supplier identifier.</param>
public sealed record GetRebatePayablesQuery(string SupplierId) : IRequest<RebatePayablesResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
