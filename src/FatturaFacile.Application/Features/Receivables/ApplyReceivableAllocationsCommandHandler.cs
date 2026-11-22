// <copyright file="ApplyReceivableAllocationsCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="ApplyReceivableAllocationsCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class ApplyReceivableAllocationsCommandHandler(IReceivablesService service) : IRequestHandler<ApplyReceivableAllocationsCommand, ReceivablesActionResult>
{
    /// <inheritdoc/>
    public Task<ReceivablesActionResult> Handle(ApplyReceivableAllocationsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.ApplyAllocationsAsync(request.Request, cancellationToken);
    }
}
