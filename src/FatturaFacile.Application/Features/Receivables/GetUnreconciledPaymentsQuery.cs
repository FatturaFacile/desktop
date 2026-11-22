// <copyright file="GetUnreconciledPaymentsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads the payments of a customer that are not yet fully allocated.
/// </summary>
/// <param name="CustomerId">The customer.</param>
public sealed record GetUnreconciledPaymentsQuery(Guid CustomerId) : IRequest<ReceivablesLoad<IReadOnlyList<UnreconciledPayment>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
