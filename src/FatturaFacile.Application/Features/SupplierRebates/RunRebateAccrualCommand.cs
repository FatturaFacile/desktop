// <copyright file="RunRebateAccrualCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Runs the periodic accrual of a rebate agreement on the server (idempotent per period end). It needs the server because the
/// accrual books a journal entry.
/// </summary>
/// <param name="AgreementId">The agreement identifier.</param>
/// <param name="PeriodEnd">The end of the period, or null for today.</param>
/// <param name="PeriodStart">The start of the period, or null to let the server derive it.</param>
public sealed record RunRebateAccrualCommand(string AgreementId, DateOnly? PeriodEnd = null, DateOnly? PeriodStart = null) : IRequest<SupplierRebatesResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
