// <copyright file="DeleteRebateContributionCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Removes a planned contribution of a rebate agreement. It needs the server.
/// </summary>
/// <param name="AgreementId">The agreement identifier.</param>
/// <param name="ContributionId">The contribution identifier.</param>
public sealed record DeleteRebateContributionCommand(string AgreementId, string ContributionId) : IRequest<SupplierRebatesResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
