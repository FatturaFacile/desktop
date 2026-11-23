// <copyright file="CheckCustomerCreditQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Runs a credit check for an operation; offline, it is decided locally from the cached credit position.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="Scope">The operation.</param>
/// <param name="Amount">What the operation adds to the exposure.</param>
public sealed record CheckCustomerCreditQuery(Guid CustomerId, CreditScope Scope, decimal Amount) : IRequest<ReceivablesLoad<CreditDecision>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
