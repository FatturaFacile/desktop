// <copyright file="GetManufacturingOverviewQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing;
using MediatR;

namespace FatturaFacile.Application.Features.Manufacturing;

/// <summary>
/// Handler for <see cref="GetManufacturingOverviewQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetManufacturingOverviewQueryHandler(IManufacturingStore store) : IRequestHandler<GetManufacturingOverviewQuery, ManufacturingOverview>
{
    /// <inheritdoc/>
    public Task<ManufacturingOverview> Handle(GetManufacturingOverviewQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return store.GetOverviewAsync(cancellationToken);
    }
}
