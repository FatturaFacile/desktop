// <copyright file="GetAnnualVatReconciliationQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Builds the annual VAT reconciliation schedule of a year.
/// </summary>
/// <param name="Year">The year.</param>
public sealed record GetAnnualVatReconciliationQuery(int Year) : IRequest<AnnualVatReconciliation>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
