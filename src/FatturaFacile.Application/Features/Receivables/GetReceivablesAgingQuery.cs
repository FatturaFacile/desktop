// <copyright file="GetReceivablesAgingQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads the aging of the receivables in the buckets the server reports.
/// </summary>
/// <param name="CustomerId">The customer, null for everybody.</param>
/// <param name="Currency">The ISO currency.</param>
public sealed record GetReceivablesAgingQuery(Guid? CustomerId, string? Currency) : IRequest<ReceivablesLoad<AgingReport>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
