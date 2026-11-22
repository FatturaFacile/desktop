// <copyright file="SearchHelpdeskKnowledgeQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Searches the knowledge base for an agent working on a ticket, with the last results of the same search as offline fallback.
/// </summary>
/// <param name="Text">The text to look for.</param>
/// <param name="Locale">The knowledge base locale: it-CH, de-CH, fr-CH, en or it-IT.</param>
public sealed record SearchHelpdeskKnowledgeQuery(string Text, string Locale) : IRequest<HelpdeskLoad<IReadOnlyList<HelpdeskKbArticle>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
