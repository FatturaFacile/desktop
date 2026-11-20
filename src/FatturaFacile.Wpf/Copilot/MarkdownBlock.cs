// <copyright file="MarkdownBlock.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Copilot;

/// <summary>
/// A block of a Markdown document.
/// </summary>
/// <param name="Kind">The block kind.</param>
/// <param name="Level">The heading level, or the number of a numbered item.</param>
/// <param name="Inlines">The inline content (a single text span for code blocks).</param>
internal sealed record MarkdownBlock(MarkdownBlockKind Kind, int Level, IReadOnlyList<MarkdownInline> Inlines);
