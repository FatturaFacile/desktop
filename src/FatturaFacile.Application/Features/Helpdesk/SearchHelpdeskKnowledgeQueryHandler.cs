// <copyright file="SearchHelpdeskKnowledgeQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Handler for <see cref="SearchHelpdeskKnowledgeQuery"/>.
/// </summary>
/// <param name="service">The helpdesk service.</param>
public sealed class SearchHelpdeskKnowledgeQueryHandler(IHelpdeskService service) : IRequestHandler<SearchHelpdeskKnowledgeQuery, HelpdeskLoad<IReadOnlyList<HelpdeskKbArticle>>?>
{
    /// <inheritdoc/>
    public Task<HelpdeskLoad<IReadOnlyList<HelpdeskKbArticle>>?> Handle(SearchHelpdeskKnowledgeQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.SearchKnowledgeAsync(request.Text, request.Locale, cancellationToken);
    }
}
