// <copyright file="GetUtilizationTargetsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="GetUtilizationTargetsQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class GetUtilizationTargetsQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<GetUtilizationTargetsQuery, IReadOnlyList<UtilizationTarget>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<UtilizationTarget>> Handle(GetUtilizationTargetsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.GetUtilizationTargetsAsync(cancellationToken);
    }
}
