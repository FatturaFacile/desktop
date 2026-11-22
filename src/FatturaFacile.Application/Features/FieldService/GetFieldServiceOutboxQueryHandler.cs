// <copyright file="GetFieldServiceOutboxQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="GetFieldServiceOutboxQuery"/>.
/// </summary>
/// <param name="offline">The offline-first write service.</param>
public sealed class GetFieldServiceOutboxQueryHandler(IFieldServiceOfflineService offline) : IRequestHandler<GetFieldServiceOutboxQuery, FieldServiceOutboxSummary>
{
    /// <inheritdoc/>
    public Task<FieldServiceOutboxSummary> Handle(GetFieldServiceOutboxQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return offline.SummaryAsync(cancellationToken);
    }
}
