// <copyright file="GetTechnicianOfflineWorkQueueQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="GetTechnicianOfflineWorkQueueQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetTechnicianOfflineWorkQueueQueryHandler(IFieldServiceStore store) : IRequestHandler<GetTechnicianOfflineWorkQueueQuery, FieldServiceCached<FieldServiceTechnicianBundle>?>
{
    /// <inheritdoc/>
    public Task<FieldServiceCached<FieldServiceTechnicianBundle>?> Handle(GetTechnicianOfflineWorkQueueQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return FieldServiceCacheReader.ReadAsync(store, FieldServiceCacheKinds.Bundle, request.TechnicianId, FieldServiceBundleParser.Parse, cancellationToken);
    }
}
