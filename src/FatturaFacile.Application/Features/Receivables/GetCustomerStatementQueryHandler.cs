// <copyright file="GetCustomerStatementQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetCustomerStatementQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetCustomerStatementQueryHandler(IReceivablesService service) : IRequestHandler<GetCustomerStatementQuery, ReceivablesLoad<CustomerStatement>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<CustomerStatement>?> Handle(GetCustomerStatementQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadStatementAsync(request.CustomerId, request.From, request.To, request.Currency, cancellationToken);
    }
}
