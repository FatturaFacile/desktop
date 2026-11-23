// <copyright file="GetProcurementQueueQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="GetProcurementQueueQuery"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class GetProcurementQueueQueryHandler(IProcurementService service) : IRequestHandler<GetProcurementQueueQuery, ProcurementQueue>
{
    /// <inheritdoc/>
    public Task<ProcurementQueue> Handle(GetProcurementQueueQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadQueueAsync(cancellationToken);
    }
}
