// <copyright file="HelpdeskKbArticle.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// A knowledge base article found for an agent.
/// </summary>
/// <param name="Id">The server identifier.</param>
/// <param name="Title">The title.</param>
/// <param name="Summary">A short excerpt.</param>
public sealed record HelpdeskKbArticle(string Id, string Title, string Summary);
