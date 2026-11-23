// <copyright file="GetLipeOverviewQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Reads the Italian VAT liquidations of a year with the LIPE communication prepared for each.
/// </summary>
/// <param name="Year">The year.</param>
public sealed record GetLipeOverviewQuery(int Year) : IRequest<IReadOnlyList<LipeOverviewRow>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
