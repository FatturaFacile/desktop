// <copyright file="FlushReceivablesOutboxCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="FlushReceivablesOutboxCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class FlushReceivablesOutboxCommandHandler(IReceivablesService service) : IRequestHandler<FlushReceivablesOutboxCommand, ReceivablesFlushResult>
{
    /// <inheritdoc/>
    public Task<ReceivablesFlushResult> Handle(FlushReceivablesOutboxCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.FlushAsync(cancellationToken);
    }
}
