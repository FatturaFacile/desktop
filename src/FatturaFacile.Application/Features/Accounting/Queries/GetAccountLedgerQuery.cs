// <copyright file="GetAccountLedgerQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Accounting;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// The account ledger (mastrino) of one account with running balance, from the locally posted entries.
/// </summary>
/// <param name="AccountId">The account.</param>
/// <param name="From">The first day of the range.</param>
/// <param name="To">The last day of the range.</param>
/// <param name="Ledger">The ledger book.</param>
/// <param name="Page">The page, from 1.</param>
/// <param name="PerPage">The page size.</param>
public sealed record GetAccountLedgerQuery(Guid AccountId, DateTime From, DateTime To, LedgerBook Ledger, int Page = 1, int PerPage = 100) : IRequest<AccountLedgerResult?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
