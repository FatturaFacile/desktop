// <copyright file="MarkdownFlowDocumentBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using FatturaFacile.Application.Copilot;

namespace FatturaFacile.Wpf.Copilot;

/// <summary>
/// Renders parsed Markdown into a WPF <see cref="FlowDocument"/>. Labels of entity citations become hyperlinks
/// whose command parameter is "entity:{index}" (the index into the citation list).
/// </summary>
internal static class MarkdownFlowDocumentBuilder
{
    /// <summary>
    /// The prefix of the command parameter of an entity citation link.
    /// </summary>
    public const string EntityPrefix = "entity:";

    private static readonly Brush LinkBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x58, 0xA6, 0xFF)));
    private static readonly Brush CodeBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x21, 0x26, 0x2D)));

    /// <summary>
    /// Builds the document.
    /// </summary>
    /// <param name="markdown">The Markdown text.</param>
    /// <param name="citations">The citations of the message.</param>
    /// <param name="linkCommand">The command run when a link is clicked; the parameter is the link target.</param>
    /// <returns>The document.</returns>
    public static FlowDocument Build(string? markdown, IReadOnlyList<CopilotCitation>? citations, ICommand? linkCommand)
    {
        FlowDocument document = new()
        {
            PagePadding = new Thickness(0),
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)),
            FontSize = 13,
        };

        var entityLabels = (citations ?? [])
            .Select(static (c, i) => (Citation: c, Index: i))
            .Where(static x => x.Citation.IsEntity && !string.IsNullOrEmpty(x.Citation.Label))
            .ToList();

        foreach (var block in MarkdownParser.Parse(markdown))
        {
            document.Blocks.Add(BuildBlock(block, entityLabels, linkCommand));
        }

        return document;
    }

    private static Block BuildBlock(MarkdownBlock block, List<(CopilotCitation Citation, int Index)> entityLabels, ICommand? linkCommand)
    {
        Paragraph paragraph = new() { Margin = new Thickness(0, 0, 0, 6) };

        if (block.Kind == MarkdownBlockKind.CodeBlock)
        {
            paragraph.FontFamily = new FontFamily("Consolas");
            paragraph.Background = CodeBrush;
            paragraph.Padding = new Thickness(8);
            paragraph.Inlines.Add(new Run(block.Inlines[0].Text));
            return paragraph;
        }

        StyleBlock(paragraph, block);

        foreach (var inline in block.Inlines)
        {
            AddInline(paragraph.Inlines, inline, entityLabels, linkCommand);
        }

        return paragraph;
    }

    private static void StyleBlock(Paragraph paragraph, MarkdownBlock block)
    {
        if (block.Kind == MarkdownBlockKind.Heading)
        {
            paragraph.FontWeight = FontWeights.Bold;
            paragraph.FontSize = HeadingSize(block.Level);
        }
        else if (block.Kind == MarkdownBlockKind.Bullet)
        {
            paragraph.Margin = new Thickness(14, 0, 0, 2);
            paragraph.Inlines.Add(new Run("• "));
        }
        else if (block.Kind == MarkdownBlockKind.Numbered)
        {
            paragraph.Margin = new Thickness(14, 0, 0, 2);
            paragraph.Inlines.Add(new Run(block.Level.ToString(CultureInfo.InvariantCulture) + ". "));
        }
        else if (block.Kind == MarkdownBlockKind.Quote)
        {
            paragraph.Margin = new Thickness(10, 0, 0, 6);
            paragraph.Foreground = Brushes.Gray;
            paragraph.FontStyle = FontStyles.Italic;
        }
    }

    private static double HeadingSize(int level) => level switch
    {
        <= 1 => 18,
        2 => 16,
        _ => 14,
    };

    private static void AddInline(InlineCollection target, MarkdownInline inline, List<(CopilotCitation Citation, int Index)> entityLabels, ICommand? linkCommand)
    {
        if (inline.Kind == MarkdownInlineKind.Bold)
        {
            target.Add(new Bold(new Run(inline.Text)));
        }
        else if (inline.Kind == MarkdownInlineKind.Italic)
        {
            target.Add(new Italic(new Run(inline.Text)));
        }
        else if (inline.Kind == MarkdownInlineKind.Code)
        {
            target.Add(new Run(inline.Text) { FontFamily = new FontFamily("Consolas"), Background = CodeBrush });
        }
        else if (inline.Kind == MarkdownInlineKind.Link)
        {
            target.Add(CreateLink(inline.Text, inline.Target ?? string.Empty, linkCommand));
        }
        else
        {
            AddTextWithEntityLinks(target, inline.Text, entityLabels, linkCommand);
        }
    }

    private static void AddTextWithEntityLinks(InlineCollection target, string text, List<(CopilotCitation Citation, int Index)> entityLabels, ICommand? linkCommand)
    {
        var remaining = text;

        while (remaining.Length > 0)
        {
            var best = (Position: -1, Label: string.Empty, Index: -1);

            foreach (var (citation, index) in entityLabels)
            {
                var position = remaining.IndexOf(citation.Label, StringComparison.Ordinal);

                if (position >= 0 && (best.Position < 0 || position < best.Position))
                {
                    best = (position, citation.Label, index);
                }
            }

            if (best.Position < 0)
            {
                target.Add(new Run(remaining));
                return;
            }

            if (best.Position > 0)
            {
                target.Add(new Run(remaining[..best.Position]));
            }

            target.Add(CreateLink(best.Label, EntityPrefix + best.Index.ToString(CultureInfo.InvariantCulture), linkCommand));
            remaining = remaining[(best.Position + best.Label.Length)..];
        }
    }

    private static Hyperlink CreateLink(string text, string target, ICommand? linkCommand) =>
        new(new Run(text))
        {
            Command = linkCommand,
            CommandParameter = target,
            Foreground = LinkBrush,
            ToolTip = target,
        };

    private static SolidColorBrush Freeze(SolidColorBrush brush)
    {
        brush.Freeze();
        return brush;
    }
}
