// <copyright file="GetCustomerCreditStatusQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads the credit limit, exposure and block status of a customer.
/// </summary>
/// <param name="CustomerId">The customer.</param>
public sealed record GetCustomerCreditStatusQuery(Guid CustomerId) : IRequest<ReceivablesLoad<CustomerCreditStatus>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
