// <copyright file="GetErarioReconciliationQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Reconciles the VAT liquidations of a year with the Erario c/IVA account.
/// </summary>
/// <param name="Year">The year.</param>
public sealed record GetErarioReconciliationQuery(int Year) : IRequest<ErarioReconciliation>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
