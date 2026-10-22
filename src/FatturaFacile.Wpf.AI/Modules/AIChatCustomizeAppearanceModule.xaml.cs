// <copyright file="AIChatCustomizeAppearanceModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using Markdig;
using Microsoft.AspNetCore.Components;
using FatturaFacile.Modules.AIChat;

namespace FatturaFacile;

public partial class AIChatCustomizeAppearanceModule
{
    private readonly RenderFragment<BlazorChatMessage> messageTemplate = message => builder =>
    {
        builder.OpenComponent<AIChatCustomizeAppearanceMessageTemplate>(0);
        builder.AddAttribute(1, nameof(AIChatCustomizeAppearanceMessageTemplate.Context), message);
        builder.CloseComponent();
    };

    public AIChatCustomizeAppearanceModule()
    {
        InitializeComponent();

        WarningText = Properties.Resource.EndpointWarning;
        UpdateContentFormat();
        UpdateMessageTemplate();
        aiChatControl.MarkdownConvert += MarkdownConvert;
    }

    private void OnCustomMessageAppearanceChanged(
        object sender,
        System.Windows.RoutedEventArgs e) => UpdateMessageTemplate();

    private void OnRichFormattedResponsesChanged(
        object sender,
        System.Windows.RoutedEventArgs e) => UpdateContentFormat();

    private void UpdateContentFormat() => aiChatControl.ContentFormat =
        richFormattedResponsesCheckEdit.IsChecked == true
            ? ResponseContentFormat.Markdown
            : ResponseContentFormat.PlainText;

    private void UpdateMessageTemplate() => aiChatControl.MessageTemplate =
        customMessageAppearanceCheckEdit.IsChecked == true ? messageTemplate : null;

    private void MarkdownConvert(
        object sender,
        AIChatControlMarkdownConvertEventArgs e)
    {
        var html = Markdown.ToHtml(e.MarkdownText);
        e.HtmlText = (MarkupString)html;
    }
}
