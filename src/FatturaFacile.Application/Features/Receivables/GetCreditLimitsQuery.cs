// <copyright file="GetCreditLimitsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads the credit limit history of a customer.
/// </summary>
/// <param name="CustomerId">The customer.</param>
public sealed record GetCreditLimitsQuery(Guid CustomerId) : IRequest<ReceivablesLoad<IReadOnlyList<CustomerCreditLimit>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
