// <copyright file="GetCustomerLedgerQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetCustomerLedgerQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetCustomerLedgerQueryHandler(IReceivablesService service) : IRequestHandler<GetCustomerLedgerQuery, ReceivablesLoad<CustomerLedger>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<CustomerLedger>?> Handle(GetCustomerLedgerQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadLedgerAsync(request.CustomerId, request.From, request.To, request.Currency, cancellationToken);
    }
}
