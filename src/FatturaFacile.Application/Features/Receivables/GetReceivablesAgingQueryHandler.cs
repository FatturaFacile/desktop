// <copyright file="GetReceivablesAgingQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetReceivablesAgingQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetReceivablesAgingQueryHandler(IReceivablesService service) : IRequestHandler<GetReceivablesAgingQuery, ReceivablesLoad<AgingReport>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<AgingReport>?> Handle(GetReceivablesAgingQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadAgingAsync(request.CustomerId, request.Currency, cancellationToken);
    }
}
