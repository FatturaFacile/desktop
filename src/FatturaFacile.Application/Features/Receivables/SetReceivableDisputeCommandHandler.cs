// <copyright file="SetReceivableDisputeCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="SetReceivableDisputeCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class SetReceivableDisputeCommandHandler(IReceivablesService service) : IRequestHandler<SetReceivableDisputeCommand, ReceivablesActionResult>
{
    /// <inheritdoc/>
    public Task<ReceivablesActionResult> Handle(SetReceivableDisputeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.SetDisputeAsync(request.Request, cancellationToken);
    }
}
