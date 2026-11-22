// <copyright file="GetReceivablesQueueQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetReceivablesQueueQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetReceivablesQueueQueryHandler(IReceivablesService service) : IRequestHandler<GetReceivablesQueueQuery, ReceivablesQueue>
{
    /// <inheritdoc/>
    public Task<ReceivablesQueue> Handle(GetReceivablesQueueQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadQueueAsync(cancellationToken);
    }
}
