// <copyright file="GetTrialBalanceQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Accounting;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// The trial balance of one ledger book and period, from the locally posted entries.
/// </summary>
/// <param name="From">The first day of the period.</param>
/// <param name="To">The last day of the period.</param>
/// <param name="Ledger">The ledger book.</param>
/// <param name="Columns">The layout, 2 or 4 columns.</param>
/// <param name="CostCenterId">The optional cost center filter.</param>
public sealed record GetTrialBalanceQuery(DateTime From, DateTime To, LedgerBook Ledger, int Columns = 4, Guid? CostCenterId = null) : IRequest<TrialBalanceResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
