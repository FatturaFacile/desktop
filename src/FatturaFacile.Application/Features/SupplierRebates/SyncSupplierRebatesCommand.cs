// <copyright file="SyncSupplierRebatesCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Pulls supplier rebate or blanket agreement data from the server into the local cache.
/// </summary>
/// <param name="Scope">What to pull.</param>
/// <param name="Id">The agreement or blanket agreement identifier, for the scopes that need one.</param>
public sealed record SyncSupplierRebatesCommand(RebateSyncScope Scope, string? Id = null) : IRequest<SupplierRebatesResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
