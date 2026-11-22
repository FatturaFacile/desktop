// <copyright file="GetCustomerRiskQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads the composite credit risk score of a customer.
/// </summary>
/// <param name="CustomerId">The customer.</param>
public sealed record GetCustomerRiskQuery(Guid CustomerId) : IRequest<ReceivablesLoad<CreditRiskScore>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
