// <copyright file="DispatchPurchaseOrderCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="DispatchPurchaseOrderCommand"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class DispatchPurchaseOrderCommandHandler(IProcurementService service) : IRequestHandler<DispatchPurchaseOrderCommand, ProcurementActionResult>
{
    /// <inheritdoc/>
    public Task<ProcurementActionResult> Handle(DispatchPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.DispatchOrderAsync(request.Request, cancellationToken);
    }
}
