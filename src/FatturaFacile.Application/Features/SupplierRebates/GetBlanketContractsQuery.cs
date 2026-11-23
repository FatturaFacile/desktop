// <copyright file="GetBlanketContractsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Reads the cached list of the blanket purchase agreements. It never calls the server, so it works offline.
/// </summary>
public sealed record GetBlanketContractsQuery : IRequest<RebateCached<IReadOnlyList<BlanketContract>>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
