// <copyright file="CreateDropshipmentCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="CreateDropshipmentCommand"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class CreateDropshipmentCommandHandler(IProcurementService service) : IRequestHandler<CreateDropshipmentCommand, ProcurementActionResult>
{
    /// <inheritdoc/>
    public Task<ProcurementActionResult> Handle(CreateDropshipmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.CreateDropshipmentAsync(request.Request, cancellationToken);
    }
}
