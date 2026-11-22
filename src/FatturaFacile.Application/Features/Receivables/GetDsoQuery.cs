// <copyright file="GetDsoQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads the days sales outstanding.
/// </summary>
/// <param name="CustomerId">The customer, null for the company.</param>
/// <param name="PeriodDays">The days of the period.</param>
/// <param name="Currency">The ISO currency.</param>
public sealed record GetDsoQuery(Guid? CustomerId, int PeriodDays, string? Currency) : IRequest<ReceivablesLoad<DsoResult>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
