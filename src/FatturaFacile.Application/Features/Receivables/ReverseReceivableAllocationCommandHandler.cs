// <copyright file="ReverseReceivableAllocationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="ReverseReceivableAllocationCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class ReverseReceivableAllocationCommandHandler(IReceivablesService service) : IRequestHandler<ReverseReceivableAllocationCommand, ReceivablesActionResult>
{
    /// <inheritdoc/>
    public Task<ReceivablesActionResult> Handle(ReverseReceivableAllocationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.ReverseAllocationAsync(request.Request, cancellationToken);
    }
}
