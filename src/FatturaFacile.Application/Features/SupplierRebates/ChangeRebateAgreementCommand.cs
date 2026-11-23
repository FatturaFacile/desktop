// <copyright file="ChangeRebateAgreementCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Activates, closes or deletes a rebate agreement on the server. It needs the server.
/// </summary>
/// <param name="AgreementId">The agreement identifier.</param>
/// <param name="Action">The status change.</param>
public sealed record ChangeRebateAgreementCommand(string AgreementId, RebateAgreementAction Action) : IRequest<SupplierRebatesResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
