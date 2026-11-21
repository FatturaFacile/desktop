// <copyright file="MarkdownInlineKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Copilot;

/// <summary>
/// The kinds of inline Markdown spans understood by the Copilot renderer.
/// </summary>
internal enum MarkdownInlineKind
{
    /// <summary>Plain text.</summary>
    Text = 0,

    /// <summary>Bold text.</summary>
    Bold = 1,

    /// <summary>Italic text.</summary>
    Italic = 2,

    /// <summary>Inline code.</summary>
    Code = 3,

    /// <summary>A hyperlink.</summary>
    Link = 4,
}
