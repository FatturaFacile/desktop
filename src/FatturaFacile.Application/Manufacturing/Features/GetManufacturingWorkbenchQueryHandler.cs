// <copyright file="GetManufacturingWorkbenchQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Handles <see cref="GetManufacturingWorkbenchQuery"/>.
/// </summary>
/// <param name="store">The offline store.</param>
public sealed class GetManufacturingWorkbenchQueryHandler(IManufacturingReadStore store) : IRequestHandler<GetManufacturingWorkbenchQuery, ManufacturingWorkbench>
{
    /// <inheritdoc/>
    public Task<ManufacturingWorkbench> Handle(GetManufacturingWorkbenchQuery request, CancellationToken cancellationToken) =>
        store.GetWorkbenchAsync(cancellationToken);
}
