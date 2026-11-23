// <copyright file="GetBlanketBalanceQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Reads the cached balance of a blanket purchase agreement. It never calls the server, so it works offline.
/// </summary>
/// <param name="ContractId">The blanket agreement identifier.</param>
public sealed record GetBlanketBalanceQuery(string ContractId) : IRequest<RebateCached<BlanketBalance>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
