// <copyright file="TemplateIssue.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// A problem found hydrating a template.
/// </summary>
/// <param name="Kind">The kind of problem.</param>
/// <param name="Key">The parameter key concerned, when there is one.</param>
public sealed record TemplateIssue(TemplateIssueKind Kind, string? Key);
