// <copyright file="FlushFieldServiceOutboxCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="FlushFieldServiceOutboxCommand"/>.
/// </summary>
/// <param name="offline">The offline-first write service.</param>
public sealed class FlushFieldServiceOutboxCommandHandler(IFieldServiceOfflineService offline) : IRequestHandler<FlushFieldServiceOutboxCommand, FieldServiceFlushResult>
{
    /// <inheritdoc/>
    public Task<FieldServiceFlushResult> Handle(FlushFieldServiceOutboxCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return offline.FlushAsync(cancellationToken);
    }
}
