// <copyright file="SearchResourceCandidatesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="SearchResourceCandidatesQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class SearchResourceCandidatesQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<SearchResourceCandidatesQuery, IReadOnlyList<ResourceCandidate>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<ResourceCandidate>> Handle(SearchResourceCandidatesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.SearchCandidatesAsync(request.Search, cancellationToken);
    }
}
