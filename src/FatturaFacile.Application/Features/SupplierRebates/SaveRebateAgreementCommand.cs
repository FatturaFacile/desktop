// <copyright file="SaveRebateAgreementCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Creates a rebate agreement or updates one on the server. It needs the server.
/// </summary>
/// <param name="AgreementId">The agreement to update, or null to create.</param>
/// <param name="Input">The agreement.</param>
public sealed record SaveRebateAgreementCommand(string? AgreementId, RebateAgreementInput Input) : IRequest<SupplierRebatesResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
