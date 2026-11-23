// <copyright file="GetCustomerRiskQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetCustomerRiskQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetCustomerRiskQueryHandler(IReceivablesService service) : IRequestHandler<GetCustomerRiskQuery, ReceivablesLoad<CreditRiskScore>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<CreditRiskScore>?> Handle(GetCustomerRiskQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadRiskAsync(request.CustomerId, cancellationToken);
    }
}
