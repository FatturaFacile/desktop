// <copyright file="AIChatCustomizeAppearanceModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;

using Markdig;

using Microsoft.AspNetCore.Components;

using FatturaFacile.Modules.AIChat;
using FatturaFacile.Wpf.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module demonstrating dynamic appearance customization and rich Markdown response rendering in the AI Chat control.
/// </summary>
public partial class AIChatCustomizeAppearanceModule
{

    /// <summary>
    /// The custom message rendering template for chat bubbles.
    /// </summary>
    private readonly RenderFragment<BlazorChatMessage> messageTemplate = message => builder =>
                                                                                    {
                                                                                        builder
                                                                                           .OpenComponent<
                                                                                                AIChatCustomizeAppearanceMessageTemplate>(
                                                                                                                                          sequence: 0);

                                                                                        builder.AddAttribute(
                                                                                                             sequence: 1,
                                                                                                             name: nameof(
                                                                                                                 AIChatCustomizeAppearanceMessageTemplate
                                                                                                                    .Context),
                                                                                                             value: message);

                                                                                        builder.CloseComponent();
                                                                                    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AIChatCustomizeAppearanceModule"/> class.
    /// </summary>
    public AIChatCustomizeAppearanceModule()
    {
        InitializeComponent();

        WarningText = Resource.EndpointWarning;
        UpdateContentFormat();
        UpdateMessageTemplate();
        aiChatControl.MarkdownConvert += MarkdownConvert;
    }

    /// <summary>
    /// Handles check changes to toggle custom message appearance templates.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void OnCustomMessageAppearanceChanged(
        object                         sender,
        System.Windows.RoutedEventArgs e) => UpdateMessageTemplate();

    /// <summary>
    /// Handles check changes to toggle rich markdown formatting.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void OnRichFormattedResponsesChanged(
        object                         sender,
        System.Windows.RoutedEventArgs e) => UpdateContentFormat();

    /// <summary>
    /// Updates the chat control content format based on UI checkbox state.
    /// </summary>
    private void UpdateContentFormat() => aiChatControl.ContentFormat =
                                              richFormattedResponsesCheckEdit.IsChecked == true
                                                  ? ResponseContentFormat.Markdown
                                                  : ResponseContentFormat.PlainText;

    /// <summary>
    /// Updates the chat control message template based on UI checkbox state.
    /// </summary>
    private void UpdateMessageTemplate() => aiChatControl.MessageTemplate =
                                                customMessageAppearanceCheckEdit.IsChecked == true
                                                    ? messageTemplate
                                                    : null;

    /// <summary>
    /// Converts raw markdown into HTML markup for chat rendering.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The markdown conversion arguments.</param>
    private void MarkdownConvert(
        object                                sender,
        AIChatControlMarkdownConvertEventArgs e)
    {
        var html = Markdown.ToHtml(markdown: e.MarkdownText);
        e.HtmlText = (MarkupString)html;
    }

}
