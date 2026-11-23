// <copyright file="GetRebateAgreementDetailQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Reads the cached cockpit of a rebate agreement: the agreement, its progress, accrual runs and settlement proposals. It never
/// calls the server, so it works offline.
/// </summary>
/// <param name="AgreementId">The agreement identifier.</param>
public sealed record GetRebateAgreementDetailQuery(string AgreementId) : IRequest<RebateAgreementDetail?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
