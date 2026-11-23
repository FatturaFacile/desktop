// <copyright file="RecordDropshipDeliveryCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="RecordDropshipDeliveryCommand"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class RecordDropshipDeliveryCommandHandler(IProcurementService service) : IRequestHandler<RecordDropshipDeliveryCommand, ProcurementActionResult>
{
    /// <inheritdoc/>
    public Task<ProcurementActionResult> Handle(RecordDropshipDeliveryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.RecordDeliveryAsync(request.Request, cancellationToken);
    }
}
