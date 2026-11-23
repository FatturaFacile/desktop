// <copyright file="ReverseRebateAccrualRunCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Reverses a posted accrual run (and its ledger entry) on the server. It needs the server.
/// </summary>
/// <param name="AgreementId">The agreement identifier.</param>
/// <param name="RunId">The run identifier.</param>
/// <param name="Reason">The reason of the reversal.</param>
public sealed record ReverseRebateAccrualRunCommand(string AgreementId, string RunId, string Reason) : IRequest<SupplierRebatesResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
