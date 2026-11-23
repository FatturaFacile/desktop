// <copyright file="ProposeRebateSettlementCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Proposes the settlement of the open accruals of a rebate agreement on the server. It needs the server.
/// </summary>
/// <param name="AgreementId">The agreement identifier.</param>
/// <param name="Input">The proposal.</param>
public sealed record ProposeRebateSettlementCommand(string AgreementId, RebateProposalInput Input) : IRequest<SupplierRebatesResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
