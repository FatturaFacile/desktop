// <copyright file="MarkdownInline.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Copilot;

/// <summary>
/// An inline Markdown span.
/// </summary>
/// <param name="Kind">The span kind.</param>
/// <param name="Text">The visible text.</param>
/// <param name="Target">The link target of a link span.</param>
internal sealed record MarkdownInline(MarkdownInlineKind Kind, string Text, string? Target = null);
