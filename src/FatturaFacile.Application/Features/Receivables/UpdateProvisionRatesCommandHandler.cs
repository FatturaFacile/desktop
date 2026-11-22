// <copyright file="UpdateProvisionRatesCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="UpdateProvisionRatesCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class UpdateProvisionRatesCommandHandler(IReceivablesService service) : IRequestHandler<UpdateProvisionRatesCommand, ReceivablesActionResult>
{
    /// <inheritdoc/>
    public Task<ReceivablesActionResult> Handle(UpdateProvisionRatesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.UpdateProvisionRatesAsync(request.Request, cancellationToken);
    }
}
