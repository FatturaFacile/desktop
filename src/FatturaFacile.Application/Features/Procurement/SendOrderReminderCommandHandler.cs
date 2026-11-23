// <copyright file="SendOrderReminderCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="SendOrderReminderCommand"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class SendOrderReminderCommandHandler(IProcurementService service) : IRequestHandler<SendOrderReminderCommand, ProcurementActionResult>
{
    /// <inheritdoc/>
    public Task<ProcurementActionResult> Handle(SendOrderReminderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.SendReminderAsync(request.Request, cancellationToken);
    }
}
