// <copyright file="FlushProcurementOutboxCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="FlushProcurementOutboxCommand"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class FlushProcurementOutboxCommandHandler(IProcurementService service) : IRequestHandler<FlushProcurementOutboxCommand, ProcurementFlushResult>
{
    /// <inheritdoc/>
    public Task<ProcurementFlushResult> Handle(FlushProcurementOutboxCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.FlushAsync(cancellationToken);
    }
}
