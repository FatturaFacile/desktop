// <copyright file="GetBadDebtProvisionQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads the prudential bad-debt provision per aging bucket.
/// </summary>
/// <param name="CustomerId">The customer, null for everybody.</param>
/// <param name="Currency">The ISO currency.</param>
public sealed record GetBadDebtProvisionQuery(Guid? CustomerId, string? Currency) : IRequest<ReceivablesLoad<ProvisionSummary>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
