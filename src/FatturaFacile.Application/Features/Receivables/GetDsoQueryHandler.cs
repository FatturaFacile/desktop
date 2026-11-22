// <copyright file="GetDsoQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetDsoQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetDsoQueryHandler(IReceivablesService service) : IRequestHandler<GetDsoQuery, ReceivablesLoad<DsoResult>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<DsoResult>?> Handle(GetDsoQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadDsoAsync(request.CustomerId, request.PeriodDays, request.Currency, cancellationToken);
    }
}
