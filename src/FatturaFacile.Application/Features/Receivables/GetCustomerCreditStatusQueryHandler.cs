// <copyright file="GetCustomerCreditStatusQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetCustomerCreditStatusQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetCustomerCreditStatusQueryHandler(IReceivablesService service) : IRequestHandler<GetCustomerCreditStatusQuery, ReceivablesLoad<CustomerCreditStatus>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<CustomerCreditStatus>?> Handle(GetCustomerCreditStatusQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadCreditStatusAsync(request.CustomerId, cancellationToken);
    }
}
