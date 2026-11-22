// <copyright file="RevokeCreditOverrideCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="RevokeCreditOverrideCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class RevokeCreditOverrideCommandHandler(IReceivablesService service) : IRequestHandler<RevokeCreditOverrideCommand, ReceivablesActionResult>
{
    /// <inheritdoc/>
    public Task<ReceivablesActionResult> Handle(RevokeCreditOverrideCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.RevokeOverrideAsync(request.Request, cancellationToken);
    }
}
