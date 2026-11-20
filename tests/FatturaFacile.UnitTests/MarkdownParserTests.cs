// <copyright file="MarkdownParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.

using FatturaFacile.Wpf.Copilot;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Markdown parser behind the Copilot message rendering.
/// </summary>
public sealed class MarkdownParserTests
{
    /// <summary>
    /// Headings, lists and paragraphs become blocks.
    /// </summary>
    [Fact]
    public void Parse_ProducesHeadingListAndParagraphBlocks()
    {
        var blocks = MarkdownParser.Parse("# Titolo\n\nUn paragrafo\ncontinua.\n\n- uno\n- due\n\n1. primo\n2. secondo\n\n> citazione");

        Assert.Equal(
            [MarkdownBlockKind.Heading, MarkdownBlockKind.Paragraph, MarkdownBlockKind.Bullet, MarkdownBlockKind.Bullet, MarkdownBlockKind.Numbered, MarkdownBlockKind.Numbered, MarkdownBlockKind.Quote],
            blocks.Select(static b => b.Kind));
        Assert.Equal(1, blocks[0].Level);
        Assert.Equal("Un paragrafo continua.", blocks[1].Inlines[0].Text);
        Assert.Equal(2, blocks[5].Level);
    }

    /// <summary>
    /// Fenced code is kept verbatim.
    /// </summary>
    [Fact]
    public void Parse_KeepsFencedCodeVerbatim()
    {
        var blocks = MarkdownParser.Parse("Prima\n```\nSELECT *\n  FROM x\n```\nDopo");

        Assert.Equal([MarkdownBlockKind.Paragraph, MarkdownBlockKind.CodeBlock, MarkdownBlockKind.Paragraph], blocks.Select(static b => b.Kind));
        Assert.Equal("SELECT *\n  FROM x", blocks[1].Inlines[0].Text);
    }

    /// <summary>
    /// Bold, italic, code and links are recognised; bracketed citation labels stay plain text.
    /// </summary>
    [Fact]
    public void ParseInlines_RecognisesSpans()
    {
        var inlines = MarkdownParser.ParseInlines("**Totale** e *netto* con `code`, vedi [sito](https://example.com) e [LIVA Art. 25].");

        Assert.Equal(MarkdownInlineKind.Bold, inlines[0].Kind);
        Assert.Equal("Totale", inlines[0].Text);
        Assert.Contains(inlines, i => i.Kind == MarkdownInlineKind.Italic && i.Text == "netto");
        Assert.Contains(inlines, i => i.Kind == MarkdownInlineKind.Code && i.Text == "code");
        Assert.Contains(inlines, i => i.Kind == MarkdownInlineKind.Link && i.Target == "https://example.com" && i.Text == "sito");
        Assert.Contains(inlines, i => i.Kind == MarkdownInlineKind.Text && i.Text.Contains("[LIVA Art. 25]", StringComparison.Ordinal));
    }

    /// <summary>
    /// Empty input yields no blocks.
    /// </summary>
    [Fact]
    public void Parse_Empty_ReturnsNoBlocks()
    {
        Assert.Empty(MarkdownParser.Parse("  "));
        Assert.Empty(MarkdownParser.Parse(null));
    }
}
