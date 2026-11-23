// <copyright file="SaveRebateContributionCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Plans a contribution on a rebate agreement, or changes one. It needs the server.
/// </summary>
/// <param name="AgreementId">The agreement identifier.</param>
/// <param name="ContributionId">The contribution to change, or null to add.</param>
/// <param name="Input">The contribution.</param>
public sealed record SaveRebateContributionCommand(string AgreementId, string? ContributionId, RebateContributionInput Input) : IRequest<SupplierRebatesResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
