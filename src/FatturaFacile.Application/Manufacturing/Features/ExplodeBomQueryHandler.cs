// <copyright file="ExplodeBomQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Bom;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Handles <see cref="ExplodeBomQuery"/>.
/// </summary>
/// <param name="store">The offline store.</param>
public sealed class ExplodeBomQueryHandler(IManufacturingReadStore store) : IRequestHandler<ExplodeBomQuery, BomExplosionResult>
{
    /// <inheritdoc/>
    public Task<BomExplosionResult> Handle(ExplodeBomQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExplodeAsync(request, cancellationToken);
    }

    private async Task<BomExplosionResult> ExplodeAsync(ExplodeBomQuery request, CancellationToken cancellationToken)
    {
        var workbench = await store.GetWorkbenchAsync(cancellationToken).ConfigureAwait(false);
        return BomExplosionService.Explode(workbench.Boms.ToList(), request.BomId, request.Quantity);
    }
}
