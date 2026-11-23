// <copyright file="AdvanceRebateSettlementCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Moves a settlement proposal through the workflow (submit, approve, execute or cancel) on the server. It needs the server,
/// because the approval, the document and the ledger posting are decided there.
/// </summary>
/// <param name="AgreementId">The agreement identifier.</param>
/// <param name="ProposalId">The proposal identifier.</param>
/// <param name="CurrentStatus">The status of the proposal the user is looking at; the step must be allowed from it.</param>
/// <param name="Action">The workflow step.</param>
/// <param name="Details">The optional data of the step.</param>
public sealed record AdvanceRebateSettlementCommand(string AgreementId, string ProposalId, SettlementStatus CurrentStatus, SettlementAction Action, RebateSettlementStep Details) : IRequest<SupplierRebatesResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
