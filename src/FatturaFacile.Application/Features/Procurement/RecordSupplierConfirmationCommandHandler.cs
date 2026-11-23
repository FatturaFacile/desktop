// <copyright file="RecordSupplierConfirmationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="RecordSupplierConfirmationCommand"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class RecordSupplierConfirmationCommandHandler(IProcurementService service) : IRequestHandler<RecordSupplierConfirmationCommand, ProcurementActionResult>
{
    /// <inheritdoc/>
    public Task<ProcurementActionResult> Handle(RecordSupplierConfirmationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.RecordConfirmationAsync(request.Request, cancellationToken);
    }
}
