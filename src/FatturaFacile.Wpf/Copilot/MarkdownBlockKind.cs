// <copyright file="MarkdownBlockKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Copilot;

/// <summary>
/// The kinds of Markdown blocks understood by the Copilot renderer.
/// </summary>
internal enum MarkdownBlockKind
{
    /// <summary>A paragraph.</summary>
    Paragraph = 0,

    /// <summary>A heading; the level is 1 to 6.</summary>
    Heading = 1,

    /// <summary>A bullet list item.</summary>
    Bullet = 2,

    /// <summary>A numbered list item; the level holds the number.</summary>
    Numbered = 3,

    /// <summary>A fenced code block.</summary>
    CodeBlock = 4,

    /// <summary>A block quote.</summary>
    Quote = 5,
}
