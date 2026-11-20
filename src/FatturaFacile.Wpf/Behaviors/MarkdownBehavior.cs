// <copyright file="MarkdownBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FatturaFacile.Application.Copilot;
using FatturaFacile.Wpf.Copilot;

namespace FatturaFacile.Wpf.Behaviors;

/// <summary>
/// Renders Markdown into a <see cref="FlowDocumentScrollViewer"/> without code-behind: bind Source (the Markdown),
/// Citations and LinkCommand.
/// </summary>
[SuppressMessage("Design", "MA0182:Avoid unused internal types", Justification = "Used from XAML.")]
internal static class MarkdownBehavior
{
    /// <summary>
    /// Identifies the Source attached property (the Markdown text).
    /// </summary>
    public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
        "Source",
        typeof(string),
        typeof(MarkdownBehavior),
        new PropertyMetadata(defaultValue: null, OnChanged));

    /// <summary>
    /// Identifies the Citations attached property.
    /// </summary>
    public static readonly DependencyProperty CitationsProperty = DependencyProperty.RegisterAttached(
        "Citations",
        typeof(IReadOnlyList<CopilotCitation>),
        typeof(MarkdownBehavior),
        new PropertyMetadata(defaultValue: null, OnChanged));

    /// <summary>
    /// Identifies the LinkCommand attached property.
    /// </summary>
    public static readonly DependencyProperty LinkCommandProperty = DependencyProperty.RegisterAttached(
        "LinkCommand",
        typeof(ICommand),
        typeof(MarkdownBehavior),
        new PropertyMetadata(defaultValue: null, OnChanged));

    /// <summary>
    /// Gets the Markdown text.
    /// </summary>
    /// <param name="target">The viewer.</param>
    /// <returns>The Markdown.</returns>
    public static string? GetSource(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (string?)target.GetValue(SourceProperty);
    }

    /// <summary>
    /// Sets the Markdown text.
    /// </summary>
    /// <param name="target">The viewer.</param>
    /// <param name="value">The Markdown.</param>
    public static void SetSource(DependencyObject target, string? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets the citations.
    /// </summary>
    /// <param name="target">The viewer.</param>
    /// <returns>The citations.</returns>
    public static IReadOnlyList<CopilotCitation>? GetCitations(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (IReadOnlyList<CopilotCitation>?)target.GetValue(CitationsProperty);
    }

    /// <summary>
    /// Sets the citations.
    /// </summary>
    /// <param name="target">The viewer.</param>
    /// <param name="value">The citations.</param>
    public static void SetCitations(DependencyObject target, IReadOnlyList<CopilotCitation>? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(CitationsProperty, value);
    }

    /// <summary>
    /// Gets the link command.
    /// </summary>
    /// <param name="target">The viewer.</param>
    /// <returns>The command.</returns>
    public static ICommand? GetLinkCommand(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (ICommand?)target.GetValue(LinkCommandProperty);
    }

    /// <summary>
    /// Sets the link command.
    /// </summary>
    /// <param name="target">The viewer.</param>
    /// <param name="value">The command.</param>
    public static void SetLinkCommand(DependencyObject target, ICommand? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(LinkCommandProperty, value);
    }

    private static void OnChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FlowDocumentScrollViewer viewer)
        {
            viewer.Document = MarkdownFlowDocumentBuilder.Build(GetSource(viewer), GetCitations(viewer), GetLinkCommand(viewer));
        }
    }
}
