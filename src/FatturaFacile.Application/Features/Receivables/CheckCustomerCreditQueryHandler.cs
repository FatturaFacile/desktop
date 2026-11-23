// <copyright file="CheckCustomerCreditQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="CheckCustomerCreditQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class CheckCustomerCreditQueryHandler(IReceivablesService service) : IRequestHandler<CheckCustomerCreditQuery, ReceivablesLoad<CreditDecision>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<CreditDecision>?> Handle(CheckCustomerCreditQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.CheckCreditAsync(request.CustomerId, request.Scope, request.Amount, cancellationToken);
    }
}
