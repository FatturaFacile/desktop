// <copyright file="MarkdownParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;

namespace FatturaFacile.Wpf.Copilot;

/// <summary>
/// A small, dependency free Markdown parser covering what the assistant produces: headings, paragraphs, bullet and
/// numbered lists, quotes, fenced code, bold, italic, inline code and links.
/// </summary>
internal static class MarkdownParser
{
    private const string Fence = "```";

    /// <summary>
    /// Parses a Markdown text into blocks.
    /// </summary>
    /// <param name="markdown">The Markdown text.</param>
    /// <returns>The blocks.</returns>
    public static IReadOnlyList<MarkdownBlock> Parse(string? markdown)
    {
        List<MarkdownBlock> blocks = [];

        if (string.IsNullOrWhiteSpace(markdown))
        {
            return blocks;
        }

        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        StringBuilder paragraph = new();
        StringBuilder code = new();
        var inCode = false;

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();

            if (line.TrimStart().StartsWith(Fence, StringComparison.Ordinal))
            {
                if (inCode)
                {
                    blocks.Add(new MarkdownBlock(MarkdownBlockKind.CodeBlock, 0, [new MarkdownInline(MarkdownInlineKind.Text, code.ToString().TrimEnd('\n'))]));
                    _ = code.Clear();
                }
                else
                {
                    FlushParagraph(blocks, paragraph);
                }

                inCode = !inCode;
                continue;
            }

            if (inCode)
            {
                _ = code.Append(raw).Append('\n');
                continue;
            }

            if (line.Length == 0)
            {
                FlushParagraph(blocks, paragraph);
                continue;
            }

            if (TryParseLineBlock(line, out var block))
            {
                FlushParagraph(blocks, paragraph);
                blocks.Add(block);
                continue;
            }

            _ = paragraph.Length > 0 ? paragraph.Append(' ') : paragraph;
            _ = paragraph.Append(line.Trim());
        }

        if (inCode && code.Length > 0)
        {
            blocks.Add(new MarkdownBlock(MarkdownBlockKind.CodeBlock, 0, [new MarkdownInline(MarkdownInlineKind.Text, code.ToString().TrimEnd('\n'))]));
        }

        FlushParagraph(blocks, paragraph);
        return blocks;
    }

    /// <summary>
    /// Parses the inline spans of a text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The spans.</returns>
    public static IReadOnlyList<MarkdownInline> ParseInlines(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        List<MarkdownInline> result = [];
        StringBuilder plain = new();
        var i = 0;

        while (i < text.Length)
        {
            if (TryReadSpan(text, i, out var span, out var next))
            {
                FlushPlain(result, plain);
                result.Add(span);
                i = next;
                continue;
            }

            _ = plain.Append(text[i]);
            i++;
        }

        FlushPlain(result, plain);
        return result;
    }

    private static bool TryParseLineBlock(string line, out MarkdownBlock block)
    {
        var trimmed = line.TrimStart();

        var hashes = 0;

        while (hashes < trimmed.Length && trimmed[hashes] == '#')
        {
            hashes++;
        }

        if (hashes is >= 1 and <= 6 && trimmed.Length > hashes && trimmed[hashes] == ' ')
        {
            block = new MarkdownBlock(MarkdownBlockKind.Heading, hashes, ParseInlines(trimmed[(hashes + 1)..].Trim()));
            return true;
        }

        if (trimmed.Length > 2 && trimmed[0] is '-' or '*' or '+' && trimmed[1] == ' ')
        {
            block = new MarkdownBlock(MarkdownBlockKind.Bullet, 0, ParseInlines(trimmed[2..].Trim()));
            return true;
        }

        var digits = 0;

        while (digits < trimmed.Length && char.IsAsciiDigit(trimmed[digits]))
        {
            digits++;
        }

        if (digits is > 0 and < 4 && trimmed.Length > digits + 1 && trimmed[digits] == '.' && trimmed[digits + 1] == ' ')
        {
            var number = int.Parse(trimmed[..digits], CultureInfo.InvariantCulture);
            block = new MarkdownBlock(MarkdownBlockKind.Numbered, number, ParseInlines(trimmed[(digits + 2)..].Trim()));
            return true;
        }

        if (trimmed.Length > 1 && trimmed[0] == '>')
        {
            block = new MarkdownBlock(MarkdownBlockKind.Quote, 0, ParseInlines(trimmed[1..].Trim()));
            return true;
        }

        block = new MarkdownBlock(MarkdownBlockKind.Paragraph, 0, []);
        return false;
    }

    private static void FlushParagraph(List<MarkdownBlock> blocks, StringBuilder paragraph)
    {
        if (paragraph.Length == 0)
        {
            return;
        }

        blocks.Add(new MarkdownBlock(MarkdownBlockKind.Paragraph, 0, ParseInlines(paragraph.ToString())));
        _ = paragraph.Clear();
    }

    private static void FlushPlain(List<MarkdownInline> result, StringBuilder plain)
    {
        if (plain.Length == 0)
        {
            return;
        }

        result.Add(new MarkdownInline(MarkdownInlineKind.Text, plain.ToString()));
        _ = plain.Clear();
    }

    private static bool TryReadSpan(string text, int start, out MarkdownInline span, out int next)
    {
        var c = text[start];

        if (c == '`' && TryFind(text, start + 1, "`", out var codeEnd))
        {
            span = new MarkdownInline(MarkdownInlineKind.Code, text[(start + 1)..codeEnd]);
            next = codeEnd + 1;
            return true;
        }

        if (IsDoubleStar(text, start) && TryFind(text, start + 2, "**", out var boldEnd) && boldEnd > start + 2)
        {
            span = new MarkdownInline(MarkdownInlineKind.Bold, text[(start + 2)..boldEnd]);
            next = boldEnd + 2;
            return true;
        }

        if (CanOpenItalic(text, start) && TryFind(text, start + 1, c.ToString(), out var italicEnd) && italicEnd > start + 1)
        {
            span = new MarkdownInline(MarkdownInlineKind.Italic, text[(start + 1)..italicEnd]);
            next = italicEnd + 1;
            return true;
        }

        if (c == '[' && TryReadLink(text, start, out span, out next))
        {
            return true;
        }

        span = new MarkdownInline(MarkdownInlineKind.Text, string.Empty);
        next = start;
        return false;
    }

    private static bool IsDoubleStar(string text, int start) =>
        text[start] == '*' && start + 1 < text.Length && text[start + 1] == '*';

    private static bool CanOpenItalic(string text, int start) =>
        text[start] is '*' or '_' && start + 1 < text.Length && !char.IsWhiteSpace(text[start + 1]);

    private static bool TryReadLink(string text, int start, out MarkdownInline span, out int next)
    {
        var close = IndexFrom(text, ']', start + 1);

        if (close > start + 1 && close + 1 < text.Length && text[close + 1] == '(')
        {
            var end = IndexFrom(text, ')', close + 2);

            if (end > close + 2)
            {
                span = new MarkdownInline(MarkdownInlineKind.Link, text[(start + 1)..close], text[(close + 2)..end].Trim());
                next = end + 1;
                return true;
            }
        }

        span = new MarkdownInline(MarkdownInlineKind.Text, string.Empty);
        next = start;
        return false;
    }

    private static int IndexFrom(string text, char value, int from)
    {
        var relative = text.AsSpan(from).IndexOf(value);
        return relative < 0 ? -1 : from + relative;
    }

    private static bool TryFind(string text, int from, string token, out int index)
    {
        index = text.IndexOf(token, from, StringComparison.Ordinal);
        return index >= 0;
    }
}
